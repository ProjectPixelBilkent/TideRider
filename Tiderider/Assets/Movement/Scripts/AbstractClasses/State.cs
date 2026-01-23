public abstract class State
{
    private StateMachine machine;

    public State(StateMachine machine)
    {
        this.machine = machine;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
}
