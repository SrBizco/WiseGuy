using UnityEngine;
using UnityEngine.AI;

public class CivilianController : MonoBehaviour, IDamageable
{
    [Header("Movimiento")]
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;

    [Header("Vida")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Despawn tras morir")]
    public float despawnDelay = 4f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;
    private Vector3 startPosition;

    private RagdollActivator ragdoll;

    // 🔁 Fuerza externa que se aplica al morir (ej. atropello)
    private Vector3 externalForce = Vector3.zero;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(startPosition, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }

        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float hor = localVelocity.x;
        float vert = localVelocity.z;

        animator.SetFloat("Hor", hor);
        animator.SetFloat("Vert", vert);

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

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

        // 🧱 Aplicar fuerza externa si hubo impacto (auto, explosión, etc.)
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
        // Asignación de referencias si no están
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (ragdoll == null)
        {
            ragdoll = GetComponent<RagdollActivator>();
            ragdoll.Initialize();
        }

        currentHealth = maxHealth;

        // Reestablecer animator y pose
        animator.enabled = true;
        animator.Rebind();
        animator.Update(0f);

        // Detener ragdoll
        ragdoll.DisableRagdoll();

        // Posicionarlo correctamente sobre el suelo
        transform.position = GetGroundedPosition(transform.position);
        transform.rotation = Quaternion.identity;

        // Reiniciar lógica
        agent.enabled = true;
        agent.updateRotation = false;

        this.enabled = true;
        timer = wanderTimer;
        startPosition = transform.position;
    }

    // 👉 Llamar desde otro script para aplicar fuerza al morir
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
