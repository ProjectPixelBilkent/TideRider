using DG.Tweening;
using UnityEngine;

public class IdleState : State
{

    public float timeIn;
    public static int maxTime = 2;

    public IdleState(StateMachine machine) : base(machine) { }

    public override void Enter()
    {
        timeIn = 0;
    }

    public override void Exit()
    {
        
    }

    public override void Update()
    {
        Debug.Log("JELLYFISH IdleState Update - Enemy type: " + machine.Enemy.GetType().Name);
        if (timeIn > maxTime)
        {
            Vector2 candidate;

            do
            {
                candidate = new Vector2(Random.onUnitSphere.x, Mathf.Abs(Random.onUnitSphere.y)) * 4;
            }
            while ((candidate - (machine.Enemy.rigidBody.position - new Vector2(0, Camera.main.transform.position.y))).magnitude < 4);

            machine.ChangeState(new MoveState(machine, candidate, 5));
        }
        timeIn += Time.deltaTime;
        machine.Enemy.rigidBody.linearVelocityY = Camera.main.velocity.y;

        Vector3 rand = Random.onUnitSphere;
        machine.Enemy.rigidBody.linearVelocity += new Vector2(rand.x, rand.y) * 0.1f;
    }
}
