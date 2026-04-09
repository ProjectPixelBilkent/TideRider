using UnityEngine;

public class ChaseState : State
{
    private readonly Rigidbody2D rb;

    public ChaseState(StateMachine machine, Rigidbody2D rb) : base(machine)
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

        if (a.ship == null) return;

        float targetX = a.ship.position.x;
        float newX = Mathf.MoveTowards(rb.position.x, targetX, a.chaseSpeed * Time.deltaTime);
        float newY = rb.position.y - a.passDownSpeed * 0.5f * Time.deltaTime;

        Vector2 newPos = new Vector2(newX, newY);
        rb.MovePosition(newPos);

        Vector2 toPlayer = (Vector2)a.ship.position - newPos;
        float r2 = a.eatTriggerDistance * a.eatTriggerDistance;

        if (toPlayer.sqrMagnitude <= r2)
        {
            Vector2 dir = toPlayer.normalized;
            machine.ChangeState(new EatState(machine, rb, dir));
        }
    }
}