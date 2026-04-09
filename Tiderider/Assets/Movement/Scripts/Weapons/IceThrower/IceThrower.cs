using UnityEngine;

public class IceThrower : Bullet
{
    [SerializeField] private GameObject iceProjectilePrefab;
    [SerializeField] private float sprayAngleDegrees = 35f;
    [SerializeField] private float pauseDurationSeconds = 1f;
    [SerializeField] private float coneLength = 3f;
    [SerializeField] private float rowSpacing = 0.45f;
    [SerializeField] private int rowMultiplier = 2;
    [SerializeField] private float rowEmitInterval = 0.08f;
    [SerializeField] private float iceLifetime = 0.22f;
    [SerializeField] private float spawnOffset = 0.2f;
    [SerializeField] private float damageCooldownSeconds = 0.15f;
    [SerializeField] private int maxDamageTicksPerIce = 1;
    [SerializeField] private float perIceDamageMultiplier = 0.15f;

    private Vector3 anchorOffset;
    private float elapsed;
    private float nextRowEmitTime;
    private int nextRowIndex;

    public float ShootDurationSeconds => Mathf.Max(0.01f, GetRowCount() * Mathf.Max(0.01f, rowEmitInterval));
    public float PauseDurationSeconds => pauseDurationSeconds;

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        anchorOffset = OwnerTransform == null ? Vector3.zero : transform.position - OwnerTransform.position;
        elapsed = 0f;
        nextRowEmitTime = 0f;
        nextRowIndex = 0;
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
        transform.position = OwnerTransform.position + anchorOffset;
        rigidBody.linearVelocity = Vector2.zero;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (nextRowIndex >= GetRowCount())
        {
            Destroy(gameObject);
            return;
        }

        if (elapsed < nextRowEmitTime)
        {
            return;
        }

        EmitIceRow(nextRowIndex);
        nextRowIndex++;
        nextRowEmitTime = elapsed + Mathf.Max(0.01f, rowEmitInterval);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
    }

    private void EmitIceRow(int rowIndex)
    {
        if (iceProjectilePrefab == null)
        {
            return;
        }

        Vector3 forward = direction.normalized;
        Vector3 lateral = Vector3.Cross(forward, Vector3.forward).normalized;
        float halfAngleRadians = sprayAngleDegrees * 0.5f * Mathf.Deg2Rad;

        int projectileCount = Mathf.Max(1, (rowIndex + 1) * Mathf.Max(1, rowMultiplier));
        float distanceFromEmitter = spawnOffset + rowSpacing * (rowIndex + 1);
        float halfWidth = Mathf.Tan(halfAngleRadians) * distanceFromEmitter;

        for (int projectileIndex = 0; projectileIndex < projectileCount; projectileIndex++)
        {
            var projectile = Instantiate(iceProjectilePrefab).GetComponent<IceProjectile>();
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
            projectile.SetFlight(Mathf.Max(rowSpacing, coneLength - distanceFromEmitter + rowSpacing), iceLifetime);
            projectile.SetDamageConfig(damageCooldownSeconds, maxDamageTicksPerIce, perIceDamageMultiplier);
            projectile.Activate(shotDirection, shipSpeed);

            if (lateralOffset > 0f)
            {
                projectile.transform.Rotate(0f, 0f, 180f);
            }

            if (projectile.circleCollider != null && OwnerTransform != null && OwnerTransform.TryGetComponent(out Collider2D ownerCollider))
            {
                Physics2D.IgnoreCollision(projectile.circleCollider, ownerCollider, true);
            }
        }
    }

    private int GetRowCount()
    {
        float usableLength = Mathf.Max(rowSpacing, coneLength);
        return Mathf.Max(1, Mathf.RoundToInt(usableLength / rowSpacing));
    }
}
