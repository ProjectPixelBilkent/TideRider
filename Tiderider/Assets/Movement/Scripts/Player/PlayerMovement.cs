using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float maxVelocity = 15f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float constantUpwardSpeed = 5f;

    [Header("Steering")]
    [SerializeField] private float maxAccelerationDistance = 10f;
    [SerializeField] private float minAccelerationDistance = 0.5f;
    [SerializeField] private float accelerationMultiplier = 8f;
    [SerializeField] private AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);

    [Header("Rotation")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float maxRotationAngle = 45f;
    [SerializeField] private float rotationSmoothness = 5f;

    [Header("References")]
    [SerializeField] private Player model;

    public Vector3 currentVelocity;
    public Vector3 AdditionalVelocity;

    private Camera mainCamera;
    private Vector3 targetWorldPosition;
    private bool isTracking;

    private void Start()
    {
        model.Restore();
        mainCamera = Camera.main;
        targetWorldPosition = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            model.Restore();

        if (Input.GetMouseButtonDown(0))
            isTracking = true;

        if (Input.GetMouseButtonUp(0))
            isTracking = false;

        if (isTracking)
        {
            Vector3 mouse = Input.mousePosition;
            targetWorldPosition =
                mainCamera.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, 10f)) +
                new Vector3(0f, 8.5f, 0f);

            MoveTowardTarget();
        }
        else
        {
            SlowDown();
        }

        currentVelocity.y = Mathf.Max(currentVelocity.y, constantUpwardSpeed);
        currentVelocity += AdditionalVelocity;

        transform.position += currentVelocity * Time.deltaTime;
        UpdateRotation();
    }

    private void FixedUpdate()
    {
        ExternalEffect effect = FindExternalEffectAtPoint(transform.position);
        AdditionalVelocity = effect != null ? effect.GetAddition(this) : Vector3.zero;
    }

    private void MoveTowardTarget()
    {
        Vector3 toTarget = targetWorldPosition - transform.position;
        float distance = toTarget.magnitude;

        if (distance <= 0.001f)
            return;

        float t = Mathf.Clamp01((distance - minAccelerationDistance) / (maxAccelerationDistance - minAccelerationDistance));
        float acceleration = accelerationCurve.Evaluate(t) * accelerationMultiplier;

        Vector3 desiredVelocity = toTarget.normalized * Mathf.Min(acceleration, maxVelocity);
        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);
    }

    private void SlowDown()
    {
        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);

        if (currentVelocity.magnitude < 0.01f)
            currentVelocity = Vector3.zero;
    }

    private void UpdateRotation()
    {
        float speed = currentVelocity.magnitude;
        float targetZ = 0f;

        if (speed > 0.1f)
        {
            Vector3 direction = currentVelocity.normalized;
            float turnDirection = Vector3.Cross(Vector3.up, direction).z;
            float speedRatio = speed / forwardSpeed;
            targetZ = turnDirection * speedRatio * maxRotationAngle;
        }

        float currentZ = transform.eulerAngles.z;
        if (currentZ > 180f)
            currentZ -= 360f;

        float newZ = Mathf.LerpAngle(currentZ, targetZ, rotationSmoothness * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newZ);
    }

    public void Bounce(Vector2 force)
    {
        currentVelocity = force;
    }

    private ExternalEffect FindExternalEffectAtPoint(Vector2 worldPoint)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPoint);

        foreach (Collider2D col in colliders)
        {
            if (col == null) continue;

            ExternalEffect effect = col.GetComponentInParent<ExternalEffect>();
            if (effect != null)
                return effect;
        }

        return null;
    }
}