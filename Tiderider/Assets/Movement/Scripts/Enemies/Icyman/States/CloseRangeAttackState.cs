using UnityEngine;

public class CloseRangeAttackState : State
{
    private Rigidbody2D rb;
    private float speed;
    private float timer;
    private bool attacked;
    private bool telegraphCharged;
    private Vector2 attackDirection;

    public CloseRangeAttackState(StateMachine machine, Rigidbody2D rb, float speed, float targetX, float yOffsetFromTop)
        : base(machine)
    {
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> CloseRangeAttackState");
        timer = 0f;
        attacked = false;
        telegraphCharged = false;

        var i = (Icyman)machine.Enemy;
        attackDirection = i.GetSwipeDirection();
        i.ShowSwipeTelegraph(attackDirection, false);
    }

    public override void Update()
    {
        var i = (Icyman)machine.Enemy;

        // Keep Icyman moving with the camera scroll
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, Camera.main.velocity.y);

        timer += Time.deltaTime;

        if (!telegraphCharged && timer >= Mathf.Max(0f, i.swipeWindup - i.swipeTelegraphFullColorLead))
        {
            i.SetSwipeTelegraphCharged();
            telegraphCharged = true;
        }

        if (!attacked && timer >= i.swipeWindup)
        {
            i.DoSwipeAttack(attackDirection);
            i.HideSwipeTelegraph();
            attacked = true;
        }

        if (timer >= i.swipeWindup + i.attackRecovery)
        {
            machine.ChangeState(new Idle(machine, rb, speed));
        }
    }

    public override void Exit()
    {
        ((Icyman)machine.Enemy).HideSwipeTelegraph();
    }
}
