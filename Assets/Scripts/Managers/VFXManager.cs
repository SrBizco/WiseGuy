using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    [Header("Prefabs de efectos")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayMuzzleFlash(Transform origin)
    {
        if (muzzleFlashPrefab == null) return;

        // ✅ Instanciar como hijo del muzzle
        GameObject fx = Instantiate(muzzleFlashPrefab, origin.position, origin.rotation, origin);

        // ✅ Escalado (por si el prefab no lo tiene)
        fx.transform.localScale = Vector3.one * 0.1f;

        Destroy(fx, 2f);
    }

    public void PlayHitEffect(Vector3 position, Vector3 normal)
    {
        if (hitEffectPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(normal);
        GameObject fx = Instantiate(hitEffectPrefab, position, rot);
        Destroy(fx, 2f);
    }
}
