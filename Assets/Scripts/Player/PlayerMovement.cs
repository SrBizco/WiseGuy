using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 14f;
    public float jumpForce = 5f;
    public int maxJumps = 2;
    public int jumpCount = 0;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina;
    public float staminaRegenRate = 15f;
    public float sprintStaminaCost = 20f;
    public float jumpStaminaCost = 30f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    private float xRotation = 0f;

    private CharacterController controller;
    public CharacterController Controller => controller;
    public Vector3 velocity;
    public Vector3 Velocity => velocity;
    public float gravity = -9.81f;

    private FiniteStateMachine fsm = new FiniteStateMachine();

    private bool isProp;
    public bool isWalking { get; private set; }
    public bool isRunning { get; private set; }
    public bool isJumping { get; private set; }

    void Start()
    {
        Initialize();
    }

    void OnEnable()
    {
        Initialize();
    }

    void Initialize()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        if (controller == null)
        {
            Debug.LogError($"❌ {name} necesita un CharacterController para funcionar.");
            enabled = false;
            return;
        }

        stamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;

        isProp = gameObject.layer == LayerMask.NameToLayer("Prop");

        if (!isProp)
        {
            fsm = new FiniteStateMachine();
            fsm.Initialize(new FsmPlayerReference(this));
        }
    }

    void Update()
    {
        if (controller == null) return;

        HandleMouseLook();
        HandleMovement();
        RegenerateStamina();

        if (!isProp)
        {
            fsm?.OnUpdate();
        }
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
            isJumping = false;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        isWalking = move.magnitude > 0.1f;
        isRunning = isWalking && Input.GetKey(KeyCode.LeftShift) && stamina > 0f;

        float currentSpeed = isRunning ? sprintSpeed : walkSpeed;

        if (isRunning)
        {
            stamina -= sprintStaminaCost * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

      
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps && stamina >= jumpStaminaCost)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpCount++;
            stamina -= jumpStaminaCost;
            isJumping = true;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void RegenerateStamina()
    {
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            stamina += staminaRegenRate * Time.deltaTime;
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        }
    }
}
