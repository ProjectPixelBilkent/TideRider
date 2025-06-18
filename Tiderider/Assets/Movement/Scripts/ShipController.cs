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
    [SerializeField] private float deceleration = 5f;
    
    [Header("Distance-Based Acceleration")]
    [SerializeField] private float maxAccelerationDistance = 10f;
    [SerializeField] private float minAccelerationDistance = 0.5f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [SerializeField] private float accelerationMultiplier = 8f;
    
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