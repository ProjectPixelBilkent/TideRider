using UnityEngine;

public class Idle : State
{
    private Rigidbody2D rb;
    private float speed;
    private int next;

    private float idleDuration = 5f;
    private float idleTimer;

    // plan for next move:
    private float plannedX;
    private float plannedYOffsetFromTop; // e.g. -2 .. -6
    private float yVelocity;

    public Idle(StateMachine machine, Rigidbody2D rb, float speed) : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> Idle");
        next = Random.Range(2, 3);
        idleTimer = 0f;
        rb.linearVelocity = Vector2.zero;

        var i = (Icyman)machine.Enemy;
        idleDuration = i.idleDuration;
        var ship = GameObject.FindGameObjectWithTag("Player");
        plannedX = ship.transform.position.x + (Random.value - 0.5f) * i.xMaxDist;
        plannedX = i.ClampToScreenX(plannedX);


        plannedYOffsetFromTop = -Random.Range(1f, i.yMaxDist);
        yVelocity = 0f;
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;
        // Smoothly follow camera-relative Y while idling.
        var target = i.GetCameraRelativeTarget(plannedX, plannedYOffsetFromTop);
        Vector2 pos = rb.position;
        pos.y = Mathf.SmoothDamp(pos.y, target.y, ref yVelocity, i.ySmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        rb.MovePosition(pos);
        rb.linearVelocity = Vector2.zero;

        if (next == 1)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                Debug.Log("Idle running");
                machine.ChangeState(new MovesState(machine, rb, speed, plannedX, plannedYOffsetFromTop));
                return;
            }
        }
        else
        {
            machine.ChangeState(new MovesState(machine, rb, speed, plannedX, plannedYOffsetFromTop));
            return;
        }
    }

    public override void Exit() { }
}
