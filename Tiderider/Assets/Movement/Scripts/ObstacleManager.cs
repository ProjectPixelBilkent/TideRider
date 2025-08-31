// ObstacleManager.cs

using UnityEngine;

/// <summary>
/// Manages obstacle spawning at intervals, with random prefab selection and velocity assignment.
/// </summary>
/// <remarks>
/// Maintained by: Obstacle System
/// </remarks>
public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float obstacleSpeed = 3f;
    [SerializeField] private float spawnYOffset = 1f;

    [Header("Spawning Control")]
    [SerializeField] private bool spawningEnabled = true;

    private float timer = 0f;
    private Camera mainCamera;

    /// <summary>
    /// Initializes the main camera reference.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;
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
            SpawnObstacle();
            timer = 0f;
        }
    }

    /// <summary>
    /// Spawns a random obstacle prefab at a random X position above the screen.
    /// </summary>
    void SpawnObstacle()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return;

        // Get the screen width in world units
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        // Random X position within screen width
        float randomX = Random.Range(-screenBounds.x, screenBounds.x);

        // Y position just above the screen
        float spawnY = screenBounds.y + spawnYOffset;

        // Randomly select a prefab
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

        if (prefab.GetComponent<SurpriseObstacle>() != null )
        {
            //spawn surprises from 1/4th of the screen up to 3/4ths.
            spawnY = Random.Range((3 * mainCamera.transform.position.y - screenBounds.y)/2, (mainCamera.transform.position.y + screenBounds.y) / 2);
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


