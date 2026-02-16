using UnityEngine;

public class MoveState : State
{
    private readonly Rigidbody2D rb;

    public MoveState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter() { }
    public override void Exit() { }

    public override void Update()
    {
        var p = (PirateShip)machine.Enemy;
        p.FindPlayerIfNeeded();

        Vector2 delta = p.moveTarget - rb.position;

        if (delta.magnitude <= p.stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            machine.ChangeState(new CalculateState(machine, rb));
            return;
        }

        rb.linearVelocity = delta.normalized * p.moveSpeed;

        // NOTE: we are NOT shooting now. Only calculating p.shootDir.
        // Later you’ll use p.shootDir when bullets/weapons come back.
    }
}
