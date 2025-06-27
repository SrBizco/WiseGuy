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

    private GameObject currentVehicle;
    private bool isPossessingVehicle = false;

    private Vector3 lastPlayerPosition;

    void Start()
    {
        player = FindPlayerByLayer("Player");

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
        if (!isPossessingVehicle && Input.GetKeyDown(possessKey))
        {
            TryPossessVehicle();
        }
        else if (isPossessingVehicle && Input.GetKeyDown(releaseKey))
        {
            ExitVehicle();
        }
    }

    void TryPossessVehicle()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            GameObject hitObj = hit.collider.gameObject;

            if (hitObj.layer == LayerMask.NameToLayer("Vehicle"))
            {
                EnterVehicle(hitObj);
            }
            else
            {
                Debug.Log($"⚠️ {hitObj.name} no está en la capa 'Vehicle'. Layer: {LayerMask.LayerToName(hitObj.layer)}");
            }
        }
    }

    void EnterVehicle(GameObject vehicle)
    {
        Debug.Log($"🚗 Entrando al vehículo: {vehicle.name}");

        // Guardamos la posición del jugador antes de desactivarlo
        lastPlayerPosition = player.transform.position;
        player.SetActive(false);

        // Activamos el controller del vehículo solo ahora
        if (vehicle.GetComponent<CarController>() == null)
            vehicle.AddComponent<CarController>();

        // Reposicionamos la cámara al vehículo
        mainCamera.transform.SetParent(vehicle.transform);
        mainCamera.transform.localPosition = new Vector3(0, 2.5f, -6f); // Ajustá esto según tu modelo
        mainCamera.transform.localRotation = Quaternion.Euler(10f, 0, 0);

        currentVehicle = vehicle;
        isPossessingVehicle = true;
    }

    void ExitVehicle()
    {
        Debug.Log("🚶 Bajando del vehículo");

        if (currentVehicle != null)
        {
            // Desactivamos el controller del auto
            CarController car = currentVehicle.GetComponent<CarController>();
            if (car != null) Destroy(car);
        }

        // Restauramos la cámara
        mainCamera.transform.SetParent(player.transform);
        mainCamera.transform.localPosition = new Vector3(0, 1.5f, 0);
        mainCamera.transform.localRotation = Quaternion.identity;

        // Reactivamos al jugador
        player.transform.position = currentVehicle.transform.position + currentVehicle.transform.right * 2f; // Al lado del auto
        player.SetActive(true);

        currentVehicle = null;
        isPossessingVehicle = false;
    }

    GameObject FindPlayerByLayer(string layerName)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer(layerName))
                return obj;
        }
        return null;
    }
}
