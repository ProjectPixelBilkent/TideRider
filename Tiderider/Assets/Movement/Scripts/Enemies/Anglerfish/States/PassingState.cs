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

        if (a.revealTimer > 0f) a.revealTimer -= Time.deltaTime;

        Vector2 pos = rb.position;

        pos.y -= a.passDownSpeed * Time.fixedDeltaTime;

 
        float desiredX = pos.x;
        if (a.ship != null)
        {
            float playerX = a.ship.position.x;

            if (Mathf.Abs(playerX - pos.x) > a.xDeadZone)
                desiredX = playerX;
        }

   
        desiredX += Mathf.Sin(Time.time * a.wobbleSpeed) * a.wobbleX;

        float maxStep = a.xLockMaxSpeed * Time.fixedDeltaTime;
        pos.x = Mathf.MoveTowards(pos.x, desiredX, maxStep);

        rb.MovePosition(pos);

        if (a.ShouldReveal())
            machine.ChangeState(new RevealState(machine, rb));
    }
}
