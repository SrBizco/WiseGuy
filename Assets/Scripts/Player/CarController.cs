using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Fuerzas del vehículo")]
    public float motorTorque = 1500f;
    public float maxSpeed = 50f;
    public float brakeForce = 3000f;
    public float deceleration = 5f;
    public float maxSteerAngle = 30f;
    public float traction = 5f;

    private Rigidbody rb;
    private bool engineStarted = false;
    private bool engineStarting = false;

    private List<Wheel> wheels = new List<Wheel>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        InitializeWheels();
    }

    void OnEnable()
    {
        StartEngine();
    }

    void FixedUpdate()
    {
        if (!engineStarted || engineStarting) return;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        ApplyMotorTorque(vertical);
        ApplySteering(horizontal);
        ApplyBrakes(Input.GetKey(KeyCode.Space));
        ApplyTraction();
        LimitSpeed();
        UpdateWheelVisuals();
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
            if (impactForce > 5f)
                damageable.TakeDamage(999);
        }
    }

    void InitializeWheels()
    {
        AddWheel("FLWheelCollider", "FLVisual", true, true);
        AddWheel("FRWheelCollider", "FRVisual", true, true);
        AddWheel("RLWheelCollider", "RLVisual", false, true);
        AddWheel("RRWheelCollider", "RRVisual", false, true);
    }

    void AddWheel(string colliderName, string visualName, bool steerable, bool motorized)
    {
        var wc = transform.Find("WheelColliders/" + colliderName)?.GetComponent<WheelCollider>();
        var visual = transform.Find("WheelVisuals/" + visualName);
        if (wc != null && visual != null)
        {
            var wheel = new Wheel(wc, visual, steerable, motorized);
            wheels.Add(wheel);
        }
    }

    void ApplyMotorTorque(float input)
    {
        float torque = input * motorTorque;
        foreach (var wheel in wheels)
            wheel.ApplyMotorTorque(torque);
    }

    void ApplySteering(float input)
    {
        float steer = input * maxSteerAngle;
        foreach (var wheel in wheels)
            wheel.ApplySteering(steer);
    }

    void ApplyBrakes(bool braking)
    {
        float force = braking ? brakeForce : 0f;
        foreach (var wheel in wheels)
            wheel.ApplyBrake(force);
    }

    void ApplyTraction()
    {
        Vector3 velocity = rb.velocity;
        Vector3 forward = transform.forward;
        float angle = Vector3.SignedAngle(forward, velocity, Vector3.up);
        Vector3 correction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
        rb.velocity = Vector3.Lerp(rb.velocity, correction * velocity.magnitude, traction * Time.fixedDeltaTime);
    }

    void LimitSpeed()
    {
        Vector3 flatVel = rb.velocity;
        flatVel.y = 0;
        if (flatVel.magnitude > maxSpeed)
            rb.velocity = flatVel.normalized * maxSpeed + Vector3.up * rb.velocity.y;

        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.1f)
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
    }

    void UpdateWheelVisuals()
    {
        foreach (var wheel in wheels)
            wheel.UpdateVisual();
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

    // Clase interna Wheel
    private class Wheel
    {
        public WheelCollider wheelCollider;
        public Transform wheelVisual;
        public bool isSteerable;
        public bool isMotorized;

        private Quaternion originalLocalRotation;

        public Wheel(WheelCollider wc, Transform visual, bool steer, bool motor)
        {
            wheelCollider = wc;
            wheelVisual = visual;
            isSteerable = steer;
            isMotorized = motor;

            // Guardar la rotación original al iniciar
            if (wheelVisual != null)
                originalLocalRotation = wheelVisual.localRotation;
        }

        public void UpdateVisual()
        {
            if (wheelCollider == null || wheelVisual == null) return;

            // Posición
            wheelCollider.GetWorldPose(out Vector3 position, out _);
            wheelVisual.position = position;

            // Obtener ángulo de steer
            float steerAngle = wheelCollider.steerAngle;

            // Construir rotación final
            Quaternion steerRot = Quaternion.Euler(0f, steerAngle, 0f);

            // Mantener rotación original y sumar steer
            wheelVisual.localRotation = originalLocalRotation * steerRot;
        }

        public void ApplySteering(float steerAngle)
        {
            if (isSteerable && wheelCollider != null)
                wheelCollider.steerAngle = steerAngle;
        }

        public void ApplyMotorTorque(float torque)
        {
            if (isMotorized && wheelCollider != null)
                wheelCollider.motorTorque = torque;
        }

        public void ApplyBrake(float brakeForce)
        {
            if (wheelCollider != null)
                wheelCollider.brakeTorque = brakeForce;
        }
    }
}
