using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsSelector : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public Button button; // Botón clickeable del nivel
        public Outline outline; // Borde visual del nivel seleccionado
        public string sceneName; // Nombre de la escena a cargar
    }

    [Header("Config")]
    [SerializeField] private List<LevelEntry> levels = new List<LevelEntry>();
    [SerializeField] private Button playButton;
    [Tooltip("Índice seleccionado por defecto (-1 para ninguno)")]
    [SerializeField] private int defaultSelectedIndex = -1;

    [Header("Restricciones")]
    [Tooltip("Si está activo, solo se puede seleccionar el Nivel1 (índice0) al inicio si no hay progreso")]
    [SerializeField] private bool onlyLevel1SelectableOnStart = true;

    private int _selectedIndex = -1;

    private void Awake()
    {
        // Asegurar gestor de progreso
        LevelProgressManager.GetOrCreate();

        // Registrar listeners para cada nivel y desactivar todos los bordes
        if (levels != null)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                int idx = i; // capturar índice correctamente
                var entry = levels[i];
                if (entry == null) continue;

                if (entry.button != null)
                {
                    // Asegura referencia a Outline si no se asignó
                    if (entry.outline == null)
                        entry.outline = entry.button.GetComponent<Outline>();

                    entry.button.onClick.AddListener(() => OnLevelClicked(idx));
                }

                if (entry.outline != null)
                    entry.outline.enabled = false; // ocultar al inicio
            }
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        // Aplicar locks según progreso
        ApplyLocksFromProgress();

        // Selección por defecto (opcional)
        if (defaultSelectedIndex >= 0 && levels != null && defaultSelectedIndex < levels.Count)
        {
            Select(defaultSelectedIndex);
        }
        else
        {
            UpdatePlayButtonInteractable();
        }
    }

    private void OnDestroy()
    {
        if (levels != null)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                var b = levels[i]?.button;
                if (b != null) b.onClick.RemoveAllListeners();
            }
        }
        if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void ApplyLocksFromProgress()
    {
        if (levels == null) return;
        var progress = LevelProgressManager.Instance;
        bool restrict = onlyLevel1SelectableOnStart && progress != null;
        for (int i = 0; i < levels.Count; i++)
        {
            var btn = levels[i]?.button;
            if (btn == null) continue;
            string sceneName = levels[i].sceneName;
            bool unlocked = progress != null && progress.IsUnlocked(sceneName);
            if (unlocked)
            {
                btn.interactable = true;
            }
            else
            {
                // Si restrict activo y no desbloqueado, bloquear (salvo el índice0 que puede estar siempre desbloqueado si es primer nivel)
                btn.interactable = (i == 0);
            }
        }
        // Forzar defaultSelectedIndex al primer nivel disponible si el actual está bloqueado
        if (defaultSelectedIndex < 0 || defaultSelectedIndex >= levels.Count || !levels[defaultSelectedIndex].button.interactable)
        {
            defaultSelectedIndex = 0; // primer desbloqueado seguro
        }
    }

    private void OnLevelClicked(int index)
    {
        // Ignorar si botón bloqueado
        if (levels == null || index < 0 || index >= levels.Count) return;
        var btn = levels[index]?.button;
        if (btn != null && !btn.interactable) return;

        AudioController.instance.ButtonPressed();
        Select(index);
    }

    public void Select(int index)
    {
        if (levels == null || index < 0 || index >= levels.Count)
        {
            _selectedIndex = -1;
            UpdateOutlines();
            UpdatePlayButtonInteractable();
            return;
        }
        _selectedIndex = index;
        UpdateOutlines();
        UpdatePlayButtonInteractable();
    }

    private void UpdateOutlines()
    {
        if (levels == null) return;
        for (int i = 0; i < levels.Count; i++)
        {
            var outline = levels[i]?.outline;
            if (outline == null && levels[i]?.button != null)
                outline = levels[i].button.GetComponent<Outline>();

            if (outline != null)
                outline.enabled = (i == _selectedIndex);
        }
    }

    private void UpdatePlayButtonInteractable()
    {
        if (playButton == null) return;
        bool valid = levels != null && _selectedIndex >= 0 && _selectedIndex < levels.Count && levels[_selectedIndex].button != null && levels[_selectedIndex].button.interactable;
        playButton.interactable = valid;
    }

    private void OnPlayClicked()
    {
        if (levels == null || _selectedIndex < 0 || _selectedIndex >= levels.Count)
        {
            Debug.LogWarning("LevelsSelector: No hay nivel seleccionado.");
            return;
        }
        var entry = levels[_selectedIndex];
        if (entry == null || entry.button == null || !entry.button.interactable)
        {
            Debug.LogWarning("LevelsSelector: Nivel bloqueado.");
            return;
        }
        string sceneName = entry.sceneName;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LevelsSelector: Nivel sin nombre de escena.");
            return;
        }
        AudioController.instance.ButtonPressed();
        AudioController.instance.StopMenuMusic();
        AudioController.instance.BackgroundLevelMusic();
        SceneManager.LoadScene(sceneName);
    }

    // Llamar desde código al completar un nivel para desbloquear el siguiente
    public static void RegisterLevelCompleted(string sceneName)
    {
        LevelProgressManager.GetOrCreate().Unlock(sceneName); // asegura actual escena
        LevelProgressManager.Instance.UnlockNext(sceneName); // desbloquea siguiente
    }
}
