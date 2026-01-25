using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public float maxVelocity = 15f;
    [SerializeField] public float deceleration = 5f;

    [Header("Forward Flight")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float maxRotationAngle = 45f; // Maximum bank angle in degrees
    [SerializeField] private float rotationSmoothness = 5f; // How smoothly the ship rotates

    [Header("Distance-Based Acceleration")]
    [SerializeField] private float maxAccelerationDistance = 10f;
    [SerializeField] private float minAccelerationDistance = 0.5f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [SerializeField] private float accelerationMultiplier = 8f;

    [Header("Player")]
    [SerializeField] private Player model;

    [Header("Constant Upward Speed")]
    [SerializeField] private float constantUpwardSpeed = 5f;

    private bool isTracking = false;
    private Camera mainCamera;
    private Vector3 targetWorldPosition;
    public Vector3 currentVelocity = Vector3.zero;
    private float targetRotationZ = 0f;

    public Vector3 AdditionalVelocity = Vector3.zero;

    /// <summary>
    /// Initialize components and setup.
    /// </summary>
    void Start()
    {
        model.Restore();
        mainCamera = Camera.main;
        targetWorldPosition = transform.position;
    }

    /// <summary>
    /// Handle input and update ship movement.
    /// </summary>
    void Update()
    {
        HandleInput();

        if (isTracking)
        {
            UpdateMousePosition();
        }

        HandleMovement();
        HandleRotation();
    }

    private void FixedUpdate()
    {
        var effect = FindExternalEffectAtScreenPoint(transform.position);
        AdditionalVelocity = effect != null ? effect.GetAddition(this) : Vector3.zero;
    }

    /// <summary>
    /// Handle mouse input for tracking.
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            model.Restore();
        }

        if (Input.GetMouseButtonDown(0))
        {
            isTracking = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isTracking = false;
        }
    }

    /// <summary>
    /// Handle ship movement with distance-based acceleration.
    /// </summary>
    private void HandleMovement()
    {
        if (isTracking)
        {
            ApplyAcceleration();
        }
        else if (currentVelocity.magnitude > 0.01f)
        {
            ApplyDeceleration();
        }

        // Add constant upward speed
        currentVelocity.y = Mathf.Max(currentVelocity.y, constantUpwardSpeed);

        currentVelocity += AdditionalVelocity;

        ApplyMovement();
    }

    /// <summary>
    /// Rotates the ship's upper part (transform.up) towards the mouse cursor, with turn speed based on current velocity.
    /// </summary>
    private void HandleRotation()
    {
        // Calculate lateral speed (how fast we're moving toward cursor)
        float lateralSpeed = currentVelocity.magnitude;

        // Calculate rotation based on ratio of lateral speed to forward speed
        float speedRatio = lateralSpeed / forwardSpeed;

        // Determine rotation direction based on movement direction
        Vector3 movementDirection = currentVelocity.normalized;
        float rotationDirection = Vector3.Cross(Vector3.up, movementDirection).z;

        // Calculate target rotation angle
        targetRotationZ = rotationDirection * speedRatio * maxRotationAngle;

        // If not moving, gradually return to level flight
        if (lateralSpeed < 0.1f)
        {
            targetRotationZ = 0f;
        }

        // Smoothly rotate toward target angle
        float currentRotationZ = transform.eulerAngles.z;
        if (currentRotationZ > 180f) currentRotationZ -= 360f; // Normalize to -180 to 180

        float newRotationZ = Mathf.LerpAngle(currentRotationZ, targetRotationZ, rotationSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newRotationZ);
    }

    /// <summary>
    /// Apply distance-based acceleration towards mouse.
    /// </summary>
    private void ApplyAcceleration()
    {
        Vector3 directionToTarget = (targetWorldPosition - transform.position);
        float distanceToTarget = directionToTarget.magnitude;
        Vector3 normalizedDirection = directionToTarget.normalized;

        // Calculate acceleration strength based on distance
        float normalizedDistance = Mathf.Clamp01((distanceToTarget - minAccelerationDistance) /
                                                (maxAccelerationDistance - minAccelerationDistance));
        float curveValue = accelerationCurve.Evaluate(normalizedDistance);
        float accelerationStrength = curveValue * accelerationMultiplier;

        // Calculate desired velocity
        float desiredSpeed = Mathf.Min(accelerationStrength, maxVelocity);
        Vector3 desiredVelocity = normalizedDirection * desiredSpeed;

        // Apply acceleration
        float lerpRate = accelerationStrength * Time.deltaTime;
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, lerpRate);
    }

    /// <summary>
    /// Apply deceleration when not tracking.
    /// </summary>
    private void ApplyDeceleration()
    {
        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);

        if (currentVelocity.magnitude < 0.01f)
        {
            currentVelocity = Vector3.zero;
        }
    }

    public void Bounce(Vector2 force)
    {
        currentVelocity = force;
    }

    /// <summary>
    /// Apply velocity to ship position.
    /// </summary>
    private void ApplyMovement()
    {
        if (currentVelocity.magnitude > 0.001f)
        {
            transform.position += currentVelocity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Casts a ray from the camera through the given screen point (in pixels) and returns
    /// the first ExternalEffect encountered along the ray. This uses Physics2D.GetRayIntersectionAll
    /// so colliders in front will NOT block discovering ExternalEffect behind them.
    /// </summary>
    public ExternalEffect FindExternalEffectAtScreenPoint(Vector2 worldPoint)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPoint);

        foreach (var col in colliders)
        {
            if (col == null) continue;
            var ext = col.GetComponentInParent<ExternalEffect>();
            if (ext != null)
                return ext; // return the first ExternalEffect found
        }

        return null; // none found
    }

    /// <summary>
    /// Update mouse position target.
    /// </summary>
    private void UpdateMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        targetWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f)) + new Vector3(0, 8.5f, 0);
    }
}
