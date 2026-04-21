using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Jellyfish : Enemy
{
    private const int RadiusFillTextureSize = 128;
    private const float RadiusFillSoftEdge = 0.08f;
    private static Sprite radiusFillSprite;

    [Header("Animation")]
    public Sprite idleSprite;
    public Sprite movementStretchSprite;
    public Sprite movementExtendSprite;
    public Sprite attackSprite;
    [Min(0f)] public float idleSpriteInterval = 0.2f;
    [Min(0f)] public float movementStretchSpriteInterval = 0.2f;
    [Min(0f)] public float movementExtendSpriteInterval = 0.2f;

    [Header("Behavior")]
    public float topOffsetFromCamera = 3.5f;
    public float lingerTime = 20f;
    public float downSpeed = 1.0f;
    public float bottomOffsetFromCamera = -6f;

    [Header("Shock")]
    public float shockRadius = 1.7f;
    public float visualShockRadius = 1.7f;
    public int shockDamage = 10;
    public float shockDuration = 3f;
    public float shockCooldown = 3f;
    [Min(0f)] public float shockDamageInterval = 0.5f;
    [HideInInspector] public bool isShockCharged = true;
    public Player playerTarget;

    [Header("Lightning")]
    public GameObject lightningPrefab;
    public float lightningSpeed = 25f;

    [Header("Shock Visuals")]
    public Color chargedColor = new Color(0.5f, 0.9f, 1f);
    public Color rechargingColor = new Color(0.4f, 0.4f, 0.4f);
    public Color chargedRadiusColor = new Color(0.35f, 0.75f, 1f);
    public Color rechargingRadiusColor = new Color(0.45f, 0.45f, 0.5f);
    public Color chargedRadiusFillColor = new Color(0.35f, 0.75f, 1f);
    public Color rechargingRadiusFillColor = new Color(0.45f, 0.45f, 0.5f);
    public Color attackFlashColorA = Color.white;
    public Color attackFlashColorB = new Color(0.5f, 0.9f, 1f);
    [Min(0f)] public float attackFlashInterval = 0.08f;
    [Range(0f, 1f)] public float radiusAlpha = 0.4f;
    [Range(0f, 1f)] public float radiusFillAlpha = 0.14f;
    [Range(0f, 1f)] public float attackFlashRadiusAlpha = 0.8f;
    public float radiusLineWidth = 0.05f;
    [Tooltip("Number of segments used to draw the radius circle.")]
    public int radiusSegments = 48;

    [Header("Attack Ring")]
    public Sprite attackRingSprite;
    [Min(1)] public int attackRingSpriteCount = 8;
    [Range(0f, 1f)] public float attackRingRadiusFactor = 0.65f;
    public Vector2 attackRingSpriteScale = Vector2.one;
    public float attackRingSpriteAngleOffset = 0f;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    private LineRenderer radiusLine;
    private SpriteRenderer radiusFillRenderer;
    private Coroutine spriteSwapLoop;
    private Transform radiusLineRoot;
    private Transform radiusFillRoot;
    private Transform attackRingRoot;
    private SpriteRenderer[] attackRingRenderers;
    private float shockCooldownTimer;

    protected override void Start()
    {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = chargedColor;

        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<Player>();

        // Ignore physical collision with the player so they don't bounce out of shock radius
        if (playerTarget != null)
        {
            Collider2D myCol = GetComponent<Collider2D>();
            Collider2D playerCol = playerTarget.GetComponent<Collider2D>();
            if (myCol != null && playerCol != null)
                Physics2D.IgnoreCollision(myCol, playerCol, true);
        }

        SetupRadiusLine();
        SetupAttackRing();
        RefreshShockChargeVisuals();

        fsm = new StateMachine();
        fsm.Init(new IdleState(fsm), this);

        StartMovementAnimation();
        StartCoroutine(ShockCheckLoop());
    }

    private IEnumerator SpriteSwapLoop()
    {
        while (true)
        {
            SetSprite(idleSprite);
            yield return new WaitForSeconds(GetPositiveInterval(idleSpriteInterval));

            SetSprite(movementStretchSprite);
            yield return new WaitForSeconds(GetPositiveInterval(movementStretchSpriteInterval));

            SetSprite(movementExtendSprite);
            yield return new WaitForSeconds(GetPositiveInterval(movementExtendSpriteInterval));
        }
    }

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
            UpdateShockVisualAlignment();
        }
    }

    private float GetPositiveInterval(float interval)
    {
        return Mathf.Max(0.01f, interval);
    }

    public void StartMovementAnimation()
    {
        if (spriteSwapLoop != null)
        {
            return;
        }

        spriteSwapLoop = StartCoroutine(SpriteSwapLoop());
    }

    public void StopMovementAnimation()
    {
        if (spriteSwapLoop != null)
        {
            StopCoroutine(spriteSwapLoop);
            spriteSwapLoop = null;
        }

        SetIdleSprite();
    }

    public void SetIdleSprite()
    {
        SetSprite(idleSprite);
    }

    public void SetAttackSprite()
    {
        SetSprite(attackSprite);
    }

    private void SetupAttackRing()
    {
        if (attackRingSprite == null)
        {
            return;
        }

        if (attackRingRoot == null)
        {
            attackRingRoot = new GameObject("AttackRingSprites").transform;
            attackRingRoot.SetParent(transform, false);
        }

        UpdateShockVisualAlignment();

        int count = Mathf.Max(1, attackRingSpriteCount);
        attackRingRenderers = new SpriteRenderer[count];

        int sortingLayerId = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        int sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 0;

        for (int i = 0; i < count; i++)
        {
            var child = new GameObject($"AttackRingSprite_{i}");
            child.transform.SetParent(attackRingRoot, false);
            child.transform.localScale = new Vector3(attackRingSpriteScale.x, attackRingSpriteScale.y, 1f);

            var sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = attackRingSprite;
            sr.sortingLayerID = sortingLayerId;
            sr.sortingOrder = sortingOrder;
            sr.enabled = false;
            attackRingRenderers[i] = sr;
        }

        PositionAttackRingSprites();
    }

    private void PositionAttackRingSprites()
    {
        if (attackRingRenderers == null || attackRingRenderers.Length == 0)
        {
            return;
        }

        float visualRadius = visualShockRadius * attackRingRadiusFactor;
        float angleStep = 360f / attackRingRenderers.Length;

        for (int i = 0; i < attackRingRenderers.Length; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(Mathf.Cos(angle) * visualRadius, Mathf.Sin(angle) * visualRadius, 0f);
            var sr = attackRingRenderers[i];
            sr.transform.localPosition = localPosition;
            sr.transform.localRotation = Quaternion.Euler(0f, 0f, angleStep * i + attackRingSpriteAngleOffset);
            sr.transform.localScale = new Vector3(attackRingSpriteScale.x, attackRingSpriteScale.y, 1f);
        }
    }

    public void SetAttackRingVisible(bool visible, Color color)
    {
        if (attackRingRenderers == null)
        {
            return;
        }

        for (int i = 0; i < attackRingRenderers.Length; i++)
        {
            var sr = attackRingRenderers[i];
            if (sr == null) continue;
            sr.enabled = visible;
            if (visible)
            {
                sr.color = color;
            }
        }
    }

    public void StartShockCooldown()
    {
        shockCooldownTimer = Mathf.Max(0f, shockCooldown);
        isShockCharged = shockCooldownTimer <= 0f;
        RefreshShockChargeVisuals();
    }

    public void RefreshShockChargeVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isShockCharged ? chargedColor : rechargingColor;
        }

        UpdateRadiusColor();
    }

    public Vector3 GetShockCenterWorld()
    {
        return transform.TransformPoint(GetShockCenterLocal());
    }

    private Vector3 GetShockCenterLocal()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            return spriteRenderer.sprite.bounds.center;
        }

        return Vector3.zero;
    }

    private void UpdateShockVisualAlignment()
    {
        Vector3 localCenter = GetShockCenterLocal();

        if (radiusLineRoot != null)
        {
            radiusLineRoot.localPosition = localCenter;
        }

        if (radiusFillRoot != null)
        {
            radiusFillRoot.localPosition = localCenter;
        }

        if (attackRingRoot != null)
        {
            attackRingRoot.localPosition = localCenter;
        }
    }

    private void SetupRadiusLine()
    {
        SetupRadiusFill();

        GameObject lineObj = new GameObject("ShockRadiusVisual");
        radiusLineRoot = lineObj.transform;
        radiusLineRoot.SetParent(transform, false);

        radiusLine = lineObj.AddComponent<LineRenderer>();
        radiusLine.useWorldSpace = false;
        radiusLine.loop = true;
        radiusLine.positionCount = radiusSegments;
        radiusLine.startWidth = radiusLineWidth;
        radiusLine.endWidth = radiusLineWidth;
        radiusLine.sortingLayerName = spriteRenderer != null ? spriteRenderer.sortingLayerName : "Default";
        radiusLine.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder - 1 : 0;

        // Use a simple unlit material so it works without any shader setup
        radiusLine.material = new Material(Shader.Find("Sprites/Default"));

        DrawRadiusCircle();
        UpdateRadiusColor();
        UpdateShockVisualAlignment();
    }

    private void SetupRadiusFill()
    {
        GameObject fillObj = new GameObject("ShockRadiusFill");
        radiusFillRoot = fillObj.transform;
        radiusFillRoot.SetParent(transform, false);

        radiusFillRenderer = fillObj.AddComponent<SpriteRenderer>();
        radiusFillRenderer.sprite = GetRadiusFillSprite();
        radiusFillRenderer.sortingLayerID = spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        radiusFillRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder - 2 : -2;

        UpdateRadiusFillScale();
        UpdateShockVisualAlignment();
    }

    private void DrawRadiusCircle()
    {
        if (radiusLine == null) return;
        float angleStep = 360f / radiusSegments;
        for (int i = 0; i < radiusSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            radiusLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * visualShockRadius, Mathf.Sin(angle) * visualShockRadius, 0f));
        }
    }

    private void UpdateRadiusFillScale()
    {
        if (radiusFillRenderer == null) return;

        float diameter = visualShockRadius * 2f;
        radiusFillRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    public void UpdateRadiusColor()
    {
        if (radiusLine == null) return;
        Color c = isShockCharged ? chargedRadiusColor : rechargingRadiusColor;
        c.a = radiusAlpha;
        radiusLine.startColor = c;
        radiusLine.endColor = c;

        if (radiusFillRenderer != null)
        {
            Color fill = isShockCharged ? chargedRadiusFillColor : rechargingRadiusFillColor;
            fill.a = radiusFillAlpha;
            radiusFillRenderer.color = fill;
        }
    }

    public void SetRadiusColor(Color color, float alpha)
    {
        if (radiusLine == null) return;
        color.a = alpha;
        radiusLine.startColor = color;
        radiusLine.endColor = color;

        if (radiusFillRenderer != null)
        {
            Color fill = color;
            fill.a = Mathf.Clamp01(alpha * 0.25f);
            radiusFillRenderer.color = fill;
        }
    }

    private IEnumerator ShockCheckLoop()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        while (true)
        {
            if (!isShockCharged && shockCooldownTimer > 0f)
            {
                shockCooldownTimer = Mathf.Max(0f, shockCooldownTimer - Time.fixedDeltaTime);
                if (shockCooldownTimer <= 0f)
                {
                    isShockCharged = true;
                    RefreshShockChargeVisuals();
                }
            }

            if (isShockCharged && playerTarget != null)
            {
                float sqrDist = ((Vector2)(playerTarget.transform.position - GetShockCenterWorld())).sqrMagnitude;
                if (sqrDist <= shockRadius * shockRadius)
                {
                    isShockCharged = false;
                    fsm.ChangeState(new ShockedState(fsm));
                }
            }
            yield return wait;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color fillColor = isShockCharged ? chargedRadiusColor : rechargingRadiusColor;
        fillColor.a = isShockCharged ? 0.35f : 0.2f;
        Gizmos.color = fillColor;
        Gizmos.DrawSphere(GetShockCenterWorld(), visualShockRadius);

        Color wireColor = isShockCharged ? chargedRadiusColor : rechargingRadiusColor;
        wireColor.a = isShockCharged ? 0.9f : 0.6f;
        Gizmos.color = wireColor;
        Gizmos.DrawWireSphere(GetShockCenterWorld(), visualShockRadius);
    }

    private void OnDisable()
    {
        StopMovementAnimation();
        SetAttackRingVisible(false, Color.white);
    }

    private static Sprite GetRadiusFillSprite()
    {
        if (radiusFillSprite != null)
        {
            return radiusFillSprite;
        }

        Texture2D texture = new Texture2D(RadiusFillTextureSize, RadiusFillTextureSize, TextureFormat.RGBA32, false);
        texture.name = "JellyfishRadiusFill";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((RadiusFillTextureSize - 1) * 0.5f, (RadiusFillTextureSize - 1) * 0.5f);
        float radius = (RadiusFillTextureSize - 1) * 0.5f;

        for (int y = 0; y < RadiusFillTextureSize; y++)
        {
            for (int x = 0; x < RadiusFillTextureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01((1f - distance) / RadiusFillSoftEdge);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        radiusFillSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, RadiusFillTextureSize, RadiusFillTextureSize),
            new Vector2(0.5f, 0.5f),
            RadiusFillTextureSize);
        return radiusFillSprite;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (attackRingSprite == null)
        {
            attackRingSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Movement/RealAssets/jellyfish/jellyfish attack.png");
        }

        radiusSegments = Mathf.Max(3, radiusSegments);
        shockRadius = Mathf.Max(0f, shockRadius);
        visualShockRadius = Mathf.Max(0f, visualShockRadius);
        shockDuration = Mathf.Max(0f, shockDuration);
        shockCooldown = Mathf.Max(0f, shockCooldown);

        DrawRadiusCircle();
        UpdateRadiusFillScale();
        UpdateRadiusColor();
        UpdateShockVisualAlignment();
    }
#endif
}
