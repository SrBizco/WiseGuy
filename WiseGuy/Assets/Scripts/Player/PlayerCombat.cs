using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Settings")]
    public float meleeRange = 2f;
    public int meleeDamage = 10;
    public float meleeCooldown = 0.5f;
    private float lastMeleeTime = -Mathf.Infinity;

    [Header("Ranged Settings")]
    public float shootRange = 100f;
    public int bulletDamage = 25;

    [Header("Referencias")]
    public Camera playerCamera;
    public GameObject pistolObject;
    private Transform weaponMuzzleTransform;

    private bool hasWeaponEquipped = false;

    void Start()
    {
        if (pistolObject != null)
        {
            Transform[] allChildren = pistolObject.GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                if (t.name == "MuzzlePoint")
                {
                    weaponMuzzleTransform = t;
                    break;
                }
            }

            if (weaponMuzzleTransform == null)
                Debug.LogError("❌ No se encontró un hijo llamado 'MuzzlePoint' dentro de " + pistolObject.name);
        }
        else
        {
            Debug.LogError("❌ No se asignó 'pistolObject' en el inspector.");
        }
    }
    void Update()
    {
        HandleWeaponToggle();

        if (Input.GetButtonDown("Fire1"))
        {
            if (hasWeaponEquipped)
                TryShoot();
            else
                TryMelee();
        }
    }

    void HandleWeaponToggle()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            hasWeaponEquipped = !hasWeaponEquipped;
            pistolObject.SetActive(hasWeaponEquipped);
        }
    }

    void TryMelee()
    {
        if (Time.time - lastMeleeTime < meleeCooldown)
            return;

        lastMeleeTime = Time.time;

        AudioManager.Instance?.PlayMelee(); // 🔊

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, meleeRange))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(meleeDamage);
                Debug.Log($"👊 Melee hit: {hit.collider.name}");
            }
        }
        else
        {
            Debug.Log("👊 Melee swing - al aire");
        }
    }

    void TryShoot()
    {
        AudioManager.Instance?.PlayShoot();
        VFXManager.Instance?.PlayMuzzleFlash(weaponMuzzleTransform);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootRange))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(bulletDamage);
            }

            VFXManager.Instance?.PlayHitEffect(hit.point, hit.normal);
        }
    }
}