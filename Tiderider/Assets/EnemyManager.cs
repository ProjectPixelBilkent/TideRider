using UnityEngine;

public class EnemyManager : MonoBehaviour
// ObstacleManager.cs

{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float obstacleSpeed = 3f;
    [SerializeField] private float spawnYOffset = 1f;

    [Header("Spawning Control")]
    [SerializeField] private bool spawningEnabled = true;

    private float timer = 0f;
    private Camera mainCamera;

    // Reference to PlayArea's SpriteRenderer
    private SpriteRenderer playAreaRenderer;

    /// <summary>
    /// Initializes the main camera reference and PlayArea bounds.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;

        // Find LevelManager in the scene
        GameObject levelManager = GameObject.Find("LevelManager");
        if (levelManager != null)
        {
            // Find PlayArea child under LevelManager
            Transform playAreaTransform = levelManager.transform.Find("PlayArea");
            if (playAreaTransform != null)
            {
                playAreaRenderer = playAreaTransform.GetComponent<SpriteRenderer>();
            }
            else
            {
                Debug.LogWarning("PlayArea child not found under LevelManager.");
            }
        }
        else
        {
            Debug.LogWarning("LevelManager object not found in the scene.");
        }
    }

    /// <summary>
    /// Handles obstacle spawn timing.
    /// </summary>
    void Update()
    {
        if (!spawningEnabled)
            return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    /// <summary>
    /// Spawns a random obstacle prefab at a random X position above the screen.
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || playAreaRenderer == null)
            return;

        // Get PlayArea bounds
        Bounds bounds = playAreaRenderer.bounds;

        // Random X position within PlayArea width
        float randomX = Random.Range(bounds.min.x, bounds.max.x);

        // Y position at the top of PlayArea
        float spawnY = bounds.max.y + spawnYOffset;

        // Randomly select a prefab
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        if (prefab.GetComponent<SurpriseObstacle>() != null)
        {
            //spawn surprises from 1/4th of the screen up to 3/4ths.
            // Spawn surprises from 1/4th up to 3/4ths of PlayArea height
            float minY = Mathf.Lerp(bounds.min.y, bounds.max.y, 0.25f);
            float maxY = Mathf.Lerp(bounds.min.y, bounds.max.y, 0.75f);
            spawnY = Random.Range(minY, maxY);
        }

        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);
        GameObject obstacle = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Add movement script or velocity
        Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, -obstacleSpeed);
        }
    }

    /// <summary>
    /// Enables or disables obstacle spawning at runtime.
    /// </summary>
    /// <param name="enabled">If true, enables spawning; otherwise disables it.</param>
    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
    }
}
