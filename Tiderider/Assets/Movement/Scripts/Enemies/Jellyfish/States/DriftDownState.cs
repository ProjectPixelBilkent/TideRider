using UnityEngine;

public class DriftDownState : State
{
    private Rigidbody2D rb;
    private float downSpeed;

    public DriftDownState(StateMachine machine, Rigidbody2D rb, float downSpeed) : base(machine)
    {
        this.rb = rb;
        this.downSpeed = downSpeed;
    }

    public override void Enter()
    {
        
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        rb.linearVelocity = Vector2.down * downSpeed;

        var j = (Jellyfish)machine.Enemy;
        if (rb.position.y < j.GetBottomY())
        {
            Object.Destroy(j.gameObject);
        }
    }
}
