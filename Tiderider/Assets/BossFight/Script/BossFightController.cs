using UnityEngine;

public class BossFightController : MonoBehaviour
{
    private enum SirenPhase
    {
        Standing,
        Warning,
        Attack
    }

    [Header("Scene References")]
    [SerializeField] private SpriteRenderer sirenRenderer;
    [SerializeField] private SpriteRenderer aldricRenderer;
    [SerializeField] private Transform[] lanePositions;
    [SerializeField] private Transform sirenAttackOriginLeft;
    [SerializeField] private Transform sirenAttackOriginRight;
    [SerializeField] private Transform aldricHitTarget;
    [SerializeField] private Transform telegraphLineTop;
    [SerializeField] private Transform telegraphLineBottom;

    [Header("Siren Sprites")]
    [SerializeField] private Sprite sirenStandingSprite;
    [SerializeField] private Sprite sirenIdleLeftSprite;
    [SerializeField] private Sprite sirenIdleRightSprite;
    [SerializeField] private Sprite sirenAttackLeftSprite;
    [SerializeField] private Sprite sirenAttackRightSprite;

    [Header("Aldric Sprites")]
    [SerializeField] private Sprite aldricIdleSprite;
    [SerializeField] private Sprite aldricPunchSprite;

    [Header("Player Movement")]
    [SerializeField] private int startingLaneIndex = 1;
    [SerializeField] private float laneMoveSpeed = 12f;
    [SerializeField] private float swipeThresholdPixels = 60f;
    [SerializeField] private float punchDuration = 0.25f;

    [Header("Siren Timing")]
    [SerializeField] private float standingDuration = 1.1f;
    [SerializeField] private float warningDuration = 0.8f;
    [SerializeField] private float attackDuration = 0.35f;

    [Header("Telegraph")]
    [SerializeField] private float idleLineLengthMultiplier = 0.22f;
    [SerializeField] private float lineSpacing = 0.24f;
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private float lineTargetYOffset = 0.5f;
    [SerializeField] private float attackAreaWidth = 2.2f;
    [SerializeField] private Color warningStartColor = new Color(1f, 0.45f, 0.45f, 0.55f);
    [SerializeField] private Color warningEndColor = new Color(1f, 0.28f, 0.28f, 0.8f);
    [SerializeField] private Color attackColor = new Color(1f, 0f, 0f, 1f);

    private LineRenderer topLineRenderer;
    private LineRenderer bottomLineRenderer;
    private Vector2 swipeStartPosition;
    private bool swipeInProgress;
    private int currentLaneIndex;
    private int targetedLaneIndex;
    private bool isPunching;
    private bool attackUsesRightSprite;
    private float punchTimer;
    private float phaseTimer;
    private SirenPhase currentPhase;

    private void Awake()
    {
        if (lanePositions == null || lanePositions.Length < 3)
        {
            Debug.LogWarning("BossFightController requires exactly three lane positions.");
            enabled = false;
            return;
        }

        currentLaneIndex = Mathf.Clamp(startingLaneIndex, 0, lanePositions.Length - 1);
        targetedLaneIndex = currentLaneIndex;

        SetupLineRenderer(ref topLineRenderer, telegraphLineTop, "TelegraphLineTopRenderer");
        SetupLineRenderer(ref bottomLineRenderer, telegraphLineBottom, "TelegraphLineBottomRenderer");

        if (aldricRenderer != null && aldricIdleSprite != null)
        {
            aldricRenderer.sprite = aldricIdleSprite;
        }

        if (sirenRenderer != null)
        {
            sirenRenderer.sprite = ResolveStandingSprite();
        }

        if (lanePositions[currentLaneIndex] != null && aldricRenderer != null)
        {
            aldricRenderer.transform.position = lanePositions[currentLaneIndex].position;
        }

        SetTelegraphVisible(false);
        EnterStandingPhase();
    }

    private void Update()
    {
        if (!enabled)
        {
            return;
        }

        HandleSwipeInput();
        UpdateAldricMovement();
        UpdatePunchState();
        UpdateSirenState();
        UpdateTelegraphVisual();
    }

    private void HandleSwipeInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                swipeStartPosition = touch.position;
                swipeInProgress = true;
            }
            else if (swipeInProgress && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
            {
                EvaluateSwipe(touch.position);
                swipeInProgress = false;
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            swipeStartPosition = Input.mousePosition;
            swipeInProgress = true;
        }
        else if (swipeInProgress && Input.GetMouseButtonUp(0))
        {
            EvaluateSwipe(Input.mousePosition);
            swipeInProgress = false;
        }
    }

    private void EvaluateSwipe(Vector2 endPosition)
    {
        Vector2 swipeDelta = endPosition - swipeStartPosition;

        if (swipeDelta.magnitude < swipeThresholdPixels)
        {
            return;
        }

        if (Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x) && swipeDelta.y > 0f)
        {
            TriggerPunch();
            return;
        }

        if (Mathf.Abs(swipeDelta.x) >= Mathf.Abs(swipeDelta.y))
        {
            MoveLane(swipeDelta.x > 0f ? 1 : -1);
        }
    }

    private void MoveLane(int direction)
    {
        currentLaneIndex = Mathf.Clamp(currentLaneIndex + direction, 0, lanePositions.Length - 1);
    }

    private void TriggerPunch()
    {
        if (isPunching)
        {
            return;
        }

        isPunching = true;
        punchTimer = punchDuration;

        if (aldricRenderer != null && aldricPunchSprite != null)
        {
            aldricRenderer.sprite = aldricPunchSprite;
        }
    }

    private void UpdateAldricMovement()
    {
        if (aldricRenderer == null)
        {
            return;
        }

        Transform laneTarget = lanePositions[currentLaneIndex];
        if (laneTarget == null)
        {
            return;
        }

        aldricRenderer.transform.position = Vector3.MoveTowards(
            aldricRenderer.transform.position,
            laneTarget.position,
            laneMoveSpeed * Time.deltaTime);
    }

    private void UpdatePunchState()
    {
        if (!isPunching)
        {
            return;
        }

        punchTimer -= Time.deltaTime;
        if (punchTimer > 0f)
        {
            return;
        }

        isPunching = false;

        if (aldricRenderer != null && aldricIdleSprite != null)
        {
            aldricRenderer.sprite = aldricIdleSprite;
        }
    }

    private void UpdateSirenState()
    {
        phaseTimer -= Time.deltaTime;

        if (phaseTimer > 0f)
        {
            return;
        }

        switch (currentPhase)
        {
            case SirenPhase.Standing:
                EnterWarningPhase();
                break;
            case SirenPhase.Warning:
                EnterAttackPhase();
                break;
            default:
                EnterStandingPhase();
                break;
        }
    }

    private void EnterStandingPhase()
    {
        currentPhase = SirenPhase.Standing;
        phaseTimer = standingDuration;

        if (sirenRenderer != null)
        {
            sirenRenderer.sprite = ResolveStandingSprite();
        }

        SetTelegraphVisible(false);
    }

    private void EnterWarningPhase()
    {
        currentPhase = SirenPhase.Warning;
        phaseTimer = warningDuration;
        targetedLaneIndex = currentLaneIndex;
        attackUsesRightSprite = Random.value > 0.5f;

        if (sirenRenderer != null)
        {
            sirenRenderer.sprite = attackUsesRightSprite ? sirenIdleRightSprite : sirenIdleLeftSprite;
        }

        SetTelegraphVisible(true);
        UpdateTelegraphVisual();
    }

    private void EnterAttackPhase()
    {
        currentPhase = SirenPhase.Attack;
        phaseTimer = attackDuration;

        if (sirenRenderer != null)
        {
            sirenRenderer.sprite = attackUsesRightSprite ? sirenAttackRightSprite : sirenAttackLeftSprite;
        }

        UpdateTelegraphVisual();
    }

    private void UpdateTelegraphVisual()
    {
        if (topLineRenderer == null || bottomLineRenderer == null)
        {
            return;
        }

        if (currentPhase == SirenPhase.Standing)
        {
            SetTelegraphVisible(false);
            return;
        }

        SetTelegraphVisible(true);

        float progress = currentPhase == SirenPhase.Warning
            ? 1f - Mathf.Clamp01(phaseTimer / Mathf.Max(0.0001f, warningDuration))
            : 1f;

        float lengthMultiplier = currentPhase == SirenPhase.Warning
            ? Mathf.Lerp(idleLineLengthMultiplier, 1f, progress)
            : 1f;

        Color lineColor = currentPhase == SirenPhase.Warning
            ? Color.Lerp(warningStartColor, warningEndColor, progress)
            : attackColor;

        Vector3 origin = ResolveAttackOrigin();
        Vector3 fullTarget = ResolveLaneTargetPoint(targetedLaneIndex);
        Vector3 targetOffset = Vector3.right * (attackAreaWidth * 0.5f);
        Vector3 fullLeftTarget = fullTarget - targetOffset;
        Vector3 fullRightTarget = fullTarget + targetOffset;
        Vector3 startOffset = Vector3.right * (lineSpacing * 0.5f);

        Vector3 shortenedLeftTarget = Vector3.Lerp(origin, fullLeftTarget, lengthMultiplier);
        Vector3 shortenedRightTarget = Vector3.Lerp(origin, fullRightTarget, lengthMultiplier);

        ApplyLine(topLineRenderer, origin - startOffset, shortenedLeftTarget, lineColor);
        ApplyLine(bottomLineRenderer, origin + startOffset, shortenedRightTarget, lineColor);
    }

    private Vector3 ResolveAttackOrigin()
    {
        Transform origin = attackUsesRightSprite ? sirenAttackOriginRight : sirenAttackOriginLeft;
        return origin != null ? origin.position : Vector3.zero;
    }

    private Vector3 ResolveLaneTargetPoint(int laneIndex)
    {
        int clampedLane = Mathf.Clamp(laneIndex, 0, lanePositions.Length - 1);
        Vector3 lanePosition = lanePositions[clampedLane] != null ? lanePositions[clampedLane].position : Vector3.zero;

        if (aldricHitTarget == null)
        {
            lanePosition.y += lineTargetYOffset;
            return lanePosition;
        }

        return new Vector3(lanePosition.x, aldricHitTarget.position.y, aldricHitTarget.position.z);
    }

    private Sprite ResolveStandingSprite()
    {
        if (sirenStandingSprite != null)
        {
            return sirenStandingSprite;
        }

        return sirenIdleLeftSprite != null ? sirenIdleLeftSprite : sirenRenderer != null ? sirenRenderer.sprite : null;
    }

    private void SetupLineRenderer(ref LineRenderer lineRenderer, Transform host, string childName)
    {
        if (host == null)
        {
            return;
        }

        lineRenderer = host.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = host.gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCapVertices = 4;
        lineRenderer.sortingLayerName = sirenRenderer != null ? sirenRenderer.sortingLayerName : "Default";
        lineRenderer.sortingOrder = sirenRenderer != null ? sirenRenderer.sortingOrder + 1 : 6;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.enabled = false;

        if (host.name != childName)
        {
            host.name = childName;
        }
    }

    private void ApplyLine(LineRenderer lineRenderer, Vector3 start, Vector3 end, Color color)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    private void SetTelegraphVisible(bool visible)
    {
        if (topLineRenderer != null)
        {
            topLineRenderer.enabled = visible;
        }

        if (bottomLineRenderer != null)
        {
            bottomLineRenderer.enabled = visible;
        }
    }
}
