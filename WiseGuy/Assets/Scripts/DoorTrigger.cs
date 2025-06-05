using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar o descargar (ej: Store, RacingCity)")]
    public string sceneToLoad;

    [Tooltip("Marcar como 'true' si esta puerta lleva de vuelta al exterior")]
    public bool isExit = false;

    [Tooltip("Layer numérico asignado al jugador (ej: 6 para PlayerLayer)")]
    public int playerLayer = 6;

    private bool playerNearby = false;

    void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("❌ SceneTransitionManager.Instance no está disponible.");
                return;
            }

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning("⚠️ sceneToLoad no está asignado en la puerta: " + gameObject.name);
                return;
            }

            if (isExit)
                SceneTransitionManager.Instance.ExitInterior(sceneToLoad);
            else
                SceneTransitionManager.Instance.EnterInterior(sceneToLoad);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            playerNearby = true;
            
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            playerNearby = false;
            
        }
    }
}
