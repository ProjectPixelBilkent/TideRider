using UnityEngine;

/// <summary>
/// Ship controller that follows mouse position with distance-based acceleration and momentum physics.
/// The ship accelerates stronger when the mouse is farther away, creating realistic pull mechanics.
/// </summary>
/// <remarks>
/// Maintained by: Mouse Tracking System
/// </remarks>
public class ShipController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float maxVelocity = 15f;
    [SerializeField] private float deceleration = 10f;
    
    [Header("Distance-Based Acceleration")]
    [SerializeField] private float maxAccelerationDistance = 10f;
    [SerializeField] private float minAccelerationDistance = 0.5f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [SerializeField] private float accelerationMultiplier = 8f;

    [Header("Rotation Settings")]
    [SerializeField] private float minTurnSpeed = 90f; // degrees per second
    [SerializeField] private float maxTurnSpeed = 360f; // degrees per second

    [Header("Alignment Settings")]
    [Tooltip("Distance to cursor at which the ship snaps to alignment.")]
    [SerializeField] private float snapDistance = 0.3f; // Distance to snap to alignment

    /// <summary>
    /// Alignment mode to use when snapping near the cursor. Set in the Inspector.
    /// </summary>
    [Tooltip("Choose whether the ship snaps horizontally or vertically when close to the cursor.")]
    [SerializeField] private AlignmentMode alignmentMode = AlignmentMode.Horizontal;

    /// <summary>
    /// Alignment modes for snapping the ship's upper part.
    /// </summary>
    private enum AlignmentMode { Horizontal, Vertical }

    private bool isTracking = false;
    private Camera mainCamera;
    private Vector3 targetWorldPosition;
    private Vector3 currentVelocity = Vector3.zero;
    
    /// <summary>
    /// Initialize components and setup.
    /// </summary>
    void Start()
    {
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
        HandleAlignmentToggle();
    }
    
    /// <summary>
    /// Handle mouse input for tracking.
    /// </summary>
    private void HandleInput()
    {
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
        
        ApplyMovement();
    }

    /// <summary>
    /// Rotates the ship's upper part (transform.up) towards the mouse cursor, with turn speed based on current velocity.
    /// </summary>
    private void HandleRotation()
    {
        Vector3 directionToTarget = (targetWorldPosition - transform.position);
        directionToTarget.z = 0f;
        float distance = directionToTarget.magnitude;

        if (directionToTarget.sqrMagnitude < 0.001f)
            return;

        // Hız büyüklüğüne göre dönüş hızı belirle
        float speed = currentVelocity.magnitude;
        float turnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speed / maxVelocity);

        // Hedef açıyı bul
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = transform.eulerAngles.z;

        if (distance < snapDistance)
        {
            if (alignmentMode == AlignmentMode.Horizontal)
                targetAngle = Mathf.Abs(Mathf.DeltaAngle(targetAngle, 0f)) < Mathf.Abs(Mathf.DeltaAngle(targetAngle, 180f)) ? 0f : 180f;
            else // Vertical
                targetAngle = Mathf.Abs(Mathf.DeltaAngle(targetAngle, 90f)) < Mathf.Abs(Mathf.DeltaAngle(targetAngle, 270f)) ? 90f : 270f;
        }

        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    /// <summary>
    /// Toggles the alignment mode between horizontal and vertical when the user presses 'T'.
    /// </summary>
    private void HandleAlignmentToggle()
    {
        if (Input.GetKeyDown(KeyCode.T)) // Press 'T' to toggle
        {
            alignmentMode = alignmentMode == AlignmentMode.Horizontal ? AlignmentMode.Vertical : AlignmentMode.Horizontal;
        }
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
    /// Update mouse position target.
    /// </summary>
    private void UpdateMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        targetWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10f));
    }
} 