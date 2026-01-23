public class StateMachine
{
    private State state;

    public void ChangeState(State state)
    {
        this.state.Exit();
        state.Enter();
        this.state = state;
    }

    private void FixedUpdate()
    {
        state.Update();
    }
}
