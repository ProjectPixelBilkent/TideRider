using UnityEngine;

public class PirateShip : Enemy
{
    [Header("Target")]
    public Rigidbody2D playerRb; // assign or auto-find by tag "Player"

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.25f;

    [Header("Smoother movement")]
    public float maxSpeed = 4f;
    public float maxAccel = 12f;

    [Tooltip("How often to recalculate optimal firing position (in seconds)")]
    public float recalcInterval = 0.25f;
    [HideInInspector] public float recalcTimer;

    [Header("Directional Speed (up faster)")]
    public float maxSpeedUp = 6f;
    public float maxSpeedDown = 3.5f;
    public float maxSpeedSide = 4f;
    public float maxAccelUp = 18f;
    public float maxAccelDown = 12f;
    public float maxAccelSide = 12f;

    [Header("Aim & Bullet Velocity")]
    [Tooltip("Bullet speed used for lead aim calculations - critical for hitting moving targets")]
    public float bulletSpeed = 12f;

    [Header("Positioning for a hit")]
    [Tooltip("Optimal distance from player to shoot from")]
    public float desiredRange = 6f;
    [Tooltip("How much to offset perpendicular to shot direction (strafing)")]
    public float strafeOffset = 1.5f;
    [HideInInspector] public int strafeSign = 1; // flips +1 / -1 each recalc

    [Header("Spawn (optional)")]
    public bool autoSpawnFromTop = true;
    public float spawnAboveScreen = 0.25f;
    [Range(0f, 0.5f)]
    public float spawnXPaddingViewport = 0.08f;

    // Shared values between states
    [HideInInspector] public Vector2 moveTarget;  // Where to move to (world space)
    [HideInInspector] public Vector2 shootDir;    // Direction to shoot for lead aim

    protected override void Start()
    {
        base.Start();
        FindPlayerIfNeeded();
        Debug.Log("PIRATE SHIP START: " + gameObject.name);
        fsm = new StateMachine();
        fsm.Init(new SpawnState(fsm, rb), this);
    }

    public void FindPlayerIfNeeded()
    {
        if (playerRb != null) return;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerRb = p.GetComponent<Rigidbody2D>();
    }

    public void ForceSpawnFromCameraTop()
    {
        var cam = Camera.main;
        if (!cam) return;

        float z = transform.position.z;
        float camDist = Mathf.Abs(cam.transform.position.z - z);

        Vector3 topCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, camDist));
        Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, camDist));

        float xMin = cam.ViewportToWorldPoint(new Vector3(spawnXPaddingViewport, 1f, camDist)).x;
        float xMax = cam.ViewportToWorldPoint(new Vector3(1f - spawnXPaddingViewport, 1f, camDist)).x;

        float startX = Random.Range(xMin, xMax);
        float screenHeight = Mathf.Abs(topCenter.y - bottomCenter.y);
        float startY = topCenter.y + screenHeight * spawnAboveScreen;

        rb.position = new Vector2(startX, startY);
        rb.linearVelocity = Vector2.zero;
    }

    // Utility functions for camera space conversions
    public static Vector2 ToCamSpace(Vector2 world, Vector2 cam) => world - cam;
    public static Vector2 ToWorldSpace(Vector2 camSpace, Vector2 cam) => camSpace + cam;
}