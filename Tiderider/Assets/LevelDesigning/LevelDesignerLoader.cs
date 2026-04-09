using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class LevelDesignerLoader : MonoBehaviour
{
    public enum SpawnObjectType
    {
        Obstacle,
        ExternalEffect,
        Enemy
    }

    public enum TerrainType
    {
        General,
        Ice,
        Misty
    }

    [System.Serializable]
    public class SavedObjectData
    {
        public string prefabId;
        public string name;
        public SpawnObjectType objectType;

        public int spriteNo;
        public TerrainType typeOfTerrain;

        public float posX;
        public float posY;
        public float posZ;

        public float rotX;
        public float rotY;
        public float rotZ;
        public float rotW;

        public float scaleX;
        public float scaleY;
        public float scaleZ;
    }

    [System.Serializable]
    public class SavedSceneData
    {
        public List<SavedObjectData> objects = new List<SavedObjectData>();
    }

    [MenuItem("Tools/Load Scene Objects From JSON")]
    public static void LoadSceneObjects()
    {
        float saveConstant = 1.8f;

        string path = EditorUtility.OpenFilePanel(
            "Load Scene Objects JSON",
            Application.persistentDataPath,
            "json"
        );

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Load cancelled.");
            return;
        }

        string json = File.ReadAllText(path);
        SavedSceneData sceneData = JsonUtility.FromJson<SavedSceneData>(json);

        if (sceneData == null || sceneData.objects == null || sceneData.objects.Count == 0)
        {
            Debug.LogWarning("No objects found in JSON.");
            return;
        }

        PrefabList prefabList = Object.FindFirstObjectByType<PrefabList>();
        if (prefabList == null)
        {
            Debug.LogError("No PrefabList found in the scene.");
            return;
        }

        Dictionary<string, GameObject> prefabLookup = BuildPrefabLookup(prefabList);

        GameObject root = new GameObject("LoadedLevel_" + Path.GetFileNameWithoutExtension(path));
        Undo.RegisterCreatedObjectUndo(root, "Load Scene Objects");

        int loadedCount = 0;
        int skippedCount = 0;

        foreach (SavedObjectData data in sceneData.objects)
        {
            if (string.IsNullOrEmpty(data.prefabId))
            {
                Debug.LogWarning($"Skipped object '{data.name}' because prefabId is empty.");
                skippedCount++;
                continue;
            }

            if (!prefabLookup.TryGetValue(data.prefabId, out GameObject prefab) || prefab == null)
            {
                Debug.LogWarning($"No prefab found for prefabId '{data.prefabId}' (object '{data.name}').");
                skippedCount++;
                continue;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null)
            {
                Debug.LogWarning($"Failed to instantiate prefabId '{data.prefabId}' (object '{data.name}').");
                skippedCount++;
                continue;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate Loaded Object");
            instance.transform.SetParent(root.transform);

            instance.transform.position = new Vector3(
                data.posX / saveConstant,
                data.posY / saveConstant,
                data.posZ / saveConstant
            );

            instance.transform.rotation = new Quaternion(
                data.rotX,
                data.rotY,
                data.rotZ,
                data.rotW
            );

            instance.transform.localScale = new Vector3(
                data.scaleX / saveConstant,
                data.scaleY / saveConstant,
                data.scaleZ / saveConstant
            );

            if (!string.IsNullOrEmpty(data.name))
                instance.name = data.name;

            ApplyObjectSpecificData(instance, data);

            loadedCount++;
        }

        Debug.Log($"Loaded {loadedCount} objects from: {path}. Skipped: {skippedCount}");
        Selection.activeGameObject = root;
    }

    private static Dictionary<string, GameObject> BuildPrefabLookup(PrefabList prefabList)
    {
        Dictionary<string, GameObject> lookup = new Dictionary<string, GameObject>();

        foreach (GameObject prefab in prefabList.prefabs)
        {
            if (prefab == null)
                continue;

            string prefabId = GetPrefabId(prefab);
            if (string.IsNullOrEmpty(prefabId))
            {
                Debug.LogWarning($"Prefab '{prefab.name}' is missing a component with a prefabId field.");
                continue;
            }

            if (!lookup.ContainsKey(prefabId))
                lookup.Add(prefabId, prefab);
            else
                Debug.LogWarning($"Duplicate prefabId '{prefabId}' found on prefab '{prefab.name}'.");
        }

        return lookup;
    }

    private static string GetPrefabId(GameObject prefab)
    {
        Obstacle obstacle = prefab.GetComponent<Obstacle>();
        if (obstacle != null)
            return obstacle.prefabId;

        Enemy enemy = prefab.GetComponent<Enemy>();
        if (enemy != null)
            return enemy.prefabId;

        ExternalEffect effect = prefab.GetComponent<ExternalEffect>();
        if (effect != null)
            return effect.prefabId;

        return null;
    }

    private static void ApplyObjectSpecificData(GameObject instance, SavedObjectData data)
    {
        switch (data.objectType)
        {
            case SpawnObjectType.Obstacle:
                ApplyObstacleData(instance, data);
                break;

            case SpawnObjectType.Enemy:
            case SpawnObjectType.ExternalEffect:
            default:
                break;
        }
    }

    private static void ApplyObstacleData(GameObject instance, SavedObjectData data)
    {
        Obstacle obstacle = instance.GetComponent<Obstacle>();
        if (obstacle == null)
            return;

        // Set sprite if your Obstacle supports it.
        // Assumes a method like SetSpriteNo(int) exists.
        // If your actual method name differs, replace this call.
        TrySetObstacleSprite(obstacle, data.spriteNo);

        // Set terrain if your Obstacle supports it.
        // Assumes a method or property exists to update terrain.
        TrySetObstacleTerrain(obstacle, data.typeOfTerrain);
    }

    private static void TrySetObstacleSprite(Obstacle obstacle, int spriteNo)
    {
        if (spriteNo < 0)
            return;

        // Replace with your real method if different.
        // Example:
        // obstacle.SetSpriteNo(spriteNo);

        var method = typeof(Obstacle).GetMethod("SetSpriteNo");
        if (method != null)
        {
            method.Invoke(obstacle, new object[] { spriteNo });
            return;
        }

        method = typeof(Obstacle).GetMethod("setSpriteNo");
        if (method != null)
        {
            method.Invoke(obstacle, new object[] { spriteNo });
            return;
        }

        Debug.LogWarning($"Obstacle '{obstacle.name}' could not restore spriteNo because no SetSpriteNo/setSpriteNo method was found.");
    }

    private static void TrySetObstacleTerrain(Obstacle obstacle, TerrainType terrainType)
    {
        Obstacle.TerrainType obstacleTerrain = ConvertTerrainTypeBack(terrainType);

        // Replace with your real method if different.
        // Example:
        // obstacle.SetTerrainType(obstacleTerrain);

        var method = typeof(Obstacle).GetMethod("SetTerrainType");
        if (method != null)
        {
            method.Invoke(obstacle, new object[] { obstacleTerrain });
            return;
        }

        method = typeof(Obstacle).GetMethod("setTerrainType");
        if (method != null)
        {
            method.Invoke(obstacle, new object[] { obstacleTerrain });
            return;
        }

        var property = typeof(Obstacle).GetProperty("terrainType");
        if (property != null && property.CanWrite)
        {
            property.SetValue(obstacle, obstacleTerrain);
            return;
        }

        Debug.LogWarning($"Obstacle '{obstacle.name}' could not restore terrain because no SetTerrainType/setTerrainType/property was found.");
    }

    private static Obstacle.TerrainType ConvertTerrainTypeBack(TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Ice:
                return Obstacle.TerrainType.Ice;
            case TerrainType.Misty:
                return Obstacle.TerrainType.Misty;
            case TerrainType.General:
            default:
                return Obstacle.TerrainType.General;
        }
    }
}