using UnityEngine;

public class SnowballAttackState : State
{
    private Rigidbody2D rb;
    private float speed;
    private bool shot;
    private float timer;

    public SnowballAttackState(StateMachine machine, Rigidbody2D rb, float speed) : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        shot = false;
        timer = 0f;
        ((Icyman)machine.Enemy).SetSnowballWindupSprite();
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);

        if (!shot && timer >= i.snowballWindup)
        {
            i.ShootSnowballAt(i.GetRandomSnowballTarget());
            shot = true;
            i.SetSnowballRecoverySprite();
        }

        timer += Time.deltaTime;
        if (shot && timer >= i.snowballWindup + i.attackRecovery)
        {
            i.SetDefaultSprite();
            machine.ChangeState(new Idle(machine, rb, speed));
        }
    }

    public override void Exit()
    {
        ((Icyman)machine.Enemy).SetDefaultSprite();
    }
}
