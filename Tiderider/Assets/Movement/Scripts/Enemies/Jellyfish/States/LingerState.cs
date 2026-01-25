using UnityEngine;

public class LingerState : State
{
    private Rigidbody2D rb;
    private float timeLeft;

    public LingerState(StateMachine machine, Rigidbody2D rb, float seconds) : base(machine)
    {
        this.rb = rb;
        timeLeft = seconds;
    }

    public override void Enter()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void Exit() { }

    public override void Update()
    {
        timeLeft -= Time.deltaTime;
        rb.linearVelocity = Vector2.zero;

        if (timeLeft <= 0f)
        {
            var j = (Jellyfish)machine.Enemy;
            machine.ChangeState(new DriftDownState(machine, rb, j.downSpeed));
        }
    }
}
