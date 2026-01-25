using UnityEngine;

public class Jellyfish : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        fsm = new StateMachine();
        fsm.Init(new MoveState(fsm, new UnityEngine.Vector3(0, 50, 0), rb, speed), this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
