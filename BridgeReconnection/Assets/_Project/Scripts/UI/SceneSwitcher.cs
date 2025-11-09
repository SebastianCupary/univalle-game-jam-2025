using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Botón Inicio")] public Button inicioButton; public string inicioScene;
    [Header("Botón Ajustes")] public Button ajustesButton; public string ajustesScene;
    [Header("Botón Salir")] public Button salirButton; // Cierra la aplicación

    private void Awake()
    {
        if (inicioButton != null) inicioButton.onClick.AddListener(() => LoadScene(inicioScene));
        if (ajustesButton != null) ajustesButton.onClick.AddListener(() => LoadScene(ajustesScene));
        if (salirButton != null) salirButton.onClick.AddListener(QuitGame);
    }

    private void OnDestroy()
    {
        if (inicioButton != null) inicioButton.onClick.RemoveAllListeners();
        if (ajustesButton != null) ajustesButton.onClick.RemoveAllListeners();
        if (salirButton != null) salirButton.onClick.RemoveAllListeners();
    }

    private void LoadScene(string sceneName)
    {
        AudioController.instance.ButtonPressed();
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneSwitcher: Nombre de escena vacío.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    private void QuitGame()
    {
        AudioController.instance.ButtonPressed();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Detiene modo Play en Editor
        #else
        Application.Quit();
        #endif
    }
    public void credits()
    {
               AudioController.instance.ButtonPressed();
        SceneManager.LoadScene("Creditos");
    }
}
