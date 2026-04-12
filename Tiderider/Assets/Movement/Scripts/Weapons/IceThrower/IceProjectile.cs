using UnityEngine;
using System.Collections.Generic;

public class IceProjectile : Bullet
{
    [SerializeField] private float spinSpeed = 720f;

    private static readonly Dictionary<int, float> sharedDamageTimes = new();

    private float travelSpeed;
    private float lifetime;
    private float elapsed;
    private float damageCooldownSeconds = 0.15f;
    private float perIceDamageMultiplier = 0.15f;
    private int maxDamageTicks = 1;
    private int damageTickCount;

    public void SetFlight(float coneLength, float iceLifetime)
    {
        lifetime = Mathf.Max(0.05f, iceLifetime);
        travelSpeed = Mathf.Max(0.01f, coneLength / lifetime);
    }

    public void SetDamageConfig(float cooldownSeconds, int maxTicks, float damageMultiplier)
    {
        damageCooldownSeconds = Mathf.Max(0f, cooldownSeconds);
        maxDamageTicks = Mathf.Max(0, maxTicks);
        perIceDamageMultiplier = Mathf.Max(0f, damageMultiplier);
    }

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);
        elapsed = 0f;
        damageTickCount = 0;
    }

    private void Update()
    {
        transform.position += Time.deltaTime * SceneObjectSpawner.UpwardsMovement;
    }

    protected override void FixedUpdate()
    {
        if (rigidBody == null)
        {
            return;
        }

        elapsed += Time.fixedDeltaTime;
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        rigidBody.linearVelocity = Vector2.zero;
        transform.Rotate(0f, 0f, -spinSpeed * Time.fixedDeltaTime);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        TryApplyIceDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryApplyIceDamage(collision.collider);
    }

    private void TryApplyIceDamage(Collider2D targetCollider)
    {
        if (targetCollider == null || WeaponLevel == null)
        {
            return;
        }

        if (!targetCollider.TryGetComponent(out HasHealth health))
        {
            return;
        }

        if (!ShouldDamageTarget(health))
        {
            return;
        }

        if (maxDamageTicks > 0 && damageTickCount >= maxDamageTicks)
        {
            return;
        }

        int damageKey = GetSharedDamageKey(health);
        if (sharedDamageTimes.TryGetValue(damageKey, out float lastDamageTime)
            && Time.time - lastDamageTime < damageCooldownSeconds)
        {
            return;
        }

        int damageToApply = Mathf.Max(1, Mathf.RoundToInt(WeaponLevel.damage * perIceDamageMultiplier));
        health.TakeDamage(damageToApply);
        sharedDamageTimes[damageKey] = Time.time;
        damageTickCount++;
    }

    private int GetSharedDamageKey(HasHealth health)
    {
        int ownerId = OwnerTransform == null ? 0 : OwnerTransform.GetInstanceID();
        int weaponId = Weapon == null ? 0 : Weapon.GetInstanceID();
        int targetId = health.GetInstanceID();

        unchecked
        {
            int hash = 17;
            hash = hash * 31 + ownerId;
            hash = hash * 31 + weaponId;
            hash = hash * 31 + targetId;
            return hash;
        }
    }

    private bool ShouldDamageTarget(HasHealth health)
    {
        if (health == null)
        {
            return false;
        }

        if (PlayerBullet)
        {
            return !health.CompareTag("Player");
        }

        return health.CompareTag("Player");
    }
}
