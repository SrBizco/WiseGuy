using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public GameObject loadingScreen;
    public Slider loadingBar;

    [Tooltip("Layer asignado al jugador (ej: Player = 6)")]
    public int playerLayer = 6;

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private GameObject player;

    private string currentScene = "";
    private string sceneToLoadNext = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartCoroutine(LoadInitialScene());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator LoadInitialScene()
    {
        Debug.Log("🔷 [Init] Cargando escena inicial: RacingCity");
        sceneToLoadNext = "RacingCity";
        yield return StartCoroutine(SwapSceneRoutine(sceneToLoadNext));
    }

    public void EnterInterior(string sceneName)
    {
        player = FindPlayerByLayer();
        if (player == null)
        {
            Debug.LogError("⚠️ No se encontró el jugador.");
            return;
        }

        savedPosition = player.transform.position;
        savedRotation = player.transform.rotation;
        sceneToLoadNext = sceneName;

        Debug.Log($"🟡 [EnterInterior] Entrando a escena: {sceneName}");
        StartCoroutine(SwapSceneRoutine(sceneName));
    }

    public void ExitInterior(string backToScene)
    {
        sceneToLoadNext = backToScene;
        Debug.Log($"🔙 [ExitInterior] Saliendo a escena: {backToScene}");
        StartCoroutine(SwapSceneRoutine(backToScene, true));
    }

    private IEnumerator SwapSceneRoutine(string newSceneName, bool returning = false)
    {
        loadingScreen.SetActive(true);
        Debug.Log($"⚙️ [Transition] Comenzando transición a: {newSceneName}");

        if (!string.IsNullOrEmpty(currentScene))
        {
            Debug.Log($"♻️ [Unload] Descargando escena actual: {currentScene}");
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentScene);
            while (!unload.isDone)
            {
                loadingBar.value = Mathf.Clamp01(unload.progress / 0.9f);
                yield return null;
            }
            Debug.Log("✅ [Unload] Escena descargada.");
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        Debug.Log($"⏳ [Load] Cargando escena: {newSceneName}");
        while (!load.isDone)
        {
            loadingBar.value = Mathf.Clamp01(load.progress / 0.9f);
            yield return null;
        }

        Scene loadedScene;
        while (true)
        {
            loadedScene = SceneManager.GetSceneByName(newSceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
                break;
            yield return null;
        }

        SceneManager.SetActiveScene(loadedScene);
        Debug.Log($"✅ [Scene] Escena marcada como activa: {newSceneName}");

        player = FindPlayerByLayer();
        if (player != null)
        {
            Debug.Log("👤 [FindPlayer] Player encontrado: " + player.name);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            if (returning)
            {
                Debug.Log("↩️ [Restore] Restaurando posición anterior...");
                player.transform.SetPositionAndRotation(savedPosition, savedRotation);
            }
            else
            {
                Debug.Log("📍 [Spawn] Buscando punto de aparición...");
                Transform spawn = FindSpawnPointInScene(loadedScene);
                if (spawn != null)
                {
                    Debug.Log($"✅ [SpawnPoint] SpawnPoint encontrado: {spawn.name} @ {spawn.position}");
                    player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
                }
                else
                {
                    Debug.LogWarning("⚠️ [SpawnPoint] SpawnPoint no encontrado en escena.");
                }
            }

            yield return null;

            if (!player.activeSelf)
            {
                Debug.Log("🔛 [Player] Activando player...");
                player.SetActive(true);
            }

            if (cc) cc.enabled = true;
            Debug.Log("✅ [Player] Posicionado correctamente.");
        }

        currentScene = newSceneName;
        Debug.Log($"✅ [Transition] Finalizada transición. Escena activa: {currentScene}");

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        loadingScreen.SetActive(false);
        Debug.Log("🟢 [UI] Pantalla de carga oculta.");
    }

    private GameObject FindPlayerByLayer()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == playerLayer)
                return obj;
        }
        return null;
    }

    private Transform FindSpawnPointInScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in children)
            {
                if (t.name == "SpawnPoint")
                    return t;
            }
        }
        return null;
    }
}

