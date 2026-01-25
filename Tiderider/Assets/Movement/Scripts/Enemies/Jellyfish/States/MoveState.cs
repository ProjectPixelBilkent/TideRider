using UnityEngine;

public class MoveState : State
{
    private Vector2 m_Position;
    private Rigidbody2D rb;
    private float speed;

    public MoveState(StateMachine machine, Vector2 moveTo, Rigidbody2D rb, float speed) : base(machine) 
    {
        m_Position = moveTo;
        this.rb = rb;
        this.speed = speed;
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        rb.linearVelocity = (m_Position - rb.position).normalized * speed;
    }
}
