using UnityEngine;

public class Walk : BaseState
{
    public Walk(FiniteStateMachine fsm) : base(fsm)
    {
        playerState = PlayerState.Walk;
    }

    public override void OnEnter()
    {
        Debug.Log("Estado actual: Walk");
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
        else if (!reference.Movement.isWalking)
        {
            fsm.ChangeTo(PlayerState.Idle);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Salida de Walk");
    }
}
