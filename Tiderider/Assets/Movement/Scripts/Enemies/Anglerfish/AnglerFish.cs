using UnityEngine;

public class AnglerFish : Enemy
{
    [Header("References")]
    [SerializeField] public Transform ship;
    public SpriteRenderer renderer;
    public Sprite disguisedSprite;
    public Sprite revealedSprite;

    [Header("Reveal")]
    public float revealDelay = 0.4f;
    public float revealDistance = 1.8f;
    public bool isRevealed;

    [Header("Spawn / Lurking")]
    public bool autoSpawnFromTop = true;
    public float spawnAboveScreen = 0.25f;
    public bool randomizeSpawnX = true;
    [Range(0f, 0.5f)] public float spawnXPaddingViewport = 0.08f;

    [Header("Top Lurk Movement")]
    public float topOffset = 3.5f;
    public float topFollowSpeed = 1.5f;
    public float xLockMaxSpeed = 4.0f;

    [Header("Blocking")]
    public float blockAheadDistance = 1.2f;
    public float playerMoveThreshold = 0.05f;

    [Header("Lurk")]
    public float lurkTime = 1.0f;

    [Header("Wobble")]
    public float wobbleX = 0.15f;
    public float wobbleSpeed = 1.2f;

    [Header("Chase")]
    public float chaseSpeed = 3.0f;
    public float chaseYOffset = 0.8f;

    [Header("Damage")]
    public int chaseContactDamage = 10;
    public float contactDamageDistance = 0.8f;
    public float damageCooldown = 1.0f;

    [HideInInspector] public float revealTimer;

    protected override void Start()
    {
        base.Start();

        if (renderer == null)
            renderer = GetComponent<SpriteRenderer>();

        if (renderer != null && disguisedSprite != null)
            renderer.sprite = disguisedSprite;

        FindShipIfNeeded();

        isRevealed = false;
        revealTimer = revealDelay;

        if (autoSpawnFromTop)
            ForceSpawnFromCameraTop();

        fsm = new StateMachine();
        fsm.Init(new PassingState(fsm, rb), this);
    }

    public void FindShipIfNeeded()
    {
        if (ship != null) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            ship = p.transform;
    }

    public bool ShouldReveal()
    {
        if (ship == null) return false;
        if (revealTimer > 0f) return false;

        float r2 = revealDistance * revealDistance;
        float d2 = ((Vector2)(ship.position - transform.position)).sqrMagnitude;
        return d2 <= r2;
    }

    public void DoReveal()
    {
        isRevealed = true;

        if (renderer != null && revealedSprite != null)
            renderer.sprite = revealedSprite;
    }

    public void ForceSpawnFromCameraTop()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float z = transform.position.z;
        float camDist = Mathf.Abs(cam.transform.position.z - z);

        Vector3 topCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, camDist));
        Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, camDist));

        float xMin = cam.ViewportToWorldPoint(new Vector3(spawnXPaddingViewport, 1f, camDist)).x;
        float xMax = cam.ViewportToWorldPoint(new Vector3(1f - spawnXPaddingViewport, 1f, camDist)).x;

        FindShipIfNeeded();

        float startX = randomizeSpawnX
            ? Random.Range(xMin, xMax)
            : Mathf.Clamp(ship != null ? ship.position.x : topCenter.x, xMin, xMax);

        float screenHeight = Mathf.Abs(topCenter.y - bottomCenter.y);
        float startY = topCenter.y + screenHeight * spawnAboveScreen;

        rb.position = new Vector2(startX, startY);
    }

    public override void Die()
    {
        base.Die();
        gameObject.SetActive(false);
    }
}
