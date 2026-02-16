using UnityEngine;

public class Jellyfish : Enemy
{
    [Header("Behavior")]
    public float topOffsetFromCamera = 3.5f;   // where "top" is relative to camera
    public float lingerTime = 20f;             // seconds to stay near top
    public float downSpeed = 1.0f;             // drift speed downward
    public float bottomOffsetFromCamera = -6f; // how far below camera before destroy

    protected override void Start()
    {
        base.Start();

        fsm = new StateMachine();
        fsm.Init(new IdleState(fsm), this);
    }
}
