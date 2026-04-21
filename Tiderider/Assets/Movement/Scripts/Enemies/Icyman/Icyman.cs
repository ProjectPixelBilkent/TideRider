using UnityEngine;

public class Icyman : Enemy
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Snowball Animation")]
    public Sprite defaultSprite;
    public Sprite snowballWindupSprite;
    public Sprite snowballRecoverySprite;

    [Header("Behavior")]
    public float xMaxDist = 5f;
    public float yMaxDist = 5f;
    public float ySmoothTime = 0.35f;
    public float idleDuration = 5f;
    public float maxMoveDuration = 3f;
    public float attackRecovery = 1f;

    [Header("Close Range Attack")]
    [Min(0)]
    public int swipeDamage = 10;

    [Tooltip("If the player is within this distance, Icyman uses the melee attack instead of a snowball.")]
    [Min(0f)]
    public float swipeTriggerDistance = 2.5f;

    [Tooltip("How far the spin attack reaches.")]
    [Min(0.1f)]
    public float swipeRange = 1.5f;

    [Tooltip("How long the spin attack lasts.")]
    [Min(0f)]
    public float spinDuration = 1f;

    [Tooltip("Degrees per second while spinning.")]
    public float spinSpeed = 720f;

    [Tooltip("How often the spin attack applies damage while the player is in range.")]
    [Min(0f)]
    public float spinDamageInterval = 0.25f;

    [Tooltip("Optional layer mask to limit what the melee can hit.")]
    public LayerMask playerLayer;

    [Header("Snowball Attack")]
    public GameObject snowballPrefab;
    public Transform snowballSpawn;
    public float snowballSpeed = 6f;
    public float snowballWindup = 0.5f;

    [Tooltip("Cone angle in degrees for random snowball aim, centered toward the player.")]
    [Range(0f, 180f)]
    public float snowballAimConeAngle = 30f;

    public float topOffsetFromCamera = 3.5f;

    protected override void Start()
    {
        base.Start();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && defaultSprite == null) defaultSprite = spriteRenderer.sprite;

        fsm = new StateMachine();
        fsm.Init(new Idle(fsm, rb, speed), this);
    }

    private new void Update()
    {
        UpdateFacing();
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

    public void DoSpinAttackDamage()
    {
        Vector2 origin = transform.position;
        Collider2D[] hits = playerLayer.value != 0
            ? Physics2D.OverlapCircleAll(origin, swipeRange, playerLayer)
            : Physics2D.OverlapCircleAll(origin, swipeRange);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col == null || !col.CompareTag("Player")) continue;

            if (col.TryGetComponent(out HasHealth health))
            {
                health.TakeDamage(swipeDamage);
            }
        }
    }

    public void ResetSpinRotation()
    {
        Vector3 angles = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        if (angles.x != 0f || angles.y != 0f)
        {
            transform.rotation = Quaternion.Euler(angles.x, angles.y, 0f);
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

        if (SoundLibrary.Instance != null)
            SoundLibrary.Instance.Play("icy_man_throw");
    }

    public Vector2 GetRandomSnowballTarget()
    {
        Vector2 origin = snowballSpawn != null ? (Vector2)snowballSpawn.position : (Vector2)transform.position;
        var player = FindPlayerTransform();
        if (player == null)
        {
            return origin + Vector2.down * 5f;
        }

        Vector2 toPlayer = (Vector2)player.position - origin;
        if (toPlayer.sqrMagnitude <= 0.0001f)
        {
            return origin + Vector2.down * 5f;
        }

        float randomAngle = Random.Range(-snowballAimConeAngle * 0.5f, snowballAimConeAngle * 0.5f);
        Vector2 randomizedDirection = Quaternion.Euler(0f, 0f, randomAngle) * toPlayer.normalized;
        return origin + randomizedDirection * toPlayer.magnitude;
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
