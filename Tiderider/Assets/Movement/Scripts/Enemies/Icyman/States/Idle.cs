using UnityEngine;

public class Idle : State
{
    private Rigidbody2D rb;
    private float speed;
    private int next;
    private float idleDuration = 10f;
    public float timeIn;
    private float plannedX;
    private float plannedYOffsetFromTop;
    private float yVelocity;

    public Idle(StateMachine machine, Rigidbody2D rb, float speed) : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> Idle");
        next = Random.Range(1, 5);
        if (next == 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);
        }

        timeIn = 0f;
        rb.linearVelocity = Vector2.zero;

        var i = (Icyman)machine.Enemy;
        idleDuration = i.idleDuration;
        plannedX = Random.Range(-i.xMaxDist, i.xMaxDist);
        plannedYOffsetFromTop = -Random.Range(1f, i.yMaxDist);
        yVelocity = 0f;
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;
        var target = i.GetCameraRelativeTarget(plannedX, plannedYOffsetFromTop);
        Vector2 pos = rb.position;
        pos.y = Mathf.SmoothDamp(pos.y, target.y, ref yVelocity, i.ySmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        rb.MovePosition(pos);
        rb.linearVelocity = Vector2.zero;
        //idle
        if (next == 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);
            timeIn += Time.deltaTime;
            if (timeIn >= idleDuration)
            {
                next = Random.Range(1, 4);
                return;
            }
        }
        //move
        if (next == 2)
        {
            Vector2 candidate = new Vector2(Random.Range(-3f, 3f), 0f);
            machine.ChangeState(new MovesState(machine, candidate, speed, rb));
            return;
        }
        // snowball/melee attack
        if (next >= 3)
        {
            var player = i.FindPlayerTransform();
            float distanceToPlayer = player != null
                ? Vector2.Distance(machine.Enemy.transform.position, player.position)
                : float.PositiveInfinity;

            if (distanceToPlayer <= i.swipeTriggerDistance)
            {
                machine.ChangeState(new CloseRangeAttackState(machine, rb, speed, plannedX, plannedYOffsetFromTop));
            }
            else
            {
                machine.ChangeState(new SnowballAttackState(machine, rb, speed));
            }
        }
    }

    public override void Exit() { }
}
