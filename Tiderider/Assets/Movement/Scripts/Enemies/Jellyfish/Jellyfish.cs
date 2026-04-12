using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Jellyfish : Enemy
{
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
    public int shockDamage = 10;
    public float shockCooldown = 3f;
    [Min(0f)] public float shockDamageInterval = 0.25f;
    [HideInInspector] public bool isShockCharged = true;
    public Player playerTarget;

    [Header("Lightning")]
    public GameObject lightningPrefab;
    public float lightningSpeed = 25f;

    [Header("Shock Visuals")]
    public Color chargedColor = new Color(0.5f, 0.9f, 1f);
    public Color rechargingColor = new Color(0.4f, 0.4f, 0.4f);
    public Color attackFlashColorA = Color.white;
    public Color attackFlashColorB = new Color(0.5f, 0.9f, 1f);
    [Min(0f)] public float attackFlashInterval = 0.08f;
    [Range(0f, 1f)] public float radiusAlpha = 0.4f;
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
    private Coroutine spriteSwapLoop;
    private Transform attackRingRoot;
    private SpriteRenderer[] attackRingRenderers;

    protected override void Start()
    {
        base.Start();
        Debug.Log("JELLYFISH START: " + gameObject.name);

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = chargedColor;

        if (playerTarget == null)
            playerTarget = FindFirstObjectByType<Player>();

        SetupRadiusLine();
        SetupAttackRing();

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
            attackRingRoot.localPosition = Vector3.zero;
        }

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

        float radius = shockRadius * attackRingRadiusFactor;
        float angleStep = 360f / attackRingRenderers.Length;

        for (int i = 0; i < attackRingRenderers.Length; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
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

    private void SetupRadiusLine()
    {
        GameObject lineObj = new GameObject("ShockRadiusVisual");
        lineObj.transform.SetParent(transform, false);

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
    }

    private void DrawRadiusCircle()
    {
        if (radiusLine == null) return;
        float angleStep = 360f / radiusSegments;
        for (int i = 0; i < radiusSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            radiusLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * shockRadius, Mathf.Sin(angle) * shockRadius, 0f));
        }
    }

    public void UpdateRadiusColor()
    {
        if (radiusLine == null) return;
        Color c = isShockCharged ? chargedColor : rechargingColor;
        c.a = radiusAlpha;
        radiusLine.startColor = c;
        radiusLine.endColor = c;
    }

    public void SetRadiusColor(Color color, float alpha)
    {
        if (radiusLine == null) return;
        color.a = alpha;
        radiusLine.startColor = color;
        radiusLine.endColor = color;
    }

    private IEnumerator ShockCheckLoop()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        while (true)
        {
            if (isShockCharged && playerTarget != null)
            {
                float sqrDist = ((Vector2)(playerTarget.transform.position - transform.position)).sqrMagnitude;
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
        Gizmos.color = isShockCharged ? new Color(0.2f, 0.8f, 1f, 0.35f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
        Gizmos.DrawSphere(transform.position, shockRadius);
        Gizmos.color = isShockCharged ? new Color(0.2f, 0.8f, 1f, 0.9f) : new Color(0.5f, 0.5f, 0.5f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, shockRadius);
    }

    private void OnDisable()
    {
        StopMovementAnimation();
        SetAttackRingVisible(false, Color.white);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (attackRingSprite == null)
        {
            attackRingSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Movement/RealAssets/jellyfish/jellyfish attack.png");
        }
    }
#endif
}
