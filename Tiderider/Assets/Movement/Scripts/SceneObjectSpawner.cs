using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EdgeCollider2D))]
public class SceneObjectSpawner : MonoBehaviour
{
    public static TextAsset sceneJsonFile;
    [SerializeField] private TextAsset jsonForTesting;

    [Header("Spawn Control")]
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private float spawnAheadDistance = 20f;
    [SerializeField] private float enemySpawnAheadDistance = 7.5f;
    [SerializeField] private float bufferAfterEnemy = 7.5f;
    [SerializeField] private Transform spawnedParent;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> prefabEntries = new List<GameObject>();

    [Header("Level Control")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private string shipTag = "Player";
    [Header("Monster")]
    [SerializeField] private GameObject monster;
    [SerializeField] private float monsterYOffset = -0.76f;

    [Header("Scene Objects")]
    [SerializeField] private GameObject blackBackground;
    [SerializeField] private GameObject dialogueCanvas;

    [Header("Background Prefabs")]
    [SerializeField] private GameObject normalWorldBackgroundPrefab;
    [SerializeField] private GameObject mistyWorldBackgroundPrefab;
    [SerializeField] private GameObject mistyWaterBackgroundPrefab;
    [SerializeField] private GameObject icyWorldBackgroundPrefab;
    [SerializeField] private GameObject icyWaterBackgroundPrefab;

    [Header("Misty Atmosphere")]
    [Tooltip("Cloud sprites cropped tightly to the cloud (pivot in the centre).")]
    [SerializeField] private List<Sprite> mistOverlaySprites = new List<Sprite>();
    [Tooltip("Empty vertical space between one cloud and the next, in world units.")]
    [SerializeField] private Vector2 mistVerticalGapRange = new Vector2(1.5f, 6f);
    [SerializeField] private Vector2 mistScaleRange = new Vector2(0.7f, 1.2f);
    [Tooltip("Material using the TideRider/CloudBlur shader: blurs and greys out what is under the cloud. Blur and tint are set on the material.")]
    [SerializeField] private Material mistMaterial;
    [Tooltip("How strongly the blurred, greyed view replaces the sharp view under a cloud (1 = fully blurred).")]
    [SerializeField] private Vector2 mistAlphaRange = new Vector2(0.8f, 0.92f);
    [Tooltip("How fast clouds scroll compared to the sea. Above 1 makes them feel closer to the camera than the ship.")]
    [SerializeField] private float mistParallax = 1.3f;
    [Tooltip("Random sideways drift speed, in world units per second.")]
    [SerializeField] private float mistMaxSideDrift = 0.3f;
    [Tooltip("How far a cloud's centre may sit from the middle, as a fraction of half the playfield width.")]
    [SerializeField, Range(0f, 1f)] private float mistHorizontalSpread = 0.6f;
    [Tooltip("Minimum sideways shift from the previous cloud so consecutive clouds don't line up.")]
    [SerializeField] private float mistMinHorizontalShift = 1.2f;
    [Tooltip("Sorting layer for clouds. Must come after the 2D Renderer's 'Foremost Sorting Layer' (GameObjects) so everything up to the ship is blurred; 'PermaUI' (borders/UI) stays above.")]
    [SerializeField] private string mistSortingLayer = "Clouds";
    [Tooltip("Order within the sorting layer; keep it above everything else on that layer.")]
    [SerializeField] private float mistSortingOrder = 200f;

    private EdgeCollider2D edgeCollider;

    public static Vector3 UpwardsMovement { get; private set; }

    private readonly Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();
    private List<SavedObjectData> objectsToSpawn = new List<SavedObjectData>();
    private int nextSpawnIndex = 0;

    private Enemy activeEnemy;
    public bool isPausedForEnemy = false;
    public bool isPausedForDialogue = false;
    private string lastConversationId;

    private Vector3 lastEnemyOriginalSpawnOffset = Vector3.zero;
    private Vector3 postEnemyObstacleOffset = Vector3.zero;

    private EndingObject[] endingObjects;
    [DoNotSerialize] public bool isInEndingSequence;

    public bool HasGameplayStarted { get; private set; }
    private TerrainType currentTerrain;
    private class MistCloud
    {
        public Transform Transform;
        public float HalfHeight;
        public Vector2 Velocity;
    }

    private readonly List<MistCloud> activeMistClouds = new List<MistCloud>();
    private Transform mistContainer;
    private MistCloud lastMistCloud;
    private float nextMistGap;
    private TMP_Text tapToStartText;

    private void Awake()
    {
        BuildPrefabMap();
        LoadSceneData();
    }

    private void Start()
    {
        UpwardsMovement = Vector3.up * moveSpeed;

        if (mainCamera == null)
            mainCamera = Camera.main;

        edgeCollider = GetComponent<EdgeCollider2D>();
        SetupEdgeCollider();

        edgeCollider.isTrigger = false;
        gameObject.layer = LayerMask.NameToLayer("Default");

        currentTerrain = GetLevelTerrain() == null ? TerrainType.General: (TerrainType) GetLevelTerrain();
        SpawnBackgrounds();
        PrewarmMistClouds();
        EnsureTapToStartPrompt();
        UpdateTapToStartPrompt();
        PlayBGMForTerrain();
    }

    private void PlayBGMForTerrain()
    {
        if (SoundLibrary.Instance == null || objectsToSpawn == null)
            return;

        string bgmId;
        if(GetLevelTerrain() == null)
        {
            return;
        }
        switch(GetLevelTerrain())
        {
            case TerrainType.General:
                bgmId = "world_1";
                break;
            case TerrainType.Ice:
                bgmId = "world_3";
                break;
            case TerrainType.Misty:
                bgmId = "world_2";
                break;
            default:
                return;
        };

        SoundLibrary.Instance.PlayBGM(bgmId);
    }

    private TerrainType? GetLevelTerrain()
    {
        var firstObstacle = objectsToSpawn.FirstOrDefault(o => o.objectType == SpawnObjectType.Obstacle);
        if (firstObstacle == null)
        {
            Debug.LogWarning("No obstacle found in level data. Defaulting terrain to General.");
            return null;
        }

        return firstObstacle.typeOfTerrain;
    }

    private void SpawnBackgrounds()
    {
        DestroyExistingBackground("NormalWorldBackground");
        DestroyExistingBackground("MistyWorldBackground");
        DestroyExistingBackground("MistyWater");
        DestroyExistingBackground("icyBackground");
        DestroyExistingBackground("Icywater");

        switch (currentTerrain)
        {
            case TerrainType.General:
                SpawnBackgroundPrefab(normalWorldBackgroundPrefab);
                break;
            case TerrainType.Misty:
                SpawnBackgroundPrefab(mistyWorldBackgroundPrefab);
                SpawnBackgroundPrefab(mistyWaterBackgroundPrefab);
                break;
            case TerrainType.Ice:
                SpawnBackgroundPrefab(icyWaterBackgroundPrefab);
                SpawnBackgroundPrefab(icyWorldBackgroundPrefab);
                break;
        }
    }

    private void SpawnBackgroundPrefab(GameObject prefab)
    {
        if (prefab == null)
            return;

        Instantiate(prefab);
    }

    private void DestroyExistingBackground(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
            Destroy(existing);
    }

    private DialogueManager dialogueManager;
    private System.Action runtimeDialogueFinishedCallback;
    private bool runtimeDialogueMarkCompleted = true;

    private void Update()
    {
        if (isPausedForDialogue)
        {
            UpdateTapToStartPrompt();
            return;
        }

        if (ProcessPendingDialogueBeforeGameplayStart())
        {
            UpdateTapToStartPrompt();
            return;
        }

        if (!HasGameplayStarted)
        {
            UpdateTapToStartPrompt();
            TryStartGameplayFromCurrentInput();
            return;
        }

        UpdateTapToStartPrompt();

        // Move camera and boundary upward
        Vector3 move = Vector3.up * moveSpeed * Time.deltaTime;
        if (mainCamera != null)
            mainCamera.transform.position += move;
        if (edgeCollider != null)
            edgeCollider.transform.position += move;

        HandleMistyAtmosphere();

        // Keep monster at the bottom edge of the camera
        if (monster != null && mainCamera != null)
        {
            float camHeight = 2f * mainCamera.orthographicSize;
            Vector3 camPos = mainCamera.transform.position;
            float bottom = camPos.y - camHeight / 2f;
            Vector3 targetPos = new Vector3(camPos.x, bottom + monsterYOffset, 1);
            Vector3 delta = targetPos - monster.transform.position;
            monster.transform.Translate(delta, Space.World);
        }

        if (objectsToSpawn == null || objectsToSpawn.Count == 0)
            return;

        if (Camera.main == null)
            return;

        // Fallback: if the active enemy was destroyed without firing OnEnemyDied, resume spawning.
        if (isPausedForEnemy && activeEnemy == null)
        {
            if (Camera.main != null)
                postEnemyObstacleOffset = new Vector3(0, Camera.main.transform.position.y + bufferAfterEnemy, 0);
            isPausedForEnemy = false;
        }

        if (isPausedForEnemy)
            return;

        float cameraY = Camera.main.transform.position.y;

        while (nextSpawnIndex < objectsToSpawn.Count)
        {
            SavedObjectData data = objectsToSpawn[nextSpawnIndex];
            bool isSpecialType = data.objectType == SpawnObjectType.Enemy || data.objectType == SpawnObjectType.EndingObject;
            float threshold = isSpecialType ? enemySpawnAheadDistance : (data.objectType == SpawnObjectType.Dialogue ? 0: spawnAheadDistance);

            if (GetSpawnPosition(data).y > cameraY + threshold)
                break;

            SpawnObject(data);
            nextSpawnIndex++;

            if (isPausedForEnemy || isPausedForDialogue)
                break;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Misty atmosphere: clouds live in world space, enter from above the screen, scroll down a bit
    // faster than the sea (parallax) and are removed once they leave the bottom of the screen.
    // They sit above every gameplay object and, via the CloudBlur material, blur and grey out what is beneath.
    // ---------------------------------------------------------------------------------------------

    private bool IsMistActive => currentTerrain == TerrainType.Misty && mainCamera != null && mistOverlaySprites.Count > 0;

    /// <summary>Fills the starting screen with clouds so the level doesn't begin with empty sky.</summary>
    private void PrewarmMistClouds()
    {
        if (!IsMistActive)
            return;

        float camY = mainCamera.transform.position.y;
        float halfView = mainCamera.orthographicSize;
        float bottom = camY - halfView;
        float top = camY + halfView;

        float cursor = bottom + Random.Range(0f, mistVerticalGapRange.y);
        while (cursor < top)
        {
            MistCloud cloud = SpawnMistCloud(0f);
            if (cloud == null)
                return;

            cloud.Transform.position = new Vector3(cloud.Transform.position.x, cursor + cloud.HalfHeight, 0f);
            cursor += cloud.HalfHeight * 2f + RandomMistGap();
        }

        nextMistGap = RandomMistGap();
    }

    private void HandleMistyAtmosphere()
    {
        if (!IsMistActive)
            return;

        float camY = mainCamera.transform.position.y;
        float halfView = mainCamera.orthographicSize;
        float screenTop = camY + halfView;
        float screenBottom = camY - halfView;

        // Clouds stay put in the world (so they scroll with the sea) plus extra downward speed for parallax.
        for (int i = activeMistClouds.Count - 1; i >= 0; i--)
        {
            MistCloud cloud = activeMistClouds[i];
            if (cloud.Transform == null)
            {
                activeMistClouds.RemoveAt(i);
                continue;
            }

            cloud.Transform.position += (Vector3)(cloud.Velocity * Time.deltaTime);

            if (cloud.Transform.position.y + cloud.HalfHeight < screenBottom)
            {
                Destroy(cloud.Transform.gameObject);
                activeMistClouds.RemoveAt(i);
            }
        }

        // Spawn the next cloud just above the screen once the previous one has moved far enough down.
        float lastCloudTop = lastMistCloud != null && lastMistCloud.Transform != null
            ? lastMistCloud.Transform.position.y + lastMistCloud.HalfHeight
            : float.MinValue;

        if (screenTop - lastCloudTop >= nextMistGap)
        {
            MistCloud cloud = SpawnMistCloud(screenTop);
            if (cloud != null)
                nextMistGap = RandomMistGap();
        }
    }

    /// <summary>Creates one cloud whose bottom edge sits at <paramref name="bottomY"/>.</summary>
    private MistCloud SpawnMistCloud(float bottomY)
    {
        Sprite sprite = mistOverlaySprites[Random.Range(0, mistOverlaySprites.Count)];
        if (sprite == null)
            return null;

        if (mistContainer == null)
            mistContainer = new GameObject("MistClouds").transform;

        float scale = Random.Range(mistScaleRange.x, mistScaleRange.y);
        float halfHeight = sprite.bounds.extents.y * scale;

        GameObject cloudObject = new GameObject("MistCloud");
        cloudObject.transform.SetParent(mistContainer, false);
        cloudObject.transform.position = new Vector3(PickMistX(), bottomY + halfHeight, 0f);
        cloudObject.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer renderer = cloudObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        if (mistMaterial != null)
            renderer.sharedMaterial = mistMaterial;
        renderer.flipX = Random.value < 0.5f;
        renderer.sortingLayerName = mistSortingLayer;
        renderer.sortingOrder = Mathf.RoundToInt(mistSortingOrder);
        renderer.color = new Color(1f, 1f, 1f, Random.Range(mistAlphaRange.x, mistAlphaRange.y));

        // The camera climbs at moveSpeed, so a cloud standing still in the world already scrolls with the sea.
        // Moving it down by (parallax - 1) * moveSpeed makes it pass by faster, i.e. nearer the camera.
        float extraDownSpeed = Mathf.Max(0f, mistParallax - 1f) * moveSpeed;
        float sideDrift = Random.Range(-mistMaxSideDrift, mistMaxSideDrift);

        MistCloud cloud = new MistCloud
        {
            Transform = cloudObject.transform,
            HalfHeight = halfHeight,
            Velocity = new Vector2(sideDrift, -extraDownSpeed)
        };

        activeMistClouds.Add(cloud);
        lastMistCloud = cloud;
        return cloud;
    }

    private float PickMistX()
    {
        float halfWidth = AspectRatioController.DesignedHalfWidth > 0f
            ? AspectRatioController.DesignedHalfWidth
            : mainCamera.orthographicSize * mainCamera.aspect;
        float range = halfWidth * mistHorizontalSpread;
        float centerX = mainCamera.transform.position.x;
        float previousX = lastMistCloud != null && lastMistCloud.Transform != null
            ? lastMistCloud.Transform.position.x
            : float.NaN;

        float x = centerX;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            x = centerX + Random.Range(-range, range);
            if (float.IsNaN(previousX) || Mathf.Abs(x - previousX) >= mistMinHorizontalShift)
                break;
        }

        return x;
    }

    private float RandomMistGap()
    {
        return Random.Range(mistVerticalGapRange.x, mistVerticalGapRange.y);
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

        //Debug.Log($"Loaded {objectsToSpawn.Count} objects from assigned TextAsset: {(sceneJsonFile == null ? jsonForTesting: sceneJsonFile).name}");
    }

    private Vector3 GetSpawnPosition(SavedObjectData data)
    {
        Vector3 basePosition = new Vector3(
            data.posX,
            data.posY + yOffset,
            data.posZ
        );

        basePosition += postEnemyObstacleOffset - lastEnemyOriginalSpawnOffset + new Vector3(0,  bufferAfterEnemy, 0);

        return basePosition;
    }

    private bool StandaloneConversation(string id)
    {
        return id == "scene_0" || id == "reunion_scene";
    }

    private bool ProcessPendingDialogueBeforeGameplayStart()
    {
        if (HasGameplayStarted || objectsToSpawn == null || nextSpawnIndex >= objectsToSpawn.Count || Camera.main == null)
            return false;

        SavedObjectData data = objectsToSpawn[nextSpawnIndex];
        if (data.objectType != SpawnObjectType.Dialogue)
            return false;

        float cameraY = Camera.main.transform.position.y;
        if (GetPreGameplaySpawnPosition(data).y > cameraY)
            return false;

        TriggerMidLevelDialogue(data.conversationId);
        nextSpawnIndex++;
        return true;
    }

    private Vector3 GetPreGameplaySpawnPosition(SavedObjectData data)
    {
        return new Vector3(
            data.posX,
            data.posY + yOffset,
            data.posZ
        );
    }

    private void TryStartWithOpeningStandaloneDialogue()
    {
        if (objectsToSpawn == null || objectsToSpawn.Count == 0)
            return;

        SavedObjectData firstObject = objectsToSpawn[0];
        if (firstObject.objectType != SpawnObjectType.Dialogue || !StandaloneConversation(firstObject.conversationId))
            return;

        if (blackBackground != null)
            blackBackground.SetActive(true);

        nextSpawnIndex = 1;
        TriggerMidLevelDialogue(firstObject.conversationId);
    }

    private void TriggerMidLevelDialogue(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            return;

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager not found; skipping mid-level dialogue.");
            return;
        }

        if (StandaloneConversation(conversationId))
            blackBackground.SetActive(true);

        lastConversationId = conversationId;
        isPausedForDialogue = true;
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);
        dialogueManager.ConversationFinished -= HandleMidLevelDialogueFinished;
        dialogueManager.ConversationFinished += HandleMidLevelDialogueFinished;
        dialogueManager.PlayConversation(conversationId, runtimeDialogueMarkCompleted);

        if (!dialogueManager.IsConversationPlaying)
            HandleMidLevelDialogueFinished();
    }

    private void HandleMidLevelDialogueFinished()
    {
        if (dialogueManager != null)
            dialogueManager.ConversationFinished -= HandleMidLevelDialogueFinished;

        isPausedForDialogue = false;
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (StandaloneConversation(lastConversationId))
        {
            EndingObject.InvokeOnLevelCompleted();
        }

        runtimeDialogueFinishedCallback?.Invoke();
        runtimeDialogueFinishedCallback = null;
        runtimeDialogueMarkCompleted = true;
        lastConversationId = null;
        UpdateTapToStartPrompt();
    }

    public void PlayRuntimeDialogue(string conversationId, System.Action onFinished = null)
    {
        PlayRuntimeDialogue(conversationId, onFinished, true);
    }

    public void PlayRuntimeDialogue(string conversationId, System.Action onFinished, bool markCompleted)
    {
        runtimeDialogueFinishedCallback = onFinished;
        runtimeDialogueMarkCompleted = markCompleted;
        TriggerMidLevelDialogue(conversationId);
    }

    private void SpawnObject(SavedObjectData data)
    {
        if (data.objectType == SpawnObjectType.Dialogue)
        {
            TriggerMidLevelDialogue(data.conversationId);
            return;
        }

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

                endingObjects[0].transform.localPosition = new Vector3(0f, 6f, 2f);
                endingObjects[1].transform.localPosition = new Vector3(-2.75f, 2.5f, 2f);
                endingObjects[2].transform.localPosition = new Vector3(2.75f, 2.5f, 2f);

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
        HasGameplayStarted = false;
        UpdateTapToStartPrompt();
    }

    public bool TryStartGameplayFromCurrentInput()
    {
        if (HasGameplayStarted || isPausedForDialogue)
            return HasGameplayStarted;

        if (!IsFreshPressThisFrame())
            return false;

        HasGameplayStarted = true;
        UpdateTapToStartPrompt();
        return true;
    }

    private bool IsFreshPressThisFrame()
    {
        if (Input.GetMouseButtonDown(0))
            return true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
                return true;
        }

        return false;
    }

    private void SetupEdgeCollider()
    {
        if (mainCamera == null || edgeCollider == null)
            return;

        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = AspectRatioController.DESIGN_ASPECT > 0f
            ? 2f * AspectRatioController.DESIGN_ORTHO_SIZE * AspectRatioController.DESIGN_ASPECT
            : camHeight * mainCamera.aspect;
        Vector3 camPos = mainCamera.transform.position;

        float left = camPos.x - camWidth / 2f;
        float right = camPos.x + camWidth / 2f;
        float top = camPos.y + camHeight / 2f;
        float bottom = camPos.y - camHeight / 2f;

        Vector2[] points = new Vector2[5];
        points[0] = new Vector2(left, bottom);
        points[1] = new Vector2(left, top);
        points[2] = new Vector2(right, top);
        points[3] = new Vector2(right, bottom);
        points[4] = points[0];

        edgeCollider.points = points;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(shipTag))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector3 camPos = mainCamera.transform.position;
                float camHeight = 2f * mainCamera.orthographicSize;
                float camWidth = AspectRatioController.DESIGN_ASPECT > 0f
                    ? 2f * AspectRatioController.DESIGN_ORTHO_SIZE * AspectRatioController.DESIGN_ASPECT
                    : camHeight * mainCamera.aspect;
                float left = camPos.x - camWidth / 2f;
                float right = camPos.x + camWidth / 2f;
                float top = camPos.y + camHeight / 2f;
                float bottom = camPos.y - camHeight / 2f;

                Vector3 pos = rb.position;
                pos.x = Mathf.Clamp(pos.x, left, right);
                pos.y = Mathf.Clamp(pos.y, bottom, top);
                rb.position = pos;
            }
        }
    }

    public static Vector2 GetScreenBounds()
    {
        return Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }

    private void EnsureTapToStartPrompt()
    {
        if (tapToStartText != null)
            return;

        Transform safeArea = FindSafeAreaTransform();
        if (safeArea == null)
            return;

        Transform existingPrompt = safeArea.Find("TapToStartPrompt");
        if (existingPrompt != null)
        {
            tapToStartText = existingPrompt.GetComponent<TMP_Text>();
            return;
        }

        GameObject promptObject = new GameObject("TapToStartPrompt", typeof(RectTransform), typeof(TextMeshProUGUI));
        promptObject.transform.SetParent(safeArea, false);

        RectTransform rectTransform = promptObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.12f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.12f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(720f, 120f);

        TextMeshProUGUI text = promptObject.GetComponent<TextMeshProUGUI>();
        text.text = "Tap To Start";
        text.fontSize = 44f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 1f, 1f, 0.95f);
        text.raycastTarget = false;

        Outline outline = promptObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        outline.effectDistance = new Vector2(3f, -3f);

        tapToStartText = text;
    }

    private void UpdateTapToStartPrompt()
    {
        if (tapToStartText == null)
        {
            EnsureTapToStartPrompt();
        }

        if (tapToStartText == null)
            return;

        bool shouldShow = !HasGameplayStarted && !isPausedForDialogue;
        if (tapToStartText.gameObject.activeSelf != shouldShow)
        {
            tapToStartText.gameObject.SetActive(shouldShow);
        }
    }

    private Transform FindSafeAreaTransform()
    {
        MenuManager menuManager = FindFirstObjectByType<MenuManager>();
        if (menuManager != null)
        {
            Transform safeArea = menuManager.transform.Find("SafeArea");
            if (safeArea != null)
                return safeArea;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        return canvas != null ? canvas.transform : null;
    }
}
