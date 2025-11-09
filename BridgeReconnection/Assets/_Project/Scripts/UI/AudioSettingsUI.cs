using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    private string backScene = "Niveles";

    [Header("AudioMixer exposed parameters")]
    [SerializeField] private string musicParam = "Master"; // set this to your exposed music parameter name
    [SerializeField] private string soundParam = "Sound"; // set this to your exposed SFX parameter name

    public void ControlMusic(float volume)
    {
        if (audioMixer == null) return;
        if (!HasParam(musicParam)) { Debug.LogError($"AudioMixer param not found: {musicParam}"); return; }
        audioMixer.SetFloat(musicParam, LinearToDb(Mathf.Clamp01(volume)));
       
    }

    public void ControlSound(float volume)
    {
        if (audioMixer == null) return;
        if (!HasParam(soundParam)) { Debug.LogError($"AudioMixer param not found: {soundParam}"); return; }
        audioMixer.SetFloat(soundParam, LinearToDb(Mathf.Clamp01(volume)));
        AudioController.instance.ButtonPressed();
    }

    private static float LinearToDb(float v)
    {
        // Prevent -Infinity when v ==0
        return (v <=0.0001f) ? -80f : Mathf.Log10(v) *20f;
    }

    private bool HasParam(string param)
    {
        if (string.IsNullOrEmpty(param)) return false;
        float _;
        return audioMixer.GetFloat(param, out _);
    }

    public void GoBack()
    {
        if (!string.IsNullOrEmpty(backScene))
        {
            SceneManager.LoadScene(backScene);
        }
    }
}
