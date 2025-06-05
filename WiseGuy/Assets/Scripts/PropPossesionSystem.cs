using UnityEngine;

public class PropPossessionSystem : MonoBehaviour
{
    public Camera mainCamera;
    public float maxDistance = 5f;
    public KeyCode possessKey = KeyCode.E;
    public KeyCode releaseKey = KeyCode.R;

    private GameObject player;
    private CharacterController playerController;
    private PlayerMovement playerMovement;

    private GameObject currentProp;
    private bool isPossessing = false;

    void Start()
    {
        player = FindPlayerByLayer();

        if (player == null)
        {
            Debug.LogError("❌ No se encontró el jugador en la capa 'Player'");
            return;
        }

        playerController = player.GetComponent<CharacterController>();
        playerMovement = player.GetComponent<PlayerMovement>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("❌ No se encontró una cámara asignada ni una cámara principal (MainCamera).");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!isPossessing && Input.GetKeyDown(possessKey))
        {
            TryPossess();
        }
        else if (isPossessing && Input.GetKeyDown(releaseKey))
        {
            ReleasePossession();
        }
    }

    void TryPossess()
    {
        if (mainCamera == null)
        {
            Debug.LogError("❌ Cámara no asignada.");
            return;
        }

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.layer == LayerMask.NameToLayer("Prop"))
            {
                Possess(hitObj);
            }
            else
            {
                Debug.Log($"⚠️ {hitObj.name} no está en la capa 'Prop'. Layer: {LayerMask.LayerToName(hitObj.layer)}");
            }
        }
    }

    void Possess(GameObject prop)
    {
        Debug.Log($"🧿 Poseyendo {prop.name}");

        currentProp = prop;
        isPossessing = true;

        var movementComp = player.GetComponent<PlayerMovement>();
        if (movementComp != null) Destroy(movementComp);

        var controllerComp = player.GetComponent<CharacterController>();
        if (controllerComp != null) Destroy(controllerComp);

        if (currentProp.GetComponent<CharacterController>() == null)
            currentProp.AddComponent<CharacterController>();

        PlayerMovement pm = currentProp.AddComponent<PlayerMovement>();
        pm.playerCamera = mainCamera.transform;

        mainCamera.transform.SetParent(currentProp.transform);
        mainCamera.transform.localPosition = new Vector3(0, 1.5f, 0);
        mainCamera.transform.localRotation = Quaternion.identity;
    }

    void ReleasePossession()
    {
        Debug.Log("🔁 Liberando posesión");

        if (currentProp != null)
        {
            var pm = currentProp.GetComponent<PlayerMovement>();
            if (pm != null) Destroy(pm);

            var cc = currentProp.GetComponent<CharacterController>();
            if (cc != null) Destroy(cc);
        }

        isPossessing = false;
        currentProp = null;

        if (player.GetComponent<CharacterController>() == null)
            player.AddComponent<CharacterController>();

        PlayerMovement restored = player.AddComponent<PlayerMovement>();
        restored.playerCamera = mainCamera.transform;

        mainCamera.transform.SetParent(player.transform);
        mainCamera.transform.localPosition = new Vector3(0, 1.5f, 0);
        mainCamera.transform.localRotation = Quaternion.identity;
    }


    GameObject FindPlayerByLayer()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Player"))
                return obj;
        }
        return null;
    }
}
