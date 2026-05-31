using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource runLoopSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip sakuraTheme;
    [SerializeField] private AudioClip runLoop;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.45f;
    [Range(0f, 1f)][SerializeField] private float runVolume = 0.22f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.8f;

    [Header("Rules")]
    [SerializeField] private bool stopMusicOnGameOver = false;

    private bool runLoopPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupSources();

        // Yêu cầu mới:
        // Vào game không phát nhạc.
        // Chỉ khi người chơi click "CHẠM ĐỂ CHƠI" thì GameManager mới gọi PlayGameplayAudio().
        StopAllAudio();
    }

    private void SetupSources()
    {
        if (musicSource != null)
        {
            musicSource.clip = sakuraTheme;
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
            musicSource.spatialBlend = 0f;
        }

        if (runLoopSource != null)
        {
            runLoopSource.clip = runLoop;
            runLoopSource.loop = true;
            runLoopSource.playOnAwake = false;
            runLoopSource.volume = runVolume;
            runLoopSource.spatialBlend = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
            sfxSource.spatialBlend = 0f;
        }
    }

    public void PlayGameplayAudio()
    {
        PlayMusic();
        PlayRunLoop();
    }

    public void StopAllAudio()
    {
        StopMusic();
        StopRunLoop();

        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    public void PlayMusic()
    {
        if (musicSource == null || sakuraTheme == null) return;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null) return;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.UnPause();
        }
    }

    public void PlayRunLoop()
    {
        if (runLoopSource == null || runLoop == null) return;
        if (runLoopPlaying && runLoopSource.isPlaying) return;

        runLoopSource.Play();
        runLoopPlaying = true;
    }

    public void StopRunLoop()
    {
        if (runLoopSource == null) return;

        runLoopSource.Stop();
        runLoopPlaying = false;
    }

    public void PauseRunLoop()
    {
        if (runLoopSource == null) return;
        runLoopSource.Pause();
    }

    public void ResumeRunLoop()
    {
        if (runLoopSource == null) return;

        if (runLoopPlaying)
        {
            runLoopSource.UnPause();
        }
    }

    public void PlayJump()
    {
        PlaySfx(jumpClip);
    }

    public void PlayHit()
    {
        StopRunLoop();

        if (stopMusicOnGameOver)
        {
            StopMusic();
        }

        PlaySfx(hitClip);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip);
    }

    public void PauseAllGameplayAudio()
    {
        PauseMusic();
        PauseRunLoop();
    }

    public void ResumeAllGameplayAudio()
    {
        ResumeMusic();

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.Playing)
        {
            ResumeRunLoop();
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}