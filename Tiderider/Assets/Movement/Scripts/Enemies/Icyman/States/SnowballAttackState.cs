using UnityEngine;

public class SnowballAttackState : State
{
    private Rigidbody2D rb;
    private float speed;
    private float targetX;
    private float yOffsetFromTop;
    private float yVelocity;
    private float timer;
    private bool shot;

    public SnowballAttackState(StateMachine machine, Rigidbody2D rb, float speed, float targetX, float yOffsetFromTop)
        : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
        this.targetX = targetX;
        this.yOffsetFromTop = yOffsetFromTop;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> SnowballAttackState");
        timer = 0f;
        shot = false;
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;

        var target = i.GetCameraRelativeTarget(targetX, yOffsetFromTop);
        Vector2 pos = rb.position;
        pos.y = Mathf.SmoothDamp(pos.y, target.y, ref yVelocity, i.ySmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        pos.x = Mathf.MoveTowards(pos.x, targetX, speed * i.speedMultiplier * i.xSpeedMultiplier * Time.fixedDeltaTime);
        rb.MovePosition(pos);
        rb.linearVelocity = Vector2.zero;

        if (!shot)
        {
            var player = i.FindPlayerTransform();
            Vector2 shootTarget = player != null ? (Vector2)player.position : (Vector2)i.transform.position + Vector2.down;
            i.ShootSnowballAt(shootTarget);
            shot = true;
        }

        timer += Time.fixedDeltaTime;
        if (timer >= i.attackRecovery)
        {
            machine.ChangeState(new Idle(machine, rb, speed));
        }
    }

    public override void Exit() { }
}
