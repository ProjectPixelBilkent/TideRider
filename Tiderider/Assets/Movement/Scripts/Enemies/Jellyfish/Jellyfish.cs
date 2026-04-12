using System.Collections;
using UnityEngine;

public class Jellyfish : Enemy
{
    [Header("Behavior")]
    public float topOffsetFromCamera = 3.5f;
    public float lingerTime = 20f;
    public float downSpeed = 1.0f;
    public float bottomOffsetFromCamera = -6f;

    [Header("Shock")]
    public float shockRadius = 1.7f;
    public int shockDamage = 10;
    public float shockCooldown = 3f;
    [HideInInspector] public bool isShockCharged = true;
    public Player playerTarget;

    [Header("Shock Visuals")]
    public Color chargedColor = new Color(0.5f, 0.9f, 1f);
    public Color rechargingColor = new Color(0.4f, 0.4f, 0.4f);
    [Range(0f, 1f)] public float radiusAlpha = 0.4f;
    public float radiusLineWidth = 0.05f;
    [Tooltip("Number of segments used to draw the radius circle.")]
    public int radiusSegments = 48;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    private LineRenderer radiusLine;

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

        fsm = new StateMachine();
        fsm.Init(new IdleState(fsm), this);

        StartCoroutine(ShockCheckLoop());
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
}
