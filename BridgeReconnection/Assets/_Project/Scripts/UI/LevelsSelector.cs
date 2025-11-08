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

    private int _selectedIndex = -1;

    private void Awake()
    {
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

        // Selección por defecto (opcional)
        if (defaultSelectedIndex >= 0 && levels != null && defaultSelectedIndex < levels.Count)
        {
            Select(defaultSelectedIndex);
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

    private void OnLevelClicked(int index)
    {
        AudioController.instance.ButtonPressed();
        Select(index);
    }

    public void Select(int index)
    {
        if (levels == null || index < 0 || index >= levels.Count)
        {
            _selectedIndex = -1;
            UpdateOutlines();
            return;
        }
        _selectedIndex = index;
        UpdateOutlines();
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

    private void OnPlayClicked()
    {
        if (levels == null || _selectedIndex < 0 || _selectedIndex >= levels.Count)
        {
            Debug.LogWarning("LevelsSelector: No hay nivel seleccionado.");
            return;
        }

        string sceneName = levels[_selectedIndex]?.sceneName;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("LevelsSelector: El nivel seleccionado no tiene nombre de escena configurado.");
            return;
        }

        AudioController.instance.ButtonPressed();
        AudioController.instance.StopMenuMusic();
        AudioController.instance.BackgroundLevelMusic();

        SceneManager.LoadScene(sceneName);
    }
}
