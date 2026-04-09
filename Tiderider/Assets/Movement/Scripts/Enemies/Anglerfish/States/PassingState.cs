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
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                a.ship = p.transform;
        }

        if (a.revealTimer > 0f)
            a.revealTimer -= Time.deltaTime;

        Vector2 pos = rb.position;

        float topY = Camera.main.transform.position.y + a.topOffset;
        pos.y = Mathf.MoveTowards(pos.y, topY, a.topFollowSpeed * Time.deltaTime);

        if (a.ship != null)
        {
            float playerX = a.ship.position.x;

            float dir = 0f;
            if (playerX > pos.x + 0.1f) dir = 1f;
            else if (playerX < pos.x - 0.1f) dir = -1f;

            float targetX = playerX + dir * a.blockAheadDistance;
            targetX += Mathf.Sin(Time.time * a.wobbleSpeed) * a.wobbleX;

            pos.x = Mathf.MoveTowards(pos.x, targetX, a.xLockMaxSpeed * Time.deltaTime);
        }

        rb.MovePosition(pos);

        if (a.ShouldReveal())
            machine.ChangeState(new RevealState(machine, rb));
    }
}