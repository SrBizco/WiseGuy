using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX Clips")]
    public AudioClip meleeClip;
    public AudioClip shootClip;
    public AudioClip engineStartClip;
    public AudioClip engineLoopClip;

    private AudioSource sfxSource;
    private AudioSource engineSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        engineSource = gameObject.AddComponent<AudioSource>();
        engineSource.loop = true;
        engineSource.playOnAwake = false;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayMelee() => PlaySFX(meleeClip);
    public void PlayShoot() => PlaySFX(shootClip);

    // 🚗 Audio de vehículo
    public void PlayEngineStart(System.Action onComplete = null)
    {
        if (engineStartClip == null)
        {
            onComplete?.Invoke();
            return;
        }

        engineSource.loop = false;
        engineSource.clip = engineStartClip;
        engineSource.Play();

        // Llamar a onComplete cuando termine
        Instance.StartCoroutine(InvokeAfterDelay(engineStartClip.length, onComplete));
    }

    public void PlayEngineLoop()
    {
        if (engineLoopClip != null)
        {
            engineSource.clip = engineLoopClip;
            engineSource.loop = true;
            engineSource.Play();
        }
    }

    public void StopEngineLoop()
    {
        engineSource.Stop();
    }

    private static System.Collections.IEnumerator InvokeAfterDelay(float delay, System.Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
