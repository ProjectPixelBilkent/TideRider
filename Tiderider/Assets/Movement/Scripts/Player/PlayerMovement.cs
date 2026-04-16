using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float maxTiltAngle = 45f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite turnRightSprite;
    [SerializeField] private Sprite turnLeftSprite;
    [SerializeField] private float turnSpriteAngleThreshold = 20f;
    [SerializeField] private float turnSpriteChangeCooldown = 1f;

    [Header("Movement")]
    [SerializeField] private float minForwardSpeed = 2f;
    [SerializeField] public float maxVelocity = 8f;

    [Tooltip("How far ahead the click must be to reach max speed.")]
    [SerializeField] private float maxAheadDistance = 10f;

    [Header("Input")]
    [SerializeField] private int mouseButton = 0; // 0 = left mouse button

    [Header("Bounce")]
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private float bounceDuration = 0.15f;

    [Header("Post-Bounce Slow")]
    [SerializeField] private float postBounceSpeedMultiplier = 0.5f;
    [SerializeField] private float postBounceSlowDuration = 1.25f;

    private Rigidbody2D rb;
    private Camera mainCam;

    private float targetZRotation = 0f;
    private float targetSpeed = 0f;
    private float targetMouseAngle = 0f;
    private bool hasInput = false;

    private bool isBouncing = false;
    private float bounceTimer = 0f;
    private Vector2 bounceVelocity;

    private float postBounceSlowTimer = 0f;

    public Vector3 currentVelocity;
    private Sprite currentDisplayedSprite;
    private float lastTurnSpriteChangeTime = float.NegativeInfinity;

    private readonly HashSet<ExternalEffect> activeExternalEffects = new HashSet<ExternalEffect>();

    private BulletSpawner bulletSpawner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        mainCam = Camera.main;
        bulletSpawner = GetComponent<BulletSpawner>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && defaultSprite == null)
        {
            defaultSprite = spriteRenderer.sprite;
        }

        currentDisplayedSprite = spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    private void Update()
    {
        if (bulletSpawner.objectSpawner.isPausedForDialogue)
        {
            return;
        }

        if (isBouncing)
        {
            bounceTimer -= Time.deltaTime;
            if (bounceTimer <= 0f)
            {
                isBouncing = false;
            }
        }

        if (postBounceSlowTimer > 0f)
        {
            postBounceSlowTimer -= Time.deltaTime;
        }

        bool isTouching = Input.touchCount > 0;
        hasInput = Input.GetMouseButton(mouseButton) || isTouching;

        if (hasInput)
        {
            Vector3 mouseScreenPos = isTouching ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos) + new Vector3(0, 1, 0);
            mouseWorldPos.z = 0f;

            Vector2 shipPos = rb.position;
            Vector2 toClick = (Vector2)mouseWorldPos - shipPos;

            Vector2 localClick = transform.InverseTransformPoint(mouseWorldPos);

            float horizontalFactor = Mathf.Clamp(localClick.x / 2.5f, -1f, 1f);
            targetZRotation = -horizontalFactor * maxTiltAngle;
            targetMouseAngle = Vector2.SignedAngle(transform.up, toClick);

            float aheadDistance = Mathf.Max(0f, Vector2.Dot(toClick, transform.up));
            float speedT = Mathf.Clamp01(aheadDistance / maxAheadDistance);
            targetSpeed = Mathf.Lerp(minForwardSpeed, maxVelocity, speedT);
        }
        else
        {
            targetZRotation = 0f;
            targetSpeed = minForwardSpeed;
            targetMouseAngle = 0f;
        }

        UpdateTurnSprite();
    }

    private void FixedUpdate()
    {
        if (bulletSpawner.objectSpawner.isPausedForDialogue)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 externalBonus = GetExternalEffectBonus();

        if (isBouncing)
        {
            rb.linearVelocity = bounceVelocity + externalBonus;
            currentVelocity = rb.linearVelocity;
            return;
        }

        float currentZ = rb.rotation;
        float newZ = Mathf.MoveTowardsAngle(currentZ, targetZRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newZ);

        bool inEndingSequence = bulletSpawner != null && bulletSpawner.objectSpawner != null && bulletSpawner.objectSpawner.isInEndingSequence;
        if (inEndingSequence && transform.position.y >= Camera.main.transform.position.y)
        {
            rb.linearVelocity = externalBonus;
            currentVelocity = rb.linearVelocity;
            return;
        }

        float currentSpeed = targetSpeed;

        if (postBounceSlowTimer > 0f)
        {
            currentSpeed *= postBounceSpeedMultiplier;
        }

        Vector2 forward = transform.up;
        Vector2 baseVelocity = forward * currentSpeed;

        rb.linearVelocity = baseVelocity + externalBonus;
        currentVelocity = rb.linearVelocity;
    }

    private void UpdateTurnSprite()
    {
        if (spriteRenderer == null || defaultSprite == null)
        {
            return;
        }

        Sprite targetSprite = defaultSprite;

        if (targetMouseAngle <= -turnSpriteAngleThreshold && turnRightSprite != null)
        {
            targetSprite = turnRightSprite;
        }
        else if (targetMouseAngle >= turnSpriteAngleThreshold && turnLeftSprite != null)
        {
            targetSprite = turnLeftSprite;
        }

        if (currentDisplayedSprite == targetSprite)
        {
            return;
        }

        if (Time.time - lastTurnSpriteChangeTime < turnSpriteChangeCooldown)
        {
            return;
        }

        spriteRenderer.sprite = targetSprite;
        currentDisplayedSprite = targetSprite;
        lastTurnSpriteChangeTime = Time.time;
    }

    private Vector2 GetExternalEffectBonus()
    {
        Vector2 totalBonus = Vector2.zero;

        activeExternalEffects.RemoveWhere(effect => effect == null);

        foreach (ExternalEffect effect in activeExternalEffects)
        {
            totalBonus += (Vector2)effect.GetAddition(this);
        }

        return totalBonus;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ExternalEffect effect = other.GetComponent<ExternalEffect>();
        if (effect != null)
        {
            activeExternalEffects.Add(effect);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ExternalEffect effect = other.GetComponent<ExternalEffect>();
        if (effect != null)
        {
            activeExternalEffects.Remove(effect);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contactCount == 0)
            return;

        if(collision.gameObject.GetComponent<ExternalEffect>()!=null)
        {
            return;
        }


        Vector2 normal = collision.GetContact(0).normal;

        if(collision.gameObject.GetComponent<Monster>() != null)
        {
            normal = new Vector3(0, 5, 0);
        }

        bounceVelocity = normal * bounceForce;

        isBouncing = true;
        bounceTimer = bounceDuration;

        postBounceSlowTimer = postBounceSlowDuration;
    }
}
