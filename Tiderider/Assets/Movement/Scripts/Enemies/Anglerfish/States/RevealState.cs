using UnityEngine;

public class RevealState : State
{
    private readonly Rigidbody2D rb;
    private float pause;

    public RevealState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        rb.linearVelocity = Vector2.zero;
        pause = 0.2f;

        var a = (AnglerFish)machine.Enemy;
        if (!a.isRevealed)
            a.DoReveal();
    }

    public override void Exit() { }

    public override void Update()
    {
        rb.linearVelocity = Vector2.zero;
        pause -= Time.deltaTime;

        if (pause <= 0f)
            machine.ChangeState(new ChaseState(machine, rb));
    }
}