using UnityEngine;

public class Run : BaseState
{
    public Run(FiniteStateMachine fsm) : base(fsm)
    {
        playerState = PlayerState.Run;
    }

    public override void OnEnter()
    {
        Debug.Log("Entró en estado Run");
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
        else if (!reference.Movement.isRunning && reference.Movement.isWalking)
        {
            fsm.ChangeTo(PlayerState.Walk);
        }
        else if (!reference.Movement.isWalking)
        {
            fsm.ChangeTo(PlayerState.Idle);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Salió del estado Run");
    }
}
