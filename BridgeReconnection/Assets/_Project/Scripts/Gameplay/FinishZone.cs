using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class FinishZone : MonoBehaviour
{
 [Header("Goal Settings")] public int requiredCoins =3;
 public bool stopCarOnEnter = true;
 [Tooltip("Si es true, el collider se usará como trigger")] public bool forceTrigger = true;

 [Header("UI - Panels")] public GameObject completePanel; // Panel de éxito
 public GameObject incompletePanel; // Panel de nivel incompleto (reutiliza el de FallZone)

 [Header("UI Texts")] 
 public TMP_Text completeMessageTMP; // Texto solo para el panel de completo
 public TMP_Text messageTMP; // Texto general (opcional, no se usa para incompleto)
 public string successMessage = "MONEDAS RECOLECTADAS: {0}";
 public string missingCoinsMessage = "MONEDAS FALTANTES: {0}";

 [Header("UI Elements to Hide on Finish")] 
 public GameObject CoinCounterUI; // contador de monedas
 public GameObject CreationButtonsUI; // botones de creación

 [Header("Finish Buttons")] 
 public Button restartButton; 
 public Button nextLevelButton; 
 public Button levelsButton;
 public string nextLevelName; 
 public string levelMenu = "Niveles";

 private bool _success;
 private string _currentScene;

 private void Reset()
 {
 var col = GetComponent<Collider>();
 if (col) col.isTrigger = true;
 }

 private void Awake()
 {
 if (forceTrigger)
 {
 var col = GetComponent<Collider>();
 if (col) col.isTrigger = true;
 }

 // Registrar callbacks si hay botones asignados
 if (restartButton != null)
 {
 restartButton.onClick.RemoveAllListeners();
 restartButton.onClick.AddListener(RestartLevel);
 }
 if (nextLevelButton != null)
 {
 nextLevelButton.onClick.RemoveAllListeners();
 nextLevelButton.onClick.AddListener(LoadNextLevel);
 }

 if (levelsButton != null)
 {
 levelsButton.onClick.RemoveAllListeners();
 levelsButton.onClick.AddListener(LoadLevelsMenu);
 }
 _currentScene = SceneManager.GetActiveScene().name;
 }

 private void OnTriggerEnter(Collider other)
 {
 var car = other.GetComponentInParent<car>();
 if (car == null) return;

 if (stopCarOnEnter) car.StopImmediately();

 int current = CoinManager.Instance ? CoinManager.Instance.Coins :0;
 int missing = Mathf.Max(0, requiredCoins - current);
 _success = (missing ==0);
 AudioController.instance.CarStopMovementSfx();
 // Pasa el texto ya formateado; solo se mostrará en el panel de completo
 string msg = _success ? string.Format(successMessage, current) : string.Format(missingCoinsMessage, missing);
 ShowFinishUI(_success, msg);
 }

 private void ShowFinishUI(bool success, string text)
 {
 if (CoinCounterUI) CoinCounterUI.SetActive(false);
 if (CreationButtonsUI) CreationButtonsUI.SetActive(false);

 // Mostrar panel según resultado
 if (success)
 {
 if (incompletePanel) incompletePanel.SetActive(false);
 if (completePanel) completePanel.SetActive(true);
 // Mostrar mensaje solo en panel de completo
 if (completeMessageTMP) completeMessageTMP.text = text;
 AudioController.instance.WinSound();
 // Registrar progreso
 LevelsSelector.RegisterLevelCompleted(_currentScene);
 }
 else
 {
 if (completePanel) completePanel.SetActive(false);
 if (incompletePanel) incompletePanel.SetActive(true);
 // No mostrar mensaje de éxito en panel incompleto
 if (completeMessageTMP) completeMessageTMP.text = string.Empty;
 AudioController.instance.DeathSound();
 }

 // Botones
 if (restartButton) restartButton.gameObject.SetActive(true); // siempre disponible
 bool canGoNext = success && !string.IsNullOrEmpty(nextLevelName) && nextLevelButton != null;
 if (nextLevelButton) nextLevelButton.gameObject.SetActive(canGoNext);
 }

 public void RestartLevel()
 {
 AudioController.instance.ButtonPressed();
 UIManager.SkipObjectiveOnce = true; // evitar objetivo en reload
 var scene = SceneManager.GetActiveScene();
 SceneManager.LoadScene(scene.name);
 }

 public void LoadNextLevel()
 {
 AudioController.instance.ButtonPressed();
 if (string.IsNullOrEmpty(nextLevelName)) return;
 SceneManager.LoadScene(nextLevelName);
 }

 public void LoadLevelsMenu()
 {
 AudioController.instance.ButtonPressed();
 AudioController.instance.StopLevelMusic();
 AudioController.instance.BackgroundMenuMusic();
 if (string.IsNullOrEmpty(levelMenu)) return;
 SceneManager.LoadScene(levelMenu);
 }
}
