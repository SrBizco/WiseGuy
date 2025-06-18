public abstract class BaseState
{
    protected FiniteStateMachine fsm;
    protected FsmPlayerReference reference;

    public PlayerState playerState;

    protected BaseState(FiniteStateMachine fsm)
    {
        this.fsm = fsm;
        this.reference = fsm.Reference;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}
