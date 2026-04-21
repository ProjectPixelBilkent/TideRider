using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GhostShip2 : MonoBehaviour
{
    public enum ShipState
    {
        Chase,
        Retreat,
        Dive
    }

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer shipSprite;
    [SerializeField] private Collider2D shipCollider;
    [SerializeField] private Transform shipVisual;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Movement")]
    public float sailSpeed = 2.2f;
    [SerializeField] private float maxSpeed = 5.2f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float turnSpeed = 95f;
    [SerializeField] private float retreatSpeed = 4.8f;
    [SerializeField] private float desiredAheadDistance = 6f;
    [SerializeField] private float sideOffset = 2.5f;
    [SerializeField] private float playerPredictionTime = 0.35f;
    [SerializeField] private float retreatDistance = 3.5f;
    [SerializeField] private float farDistance = 14f;
    [SerializeField] private float behindCatchupMultiplier = 1.35f;

    [Header("Wide-Turn Steering")]
    [SerializeField] private float steeringResponsiveness = 2.5f;
    [SerializeField] private float minimumForwardFactorWhileTurning = 0.12f;
    [SerializeField] private float alignmentPower = 2.4f;
    [SerializeField] private float maxDesiredHeadingChange = 55f;
    [SerializeField] private float targetLookAheadDistance = 2.2f;

    [Header("Rotation / Nose Direction")]
    [Tooltip("If the sprite nose points down at root rotation 0, use 90.")]
    public float movementNoseAngleOffset = 90f;

    [Tooltip("Extra visual correction for the visible ship graphics only.")]
    public float visualNoseAngleOffset = 0f;

    [Header("Attack")]
    [SerializeField] private Weapon weaponData;
    [SerializeField] private int weaponLevel = 0;
    [SerializeField] private float fireCooldown = 1.0f;
    [SerializeField] private float broadsideTolerance = 55f;
    [SerializeField] private float shootingMinFacingDot = 0.45f;
    [SerializeField] private int aimIterations = 6;
    [SerializeField] private float fallbackBulletSpeed = 8f;
    [SerializeField] private float aimRandomness = 0.015f;
    [SerializeField] private float minimumAimDistance = 0.1f;
    [SerializeField] private float bulletIgnoreDuration = 0.3f;
    [SerializeField] private Vector2 portCannonOffset = new Vector2(0f, 0.65f);
    [SerializeField] private Vector2 starboardCannonOffset = new Vector2(0f, -0.65f);

    [Header("Ghost Dive")]
    [SerializeField] private float diveDelay = 0.75f;
    [SerializeField] private float respawnRandomAngle = 30f;
    [SerializeField] private float respawnDistanceMin = 0.9f;
    [SerializeField] private float respawnDistanceMax = 1.25f;
    [SerializeField] private float behindDotThreshold = -0.1f;
    [SerializeField] private float respawnSpeedMultiplier = 0.75f;

    [Header("Camera")]
    [SerializeField] private float cameraPadding = 0.5f;
    [SerializeField] private float offscreenMargin = 0.2f;

    [Header("Debug")]
    [SerializeField] private ShipState currentState = ShipState.Chase;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private bool isDiving = false;

    private Camera mainCamera;
    private Vector2 lastPlayerPosition;
    private Vector2 smoothedPlayerVelocity = Vector2.zero;
    private Vector2 playerMoveDirection = Vector2.right;
    private float fireTimer = 0f;
    private int currentSideSign = 1;
    private Coroutine diveCoroutine;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        shipCollider = GetComponent<Collider2D>();
        shipSprite = GetComponentInChildren<SpriteRenderer>();

        if (shipSprite != null)
            shipVisual = shipSprite.transform;
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (shipCollider == null) shipCollider = GetComponent<Collider2D>();
        if (shipSprite == null) shipSprite = GetComponentInChildren<SpriteRenderer>();
        if (shipVisual == null && shipSprite != null) shipVisual = shipSprite.transform;

        mainCamera = Camera.main;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        if (playerTransform != null)
            lastPlayerPosition = playerTransform.position;

        ApplyVisualRotationInstant();
    }

    private void LateUpdate()
    {
        ApplyVisualRotationInstant();
    }

    private void Update()
    {
        if (playerTransform == null || rb == null)
            return;

        UpdatePlayerTracking();

        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;

        if (currentState == ShipState.Dive)
        {
            if (!isDiving)
                diveCoroutine = StartCoroutine(DiveRoutine());

            return;
        }

        if (ShouldDive())
        {
            EnterDive();
            return;
        }

        if (currentState == ShipState.Chase && CanFireBroadside() && fireTimer <= 0f)
        {
            bool fired = FireCannon();
            if (fired)
                fireTimer = fireCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || rb == null)
            return;

        if (currentState == ShipState.Dive)
            return;

        float distToPlayer = Vector2.Distance(rb.position, playerTransform.position);

        if (distToPlayer < retreatDistance)
            currentState = ShipState.Retreat;
        else
            currentState = ShipState.Chase;

        if (currentState == ShipState.Chase)
        {
            Vector2 target = GetAheadPosition();
            MoveLikeShip(target, sailSpeed, true);
        }
        else if (currentState == ShipState.Retreat)
        {
            Vector2 away = (rb.position - (Vector2)playerTransform.position).normalized;
            Vector2 retreatTarget = rb.position + away * 5f;
            MoveLikeShip(retreatTarget, retreatSpeed, false);
        }

        ClampToCamera();
    }

    private void UpdatePlayerTracking()
    {
        Vector2 currentPlayerPos = playerTransform.position;
        Vector2 rawVelocity = (currentPlayerPos - lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);

        smoothedPlayerVelocity = Vector2.Lerp(smoothedPlayerVelocity, rawVelocity, Time.deltaTime * 8f);

        if (smoothedPlayerVelocity.magnitude > 0.05f)
            playerMoveDirection = smoothedPlayerVelocity.normalized;

        lastPlayerPosition = currentPlayerPos;
    }

    private Vector2 GetAheadPosition()
    {
        Vector2 forward = playerMoveDirection.sqrMagnitude > 0.001f ? playerMoveDirection.normalized : Vector2.right;
        Vector2 side = new Vector2(-forward.y, forward.x) * currentSideSign;

        Vector2 predictedPlayer = (Vector2)playerTransform.position + smoothedPlayerVelocity * playerPredictionTime;
        Vector2 target = predictedPlayer + forward * desiredAheadDistance + side * sideOffset;

        return ClampPositionToCamera(target);
    }

    private void MoveLikeShip(Vector2 targetPosition, float preferredSpeed, bool boostIfBehind)
    {
        Vector2 shipForward = GetShipForward();

        Vector2 lookAheadPosition = rb.position + shipForward * targetLookAheadDistance;
        Vector2 toTarget = targetPosition - lookAheadPosition;

        Vector2 rawDesiredHeading = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : shipForward;

        float headingAngle = Vector2.SignedAngle(shipForward, rawDesiredHeading);
        headingAngle = Mathf.Clamp(headingAngle, -maxDesiredHeadingChange, maxDesiredHeadingChange);

        Vector2 limitedDesiredHeading =
            ((Vector2)(Quaternion.Euler(0f, 0f, headingAngle) * new Vector3(shipForward.x, shipForward.y, 0f))).normalized;

        Vector2 desiredHeading = Vector2.Lerp(
            shipForward,
            limitedDesiredHeading,
            steeringResponsiveness * Time.fixedDeltaTime
        ).normalized;

        float desiredAngle = DirectionToRootRotation(desiredHeading);
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, desiredAngle, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);

        shipForward = RotationToShipForward(newAngle);

        float speedTarget = preferredSpeed;

        if (boostIfBehind)
        {
            Vector2 playerForward = playerMoveDirection.sqrMagnitude > 0.001f ? playerMoveDirection.normalized : Vector2.right;
            Vector2 fromPlayerToShip = rb.position - (Vector2)playerTransform.position;

            if (fromPlayerToShip.sqrMagnitude > 0.001f)
            {
                float aheadDot = Vector2.Dot(fromPlayerToShip.normalized, playerForward);
                if (aheadDot < 0f)
                    speedTarget *= behindCatchupMultiplier;
            }
        }

        float facingDot = Vector2.Dot(shipForward, desiredHeading);
        float alignmentFactor = Mathf.Pow(Mathf.Clamp01((facingDot + 1f) * 0.5f), alignmentPower);
        speedTarget *= Mathf.Lerp(minimumForwardFactorWhileTurning, 1f, alignmentFactor);

        currentSpeed = Mathf.MoveTowards(currentSpeed, Mathf.Min(speedTarget, maxSpeed), acceleration * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + shipForward * currentSpeed * Time.fixedDeltaTime);
    }

    private bool ShouldDive()
    {
        Vector2 shipPos = rb.position;
        Vector2 playerPos = playerTransform.position;

        if (Vector2.Distance(shipPos, playerPos) > farDistance)
            return true;

        Vector2 playerForward = playerMoveDirection.sqrMagnitude > 0.001f
            ? playerMoveDirection.normalized
            : Vector2.right;

        Vector2 fromPlayerToShip = shipPos - playerPos;

        if (fromPlayerToShip.sqrMagnitude > 0.001f)
        {
            float aheadDot = Vector2.Dot(fromPlayerToShip.normalized, playerForward);

            if (aheadDot < behindDotThreshold)
                return true;
        }

        if (IsOutsideCamera(shipPos))
            return true;

        return false;
    }

    private void EnterDive()
    {
        if (isDiving) return;
        currentState = ShipState.Dive;
    }

    private IEnumerator DiveRoutine()
    {
        isDiving = true;

        if (shipSprite != null) shipSprite.enabled = false;
        if (shipCollider != null) shipCollider.enabled = false;

        currentSpeed = 0f;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(diveDelay);

        ReappearAheadOfPlayer();

        if (shipSprite != null) shipSprite.enabled = true;
        if (shipCollider != null) shipCollider.enabled = true;

        currentSideSign *= -1;
        currentState = ShipState.Chase;
        isDiving = false;
        diveCoroutine = null;
    }

    private void ReappearAheadOfPlayer()
    {
        Vector2 forward = playerMoveDirection.sqrMagnitude > 0.001f ? playerMoveDirection.normalized : Vector2.right;

        float angleOffset = Random.Range(-respawnRandomAngle, respawnRandomAngle);
        Vector2 rotatedForward =
            ((Vector2)(Quaternion.Euler(0f, 0f, angleOffset) * new Vector3(forward.x, forward.y, 0f))).normalized;

        float distMul = Random.Range(respawnDistanceMin, respawnDistanceMax);
        Vector2 side = new Vector2(-rotatedForward.y, rotatedForward.x) * currentSideSign;

        Vector2 spawnPos =
            (Vector2)playerTransform.position +
            rotatedForward * desiredAheadDistance * distMul +
            side * sideOffset;

        spawnPos = ClampPositionToCamera(spawnPos);
        rb.position = spawnPos;

        Vector2 toPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        float targetRotation = currentSideSign > 0
            ? DirectionToRootRotation(new Vector2(-toPlayer.y, toPlayer.x))
            : DirectionToRootRotation(new Vector2(toPlayer.y, -toPlayer.x));

        rb.rotation = targetRotation;
        currentSpeed = sailSpeed * respawnSpeedMultiplier;
    }

    private bool CanFireBroadside()
    {
        if (playerTransform == null)
            return false;

        Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        Vector2 shipForward = GetShipForward();

        float angle = Vector2.Angle(shipForward, toPlayer);
        float error = Mathf.Abs(angle - 90f);
        float facingDot = Vector2.Dot(shipForward, toPlayer);

        return error <= broadsideTolerance && Mathf.Abs(facingDot) >= shootingMinFacingDot;
    }

    private Vector2 GetBestCannonOffset()
    {
        if (playerTransform == null)
            return portCannonOffset;

        Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

        Vector2 shipLeftSide = GetShipLeftSide();
        Vector2 shipRightSide = -shipLeftSide;

        float portDot = Vector2.Dot(shipLeftSide, toPlayer);
        float starboardDot = Vector2.Dot(shipRightSide, toPlayer);

        return portDot >= starboardDot ? portCannonOffset : starboardCannonOffset;
    }

    private WeaponLevel GetCurrentWeaponLevel()
    {
        if (weaponData == null || weaponData.weaponLevels == null || weaponData.weaponLevels.Length == 0)
            return null;

        int index = Mathf.Clamp(weaponLevel, 0, weaponData.weaponLevels.Length - 1);
        return weaponData.weaponLevels[index];
    }

    private float GetBulletSpeed()
    {
        WeaponLevel level = GetCurrentWeaponLevel();

        if (level == null || level.speedOfBullet <= 0f)
            return fallbackBulletSpeed;

        return level.speedOfBullet;
    }

    private Vector2 GetPredictedAimTarget(Vector2 spawnPos)
    {
        if (playerTransform == null)
            return spawnPos;

        Vector2 playerPos = playerTransform.position;
        Vector2 playerVel = smoothedPlayerVelocity;
        float bulletSpeed = GetBulletSpeed();

        if (bulletSpeed <= 0.001f)
            return playerPos;

        Vector2 predicted = playerPos;

        for (int i = 0; i < aimIterations; i++)
        {
            float dist = Vector2.Distance(spawnPos, predicted);
            float timeToReach = dist / bulletSpeed;
            Vector2 next = playerPos + playerVel * timeToReach;

            if ((next - predicted).sqrMagnitude < 0.0001f)
                break;

            predicted = next;
        }

        predicted += Random.insideUnitCircle * aimRandomness;
        return predicted;
    }

    private bool FireCannon()
    {
        if (weaponData == null || weaponData.projectilePrefab == null)
        {
            Debug.LogWarning("GhostShip2: weaponData or projectilePrefab missing.");
            return false;
        }

        WeaponLevel level = GetCurrentWeaponLevel();
        if (level == null)
        {
            Debug.LogWarning("GhostShip2: weapon level missing.");
            return false;
        }

        Vector2 cannonOffset = GetBestCannonOffset();
        Vector2 spawnPos = transform.TransformPoint(cannonOffset);
        Vector2 aimTarget = GetPredictedAimTarget(spawnPos);

        Vector2 dir = aimTarget - spawnPos;

        if (dir.magnitude < minimumAimDistance)
            dir = GetShipForward();
        else
            dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject bulletObj = Instantiate(
            weaponData.projectilePrefab,
            spawnPos,
            Quaternion.Euler(0f, 0f, angle)
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Weapon = weaponData;
            bullet.WeaponLevel = level;
            bullet.PlayerBullet = false;
            bullet.Activate(dir, rb.linearVelocity);
        }

        Collider2D bulletCollider = bulletObj.GetComponent<Collider2D>();
        if (bulletCollider != null && shipCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, shipCollider, true);
            StartCoroutine(RestoreCollisionAfterDelay(bulletCollider, shipCollider, bulletIgnoreDuration));
        }

        return true;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Destroy(gameObject);
    }

    private void ClampToCamera()
    {
        if (mainCamera == null) return;

        Vector2 clamped = ClampPositionToCamera(rb.position);
        if ((clamped - rb.position).sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(clamped);
            currentSpeed *= 0.75f;
        }
    }

    private Vector2 ClampPositionToCamera(Vector2 position)
    {
        if (mainCamera == null) return position;

        float camH = mainCamera.orthographicSize;
        float camW = camH * mainCamera.aspect;
        Vector2 camPos = mainCamera.transform.position;

        float minX = camPos.x - camW + cameraPadding;
        float maxX = camPos.x + camW - cameraPadding;
        float minY = camPos.y - camH + cameraPadding;
        float maxY = camPos.y + camH - cameraPadding;

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        return position;
    }

    private bool IsOutsideCamera(Vector2 worldPos)
    {
        if (mainCamera == null) return false;

        Vector3 vp = mainCamera.WorldToViewportPoint(worldPos);

        return vp.x < -offscreenMargin ||
               vp.x > 1f + offscreenMargin ||
               vp.y < -offscreenMargin ||
               vp.y > 1f + offscreenMargin;
    }

    private IEnumerator RestoreCollisionAfterDelay(Collider2D bulletCollider, Collider2D ownerCollider, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bulletCollider != null && ownerCollider != null)
            Physics2D.IgnoreCollision(bulletCollider, ownerCollider, false);
    }

    private float DirectionToRootRotation(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return rb != null ? rb.rotation : 0f;

        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + movementNoseAngleOffset;
    }

    private Vector2 RotationToShipForward(float rootRotationDegrees)
    {
        float baseAngle = (rootRotationDegrees - movementNoseAngleOffset) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(baseAngle), Mathf.Sin(baseAngle)).normalized;
    }

    private Vector2 GetShipForward()
    {
        float rotation = rb != null ? rb.rotation : transform.eulerAngles.z;
        return RotationToShipForward(rotation);
    }

    private Vector2 GetShipLeftSide()
    {
        Vector2 forward = GetShipForward();
        return new Vector2(-forward.y, forward.x).normalized;
    }

    private void ApplyVisualRotationInstant()
    {
        if (shipVisual == null || rb == null)
            return;

        shipVisual.rotation = Quaternion.Euler(0f, 0f, rb.rotation + visualNoseAngleOffset);
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, GetDebugAheadPosition());
        Gizmos.DrawSphere(GetDebugAheadPosition(), 0.2f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + GetShipForward() * 1.5f);
    }

    private Vector2 GetDebugAheadPosition()
    {
        Vector2 forward = playerMoveDirection.sqrMagnitude > 0.001f ? playerMoveDirection.normalized : Vector2.right;
        Vector2 side = new Vector2(-forward.y, forward.x) * currentSideSign;
        return (Vector2)playerTransform.position + forward * desiredAheadDistance + side * sideOffset;
    }
}