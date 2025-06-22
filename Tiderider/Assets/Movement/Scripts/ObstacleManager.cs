using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float obstacleSpeed = 3f;
    [SerializeField] private float spawnYOffset = 1f;

    private float timer = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

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

        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        // Randomly select a prefab
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        GameObject obstacle = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Add movement script or velocity
        Rigidbody2D rb = obstacle.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, -obstacleSpeed);
        }
    }
}


