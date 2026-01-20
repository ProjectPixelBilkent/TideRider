using UnityEngine;

public class ImitatorAngler2D : MonoBehaviour
{
    public enum State { Passing, Revealed, Dead }
    public State state = State.Passing;

    [Header("References")]
    public Transform ship;                 
    public SpriteRenderer renderer;
    public Sprite disguisedSprite;         // ghost ship sprite
    public Sprite revealedSprite;          // angler sprite
    public AnglerWeakPoint weakPoint;

    [Header("Testing / Placement")]
    [Tooltip("Turn ON to force-spawn above the camera top. Keep OFF while you place it manually in the scene.")]
    public bool autoSpawnFromTop = false;

    [Tooltip("Prevents instant reveal if you hand-place it near the player.")]
    public float revealDelay = 0.4f; 

    [Header("Spawn / Passing By (from top of screen)")]
    [Tooltip("How many 'screen heights' above the top to spawn. 0.2 = a little above, 1 = one full screen above.")]
    public float spawnAboveScreen = 0.25f;

    [Tooltip("Randomize spawn X across the top of the camera view.")]
    public bool randomizeSpawnX = true;

    [Range(0f, 0.5f)]
    public float spawnXPaddingViewport = 0.08f;

    public float passDownSpeed = 2.0f;    

    [Header("Passing X Lock (Block the player)")]
    [Tooltip("Smaller = locks onto player's X more aggressively (try 0.08 - 0.18).")]
    public float xLockSmoothTime = 0.12f;

    [Tooltip("Max horizontal speed while passing (how hard it can block).")]
    public float xLockMaxSpeed = 6.0f;

    [Tooltip("Ignores tiny left/right jitter so it doesn't micro-correct every frame.")]
    public float xDeadZone = 0.05f;

    [Header("Float (optional)")]
    public float wobbleX = 0.05f;
    public float wobbleSpeed = 2.0f;

    [Header("Reveal")]
    public float revealDistance = 1.8f;    // reveal when VERY close

    [Header("Attack / Chase")]
    public float chaseSpeed = 4.5f;

    float baseX;          // wobble is applied around this so it doesn't drift forever
    float revealTimer;
    float xSmoothVel;     // SmoothDamp velocity ref

    void Reset()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        if (renderer == null) renderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // visuals start as ship
        if (renderer && disguisedSprite) renderer.sprite = disguisedSprite;

        if (weakPoint) weakPoint.gameObject.SetActive(false);

        // find ship if not assigned 
        if (ship == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) ship = p.transform;
        }

        revealTimer = revealDelay;

        //auto spawn off for now
        if (autoSpawnFromTop)
            ForceSpawnFromCameraTop();
        else
            baseX = transform.position.x; // important for wobble when hand-placed
    }

    void Update()
    {
        if (state == State.Dead) return;

        // If ship gets assigned later, we still work
        if (ship == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) ship = p.transform;
        }

        if (revealTimer > 0f) revealTimer -= Time.deltaTime;

        if (state == State.Passing)
        {
            PassByFromTop();

            if (revealTimer <= 0f)
                TryReveal();
        }
        else if (state == State.Revealed)
        {
            ChaseShip();
        }
    }

    void ForceSpawnFromCameraTop()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float z = transform.position.z;

        
        float camDist = Mathf.Abs(cam.transform.position.z - z);

       
        Vector3 topCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, camDist));
        Vector3 bottomCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, camDist));


        float xMin = cam.ViewportToWorldPoint(new Vector3(spawnXPaddingViewport, 1f, camDist)).x;
        float xMax = cam.ViewportToWorldPoint(new Vector3(1f - spawnXPaddingViewport, 1f, camDist)).x;

        float startX;
        if (randomizeSpawnX)
        {
            startX = Random.Range(xMin, xMax);
        }
        else
        {
            startX = (ship != null) ? ship.position.x : topCenter.x;
            startX = Mathf.Clamp(startX, xMin, xMax);
        }

        
        float screenHeight = Mathf.Abs(topCenter.y - bottomCenter.y);
        float startY = topCenter.y + screenHeight * spawnAboveScreen;

        baseX = startX;
        xSmoothVel = 0f;

        transform.position = new Vector3(startX, startY, z);
    }

    void PassByFromTop()
    {
        
        Vector3 pos = transform.position;
        pos.y -= passDownSpeed * Time.deltaTime;

        // Actively lock onto player's X so it tries to block
        if (ship != null)
        {
            float targetX = ship.position.x;

            // Optional deadzone to avoid jittery micro-corrections
            if (Mathf.Abs(targetX - baseX) < xDeadZone)
                targetX = baseX;

            baseX = Mathf.SmoothDamp(baseX, targetX, ref xSmoothVel, xLockSmoothTime, xLockMaxSpeed);
        }

       
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleX;
        pos.x = baseX + wobble;

        transform.position = pos;
    }

    void TryReveal()
    {
        if (ship == null) return;

        Vector3 delta = ship.position - transform.position;
        float d2 = delta.sqrMagnitude;
        float r2 = revealDistance * revealDistance;

        if (d2 <= r2)
            Reveal();
    }

    public void Reveal()
    {
        if (state != State.Passing) return;

        state = State.Revealed;

        if (renderer && revealedSprite) renderer.sprite = revealedSprite;

        if (weakPoint)
        {
            weakPoint.gameObject.SetActive(true);
            weakPoint.owner = this;
        }
    }

    void ChaseShip()
    {
        if (ship == null) return;

        Vector3 dir = (ship.position - transform.position).normalized;
        transform.position += dir * chaseSpeed * Time.deltaTime;
    }

    public void Die()
    {
        state = State.Dead;
        gameObject.SetActive(false);
    }
}
