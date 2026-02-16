using UnityEngine;

public class ShockedState : State
{
    public ShockedState(StateMachine machine) : base(machine)
    {
    }

    public override void Enter()
    {
        //TODO: damage the player
    }

    public override void Exit() {}

    public override void Update()
    {
        throw new System.NotImplementedException();
    }
}
