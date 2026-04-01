using UnityEngine;

public class Idle : State
{
    private Rigidbody2D rb;
    private float speed;
    private int next;

    private float idleDuration = 10f;
    public float timeIn;

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
        next = Random.Range(1, 5);
        next = 4; 
        if (next == 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);
        }
        timeIn = 0;
        rb.linearVelocity = Vector2.zero;

        var i = (Icyman)machine.Enemy;
        idleDuration = i.idleDuration;;
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);
            timeIn += Time.deltaTime;
            if (timeIn >= idleDuration)
            {
                Debug.Log("Idle running");
                next = Random.Range(1, 5);
                return;
            }
        }
        if (next == 2) 
        {
            Vector2 candidate;
            
            float rndNumber = Random.Range(-3f, 3f);
            candidate = new Vector2(rndNumber, 0f);

            Debug.Log("icy: " + candidate.x + " " + candidate.y);


            machine.ChangeState(new MovesState(machine, candidate, speed, rb));
        }
        if (next == 3)
        {
            machine.ChangeState(new SnowballAttackState(machine, rb, speed));
        }
        if (next == 4)
        {
            machine.ChangeState(new CloseRangeAttackState(machine, rb, speed, plannedX, plannedYOffsetFromTop));
        }
        timeIn += Time.deltaTime;
        machine.Enemy.rigidBody.linearVelocityY = Camera.main.velocity.y;
        return;
    }

    public override void Exit() { }
}
