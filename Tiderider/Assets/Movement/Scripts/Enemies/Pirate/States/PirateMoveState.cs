using UnityEngine;

public class PirateMoveState : State
{
    private readonly Rigidbody2D rb;

    public PirateMoveState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        var p = (PirateShip)machine.Enemy;
        p.FindPlayerIfNeeded();

        if (p.playerRb == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 piratePos = rb.position;
        Vector2 playerPos = p.playerRb.position;
        Vector2 playerVel = p.playerRb.linearVelocity;

        // Recalculate target sometimes
        p.recalcTimer -= Time.deltaTime;
        if (p.recalcTimer <= 0f)
        {
            machine.ChangeState(new PirateCalculateState(machine, rb));
            return;
        }

        // Continuously keep target ahead of the player
        float leadTime = 0.75f;
        Vector2 leadOffset = playerVel * leadTime;
        p.moveTarget = playerPos + leadOffset;

        Vector2 toTarget = p.moveTarget - piratePos;
        float dist = toTarget.magnitude;

        if (dist < 0.01f)
        {
            rb.linearVelocity = Vector2.MoveTowards(
                rb.linearVelocity,
                playerVel,
                p.maxAccelSide * Time.deltaTime
            );
            return;
        }

        Vector2 chaseDir = toTarget.normalized;

        // Catch-up strength gets bigger when farther away
        float catchUpStrength = Mathf.Clamp(dist * 1.5f, 2f, 12f);

        // Desired velocity = player's velocity + extra velocity toward target
        Vector2 desiredVel = playerVel + chaseDir * catchUpStrength;

        // Clamp per-axis using pirate limits
        float maxYSpeed = desiredVel.y >= 0f ? p.maxSpeedUp : p.maxSpeedDown;

        desiredVel.x = Mathf.Clamp(desiredVel.x, -p.maxSpeedSide, p.maxSpeedSide);
        desiredVel.y = Mathf.Clamp(desiredVel.y, -maxYSpeed, maxYSpeed);

        Vector2 currentVel = rb.linearVelocity;

        float accelX = p.maxAccelSide;
        float accelY = desiredVel.y > currentVel.y ? p.maxAccelUp : p.maxAccelDown;

        currentVel.x = Mathf.MoveTowards(currentVel.x, desiredVel.x, accelX * Time.deltaTime);
        currentVel.y = Mathf.MoveTowards(currentVel.y, desiredVel.y, accelY * Time.deltaTime);

        rb.linearVelocity = currentVel;

        // Keep shooting direction updated
        Vector2 shootDir = LeadAim2D.GetShootDirection(
            piratePos,
            playerPos,
            playerVel,
            p.bulletSpeed
        );

        if (shootDir.sqrMagnitude > 0.0001f)
            p.shootDir = shootDir.normalized;
    }
}