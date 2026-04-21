using UnityEngine;

public class CloseRangeAttackState : State
{
    private Rigidbody2D rb;
    private float speed;
    private float timer;
    private float damageTimer;

    public CloseRangeAttackState(StateMachine machine, Rigidbody2D rb, float speed, float targetX, float yOffsetFromTop) : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> CloseRangeAttackState");
        timer = 0f;
        damageTimer = 0f;
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);
        timer += Time.deltaTime;
        damageTimer += Time.deltaTime;
        i.transform.Rotate(0f, 0f, i.spinSpeed * Time.deltaTime);

        if (i.spinDamageInterval <= 0f)
        {
            i.DoSpinAttackDamage();
        }
        else
        {
            while (damageTimer >= i.spinDamageInterval)
            {
                i.DoSpinAttackDamage();
                damageTimer -= i.spinDamageInterval;
            }
        }

        if (timer >= i.spinDuration)
        {
            i.ResetSpinRotation();
            machine.ChangeState(new Idle(machine, rb, speed));
        }
    }

    public override void Exit()
    {
        ((Icyman)machine.Enemy).ResetSpinRotation();
    }
}
