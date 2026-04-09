using UnityEngine;

public class Flamethrower : Bullet
{
    [SerializeField] private GameObject flameProjectilePrefab;
    [SerializeField] private float sprayAngleDegrees = 35f;
    [SerializeField] private float shootDurationSeconds = 1f;
    [SerializeField] private float pauseDurationSeconds = 1f;
    [SerializeField] private float coneLength = 3f;
    [SerializeField] private float rowSpacing = 0.45f;
    [SerializeField] private float emitInterval = 0.05f;
    [SerializeField] private float flameLifetime = 0.22f;
    [SerializeField] private float spawnOffset = 0.2f;
    [SerializeField] private float damageCooldownSeconds = 0.15f;
    [SerializeField] private int maxDamageTicksPerFlame = 1;
    [SerializeField] private float perFlameDamageMultiplier = 0.15f;

    private Vector3 anchorOffset;
    private float elapsed;
    private float nextEmitTime;

    public float ShootDurationSeconds => shootDurationSeconds;
    public float PauseDurationSeconds => pauseDurationSeconds;
    public float ConeLength => coneLength;
    public float SprayAngleDegrees => sprayAngleDegrees;

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        anchorOffset = OwnerTransform == null ? Vector3.zero : transform.position - OwnerTransform.position;
        elapsed = 0f;
        nextEmitTime = 0f;
    }

    protected override void FixedUpdate()
    {
        if (WeaponLevel == null || rigidBody == null)
        {
            return;
        }

        if (OwnerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.fixedDeltaTime;
        if (elapsed >= shootDurationSeconds)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = OwnerTransform.position + anchorOffset;
        rigidBody.linearVelocity = Vector2.zero;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (elapsed < nextEmitTime)
        {
            return;
        }

        nextEmitTime = elapsed + emitInterval;
        EmitFlameProjectile();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
    }

    private void EmitFlameProjectile()
    {
        if (flameProjectilePrefab == null)
        {
            return;
        }

        float usableLength = Mathf.Max(rowSpacing, coneLength);
        int rowCount = Mathf.Max(1, Mathf.RoundToInt(usableLength / rowSpacing));
        Vector3 forward = direction.normalized;
        Vector3 lateral = Vector3.Cross(forward, Vector3.forward).normalized;
        float halfAngleRadians = sprayAngleDegrees * 0.5f * Mathf.Deg2Rad;

        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            int projectileCount = rowIndex + 1;
            float distanceFromEmitter = spawnOffset + rowSpacing * (rowIndex + 1);
            float halfWidth = Mathf.Tan(halfAngleRadians) * distanceFromEmitter;

            for (int projectileIndex = 0; projectileIndex < projectileCount; projectileIndex++)
            {
                var projectile = Instantiate(flameProjectilePrefab).GetComponent<FlameProjectile>();
                if (projectile == null)
                {
                    return;
                }

                float lateralOffset =
                    projectileCount == 1
                    ? 0f
                    : Mathf.Lerp(-halfWidth, halfWidth, projectileIndex / (float)(projectileCount - 1));

                Vector3 spawnPosition =
                    transform.position
                    + forward * distanceFromEmitter
                    + lateral * lateralOffset;

                Vector3 shotDirection = (forward * distanceFromEmitter + lateral * lateralOffset).normalized;

                projectile.Weapon = Weapon;
                projectile.Level = Level;
                projectile.WeaponLevel = WeaponLevel;
                projectile.PlayerBullet = PlayerBullet;
                projectile.OwnerTransform = OwnerTransform;

                projectile.transform.position = spawnPosition;
                projectile.SetFlight(Mathf.Max(rowSpacing, coneLength - distanceFromEmitter + rowSpacing), flameLifetime);
                projectile.SetDamageConfig(damageCooldownSeconds, maxDamageTicksPerFlame, perFlameDamageMultiplier);
                projectile.Activate(shotDirection, shipSpeed);

                if (projectile.circleCollider != null && OwnerTransform != null && OwnerTransform.TryGetComponent(out Collider2D ownerCollider))
                {
                    Physics2D.IgnoreCollision(projectile.circleCollider, ownerCollider, true);
                }
            }
        }
    }
}
