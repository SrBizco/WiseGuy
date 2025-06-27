using UnityEngine;

public class Jump : BaseState
{
    public Jump(FiniteStateMachine fsm) : base(fsm)
    {
        playerState = PlayerState.Jump;
    }

    public override void OnEnter()
    {
        Debug.Log("Entró en estado Jump");
    }

    public override void OnUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isJumping = reference.IsJumping;

        reference.Animate(horizontal, vertical, isJumping);

        if (reference.Controller.isGrounded && reference.Movement.velocity.y <= 0)
        {
            if (reference.Movement.isRunning)
                fsm.ChangeTo(PlayerState.Run);
            else if (reference.Movement.isWalking)
                fsm.ChangeTo(PlayerState.Walk);
            else
                fsm.ChangeTo(PlayerState.Idle);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Salió del estado Jump");
    }
}
