using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 14f;
    public float jumpForce = 5f;
    public int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float staminaRegenRate = 15f;
    public float sprintStaminaCost = 20f; // por segundo
    public float jumpStaminaCost = 30f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    private float xRotation = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    public float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        stamina = maxStamina;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        RegenerateStamina();
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0f;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (isSprinting)
        {
            stamina -= sprintStaminaCost * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Salto
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps && stamina >= jumpStaminaCost)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpCount++;
            stamina -= jumpStaminaCost;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void RegenerateStamina()
    {
        if (!Input.GetKey(KeyCode.LeftShift)) // no correr = se regenera
        {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
    }
}
