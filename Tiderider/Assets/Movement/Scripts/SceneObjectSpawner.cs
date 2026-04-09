using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SceneObjectSpawner : MonoBehaviour
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

    [Header("JSON")]
    [SerializeField] private string fileName = "scene_objects.json";

    [Header("Spawn Control")]
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float spawnAheadDistance = 10f;
    [SerializeField] private Transform spawnedParent;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> prefabEntries = new List<GameObject>();

    private readonly Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();
    private List<SavedObjectData> objectsToSpawn = new List<SavedObjectData>();
    private int nextSpawnIndex = 0;

    private Enemy activeEnemy;
    public bool isPausedForEnemy = false;

    // Offset applied to remaining obstacle spawns after an enemy dies
    private Vector3 postEnemyObstacleOffset = Vector3.zero;

    private string FilePath => Path.Combine(Application.persistentDataPath, fileName);

    private void Awake()
    {
        BuildPrefabMap();
        LoadSceneData();
    }

    private void Update()
    {
        if (objectsToSpawn == null || objectsToSpawn.Count == 0)
            return;

        if (Camera.main == null)
            return;

        float currentY = Camera.main.transform.position.y + spawnAheadDistance;

        // Do not continue sequence while waiting for enemy death
        if (isPausedForEnemy)
            return;

        while (nextSpawnIndex < objectsToSpawn.Count &&
               GetSpawnPosition(objectsToSpawn[nextSpawnIndex]).y <= currentY)
        {
            print(nextSpawnIndex);
            SavedObjectData data = objectsToSpawn[nextSpawnIndex];
            SpawnObject(data);
            nextSpawnIndex++;

            // If this spawned object is an enemy, stop further spawning now
            if (isPausedForEnemy)
                break;
        }
    }

    private void BuildPrefabMap()
    {
        prefabMap.Clear();

        foreach (var entry in prefabEntries)
        {
            if (entry == null)
                continue;

            Obstacle obstacle = entry.GetComponent<Obstacle>();
            if (obstacle != null && !string.IsNullOrEmpty(obstacle.prefabId))
            {
                if (!prefabMap.ContainsKey(obstacle.prefabId))
                    prefabMap.Add(obstacle.prefabId, entry);

                continue;
            }

            Enemy enemy = entry.GetComponent<Enemy>();
            if (enemy != null && !string.IsNullOrEmpty(enemy.prefabId))
            {
                if (!prefabMap.ContainsKey(enemy.prefabId))
                    prefabMap.Add(enemy.prefabId, entry);
            }

            ExternalEffect effect = entry.GetComponent<ExternalEffect>();
            if (effect != null && !string.IsNullOrEmpty(effect.prefabId))
            {
                if (!prefabMap.ContainsKey(effect.prefabId))
                    prefabMap.Add(effect.prefabId, entry);
            }
        }
    }

    private void LoadSceneData()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogError($"Scene JSON not found at: {FilePath}");
            return;
        }

        string json = File.ReadAllText(FilePath);
        SavedSceneData sceneData = JsonUtility.FromJson<SavedSceneData>(json);

        if (sceneData == null || sceneData.objects == null)
        {
            Debug.LogError("Failed to parse scene JSON.");
            return;
        }

        objectsToSpawn = sceneData.objects
            .OrderBy(o => o.posY)
            .ToList();

        nextSpawnIndex = 0;

        Debug.Log($"Loaded {objectsToSpawn.Count} objects from: {FilePath}");
    }

    private Vector3 GetSpawnPosition(SavedObjectData data)
    {
        Vector3 basePosition = new Vector3(
            data.posX,
            data.posY + yOffset,
            data.posZ
        );

        basePosition += postEnemyObstacleOffset;

        return basePosition;
    }

    private void SpawnObject(SavedObjectData data)
    {
        if (!prefabMap.TryGetValue(data.prefabId, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning($"No prefab mapped for prefabId: {data.prefabId}");
            return;
        }

        Vector3 position = GetSpawnPosition(data);

        Quaternion rotation = new Quaternion(
            data.rotX,
            data.rotY,
            data.rotZ,
            data.rotW
        );

        GameObject obj = Instantiate(prefab, position, rotation, spawnedParent);
        obj.name = data.name;

        obj.transform.localScale = new Vector3(
            data.scaleX,
            data.scaleY,
            data.scaleZ
        );

        if (data.objectType == SpawnObjectType.Obstacle)
        {
            Obstacle obstacle = obj.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.SetSpriteIndex(data.spriteNo);
            }
        }
        else if (data.objectType == SpawnObjectType.Enemy)
        {
            Enemy enemy = obj.GetComponent<Enemy>();
            if (enemy != null)
            {
                activeEnemy = enemy;
                isPausedForEnemy = true;
                enemy.OnEnemyDied += HandleEnemyDied;
            }
        }
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        if (enemy != null)
            enemy.OnEnemyDied -= HandleEnemyDied;

        if (activeEnemy == enemy)
            activeEnemy = null;

        // Capture the camera's current position as the offset for remaining obstacles
        if (Camera.main != null)
        {
            postEnemyObstacleOffset = new Vector3(0, Camera.main.transform.position.y, 0);
        }

        isPausedForEnemy = false;
    }

    public void ResetSpawner()
    {
        nextSpawnIndex = 0;
        isPausedForEnemy = false;
        activeEnemy = null;
        postEnemyObstacleOffset = Vector3.zero;
    }
}