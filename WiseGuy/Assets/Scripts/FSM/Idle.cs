using UnityEngine;

public class Idle : BaseState
{
    public Idle(FiniteStateMachine fsm) : base(fsm)
    {
        playerState = PlayerState.Idle;
    }

    public override void OnEnter()
    {
        Debug.Log("Estado actual: Idle");
    }

    public override void OnUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isJumping = reference.IsJumping;

        reference.Animate(horizontal, vertical, isJumping);

        if (reference.Movement.isJumping)
        {
            fsm.ChangeTo(PlayerState.Jump);
        }
        else if (reference.Movement.isRunning)
        {
            fsm.ChangeTo(PlayerState.Run);
        }
        else if (reference.Movement.isWalking)
        {
            fsm.ChangeTo(PlayerState.Walk);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Salida de Idle");
    }
}
