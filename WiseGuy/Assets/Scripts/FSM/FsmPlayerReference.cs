using UnityEngine;

public class FsmPlayerReference
{
    public PlayerMovement Movement { get; private set; }
    public CharacterController Controller { get; private set; }
    public Animator Animator { get; private set; }

    public bool IsWalking => Movement.isWalking;
    public bool IsRunning => Movement.isRunning;
    public bool IsJumping => Movement.isJumping;
    public bool IsGrounded => Controller.isGrounded;

    public FsmPlayerReference(PlayerMovement movement)
    {
        Movement = movement;
        Controller = movement.GetComponent<CharacterController>();
        Animator = movement.GetComponent<Animator>(); // ⬅️ CAMBIADO: busca en el mismo GameObject
    }

    public void Animate(float horizontalInput, float verticalInput, bool isJump)
    {
        if (Animator == null) return;

        // Calcula dirección total
        float inputMagnitude = Mathf.Clamp01(new Vector2(horizontalInput, verticalInput).magnitude);

        // Decide si está corriendo o caminando
        float vertValue = 0f;
        if (IsRunning)
            vertValue = 1f;     // correr
        else if (IsWalking)
            vertValue = 0.5f;   // caminar
        else
            vertValue = 0f;     // idle

        Animator.SetFloat("Hor", horizontalInput);
        Animator.SetFloat("Vert", vertValue);
        Animator.SetBool("IsJump", isJump);
    }
}
