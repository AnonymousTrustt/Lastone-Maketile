using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundAudioManager : MonoBehaviour
{
    private static BackgroundAudioManager instance;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private string fallbackResourcesPath = "Audio/Stardew Valley OST Spring";
    [SerializeField, Range(0f, 1f)] private float volume = 0.18f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool persistBetweenScenes = true;
    [SerializeField] private bool logPlaybackStatus = true;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    public bool IsAudioOn => audioSource == null || !audioSource.mute;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateManagerIfMissing()
    {
        if (instance != null || FindFirstObjectByType<BackgroundAudioManager>() != null)
        {
            return;
        }

        GameObject gameManager = GameObject.Find("GameManager");
        GameObject audioObject = new GameObject("BackgroundAudioManager");

        if (gameManager != null)
        {
            audioObject.transform.SetParent(gameManager.transform);
        }

        audioObject.AddComponent<AudioSource>();
        audioObject.AddComponent<BackgroundAudioManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (persistBetweenScenes)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        LoadFallbackClipIfNeeded();
        ConfigureAudioSource();
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayMusic();
        }
    }

    private void OnValidate()
    {
        volume = Mathf.Clamp01(volume);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            ConfigureAudioSource();
        }
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void LoadFallbackClipIfNeeded()
    {
        if (musicClip != null || string.IsNullOrWhiteSpace(fallbackResourcesPath))
        {
            return;
        }

        musicClip = Resources.Load<AudioClip>(fallbackResourcesPath);
    }

    public void PlayMusic()
    {
        LoadFallbackClipIfNeeded();

        if (musicClip == null)
        {
            Debug.LogWarning($"BackgroundAudioManager has no music clip assigned and could not load Resources/{fallbackResourcesPath}.");
            return;
        }

        ConfigureAudioSource();

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (logPlaybackStatus)
        {
            Debug.Log($"Background music playing: {musicClip.name}, Volume: {audioSource.volume}, Loop: {audioSource.loop}");
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public bool ToggleAudio()
    {
        SetMuted(IsAudioOn);
        return IsAudioOn;
    }

    public void SetMuted(bool muted)
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            return;
        }

        audioSource.mute = muted;

        if (!muted && !audioSource.isPlaying)
        {
            PlayMusic();
        }
    }

    public void FadeOut(float duration)
    {
        StartFade(0f, duration, stopWhenComplete: true);
    }

    public void FadeIn(float duration)
    {
        if (musicClip != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        StartFade(volume, duration, stopWhenComplete: false);
    }

    private void StartFade(float targetVolume, float duration, bool stopWhenComplete)
    {
        if (audioSource == null)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(targetVolume, duration, stopWhenComplete));
    }

    private IEnumerator FadeRoutine(float targetVolume, float duration, bool stopWhenComplete)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (stopWhenComplete)
        {
            audioSource.Stop();
        }

        fadeCoroutine = null;
    }
}
