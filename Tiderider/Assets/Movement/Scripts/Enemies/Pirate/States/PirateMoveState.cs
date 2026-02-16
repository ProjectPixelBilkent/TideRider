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
        var p = (PirateShip)machine.Enemy;
        // Reset the recalculation timer when entering this state
        p.recalcTimer = p.recalcInterval;
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        var p = (PirateShip)machine.Enemy;
        Debug.Log("PirateMoveState controlling: " + p.gameObject.name);
        p.FindPlayerIfNeeded();

        // Count down recalc timer - when it hits zero, recalculate optimal position
        p.recalcTimer -= Time.fixedDeltaTime;
        if (p.recalcTimer <= 0f)
        {
            // Time to recalculate optimal firing position
            machine.ChangeState(new PirateCalculateState(machine, rb));
            return;
        }

        // Move towards the calculated target position
        Vector2 delta = p.moveTarget - rb.position;
        Debug.Log("Pirate at: " + rb.position + ", Target: " + p.moveTarget + ", Delta: " + delta);
        // Smooth steering with different up/down speeds
        float dx = delta.x;
        float dy = delta.y;

        // Pick directional max speeds
        float sx = p.maxSpeedSide;
        float sy = (dy > 0f) ? p.maxSpeedUp : p.maxSpeedDown;

        // Build desired velocity with separate caps
        Vector2 desiredVel = Vector2.zero;
        if (delta.sqrMagnitude > 0.0001f)
        {
            Vector2 dir = delta.normalized;
            desiredVel = new Vector2(dir.x * sx, dir.y * sy);
        }

        // Pick directional accel
        float ax = p.maxAccelSide;
        float ay = (desiredVel.y > rb.linearVelocity.y) ? p.maxAccelUp : p.maxAccelDown;

        // Move current velocity toward desired velocity per-axis
        Vector2 v = rb.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, desiredVel.x, ax * Time.fixedDeltaTime);
        v.y = Mathf.MoveTowards(v.y, desiredVel.y, ay * Time.fixedDeltaTime);
        rb.linearVelocity = v;
    }
}