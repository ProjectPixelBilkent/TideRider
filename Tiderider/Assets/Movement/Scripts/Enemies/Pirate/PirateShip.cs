using UnityEngine;

public class PirateShip : Enemy
{
    [Header("Target")]
    public Rigidbody2D playerRb;          // assign or auto-find by tag "Player"

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.25f;

    [Header("Smoother movement")]
    public float maxSpeed = 4f;
    public float maxAccel = 12f;
    public float recalcInterval = 0.25f;
    [HideInInspector] public float recalcTimer;



    [Header("Aim (placeholder)")]
    public float bulletSpeed = 12f;       // just a float for now

    [Header("Positioning for a hit")]
    public float desiredRange = 6f;     // how far from player we try to shoot from
    public float strafeOffset = 1.5f;   // how much we offset up/down
    [HideInInspector] public int strafeSign = 1; // flips +1 / -1 each recalc


    [Header("Spawn (optional)")]
    public bool autoSpawnFromTop = true;
    public float spawnAboveScreen = 0.25f;
    [Range(0f, 0.5f)] public float spawnXPaddingViewport = 0.08f;

    // shared values between states
    [HideInInspector] public Vector2 moveTarget;
    [HideInInspector] public Vector2 shootDir;

    protected override void Start()
    {
        base.Start();

        FindPlayerIfNeeded();

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
    public static Vector2 ToCamSpace(Vector2 world, Vector2 cam) => world - cam;
    public static Vector2 ToWorldSpace(Vector2 camSpace, Vector2 cam) => camSpace + cam;

}
