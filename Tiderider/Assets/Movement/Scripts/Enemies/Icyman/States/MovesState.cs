using UnityEngine;

public class MovesState : State
{
    private Rigidbody2D rb;
    private Vector2 pos;
    private float speed;

    public MovesState(StateMachine machine, Vector2 pos, float speed, Rigidbody2D rb) : base(machine)
    {
        this.pos = pos;
        this.speed = speed;
        this.rb = rb;
    }

    public override void Enter()
    {
        Debug.Log("Icyman -> MovesState");
    }

    public override void Update()
    {
        Vector2 updatedPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y) + pos;
        Vector2 speedVec = new Vector2(updatedPos.x - machine.Enemy.transform.position.x, updatedPos.y - machine.Enemy.transform.position.y);
        machine.Enemy.rigidBody.linearVelocity = speedVec.normalized * Mathf.Min(speed, speedVec.magnitude + 0.5f) + new Vector2(0, Camera.main.velocity.y);

        if ((machine.Enemy.rigidBody.position - updatedPos).magnitude < 0.75f)
        {
            machine.ChangeState(new Idle(machine, rb, speed));
            machine.Enemy.rigidBody.linearVelocity *= 0.2f;
        }
    }

    public override void Exit() { }
}
