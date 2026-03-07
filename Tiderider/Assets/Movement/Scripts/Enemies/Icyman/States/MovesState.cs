using UnityEngine;

public class MovesState : State
{
    private Rigidbody2D rb;
    private float speed;

    private float targetX;
    private float yOffsetFromTop;

    private Vector2 target;
    private float yVelocity;

    public MovesState(StateMachine machine, Rigidbody2D rb, float speed, float targetX, float yOffsetFromTop)
        : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
        this.targetX = targetX;
        this.yOffsetFromTop = yOffsetFromTop;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> MovesState");
        var i = (Icyman)machine.Enemy;
        targetX = i.ClampToScreenX(targetX);
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;
        float effectiveSpeed = speed * i.speedMultiplier * i.xSpeedMultiplier;

        // world target moves up with camera, x stays what we planned
        target = i.GetCameraRelativeTarget(targetX, yOffsetFromTop);

        // Smoothly follow camera-relative Y, move X toward planned target.
        Vector2 pos = rb.position;
        pos.y = Mathf.SmoothDamp(pos.y, target.y, ref yVelocity, i.ySmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        pos.x = Mathf.MoveTowards(pos.x, target.x, effectiveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(pos);
        rb.linearVelocity = Vector2.zero;

        if (Mathf.Abs(target.x - pos.x) < 0.2f)
        {
            int attackChoice = Random.Range(0, 2);
            if (attackChoice == 0)
            {
                machine.ChangeState(new CloseRangeAttackState(machine, rb, speed, targetX, yOffsetFromTop));
            }
            else
            {
                machine.ChangeState(new SnowballAttackState(machine, rb, speed, targetX, yOffsetFromTop));
            }
            return;
        }
    }

    public override void Exit() { }
}
