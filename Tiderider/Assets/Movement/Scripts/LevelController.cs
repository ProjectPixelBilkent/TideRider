// LevelController.cs

using UnityEngine;

/// <summary>
/// Manages the level boundaries by adding an EdgeCollider2D around the camera's view,
/// ensuring the ship stays within the visible frame and keeps the monster at the bottom edge.
/// </summary>
/// <remarks>
/// Maintained by: Level System
/// </remarks>
[RequireComponent(typeof(EdgeCollider2D))]
public class LevelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ObstacleManager obstacleManager;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private string shipTag = "Player";
    [Header("Monster")]
    [SerializeField] private GameObject monster; // Assign in Inspector
    [SerializeField] private float monsterYOffset = 0f; // Optional offset

    private EdgeCollider2D edgeCollider;

    /// <summary>
    /// Initializes references, sets up the edge collider to match the camera bounds, and configures collision.
    /// </summary>
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (obstacleManager == null)
            obstacleManager = FindFirstObjectByType<ObstacleManager>();

        edgeCollider = GetComponent<EdgeCollider2D>();
        SetupEdgeCollider();

        edgeCollider.isTrigger = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    /// <summary>
    /// Continuously moves the camera and obstacle manager upwards.
    /// Also keeps the monster at the bottom edge of the camera.
    /// </summary>
    void Update()
    {
        Vector3 move = Vector3.up * moveSpeed * Time.deltaTime;
        if (mainCamera != null)
            mainCamera.transform.position += move;
        if (obstacleManager != null)
            obstacleManager.transform.position += move;
        if(edgeCollider != null)
            edgeCollider.transform.position += move;

        // Move monster to bottom edge of camera using transform
        if (monster != null && mainCamera != null)
        {
            float camHeight = 2f * mainCamera.orthographicSize;
            Vector3 camPos = mainCamera.transform.position;
            float bottom = camPos.y - camHeight / 2f;
            Vector3 targetPos = new Vector3(camPos.x, bottom + monsterYOffset, 0);

            // Move instantly to the target position using transform
            Vector3 delta = targetPos - monster.transform.position;
            monster.transform.Translate(delta, Space.World);
        }
    }


    /// <summary>
    /// Sets up the EdgeCollider2D to match the camera's current view.
    /// </summary>
    private void SetupEdgeCollider()
    {
        if (mainCamera == null || edgeCollider == null)
            return;

        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
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
        points[4] = points[0]; // Close the loop

        edgeCollider.points = points;
    }




    /// <summary>
    /// Keeps the ship inside the camera frame by clamping its position on collision.
    /// </summary>
    /// <param name="collision">Collision data.</param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(shipTag))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector3 camPos = mainCamera.transform.position;
                float camHeight = 2f * mainCamera.orthographicSize;
                float camWidth = camHeight * mainCamera.aspect;
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
}

