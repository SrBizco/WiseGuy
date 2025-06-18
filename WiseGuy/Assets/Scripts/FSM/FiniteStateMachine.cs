using System.Collections.Generic;

public class FiniteStateMachine
{
    public FsmPlayerReference Reference { get; private set; }
    public BaseState currentState;

    private Dictionary<PlayerState, BaseState> states = new Dictionary<PlayerState, BaseState>();

    public void Initialize(FsmPlayerReference reference)
    {
        Reference = reference;

        states.Add(PlayerState.Idle, new Idle(this));
        states.Add(PlayerState.Walk, new Walk(this));
        states.Add(PlayerState.Run, new Run(this));
        states.Add(PlayerState.Jump, new Jump(this));


        currentState = states[PlayerState.Idle];
        currentState.OnEnter();
    }

    public void OnUpdate()
    {
        currentState?.OnUpdate();
    }

    public void ChangeTo(PlayerState newState)
    {
        currentState?.OnExit();
        currentState = states[newState];
        currentState.OnEnter();
    }
}