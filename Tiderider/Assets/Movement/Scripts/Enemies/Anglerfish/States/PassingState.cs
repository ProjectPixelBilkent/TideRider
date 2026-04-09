using UnityEngine;

public class PassingState : State
{
    private readonly Rigidbody2D rb;

    public PassingState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void Exit() { }

    public override void Update()
    {
        var a = (AnglerFish)machine.Enemy;

        if (a.ship == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) a.ship = p.transform;
        }

        if (a.revealTimer > 0f)
            a.revealTimer -= Time.deltaTime;

        Vector2 pos = rb.position;

        // move downward
        pos.y -= a.passDownSpeed * Time.deltaTime;

        // follow player horizontally in a smoother way
        if (a.ship != null)
        {
            float targetX = a.ship.position.x;

            // small side-to-side wobble
            targetX += Mathf.Sin(Time.time * a.wobbleSpeed) * a.wobbleX;

            pos.x = Mathf.MoveTowards(pos.x, targetX, a.xLockMaxSpeed * Time.deltaTime);
        }

        rb.MovePosition(pos);

        if (a.ShouldReveal())
            machine.ChangeState(new RevealState(machine, rb));
    }
}