using UnityEngine;

public class ChaseState : State
{
    private readonly Rigidbody2D rb;
    private float damageCooldownTimer;
    private float lurkTimer;
    private bool isLurking;

    public ChaseState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        rb.linearVelocity = Vector2.zero;
        damageCooldownTimer = 0f;
        lurkTimer = 0f;
        isLurking = false;
    }

    public override void Exit() { }

    public override void Update()
    {
        var a = (AnglerFish)machine.Enemy;

        if (a.ship == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                a.ship = p.transform;
        }

        if (a.ship == null) return;

        if (damageCooldownTimer > 0f)
            damageCooldownTimer -= Time.deltaTime;

        Vector2 pos = rb.position;
        Vector2 playerPos = a.ship.position;

        Rigidbody2D playerRb = a.ship.GetComponent<Rigidbody2D>();
        float playerDirX = 0f;

        if (playerRb != null)
        {
            if (playerRb.linearVelocity.x > a.playerMoveThreshold) playerDirX = 1f;
            else if (playerRb.linearVelocity.x < -a.playerMoveThreshold) playerDirX = -1f;
        }

        float targetX = playerPos.x + playerDirX * a.blockAheadDistance;
        float targetY = playerPos.y + a.chaseYOffset;

        targetX += Mathf.Sin(Time.time * a.wobbleSpeed) * a.wobbleX * 0.5f;

        Vector2 targetPos = new Vector2(targetX, targetY);
        Vector2 toTarget = targetPos - pos;
        float distanceToPlayer = Vector2.Distance(pos, playerPos);

        if (distanceToPlayer <= a.contactDamageDistance * 1.5f && !isLurking)
        {
            isLurking = true;
            lurkTimer = a.lurkTime;
        }

        if (isLurking)
        {
            lurkTimer -= Time.deltaTime;

            float hoverX = Mathf.Sin(Time.time * a.wobbleSpeed) * a.wobbleX * 0.4f;
            float hoverY = Mathf.Cos(Time.time * a.wobbleSpeed) * 0.05f;

            Vector2 hoverTarget = new Vector2(targetX + hoverX, targetY + hoverY);
            Vector2 hoverMove = hoverTarget - pos;

            if (hoverMove.sqrMagnitude > 0.001f)
            {
                Vector2 step = hoverMove.normalized * a.chaseSpeed * Time.deltaTime * 0.7f;
                rb.MovePosition(pos + step);
            }

            if (lurkTimer <= 0f)
                isLurking = false;
        }
        else
        {
            if (toTarget.sqrMagnitude > 0.001f)
            {
                Vector2 step = toTarget.normalized * a.chaseSpeed * Time.deltaTime;
                rb.MovePosition(pos + step);
            }
        }

        Vector2 toPlayer = playerPos - rb.position;
        if (toPlayer.sqrMagnitude <= a.contactDamageDistance * a.contactDamageDistance && damageCooldownTimer <= 0f)
        {
            HasHealth health = a.ship.GetComponent<HasHealth>();
            if (health != null)
            {
                health.TakeDamage(a.chaseContactDamage);
                damageCooldownTimer = a.damageCooldown;
            }
        }
    }
}
