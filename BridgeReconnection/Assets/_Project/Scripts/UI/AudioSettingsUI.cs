using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Control simple de audio para escena de Ajustes.
// Requiere un AudioMixer con parámetros expuestos: "MusicVolume" y "SFXVolume" (en dB).
// Los sliders deben estar en rango [0..1].
public class AudioSettingsUI : MonoBehaviour
{
 [Header("Mixer (parámetros: MusicVolume, SFXVolume)")]
 public AudioMixer mixer;

 [Header("UI - Música")]
 public Slider musicSlider; //0..1
 public Toggle musicMuteToggle; // opcional

 [Header("UI - Sonidos")]
 public Slider sfxSlider; //0..1
 public Toggle sfxMuteToggle; // opcional

 [Header("Volver")]
 public Button backButton;
 public string backScene; // escena a la que se vuelve

 const string MusicParam = "MusicVolume";
 const string SfxParam = "SFXVolume";
 const string PrefMusic = "prefs.audio.music";
 const string PrefSfx = "prefs.audio.sfx";

 private float _lastMusicBeforeMute =1f;
 private float _lastSfxBeforeMute =1f;

 private void Awake()
 {
 // Cargar valores guardados
 float music = PlayerPrefs.HasKey(PrefMusic) ? PlayerPrefs.GetFloat(PrefMusic,1f) :1f;
 float sfx = PlayerPrefs.HasKey(PrefSfx) ? PlayerPrefs.GetFloat(PrefSfx,1f) :1f;

 if (musicSlider != null)
 {
 musicSlider.SetValueWithoutNotify(Mathf.Clamp01(music));
 musicSlider.onValueChanged.AddListener(OnMusicChanged);
 }
 if (sfxSlider != null)
 {
 sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(sfx));
 sfxSlider.onValueChanged.AddListener(OnSfxChanged);
 }
 if (musicMuteToggle != null)
 {
 musicMuteToggle.onValueChanged.AddListener(OnMusicMuteChanged);
 musicMuteToggle.SetIsOnWithoutNotify(music <=0.0001f);
 }
 if (sfxMuteToggle != null)
 {
 sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteChanged);
 sfxMuteToggle.SetIsOnWithoutNotify(sfx <=0.0001f);
 }
 if (backButton != null)
 {
 backButton.onClick.AddListener(GoBack);
 }

 // Aplicar al mixer al iniciar
 ApplyVolume(MusicParam, music);
 ApplyVolume(SfxParam, sfx);
 }

 private void OnDestroy()
 {
 if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
 if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
 if (musicMuteToggle != null) musicMuteToggle.onValueChanged.RemoveListener(OnMusicMuteChanged);
 if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.RemoveListener(OnSfxMuteChanged);
 if (backButton != null) backButton.onClick.RemoveListener(GoBack);
 }

 private void OnMusicChanged(float value)
 {
 value = Mathf.Clamp01(value);
 ApplyVolume(MusicParam, value);
 PlayerPrefs.SetFloat(PrefMusic, value);
 PlayerPrefs.Save();
 if (musicMuteToggle != null) musicMuteToggle.SetIsOnWithoutNotify(value <=0.0001f);
 if (value >0.0001f) _lastMusicBeforeMute = value;
 }

 private void OnSfxChanged(float value)
 {
 value = Mathf.Clamp01(value);
 ApplyVolume(SfxParam, value);
 PlayerPrefs.SetFloat(PrefSfx, value);
 PlayerPrefs.Save();
 if (sfxMuteToggle != null) sfxMuteToggle.SetIsOnWithoutNotify(value <=0.0001f);
 if (value >0.0001f) _lastSfxBeforeMute = value;
 }

 private void OnMusicMuteChanged(bool mute)
 {
 if (musicSlider == null) return;
 if (mute)
 {
 _lastMusicBeforeMute = Mathf.Max(_lastMusicBeforeMute, musicSlider.value);
 musicSlider.SetValueWithoutNotify(0f);
 OnMusicChanged(0f);
 }
 else
 {
 float restore = (_lastMusicBeforeMute <=0f) ?1f : _lastMusicBeforeMute;
 musicSlider.SetValueWithoutNotify(restore);
 OnMusicChanged(restore);
 }
 }

 private void OnSfxMuteChanged(bool mute)
 {
 if (sfxSlider == null) return;
 if (mute)
 {
 _lastSfxBeforeMute = Mathf.Max(_lastSfxBeforeMute, sfxSlider.value);
 sfxSlider.SetValueWithoutNotify(0f);
 OnSfxChanged(0f);
 }
 else
 {
 float restore = (_lastSfxBeforeMute <=0f) ?1f : _lastSfxBeforeMute;
 sfxSlider.SetValueWithoutNotify(restore);
 OnSfxChanged(restore);
 }
 }

 private void ApplyVolume(string parameter, float linear)
 {
 if (mixer == null) return;
 float dB = (linear <=0.0001f) ? -80f : Mathf.Log10(linear) *20f;
 mixer.SetFloat(parameter, dB);
 }

 private void GoBack()
 {
 if (!string.IsNullOrEmpty(backScene))
 {
 SceneManager.LoadScene(backScene);
 }
 }
}
