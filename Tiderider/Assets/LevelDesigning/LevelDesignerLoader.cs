using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

public class LevelDesignerLoader : MonoBehaviour
{
    public enum SpawnObjectType
    {
        Obstacle,
        ExternalEffect,
        Enemy,
        EndingObject,
        Coin
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

#if UNITY_EDITOR
    [MenuItem("Tools/Load Scene Objects From JSON")]
    public static void LoadSceneObjects()
    {
        float saveConstant = LevelDesignerScript.saveConstant;

        string levelsPath = Path.Combine(Application.dataPath, "Levels");

        // Ensure folder exists
        if (!Directory.Exists(levelsPath))
        {
            Directory.CreateDirectory(levelsPath);
        }

        string path = EditorUtility.OpenFilePanel(
            "Load Scene Objects JSON",
            levelsPath,
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

        Coin coin = prefab.GetComponent<Coin>();
        if (coin != null)
            return coin.prefabId;

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
            case SpawnObjectType.EndingObject:
            default:
                break;
        }
    }

    private static void ApplyObstacleData(GameObject instance, SavedObjectData data)
    {
        Obstacle obstacle = instance.GetComponent<Obstacle>();
        if (obstacle == null)
            return;

        // Terrain must be set before sprite so SetSpriteIndex picks the correct sprite array.
        TrySetObstacleTerrain(obstacle, data.typeOfTerrain);
        TrySetObstacleSprite(obstacle, data.spriteNo);
    }

    private static void TrySetObstacleSprite(Obstacle obstacle, int spriteNo)
    {
        if (spriteNo < 0)
            return;

        obstacle.SetSpriteIndex(spriteNo);
    }

    private static void TrySetObstacleTerrain(Obstacle obstacle, TerrainType terrainType)
    {
        Obstacle.TerrainType obstacleTerrain = ConvertTerrainTypeBack(terrainType);

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
#endif
}