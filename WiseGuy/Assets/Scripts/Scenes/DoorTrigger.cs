using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
   
    public string sceneToLoad;

   
    public bool isExit = false;

    
    public int playerLayer = 6;

    private bool playerNearby = false;

    void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("SceneTransitionManager.Instance no está disponible.");
                return;
            }

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning(" sceneToLoad no está asignado en la puerta: " + gameObject.name);
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
