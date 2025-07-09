using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float sprintSpeed = 14f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 2;
    private int jumpCount = 0;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float sprintStaminaCost = 20f;
    [SerializeField] private float jumpStaminaCost = 30f;
    private float stamina;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private Transform playerCamera;
    private float xRotation = 0f;

    [Header("Footstep")]
    [SerializeField] private float stepInterval = 0.5f;
    private float stepTimer = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;

    private FiniteStateMachine fsm;

    private bool isProp;
    public bool isWalking { get; private set; }
    public bool isRunning { get; private set; }
    public bool isJumping { get; private set; }

    public Vector3 Velocity => velocity;

    void Start()
    {
        Initialize();
        StartCoroutine(WaitForUIManagerAndSetStamina());
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

    private IEnumerator WaitForUIManagerAndSetStamina()
    {
        while (NewUIManager.Instance == null)
        {
            yield return null;
        }

        NewUIManager.Instance.SetStamina(stamina, maxStamina);
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

            if (NewUIManager.Instance != null)
                NewUIManager.Instance.SetStamina(stamina, maxStamina);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps && stamina >= jumpStaminaCost)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            jumpCount++;
            stamina -= jumpStaminaCost;
            isJumping = true;

            if (NewUIManager.Instance != null)
                NewUIManager.Instance.SetStamina(stamina, maxStamina);

            AudioManager.Instance.PlayJump();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        HandleFootsteps();
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

            if (NewUIManager.Instance != null)
                NewUIManager.Instance.SetStamina(stamina, maxStamina);
        }
    }

    void HandleFootsteps()
    {
        if (isWalking && controller.isGrounded && velocity.y <= 0f)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                if (IsOnGrass())
                {
                    AudioManager.Instance.PlayFootstepGrass();
                }
                else
                {
                    AudioManager.Instance.PlayFootstep();
                }

                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval;
        }
    }

    private bool IsOnGrass()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);

        if (Physics.SphereCast(ray, 0.3f, out RaycastHit hit, 2f))
        {
            Debug.Log($"👣 Piso: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            return hit.collider.gameObject.layer == LayerMask.NameToLayer("Grass");
        }

        Debug.Log("👣 No se detectó suelo bajo el jugador");
        return false;
    }
}
