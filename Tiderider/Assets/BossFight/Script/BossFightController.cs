using UnityEngine;
public class BossFightController : MonoBehaviour
{
    private enum SirenPhase
    {
        Standing,
        Warning,
        Attack,
        AttackRecovery
    }

    [Header("Combat State")]
    [SerializeField] private int currentAldricHp;
    [SerializeField] private int currentSirenHp;

    [Header("Combat Settings")]
    [SerializeField] private int aldricStartingHp = 10;
    [SerializeField] private int sirenStartingHp = 10;
    [SerializeField] private int aldricPunchDamage = 1;
    [SerializeField] private int sirenAttackDamage = 1;

    [Header("Gameplay Timing")]
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float aldricPunchCooldown = 0.4f;
    [SerializeField] private float returnToMiddleDelay = 0.5f;
    [SerializeField] private float standingDuration = 1.1f;
    [SerializeField] private float warningDuration = 0.8f;
    [SerializeField] private float attackDuration = 0.35f;
    [SerializeField] private float attackSpriteHoldDuration = 0.2f;

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
    [SerializeField] private Vector2 aldricPositionOffset;
    [SerializeField] private float aldricPunchForwardDistance;

    [Header("Player Movement")]
    [SerializeField] private int startingLaneIndex = 1;
    [SerializeField] private float laneMoveSpeed = 12f;
    [SerializeField] private float swipeThresholdPixels = 60f;
    [SerializeField] private float aldricWaveDistanceX;
    [SerializeField] private float aldricWaveSpeedX;
    [SerializeField] private float aldricWaveEdgePauseX;
    [SerializeField] private float aldricWaveDistanceY;
    [SerializeField] private float aldricWaveSpeedY;
    [SerializeField] private float sirenWaveDistanceX;
    [SerializeField] private float sirenWaveSpeedX;
    [SerializeField] private float sirenWaveEdgePauseX;
    [SerializeField] private float sirenWaveDistanceY;
    [SerializeField] private float sirenWaveSpeedY;

    [Header("Telegraph")]
    [SerializeField] private float idleLineLengthMultiplier = 0.22f;
    [SerializeField] private float lineSpacing = 0.24f;
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private float lineTargetYOffset = 0.5f;
    [SerializeField] private float attackAreaWidth = 2.2f;
    [SerializeField] private float lineDistanceMultiplier = 1f;
    [SerializeField] private Vector2 leftAttackOriginOffset;
    [SerializeField] private Vector2 rightAttackOriginOffset;
    [SerializeField] private float topLineAngleOffset;
    [SerializeField] private float bottomLineAngleOffset;
    [SerializeField] private Color warningFillStartColor = new Color(1f, 0.3f, 0.3f, 0.08f);
    [SerializeField] private Color warningFillEndColor = new Color(1f, 0.15f, 0.15f, 0.2f);
    [SerializeField] private Color attackFillColor = new Color(1f, 0.05f, 0.05f, 0.45f);
    [SerializeField] private Color warningStartColor = new Color(1f, 0.45f, 0.45f, 0.55f);
    [SerializeField] private Color warningEndColor = new Color(1f, 0.28f, 0.28f, 0.8f);
    [SerializeField] private Color attackColor = new Color(1f, 0f, 0f, 1f);

    [Header("Damage Flash")]
    [SerializeField] private Color aldrichDamageFlashColor = new Color(1f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color sirenDamageFlashColor = new Color(1f, 0.5f, 0.5f, 1f);
    [SerializeField] private float damageFlashDuration = 0.12f;

    private LineRenderer topLineRenderer;
    private LineRenderer bottomLineRenderer;
    private MeshFilter telegraphFillMeshFilter;
    private MeshRenderer telegraphFillMeshRenderer;
    private Mesh telegraphFillMesh;
    private Vector2 swipeStartPosition;
    private bool swipeInProgress;
    private int currentLaneIndex;
    private bool isPunching;
    private bool attackUsesRightSprite;
    private bool sirenAttackHasDamagedAldrich;
    private float punchTimer;
    private float punchCooldownTimer;
    private float phaseTimer;
    private float returnToMiddleTimer;
    private float aldrichDamageFlashTimer;
    private float sirenDamageFlashTimer;
    private Vector3 aldricPunchAnchorPosition;
    private Vector3 sirenBasePosition;
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
        returnToMiddleTimer = -1f;
        currentAldricHp = Mathf.Max(0, aldricStartingHp);
        currentSirenHp = Mathf.Max(0, sirenStartingHp);
        sirenBasePosition = sirenRenderer != null ? sirenRenderer.transform.position : Vector3.zero;

        SetupLineRenderer(ref topLineRenderer, telegraphLineTop, "TelegraphLineTopRenderer");
        SetupLineRenderer(ref bottomLineRenderer, telegraphLineBottom, "TelegraphLineBottomRenderer");
        SetupTelegraphFill();

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
            aldricRenderer.transform.position = ResolveAldricLanePosition(currentLaneIndex);
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
        UpdateSirenMovement();
        UpdateAldricMovement();
        UpdateReturnToMiddle();
        UpdatePunchState();
        UpdatePunchCooldown();
        UpdateSirenState();
        UpdateSirenAttackDamage();
        UpdateTelegraphVisual();
        UpdateDamageFlash();
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

        if (currentLaneIndex == GetMiddleLaneIndex())
        {
            returnToMiddleTimer = -1f;
            return;
        }

        returnToMiddleTimer = Mathf.Max(0f, returnToMiddleDelay);
    }

    private void TriggerPunch()
    {
        if (isPunching || punchCooldownTimer > 0f)
        {
            return;
        }

        isPunching = true;
        punchTimer = punchDuration;
        punchCooldownTimer = aldricPunchCooldown;
        aldricPunchAnchorPosition = aldricRenderer != null ? aldricRenderer.transform.position : Vector3.zero;
        ApplyDamageToSiren(aldricPunchDamage);

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

        Vector3 targetPosition = isPunching
            ? aldricPunchAnchorPosition + (Vector3.up * aldricPunchForwardDistance)
            : ResolveAldricLanePosition(currentLaneIndex);

        aldricRenderer.transform.position = Vector3.MoveTowards(
            aldricRenderer.transform.position,
            targetPosition,
            laneMoveSpeed * Time.deltaTime);
    }

    private void UpdateSirenMovement()
    {
        if (sirenRenderer == null)
        {
            return;
        }

        sirenRenderer.transform.position = sirenBasePosition + ResolveSirenWaveOffset();
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

    private void UpdatePunchCooldown()
    {
        if (punchCooldownTimer <= 0f)
        {
            return;
        }

        punchCooldownTimer = Mathf.Max(0f, punchCooldownTimer - Time.deltaTime);
    }

    private void UpdateReturnToMiddle()
    {
        if (currentLaneIndex == GetMiddleLaneIndex())
        {
            returnToMiddleTimer = -1f;
            return;
        }

        if (returnToMiddleTimer < 0f)
        {
            return;
        }

        returnToMiddleTimer -= Time.deltaTime;
        if (returnToMiddleTimer > 0f)
        {
            return;
        }

        currentLaneIndex = GetMiddleLaneIndex();
        returnToMiddleTimer = -1f;
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
            case SirenPhase.Attack:
                EnterAttackRecoveryPhase();
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
        sirenAttackHasDamagedAldrich = false;

        if (sirenRenderer != null)
        {
            sirenRenderer.sprite = attackUsesRightSprite ? sirenAttackRightSprite : sirenAttackLeftSprite;
        }

        UpdateTelegraphVisual();
    }

    private void EnterAttackRecoveryPhase()
    {
        currentPhase = SirenPhase.AttackRecovery;
        phaseTimer = attackSpriteHoldDuration;
        sirenAttackHasDamagedAldrich = false;
        SetTelegraphVisible(false);
    }

    private void UpdateSirenAttackDamage()
    {
        if (currentPhase != SirenPhase.Attack || sirenAttackHasDamagedAldrich)
        {
            return;
        }

        if (!IsAldrichInCurrentAttackLanes())
        {
            return;
        }

        ApplyDamageToAldrich(sirenAttackDamage);
        sirenAttackHasDamagedAldrich = true;
    }

    private void UpdateTelegraphVisual()
    {
        if (topLineRenderer == null || bottomLineRenderer == null)
        {
            return;
        }

        if (currentPhase == SirenPhase.Standing || currentPhase == SirenPhase.AttackRecovery)
        {
            SetTelegraphVisible(false);
            return;
        }

        SetTelegraphVisible(true);

        Vector3 origin = ResolveAttackOrigin();
        Vector3 attackCenter = ResolveAttackCenterPoint();
        Vector3 targetOffset = Vector3.right * attackAreaWidth;
        Vector3 fullLeftTarget = attackUsesRightSprite ? attackCenter : attackCenter - targetOffset;
        Vector3 fullRightTarget = attackUsesRightSprite ? attackCenter + targetOffset : attackCenter;
        Vector3 fullLeftEnd = ResolveAngledLineEnd(origin, fullLeftTarget, topLineAngleOffset, lineDistanceMultiplier);
        Vector3 fullRightEnd = ResolveAngledLineEnd(origin, fullRightTarget, bottomLineAngleOffset, lineDistanceMultiplier);
        Vector3 topLineStart;
        Vector3 bottomLineStart;
        Vector3 shortenedLeftTarget;
        Vector3 shortenedRightTarget;
        Color lineColor;
        Color fillColor;

        if (currentPhase == SirenPhase.Warning)
        {
            float progress = 1f - Mathf.Clamp01(phaseTimer / Mathf.Max(0.0001f, warningDuration));
            float lengthMultiplier = Mathf.Lerp(idleLineLengthMultiplier, 1f, progress);

            lineColor = Color.Lerp(warningStartColor, warningEndColor, progress);
            fillColor = Color.Lerp(warningFillStartColor, warningFillEndColor, progress);
            topLineStart = origin;
            bottomLineStart = origin;
            shortenedLeftTarget = Vector3.Lerp(origin, fullLeftEnd, lengthMultiplier);
            shortenedRightTarget = Vector3.Lerp(origin, fullRightEnd, lengthMultiplier);
        }
        else
        {
            float collapseProgress = 1f - Mathf.Clamp01(phaseTimer / Mathf.Max(0.0001f, attackDuration));

            lineColor = attackColor;
            fillColor = attackFillColor;
            topLineStart = Vector3.Lerp(origin, fullLeftEnd, collapseProgress);
            bottomLineStart = Vector3.Lerp(origin, fullRightEnd, collapseProgress);
            shortenedLeftTarget = fullLeftEnd;
            shortenedRightTarget = fullRightEnd;
        }

        ApplyLine(topLineRenderer, topLineStart, shortenedLeftTarget, lineColor);
        ApplyLine(bottomLineRenderer, bottomLineStart, shortenedRightTarget, lineColor);
        ApplyTelegraphFill(topLineStart, bottomLineStart, shortenedRightTarget, shortenedLeftTarget, fillColor);
    }

    private Vector3 ResolveAngledLineEnd(Vector3 start, Vector3 target, float angleOffsetDegrees, float lengthMultiplier)
    {
        Vector3 direction = target - start;
        float distance = direction.magnitude * lengthMultiplier;

        if (distance <= 0.0001f)
        {
            return start;
        }

        Vector3 rotatedDirection = Quaternion.Euler(0f, 0f, angleOffsetDegrees) * direction.normalized;
        return start + rotatedDirection * distance;
    }

    private Vector3 ResolveAttackOrigin()
    {
        Transform origin = attackUsesRightSprite ? sirenAttackOriginRight : sirenAttackOriginLeft;
        Vector2 originOffset = attackUsesRightSprite ? rightAttackOriginOffset : leftAttackOriginOffset;
        Vector3 waveOffset = ResolveSirenWaveOffset();

        if (origin == null)
        {
            return waveOffset + new Vector3(originOffset.x, originOffset.y, 0f);
        }

        return origin.position + waveOffset + new Vector3(originOffset.x, originOffset.y, 0f);
    }

    private Vector3 ResolveAttackCenterPoint()
    {
        int middleLaneIndex = GetMiddleLaneIndex();
        Vector3 lanePosition = lanePositions[middleLaneIndex] != null ? lanePositions[middleLaneIndex].position : Vector3.zero;

        if (aldricHitTarget == null)
        {
            lanePosition.y += lineTargetYOffset;
            return lanePosition;
        }

        return new Vector3(
            lanePosition.x,
            aldricHitTarget.position.y,
            aldricHitTarget.position.z);
    }

    private Vector3 ResolveAldricLanePosition(int laneIndex)
    {
        int clampedLaneIndex = Mathf.Clamp(laneIndex, 0, lanePositions.Length - 1);
        Vector3 lanePosition = lanePositions[clampedLaneIndex] != null ? lanePositions[clampedLaneIndex].position : Vector3.zero;
        Vector3 punchOffset = isPunching ? Vector3.up * aldricPunchForwardDistance : Vector3.zero;
        return lanePosition + new Vector3(aldricPositionOffset.x, aldricPositionOffset.y, 0f) + ResolveAldricWaveOffset() + punchOffset;
    }

    private void ApplyDamageToSiren(int damage)
    {
        if (damage <= 0 || currentSirenHp <= 0)
        {
            return;
        }

        currentSirenHp = Mathf.Max(0, currentSirenHp - damage);
        sirenDamageFlashTimer = damageFlashDuration;
    }

    private void ApplyDamageToAldrich(int damage)
    {
        if (damage <= 0 || currentAldricHp <= 0)
        {
            return;
        }

        currentAldricHp = Mathf.Max(0, currentAldricHp - damage);
        aldrichDamageFlashTimer = damageFlashDuration;
    }

    private void UpdateDamageFlash()
    {
        UpdateDamageFlashForRenderer(aldricRenderer, aldrichDamageFlashColor, ref aldrichDamageFlashTimer);
        UpdateDamageFlashForRenderer(sirenRenderer, sirenDamageFlashColor, ref sirenDamageFlashTimer);
    }

    private void UpdateDamageFlashForRenderer(SpriteRenderer renderer, Color flashColor, ref float timer)
    {
        if (renderer == null)
        {
            return;
        }

        if (timer <= 0f || damageFlashDuration <= 0f)
        {
            renderer.color = Color.white;
            return;
        }

        timer = Mathf.Max(0f, timer - Time.deltaTime);
        float normalized = Mathf.Clamp01(timer / damageFlashDuration);
        renderer.color = Color.Lerp(Color.white, flashColor, normalized);
    }

    private bool IsAldrichInCurrentAttackLanes()
    {
        int middleLaneIndex = GetMiddleLaneIndex();

        if (attackUsesRightSprite)
        {
            return currentLaneIndex >= middleLaneIndex;
        }

        return currentLaneIndex <= middleLaneIndex;
    }

    private Vector3 ResolveAldricWaveOffset()
    {
        return ResolveWaveOffset(
            aldricWaveDistanceX,
            aldricWaveSpeedX,
            aldricWaveEdgePauseX,
            aldricWaveDistanceY,
            aldricWaveSpeedY,
            false);
    }

    private Vector3 ResolveSirenWaveOffset()
    {
        return ResolveWaveOffset(
            sirenWaveDistanceX,
            sirenWaveSpeedX,
            sirenWaveEdgePauseX,
            sirenWaveDistanceY,
            sirenWaveSpeedY,
            true);
    }

    private Vector3 ResolveWaveOffset(
        float distanceX,
        float speedX,
        float edgePauseX,
        float distanceY,
        float speedY,
        bool reverseDirection)
    {
        if (Mathf.Abs(distanceX) <= 0f && Mathf.Abs(distanceY) <= 0f)
        {
            return Vector3.zero;
        }

        float directionMultiplier = reverseDirection ? -1f : 1f;
        float xOffset = 0f;
        float yOffset = 0f;

        if (Mathf.Abs(distanceX) > 0f && speedX > 0f)
        {
            float progressX = ResolvePausedPingPongProgress(speedX, edgePauseX);
            if (reverseDirection)
            {
                progressX = 1f - progressX;
            }

            xOffset = Mathf.Lerp(-distanceX * 0.5f, distanceX * 0.5f, progressX);
        }

        if (Mathf.Abs(distanceY) > 0f && speedY > 0f)
        {
            yOffset = Mathf.Sin(Time.time * speedY * directionMultiplier) * distanceY;
        }

        return new Vector3(
            xOffset,
            yOffset,
            0f);
    }

    private float ResolvePausedPingPongProgress(float speed, float edgePause)
    {
        float clampedPause = Mathf.Max(0f, edgePause);
        float travelDuration = 1f / speed;
        float cycleDuration = (travelDuration * 2f) + (clampedPause * 2f);
        float cycleTime = Mathf.Repeat(Time.time, cycleDuration);

        if (cycleTime < clampedPause)
        {
            return 0f;
        }

        cycleTime -= clampedPause;
        if (cycleTime < travelDuration)
        {
            return cycleTime / travelDuration;
        }

        cycleTime -= travelDuration;
        if (cycleTime < clampedPause)
        {
            return 1f;
        }

        cycleTime -= clampedPause;
        return 1f - (cycleTime / travelDuration);
    }

    private Sprite ResolveStandingSprite()
    {
        if (sirenStandingSprite != null)
        {
            return sirenStandingSprite;
        }

        return sirenIdleLeftSprite != null ? sirenIdleLeftSprite : sirenRenderer != null ? sirenRenderer.sprite : null;
    }

    private int GetMiddleLaneIndex()
    {
        return Mathf.Clamp(1, 0, lanePositions.Length - 1);
    }

    private void SetupTelegraphFill()
    {
        Transform host = telegraphLineTop != null ? telegraphLineTop.parent : transform;
        GameObject fillObject = new GameObject("TelegraphFill");
        fillObject.transform.SetParent(host, false);

        telegraphFillMeshFilter = fillObject.AddComponent<MeshFilter>();
        telegraphFillMeshRenderer = fillObject.AddComponent<MeshRenderer>();
        telegraphFillMesh = new Mesh
        {
            name = "TelegraphFillMesh"
        };
        telegraphFillMesh.MarkDynamic();
        telegraphFillMeshFilter.sharedMesh = telegraphFillMesh;

        telegraphFillMeshRenderer.sortingLayerName = sirenRenderer != null ? sirenRenderer.sortingLayerName : "Default";
        telegraphFillMeshRenderer.sortingOrder = sirenRenderer != null ? sirenRenderer.sortingOrder : 5;
        telegraphFillMeshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        telegraphFillMeshRenderer.enabled = false;
    }

    private void ApplyTelegraphFill(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft, Color color)
    {
        if (telegraphFillMesh == null || telegraphFillMeshRenderer == null)
        {
            return;
        }

        Vector3[] vertices =
        {
            topLeft,
            topRight,
            bottomRight,
            bottomLeft
        };

        int[] triangles =
        {
            0, 1, 2,
            0, 2, 3
        };

        Color[] colors =
        {
            color,
            color,
            color,
            color
        };

        telegraphFillMesh.Clear();
        telegraphFillMesh.vertices = vertices;
        telegraphFillMesh.triangles = triangles;
        telegraphFillMesh.colors = colors;
        telegraphFillMesh.RecalculateBounds();
        telegraphFillMeshRenderer.enabled = true;
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

        if (telegraphFillMeshRenderer != null)
        {
            telegraphFillMeshRenderer.enabled = visible;
        }
    }
}
