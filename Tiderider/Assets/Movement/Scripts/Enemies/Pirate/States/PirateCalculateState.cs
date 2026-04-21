using UnityEngine;

public class PirateCalculateState : State
{
    private readonly Rigidbody2D rb;

    public PirateCalculateState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        var p = (PirateShip)machine.Enemy;
        p.FindPlayerIfNeeded();

        if (p.playerRb == null)
        {
            p.moveTarget = rb.position;
            p.shootDir = Vector2.right;
            return;
        }

        Vector2 piratePos = rb.position;
        Vector2 playerPos = p.playerRb.position;
        Vector2 playerVel = p.playerRb.linearVelocity;

        // Aim IN FRONT of the player's movement, not just at the player
        float leadTime = 0.75f;
        Vector2 leadOffset = playerVel * leadTime;

        p.moveTarget = playerPos + leadOffset;

        // Update shoot direction too
        Vector2 shootDir = LeadAim2D.GetShootDirection(
            piratePos,
            playerPos,
            playerVel,
            p.bulletSpeed
        );

        if (shootDir.sqrMagnitude > 0.0001f)
            p.shootDir = shootDir.normalized;

        p.recalcTimer = p.recalcInterval;
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        machine.ChangeState(new PirateMoveState(machine, rb));
    }
}