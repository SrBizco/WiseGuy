using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Configuración")]
    public float acceleration = 1500f;
    public float maxSpeed = 50f;
    public float turnSpeed = 2.5f;
    public float drag = 1f;
    public float deceleration = 5f;

    [Header("Sensación de conducción")]
    public float traction = 5f;

    private Rigidbody rb;
    private bool engineStarted = false;
    private bool engineStarting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = drag;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        StartEngine();
    }

    void FixedUpdate()
    {
        if (!engineStarted || engineStarting) return;

        HandleMovement();
        ApplyTraction();
    }

    void HandleMovement()
    {
        float speedInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (speedInput != 0)
        {
            rb.AddForce(transform.forward * speedInput * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVelocity.magnitude > maxSpeed)
        {
            flatVelocity = flatVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(flatVelocity.x, rb.velocity.y, flatVelocity.z);
        }

        float direction = Mathf.Sign(Vector3.Dot(rb.velocity, transform.forward));
        float turnAmount = turnInput * turnSpeed * (rb.velocity.magnitude / maxSpeed) * direction;
        transform.Rotate(0, turnAmount, 0);
    }

    void ApplyTraction()
    {
        Vector3 velocity = rb.velocity;
        Vector3 forward = transform.forward;
        float angle = Vector3.SignedAngle(forward, velocity, Vector3.up);

        Vector3 correction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        rb.velocity = Vector3.Lerp(rb.velocity, correction * velocity.magnitude, traction * Time.fixedDeltaTime);
    }

    void StartEngine()
    {
        if (engineStarting || engineStarted) return;

        engineStarting = true;
        AudioManager.Instance.PlayEngineStart(() =>
        {
            engineStarting = false;
            engineStarted = true;
            AudioManager.Instance.PlayEngineLoop();
        });
    }

    void OnDisable()
    {
        if (engineStarted || engineStarting)
        {
            engineStarted = false;
            engineStarting = false;
            AudioManager.Instance.StopEngineLoop();
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent(out IDamageable damageable))
        {
            float impactForce = collision.relativeVelocity.magnitude;

            if (impactForce > 5f) // Umbral razonable para 'atropello'
            {
                damageable.TakeDamage(999); // Mata al instante
            }
        }
    }
}
