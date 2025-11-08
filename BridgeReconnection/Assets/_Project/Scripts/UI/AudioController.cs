using UnityEngine;



public class AudioController : MonoBehaviour
{
    public AudioSource backgroundMusicSource;
    public AudioSource sfxSource;

    public AudioClip effectButtonClick;
    public AudioClip effectCoinCollect;
    public AudioSource carMovement;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        backgroundMusicSource.Play();
        backgroundMusicSource.loop = true;

    }

    public void ButtonPressed()
    {
        sfxSource.clip = effectButtonClick;
        sfxSource.Play();
    }

    public void ToggleMusic(bool isOn)
    {
        if (isOn)
        {
            backgroundMusicSource.UnPause();
        }
        else
        {
            backgroundMusicSource.Pause();
        }
    }

    public void ToggleSFX(bool isOn)
    {
        if (isOn)
        {
            sfxSource.UnPause();
        }
        else
        {
            sfxSource.Pause();
        }
    }

    public void SetMusicVolume(float volume)
    {
        backgroundMusicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public void CoinSfx()
    {
        // Aquí puedes agregar el código para reproducir el sonido de moneda
        sfxSource.clip = effectCoinCollect;
        sfxSource.Play();
    }
    public void CarMovementSfx()
    {
        carMovement.Play();

    }
    public void CarStopMovementSfx()
    {
        carMovement.Pause();
    }

}
