using UnityEngine;

public class Icyman : Enemy
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public AttackTelegraphVisual swipeTelegraph;

    [Header("Snowball Animation")]
    public Sprite defaultSprite;
    public Sprite snowballWindupSprite;
    public Sprite snowballRecoverySprite;

    [Header("Behavior")]
    public float xMaxDist = 5f;
    public float yMaxDist = 5f;
    public float ySmoothTime = 0.35f;
    public float idleDuration = 5f;
    public float attackRecovery = 1f;

    [Header("Close Range Attack")]
    [Min(0)]
    public int swipeDamage = 10;

    [Tooltip("If the player is within this distance, Icyman uses the melee attack instead of a snowball.")]
    [Min(0f)]
    public float swipeTriggerDistance = 2.5f;

    [Tooltip("How far the melee cone reaches.")]
    [Min(0.1f)]
    public float swipeRange = 1.5f;

    [Tooltip("Cone width in degrees, centered toward the player.")]
    [Range(1f, 180f)]
    public float swipeAngle = 120f;

    [Tooltip("Delay before the melee hit lands and the cone disappears.")]
    [Min(0f)]
    public float swipeWindup = 0.5f;

    [Tooltip("How long before impact the telegraph switches from faded to full color.")]
    [Min(0f)]
    public float swipeTelegraphFullColorLead = 0.15f;

    [Tooltip("Initial telegraph color shown during most of the wind-up.")]
    public Color swipeTelegraphFadedColor = new Color(0.8f, 0.95f, 1f, 0.2f);

    [Tooltip("Final telegraph color shown right before the hit.")]
    public Color swipeTelegraphFullColor = new Color(0.8f, 0.95f, 1f, 0.65f);

    [Tooltip("Optional layer mask to limit what the melee can hit.")]
    public LayerMask playerLayer;

    [Header("Snowball Attack")]
    public GameObject snowballPrefab;
    public Transform snowballSpawn;
    public float snowballSpeed = 6f;
    public float snowballWindup = 0.5f;

    public float topOffsetFromCamera = 3.5f;

    protected override void Start()
    {
        base.Start();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && defaultSprite == null) defaultSprite = spriteRenderer.sprite;
        EnsureSwipeTelegraph();

        fsm = new StateMachine();
        fsm.Init(new Idle(fsm, rb, speed), this);
    }

    private void Update()
    {
        UpdateFacing();
    }

    private void EnsureSwipeTelegraph()
    {
        if (swipeTelegraph != null) return;

        var telegraphObject = new GameObject("IcymanSwipeTelegraph");
        telegraphObject.transform.SetParent(transform, false);
        telegraphObject.transform.localPosition = Vector3.zero;
        swipeTelegraph = telegraphObject.AddComponent<AttackTelegraphVisual>();
    }

    public Vector2 GetCameraRelativeTarget(float targetX, float yOffsetFromTop)
    {
        float topY = Camera.main.transform.position.y + topOffsetFromCamera;
        float y = topY + yOffsetFromTop;
        return new Vector2(targetX, y);
    }

    public Transform FindPlayerTransform()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.transform : null;
    }

    public Vector2 GetSwipeDirection()
    {
        var player = FindPlayerTransform();
        if (player == null) return Vector2.down;

        Vector2 dir = player.position - transform.position;
        if (dir.sqrMagnitude <= 0.0001f) return Vector2.down;
        return dir.normalized;
    }

    public void ShowSwipeTelegraph(Vector2 direction, bool charged)
    {
        EnsureSwipeTelegraph();
        if (swipeTelegraph == null) return;

        if (spriteRenderer != null)
        {
            swipeTelegraph.SetSorting(spriteRenderer.sortingLayerID, spriteRenderer.sortingOrder + 1);
        }

        swipeTelegraph.Show(direction, swipeRange, swipeAngle);
        swipeTelegraph.SetColor(charged ? swipeTelegraphFullColor : swipeTelegraphFadedColor);
    }

    public void SetSwipeTelegraphCharged()
    {
        if (swipeTelegraph != null)
        {
            swipeTelegraph.SetColor(swipeTelegraphFullColor);
        }
    }

    public void HideSwipeTelegraph()
    {
        if (swipeTelegraph != null)
        {
            swipeTelegraph.Hide();
        }
    }

    public void DoSwipeAttack(Vector2 direction)
    {
        Vector2 origin = transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.down;
        }

        float halfAngle = swipeAngle * 0.5f;
        Collider2D[] hits = playerLayer.value != 0
            ? Physics2D.OverlapCircleAll(origin, swipeRange, playerLayer)
            : Physics2D.OverlapCircleAll(origin, swipeRange);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null || !col.CompareTag("Player")) continue;

            Vector2 hitPoint = col.ClosestPoint(origin);
            Vector2 toHit = hitPoint - origin;
            if (toHit.sqrMagnitude <= 0.0001f)
            {
                toHit = (Vector2)col.transform.position - origin;
            }

            if (toHit.sqrMagnitude <= 0.0001f) continue;
            if (Vector2.Angle(direction, toHit.normalized) > halfAngle) continue;

            if (col.TryGetComponent(out HasHealth health))
            {
                health.TakeDamage(swipeDamage);
            }
        }
    }

    public void ShootSnowballAt(Vector2 target)
    {
        if (snowballPrefab == null)
        {
            Debug.LogWarning("Icyman: Snowball prefab not assigned.");
            return;
        }

        Vector2 spawnPos = snowballSpawn != null ? (Vector2)snowballSpawn.position : (Vector2)transform.position;
        GameObject snowball = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
        Vector2 dir = (target - spawnPos).normalized;

        if (snowball.TryGetComponent(out Snowball snowballScript))
        {
            snowballScript.Init(dir, snowballSpeed);
            snowballScript.IgnoreCollider(GetComponent<Collider2D>());
        }
    }

    public void SetDefaultSprite()
    {
        SetSprite(defaultSprite);
    }

    public void SetSnowballWindupSprite()
    {
        SetSprite(snowballWindupSprite);
    }

    public void SetSnowballRecoverySprite()
    {
        SetSprite(snowballRecoverySprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null || sprite == null)
        {
            return;
        }

        spriteRenderer.sprite = sprite;
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        var player = FindPlayerTransform();
        if (player == null)
        {
            return;
        }

        spriteRenderer.flipX = transform.position.x < player.position.x;
    }
}
