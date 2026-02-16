using UnityEngine;

public class SpawnState : State
{
    private readonly Rigidbody2D rb;

    public SpawnState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        var p = (PirateShip)machine.Enemy;

        rb.linearVelocity = Vector2.zero;

        if (p.autoSpawnFromTop)
            p.ForceSpawnFromCameraTop();
    }

    public override void Exit() { }

    public override void Update()
    {
        machine.ChangeState(new CalculateState(machine, rb));
    }
}
