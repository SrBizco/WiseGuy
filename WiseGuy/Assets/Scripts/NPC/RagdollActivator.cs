using UnityEngine;
using UnityEngine.AI;

public class RagdollActivator : MonoBehaviour
{
    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;
    private Animator animator;
    private NavMeshAgent navMeshAgent;

    private bool initialized = false;

    public void Initialize()
    {
        if (initialized) return;

        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 3f;
        }

        foreach (var col in ragdollColliders)
            col.enabled = false;

        if (TryGetComponent<Collider>(out var mainCollider))
            mainCollider.enabled = true;

        initialized = true;
    }

    public void EnableRagdoll()
    {
        if (animator != null)
        {
            animator.Update(0f);
            animator.enabled = false;
        }

        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (TryGetComponent<Collider>(out var mainCollider))
            mainCollider.enabled = false;

        Transform hips = animator?.GetBoneTransform(HumanBodyBones.Hips);
        if (hips != null)
        {
            transform.position = hips.position;
            transform.rotation = hips.rotation;
        }

        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (var col in ragdollColliders)
            col.enabled = true;

        Physics.SyncTransforms();

        // 🔻 Desactivar física del ragdoll tras un pequeño delay
        Invoke(nameof(DisableRagdollPhysics), 1.5f);
    }

    private void DisableRagdollPhysics()
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (var col in ragdollColliders)
            col.enabled = false;
    }

    public void DisableRagdoll()
    {
        // En caso de que se reactive manualmente desde el Pool
        CancelInvoke(nameof(DisableRagdollPhysics));

        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (var col in ragdollColliders)
            col.enabled = false;

        if (TryGetComponent<Collider>(out var mainCollider))
            mainCollider.enabled = true;
    }
}
