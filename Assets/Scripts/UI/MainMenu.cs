using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Nombre de la escena global")]
    [SerializeField] private string globalSceneName = "Global";

    public void OnPlayPressed()
    {
        StartCoroutine(LoadGlobalScene());
    }

    private IEnumerator LoadGlobalScene()
    {
        if (!SceneManager.GetSceneByName(globalSceneName).isLoaded)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(globalSceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
                yield return null;
        }

        Debug.Log("✅ Escena global cargada. La escena global ahora se encargará de cargar la jugable automáticamente.");

        // 👉 Esperar un frame extra para asegurar inicialización
        yield return null;

        // 👉 Descargar escena actual (MainMenu)
        Scene currentScene = SceneManager.GetActiveScene();
        Scene globalScene = SceneManager.GetSceneByName(globalSceneName);

        if (globalScene.IsValid())
        {
            SceneManager.SetActiveScene(globalScene);
            Debug.Log($"🎬 Escena activa cambiada a: {globalScene.name}");
        }

        yield return SceneManager.UnloadSceneAsync(currentScene);
        Debug.Log($"❌ Escena descargada: {currentScene.name}");
    }

    public void OnControlsPressed()
    {
        mainMenuPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void OnCreditsPressed()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void OnBackToMenuPressed()
    {
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnSettingsPressed()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnExitPressed()
    {
        Application.Quit();
        Debug.Log("Salir del juego");
    }
}
