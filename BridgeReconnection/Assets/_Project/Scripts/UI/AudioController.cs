using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Music")]
    public AudioSource backgroundMenuMusic;
    public AudioSource backgroundLevelMusic;

    [Header("SFX")]
    public AudioSource buttonClickSfx;
    public AudioSource coinCollectSfx;
    public AudioSource deathSfx;
    public AudioSource winSfx;
    public AudioSource barCreatorSfx;
    public AudioSource carMovementLoop;

    public static AudioController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayLoop(backgroundMenuMusic); // música inicial (menú)
    }

    // Helpers -----------------------------------------------------------------
    private void Play(AudioSource src)
    {
        if (src == null) return;
        src.loop = false;
        src.Play();
    }

    private void PlayLoop(AudioSource src)
    {
        if (src == null) return;
        src.loop = true;
        if (!src.isPlaying) src.Play();
    }

    private void Stop(AudioSource src)
    {
        if (src == null) return;
        src.Stop();
    }

    private void Pause(AudioSource src)
    {
        if (src == null) return;
        src.Pause();
    }

    // UI / Gameplay SFX --------------------------------------------------------
    public void ButtonPressed() => Play(buttonClickSfx);
    public void CoinSfx() => Play(coinCollectSfx);
    public void DeathSound() => Play(deathSfx);
    public void WinSound() => Play(winSfx);
    public void BarCreatorSound() => Play(barCreatorSfx);

    // Car movement -------------------------------------------------------------
    public void CarMovementSfx() => PlayLoop(carMovementLoop);
    public void CarStopMovementSfx() => Pause(carMovementLoop);

    // Music control ------------------------------------------------------------
    public void BackgroundMenuMusic() => PlayLoop(backgroundMenuMusic);
    public void StopMenuMusic() => Stop(backgroundMenuMusic);

    public void BackgroundLevelMusic() => PlayLoop(backgroundLevelMusic);
    public void StopLevelMusic() => Stop(backgroundLevelMusic);
}
