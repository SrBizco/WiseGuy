using UnityEngine;
using UnityEngine.AI;

public class CivilianController : MonoBehaviour, IDamageable
{
    [Header("Movimiento")]
    [SerializeField] protected float walkSpeed = 3.5f;
    [SerializeField] protected float runSpeed = 6f;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Despawn tras morir")]
    [SerializeField] private float despawnDelay = 4f;

    protected NavMeshAgent agent;
    protected Animator animator;
    private float timer;
    private Vector3 startPosition;

    private RagdollActivator ragdoll;
    private Vector3 externalForce = Vector3.zero;

    protected virtual void Start()
    {
        // Referencias inicializadas en ResetState
    }

    protected virtual void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(startPosition, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }

        UpdateAnimation();

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    protected virtual void UpdateAnimation()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float hor = localVelocity.x;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        float vertValue = 0f;

        if (!isMoving)
        {
            vertValue = 0f; // Idle
        }
        else
        {
            vertValue = 0.5f; // Walk por defecto para civiles
        }

        animator.SetFloat("Hor", hor);
        animator.SetFloat("Vert", vertValue);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        AudioManager.Instance.PlayDamage();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        if (hips != null)
        {
            transform.position = hips.position;
            transform.rotation = hips.rotation;
        }

        agent.enabled = false;
        animator.enabled = false;

        ragdoll.EnableRagdoll();

        if (externalForce != Vector3.zero)
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.AddForce(externalForce, ForceMode.Impulse);
            }
            externalForce = Vector3.zero;
        }

        this.enabled = false;
        Invoke(nameof(DeactivateAndReturnToPool), despawnDelay);
    }

    void DeactivateAndReturnToPool()
    {
        gameObject.SetActive(false);
    }

    public void ResetState()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (ragdoll == null)
        {
            ragdoll = GetComponent<RagdollActivator>();
            ragdoll.Initialize();
        }

        currentHealth = maxHealth;

        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);

        ragdoll.DisableRagdoll();

        transform.position = GetGroundedPosition(transform.position);
        transform.rotation = Quaternion.identity;

        agent.enabled = true;
        agent.updateRotation = false;
        agent.speed = walkSpeed;

        this.enabled = true;
        timer = wanderTimer;
        startPosition = transform.position;
    }

    public void ApplyImpact(Vector3 force)
    {
        externalForce = force;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDir = Random.insideUnitSphere * dist;
        randDir += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDir, out navHit, dist, layermask);
        return navHit.position;
    }

    Vector3 GetGroundedPosition(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, ~0))
        {
            return hit.point;
        }
        return position;
    }
}
