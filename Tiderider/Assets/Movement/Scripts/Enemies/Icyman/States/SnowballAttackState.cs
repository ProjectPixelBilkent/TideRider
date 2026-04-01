using UnityEngine;

public class SnowballAttackState : State
{
    private Rigidbody2D rb;
    private float speed;
    private bool shot;
    private float timer;

    public SnowballAttackState(StateMachine machine, Rigidbody2D rb, float speed)
        : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> SnowballAttackState");
        shot = false;
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;

        // Move Icyman upward with camera scroll
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);

        if (!shot)
        {
            var player = i.FindPlayerTransform();

            Vector2 shootTarget = player != null
                ? (Vector2)player.position
                : (Vector2)i.transform.position;

            float dist = player != null
                ? Vector2.Distance(i.transform.position, player.position)
                : 5f;

            float offset = Mathf.Clamp(dist * 0.5f, 0f, 5f);

            shootTarget += offset * Vector2.up;

            i.ShootSnowballAt(shootTarget);
            shot = true;
        }

        timer += Time.deltaTime;

        if (timer >= i.attackRecovery)
        {
            machine.ChangeState(new Idle(machine, rb, speed));
        }
    }
    public override void Exit() { }
}