using UnityEngine;
using UnityEngine.AI;

public class PoliceController : CivilianController
{
    [Header("Policía")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float shootRange = 15f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private GameObject bulletEffectPrefab;

    private float lastShotTime = -Mathf.Infinity;
    private Transform player;

    protected override void Start()
    {
        base.Start();
        player = FindPlayerByLayer("Player");

        if (weaponObject != null)
        {
            weaponObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        if (PoliceManager.Instance.IsPlayerWanted)
        {
            agent.speed = runSpeed;

            if (weaponObject != null && !weaponObject.activeSelf)
            {
                weaponObject.SetActive(true);
            }
        }
        else
        {
            agent.speed = walkSpeed;

            if (weaponObject != null && weaponObject.activeSelf)
            {
                weaponObject.SetActive(false);
            }
        }

        base.Update();

        if (!PoliceManager.Instance.IsPlayerWanted) return;
        if (player == null) return;

        HandlePursuitAndShooting();
    }

    protected override void UpdateAnimation()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float hor = localVelocity.x;

        bool isMoving = agent.velocity.magnitude > 0.1f;
        float vertValue = 0f;

        if (!isMoving)
        {
            vertValue = 0f; // Idle
        }
        else if (PoliceManager.Instance != null && PoliceManager.Instance.IsPlayerWanted)
        {
            vertValue = 1f; // Run
        }
        else
        {
            vertValue = 0.5f; // Walk
        }

        animator.SetFloat("Hor", hor);
        animator.SetFloat("Vert", vertValue);
    }

    private void HandlePursuitAndShooting()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            agent.SetDestination(player.position);

            if (distance <= shootRange && Time.time - lastShotTime >= fireRate)
            {
                ShootAtPlayer();
                lastShotTime = Time.time;
            }
        }
    }

    private void ShootAtPlayer()
    {
        Debug.Log($"{gameObject.name} dispara al jugador!");

        Vector3 dir = (player.position - transform.position).normalized;
        Ray ray = new Ray(transform.position + Vector3.up, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                IDamageable dmg = hit.collider.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(damage);
                }
            }

            if (bulletEffectPrefab != null)
            {
                Instantiate(bulletEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        // 🔊 Sonido al disparar
        AudioManager.Instance.PlayShoot();

        // 💥 VFX en arma
        if (weaponObject != null)
        {
            VFXManager.Instance.PlayMuzzleFlash(weaponObject.transform);
        }
    }

    private Transform FindPlayerByLayer(string layerName)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer(layerName))
                return obj.transform;
        }
        return null;
    }
}
