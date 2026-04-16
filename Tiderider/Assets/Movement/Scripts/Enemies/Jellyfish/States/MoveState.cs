using UnityEngine;

public class MoveState : State
{
    Vector2 pos;
    float speed;

    public MoveState(StateMachine machine, Vector2 pos, float speed) : base(machine) 
    {
        this.pos = pos;
        this.speed = speed;
    }

    public override void Enter()
    {
        
    }

    public override void Exit() {}

    public override void Update()
    {
        Vector2 updatedPos = new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y) + pos;
        Vector2 speedVec = new Vector2(updatedPos.x - machine.Enemy.transform.position.x, updatedPos.y - machine.Enemy.transform.position.y);
        machine.Enemy.rigidBody.linearVelocity = speedVec.normalized * Mathf.Min(speed, speedVec.magnitude + 0.5f) + new Vector2(0, Camera.main.velocity.y);

        if((machine.Enemy.rigidBody.position - updatedPos).magnitude < 0.75f)
        {
            machine.ChangeState(new IdleState(machine));
            machine.Enemy.rigidBody.linearVelocity *= 0.2f;
        }
    }
}
