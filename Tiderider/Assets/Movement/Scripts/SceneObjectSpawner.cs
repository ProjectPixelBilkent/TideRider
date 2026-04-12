using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SceneObjectSpawner : MonoBehaviour
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

    public static TextAsset sceneJsonFile;
    [SerializeField] private TextAsset jsonForTesting;

    [Header("Spawn Control")]
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float spawnAheadDistance = 20f;
    [SerializeField] private Transform spawnedParent;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> prefabEntries = new List<GameObject>();

    private readonly Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();
    private List<SavedObjectData> objectsToSpawn = new List<SavedObjectData>();
    private int nextSpawnIndex = 0;

    private Enemy activeEnemy;
    public bool isPausedForEnemy = false;

    private Vector3 lastEnemyOriginalSpawnOffset = Vector3.zero;
    private Vector3 postEnemyObstacleOffset = Vector3.zero;

    private EndingObject[] endingObjects;
    [DoNotSerialize] public bool isInEndingSequence;

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

        if (isPausedForEnemy)
            return;

        while (nextSpawnIndex < objectsToSpawn.Count &&
               GetSpawnPosition(objectsToSpawn[nextSpawnIndex]).y <= currentY)
        {
            SavedObjectData data = objectsToSpawn[nextSpawnIndex];
            SpawnObject(data);
            nextSpawnIndex++;

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

            EndingObject ending = entry.GetComponent<EndingObject>();
            if(ending != null && !string.IsNullOrEmpty (ending.prefabId))
            {
                if(!prefabMap.ContainsKey (ending.prefabId))
                    prefabMap.Add(ending.prefabId, entry);
            }

            Coin coin = entry.GetComponent<Coin>();
            if (coin != null && !string.IsNullOrEmpty(coin.prefabId))
            {
                if (!prefabMap.ContainsKey(coin.prefabId))
                    prefabMap.Add(coin.prefabId, entry);
            }
        }
    }

    private void LoadSceneData()
    {
        if (sceneJsonFile == null && jsonForTesting == null)
        {
            Debug.LogError("Scene JSON file is not assigned in the Inspector.");
            return;
        }

        string json = sceneJsonFile==null ? jsonForTesting.text: sceneJsonFile.text;
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

        Debug.Log($"Loaded {objectsToSpawn.Count} objects from assigned TextAsset: {(sceneJsonFile == null ? jsonForTesting: sceneJsonFile).name}");
    }

    private Vector3 GetSpawnPosition(SavedObjectData data)
    {
        Vector3 basePosition = new Vector3(
            data.posX,
            data.posY + yOffset,
            data.posZ
        );

        basePosition += postEnemyObstacleOffset - lastEnemyOriginalSpawnOffset;

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
                obstacle.SetTerrainType((Obstacle.TerrainType)data.typeOfTerrain);
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
                lastEnemyOriginalSpawnOffset = new Vector3(data.posX, data.posY + yOffset, data.posZ);
            }
        }
        else if (data.objectType == SpawnObjectType.EndingObject)
        {
            EndingObject ending = obj.GetComponent<EndingObject>();
            if (ending != null && ending.fake)
            {
                isInEndingSequence = true;
                endingObjects = new EndingObject[3];

                for (int i = 0; i < endingObjects.Length; i++)
                {
                    endingObjects[i] = Instantiate(ending);
                    endingObjects[i].transform.SetParent(Camera.main.transform, false);
                    endingObjects[i].fake = true;
                }

                int trueEnding = Random.Range(0, endingObjects.Length);
                endingObjects[trueEnding].fake = false;

                Destroy(obj);

                // Use localPosition because these are children of Camera.main.transform
                endingObjects[0].transform.localPosition = new Vector3(0f, 8f, 2f);
                endingObjects[1].transform.localPosition = new Vector3(-2.75f, 4f, 2f);
                endingObjects[2].transform.localPosition = new Vector3(2.75f, 4f, 2f);

                var mySequence = DOTween.Sequence();
                float pulseTime = 1.5f;

                mySequence.Append(DOVirtual.DelayedCall(pulseTime / 3f, () => { }));
                mySequence.Append(DOVirtual.DelayedCall(0.1f, () => { }));

                for (int i = 0; i < 3; i++)
                {
                    if (i == trueEnding)
                    {
                        continue;
                    }

                    mySequence.Join(
                        endingObjects[i].SpriteRenderer.DOColor(Color.black, pulseTime / 2f)
                    );
                }

                mySequence.Append(DOVirtual.DelayedCall(0.1f, () => { }));

                for (int i = 0; i < 3; i++)
                {
                    if (i == trueEnding)
                    {
                        continue;
                    }

                    mySequence.Join(
                        endingObjects[i].SpriteRenderer.DOColor(Color.white, pulseTime / 2f)
                    );
                }

                List<TweenCallback> tweenCallbacks = new();
                float shiftTime = 0.35f;

                for (int i = 0; i < 5; i++)
                {
                    tweenCallbacks.Add(() => { ShiftTwo(shiftTime); });
                }

                for (int i = 0; i < 2; i++)
                {
                    tweenCallbacks.Add(() => { RotateOnce(shiftTime); });
                }

                tweenCallbacks.Add(() => { ChangeOrganization(shiftTime); });

                for (int i = 0; i < 20; i++)
                {
                    mySequence.Append(
                        DOVirtual.DelayedCall(
                            shiftTime,
                            tweenCallbacks[Random.Range(0, tweenCallbacks.Count)]
                        )
                    );
                }

                mySequence.Append(DOVirtual.DelayedCall(shiftTime, () =>
                {
                    isInEndingSequence = false;
                }));
            }
        }
    }

    private void ShiftTwo(float time)
    {
        if (endingObjects == null || endingObjects.Length == 0) return;

        int first = Random.Range(0, endingObjects.Length);
        int second = (first + 1 + Random.Range(0, endingObjects.Length - 1)) % endingObjects.Length;

        Vector3 pos1 = endingObjects[first].transform.localPosition;
        Vector3 pos2 = endingObjects[second].transform.localPosition;

        endingObjects[first].transform.DOLocalMove(pos2, time);
        endingObjects[second].transform.DOLocalMove(pos1, time);
    }

    private void RotateOnce(float time)
    {
        if (endingObjects == null || endingObjects.Length == 0) return;

        int biggering = Random.Range(0, 2) == 0 ? 1 : -1;
        Vector3[] positions = new Vector3[endingObjects.Length];

        for (int i = 0; i < endingObjects.Length; i++)
        {
            positions[i] = endingObjects[i].transform.localPosition;
        }

        for (int i = 0; i < endingObjects.Length; i++)
        {
            endingObjects[i].transform.DOLocalMove(
                positions[(i + biggering + positions.Length) % positions.Length],
                time
            );
        }
    }

    private void ChangeOrganization(float time)
    {
        if (endingObjects == null || endingObjects.Length == 0) return;

        float max = float.MinValue;
        float min = float.MaxValue;

        for (int i = 0; i < endingObjects.Length; i++)
        {
            float y = endingObjects[i].transform.localPosition.y;

            if (max < y)
            {
                max = y;
            }

            if (min > y)
            {
                min = y;
            }
        }

        for (int i = 0; i < endingObjects.Length; i++)
        {
            Vector3 localPos = endingObjects[i].transform.localPosition;

            if (Mathf.Abs(localPos.y - max) < Mathf.Abs(localPos.y - min))
            {
                endingObjects[i].transform.DOLocalMoveY(min, time);
            }
            else
            {
                endingObjects[i].transform.DOLocalMoveY(max, time);
            }
        }
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        if (enemy != null)
            enemy.OnEnemyDied -= HandleEnemyDied;

        if (activeEnemy == enemy)
            activeEnemy = null;

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