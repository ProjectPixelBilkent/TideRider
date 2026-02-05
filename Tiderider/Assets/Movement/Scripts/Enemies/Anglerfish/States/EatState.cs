using UnityEngine;

public class EatState : State
{
    private readonly Rigidbody2D rb;
    private Vector2 dir;
    private float timeLeft;

    public EatState(StateMachine machine, Rigidbody2D rb, Vector2 dir) : base(machine)
    {
        this.rb = rb;
        this.dir = dir;
    }

    public override void Enter()
    {
        var a = (AnglerFish)machine.Enemy;
        timeLeft = a.eatLungeTime;

        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
        dir.Normalize();

        rb.linearVelocity = Vector2.zero;
    }

    public override void Exit() { }

    public override void Update()
    {
        var a = (AnglerFish)machine.Enemy;

        timeLeft -= Time.deltaTime;

        Vector2 pos = rb.position;
        Vector2 step = dir * a.eatLungeSpeed * Time.fixedDeltaTime;
        rb.MovePosition(pos + step);

        if (timeLeft <= 0f)
        {
            rb.linearVelocity = Vector2.zero;
            machine.ChangeState(new ChaseState(machine, rb));
        }
    }
}
