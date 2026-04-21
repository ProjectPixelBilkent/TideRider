using UnityEngine;

public class StateMachine
{
    private State state;
    public Enemy Enemy { get; private set; }

    public void Init(State state, Enemy enemy)
    {
        this.Enemy = enemy;

        this.state = state;
        this.state.Enter();
    }

    public void ChangeState(State state)
    {
        this.state?.Exit();
        state.Enter();
        this.state = state;
    }

    public void FixedUpdate()
    {
        state?.Update();
    }
}
