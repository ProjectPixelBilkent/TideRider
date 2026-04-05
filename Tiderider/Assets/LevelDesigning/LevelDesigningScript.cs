using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class LevelDesignerScript : MonoBehaviour
{
    public enum SpawnObjectType
    {
        Obstacle,
        ExternalEffect,
        Enemy
    }

    [System.Serializable]
    public class SavedObjectData
    {
        public string prefabId;
        public string name;
        public SpawnObjectType objectType;

        public int spriteNo;

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

    [MenuItem("Tools/Save Scene Objects To JSON")]
    public static void SaveSceneObjects()
    {
        float saveConstant = 1.8f;

        string path = EditorUtility.SaveFilePanel(
            "Save Scene Objects JSON",
            Application.persistentDataPath,
            "scene_objects",
            "json"
        );

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Save cancelled.");
            return;
        }

        List<SavedObjectData> sortedObjects = new List<SavedObjectData>();

        Obstacle[] obstacles = Object.FindObjectsByType<Obstacle>(FindObjectsSortMode.None);
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        ExternalEffect[] effects = Object.FindObjectsByType<ExternalEffect>(FindObjectsSortMode.None);

        sortedObjects.AddRange(
            obstacles.Select(o => new SavedObjectData
            {
                prefabId = o.prefabId,
                name = o.gameObject.name,
                objectType = SpawnObjectType.Obstacle,

                spriteNo = o.getSpriteNo(),

                posX = o.transform.position.x * saveConstant,
                posY = o.transform.position.y * saveConstant,
                posZ = o.transform.position.z * saveConstant,

                rotX = o.transform.rotation.x,
                rotY = o.transform.rotation.y,
                rotZ = o.transform.rotation.z,
                rotW = o.transform.rotation.w,

                scaleX = o.transform.localScale.x * saveConstant,
                scaleY = o.transform.localScale.y * saveConstant,
                scaleZ = o.transform.localScale.z * saveConstant
            })
        );

        sortedObjects.AddRange(
            enemies.Select(e => new SavedObjectData
            {
                prefabId = e.prefabId,
                name = e.gameObject.name,
                objectType = SpawnObjectType.Enemy,

                spriteNo = -1,

                posX = e.transform.position.x * saveConstant,
                posY = e.transform.position.y * saveConstant,
                posZ = e.transform.position.z * saveConstant,

                rotX = e.transform.rotation.x,
                rotY = e.transform.rotation.y,
                rotZ = e.transform.rotation.z,
                rotW = e.transform.rotation.w,

                scaleX = e.transform.localScale.x * saveConstant,
                scaleY = e.transform.localScale.y * saveConstant,
                scaleZ = e.transform.localScale.z * saveConstant
            })
        );

        sortedObjects.AddRange(
            effects.Select(e => new SavedObjectData
            {
                prefabId = e.prefabId,
                name = e.gameObject.name,
                objectType = SpawnObjectType.ExternalEffect,

                spriteNo = -1,

                posX = e.transform.position.x * saveConstant,
                posY = e.transform.position.y * saveConstant,
                posZ = e.transform.position.z * saveConstant,

                rotX = e.transform.rotation.x,
                rotY = e.transform.rotation.y,
                rotZ = e.transform.rotation.z,
                rotW = e.transform.rotation.w,

                scaleX = e.transform.localScale.x * saveConstant,
                scaleY = e.transform.localScale.y * saveConstant,
                scaleZ = e.transform.localScale.z * saveConstant
            })
        );

        sortedObjects = sortedObjects
            .OrderBy(s => s.posY)
            .ToList();

        SavedSceneData sceneData = new SavedSceneData
        {
            objects = sortedObjects
        };

        string json = JsonUtility.ToJson(sceneData, true);
        File.WriteAllText(path, json);

        Debug.Log($"Saved {sortedObjects.Count} objects to: {path}");
    }
}