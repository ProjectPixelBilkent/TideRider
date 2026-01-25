using UnityEngine;

public class GoToTopState : State
{
    private Rigidbody2D rb;
    private float speed;
    private Vector2 target;

    public GoToTopState(StateMachine machine, Rigidbody2D rb, float speed) : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        target = ((Jellyfish)machine.Enemy).GetTopPoint();
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        // keep x tracking player while going up (optional but usually feels better)
        var j = (Jellyfish)machine.Enemy;
        target = j.GetTopPoint();

        Vector2 delta = target - rb.position;

        if (delta.magnitude < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            machine.ChangeState(new LingerState(machine, rb, j.lingerTime));
            return;
        }

        rb.linearVelocity = delta.normalized * speed;
    }
}

