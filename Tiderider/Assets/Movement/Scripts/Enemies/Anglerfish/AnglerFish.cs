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

    [Header("Spawn / Passing")]
    public bool autoSpawnFromTop = true;
    public float spawnAboveScreen = 0.25f;
    public bool randomizeSpawnX = true;
    [Range(0f, 0.5f)] public float spawnXPaddingViewport = 0.08f;
    public float passDownSpeed = 2.0f;

    [Header("Block Player X")]
    public float xLockSmoothTime = 0.12f;
    public float xLockMaxSpeed = 6.0f;
    public float xDeadZone = 0.05f;

    [Header("Wobble")]
    public float wobbleX = 0.05f;
    public float wobbleSpeed = 2.0f;

    [Header("Chase")]
    public float chaseSpeed = 4.5f;

    [Header("Eat")]
    public float eatTriggerDistance = 0.85f;
    public float eatLungeSpeed = 10f;
    public float eatLungeTime = 0.25f;
    public float eatCooldown = 1.0f;

    [HideInInspector] public float baseX;
    [HideInInspector] public float xSmoothVel;
    [HideInInspector] public float revealTimer;

    protected override void Start()
    {
        base.Start();

        if (renderer == null) renderer = GetComponent<SpriteRenderer>();
        if (renderer && disguisedSprite) renderer.sprite = disguisedSprite;

    

        FindShipIfNeeded();

        isRevealed = false;
        revealTimer = revealDelay;

        if (autoSpawnFromTop) ForceSpawnFromCameraTop();
        else baseX = rb.position.x;

        fsm = new StateMachine();
        fsm.Init(new PassingState(fsm, rb), this); 
    }

    public void FindShipIfNeeded()
    {
        if (ship != null) return;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) ship = p.transform;
    }

    public bool ShouldReveal()
    {
        if (ship == null) return false;
        if (revealTimer > 0f) return false;

        float r2 = revealDistance * revealDistance;
        float d2 = (ship.position - transform.position).sqrMagnitude;
        return d2 <= r2;
    }

    public bool ShouldEat()
    {
        if (ship == null) return false;
        float r2 = eatTriggerDistance * eatTriggerDistance;
        float d2 = (ship.position - transform.position).sqrMagnitude;
        return d2 <= r2;
    }

    public void DoReveal()
    {
        isRevealed = true;
        if (renderer && revealedSprite) renderer.sprite = revealedSprite;

       
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

        FindShipIfNeeded();

        float startX = randomizeSpawnX
            ? Random.Range(xMin, xMax)
            : Mathf.Clamp(ship ? ship.position.x : topCenter.x, xMin, xMax);

        float screenHeight = Mathf.Abs(topCenter.y - bottomCenter.y);
        float startY = topCenter.y + screenHeight * spawnAboveScreen;

        baseX = startX;
        xSmoothVel = 0f;
        rb.position = new Vector2(startX, startY);
    }

    public override void Die()
    {
        base.Die();
        gameObject.SetActive(false);
    }
}
