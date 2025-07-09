using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Nombre de la escena Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Update()
    {
        // Verificar que NO estemos en el Main Menu
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);

        // Descargar todas las escenas activas excepto MainMenu
        Scene activeScene = SceneManager.GetActiveScene();
        SceneTransitionManager.Instance.StartCoroutine(ReturnToMenuRoutine(activeScene.name));
    }

    private System.Collections.IEnumerator ReturnToMenuRoutine(string gameplaySceneName)
    {
        // Descargamos la escena de gameplay
        yield return SceneManager.UnloadSceneAsync(gameplaySceneName);

        // También podés descargar Global si querés
        if (SceneManager.GetSceneByName("Global").isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync("Global");
        }

        // Cargar MainMenu
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    public void OnSettingsPressed()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackFromSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }
}
