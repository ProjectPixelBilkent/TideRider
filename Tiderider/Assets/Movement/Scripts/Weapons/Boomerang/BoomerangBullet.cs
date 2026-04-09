using UnityEngine;

public class BoomerangBullet : Bullet
{
    [SerializeField] private float outwardDuration = 0.22f;
    [SerializeField] private float outwardSpeedMultiplier = 1.8f;
    [SerializeField] private float returnSpeedMultiplier = 3.4f;
    [SerializeField] private float curveStrength = 2.4f;
    [SerializeField] private float maxTurnDegreesPerSecond = 1440f;
    [SerializeField] private float returnAcceleration = 16f;
    [SerializeField] private float catchDistance = 0.35f;
    [SerializeField] private float spinSpeed = 900f;

    private Vector3 travelDirection;
    private float stateTimer;
    private float currentSpeedMultiplier;
    private float curveSign;
    private bool returning;

    protected override void FixedUpdate()
    {
        if (rigidBody == null || WeaponLevel == null)
        {
            return;
        }

        stateTimer += Time.fixedDeltaTime;

        if (!returning && stateTimer >= outwardDuration)
        {
            returning = true;
        }

        Vector3 velocityDirection = travelDirection;

        if (returning)
        {
            if (OwnerTransform == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 toOwner = OwnerTransform.position - transform.position;
            if (toOwner.sqrMagnitude <= catchDistance * catchDistance)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 targetDirection = toOwner.normalized;
            Vector3 lateralCurve = Vector3.Cross(targetDirection, Vector3.forward) * (curveStrength * curveSign);
            Vector3 curvedTargetDirection = (targetDirection + lateralCurve).normalized;
            travelDirection = Vector3.RotateTowards(
                travelDirection,
                curvedTargetDirection,
                maxTurnDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f).normalized;
            velocityDirection = travelDirection;
            currentSpeedMultiplier = Mathf.MoveTowards(
                currentSpeedMultiplier,
                returnSpeedMultiplier,
                returnAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeedMultiplier = Mathf.MoveTowards(
                currentSpeedMultiplier,
                outwardSpeedMultiplier,
                returnAcceleration * Time.fixedDeltaTime);
        }

        rigidBody.linearVelocity = velocityDirection * WeaponLevel.speedOfBullet * 5f * currentSpeedMultiplier + shipSpeed;
        transform.Rotate(0f, 0f, -spinSpeed * Time.fixedDeltaTime);
    }

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);
        travelDirection = this.direction;
        stateTimer = 0f;
        currentSpeedMultiplier = outwardSpeedMultiplier;
        curveSign = this.direction.x < 0f ? -1f : 1f;
        returning = false;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.name == "LevelManager")
        {
            Destroy(gameObject);
            return;
        }

        if (collision.collider.TryGetComponent(out HasHealth health) && ShouldDamageTargetOnce(health))
        {
            health.TakeDamage(WeaponLevel.damage);
            returning = true;
        }
    }

    private bool ShouldDamageTargetOnce(HasHealth health)
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
    public override void OnBecameInvisible()
    {

    }
}
