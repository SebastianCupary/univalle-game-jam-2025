using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System; // para Serializable
using System.Collections.Generic;

[Serializable]
public struct LevelObjective
{
    public string sceneName; // nombre exacto de la escena
    public Sprite objectiveSprite; // imagen a mostrar
    public string objectiveText; // opcional (si tienes un TMP/Text para mostrar)
}

public class UIManager : MonoBehaviour
{
    public Button RoadButton;
    public Button WoodButton;
    public BarCreator barCreator;
    public GameManager gameManager;
    public BudgetManager budgetManager;

    [Header("Objective Popup")] public GameObject objectivePanel; // panel / imagen objetivo al iniciar nivel
    public Image objectiveImage; // asigna el Image hijo del panel
    [TextArea] public string fallbackObjectiveText; // texto por defecto si no se encuentra
    public Sprite fallbackObjectiveSprite; // sprite por defecto
    public List<LevelObjective> levelObjectives = new List<LevelObjective>(); // lista por nivel
    public TMPro.TMP_Text objectiveTextTMP; // opcional (asignar si quieres texto)

    [Header("Tutorial")] public TutorialPanel tutorialPanel; // tutorial
    [Tooltip("Nombre de la escena que debe abrir el tutorial al cargar")] public string autoTutorialSceneName = "Nivel_1";

    [Header("Force Measurement Settings")]
    public float measurementDuration =3.0f; // segundos de muestreo
    [Range(0.1f,1f)] public float measurementFactor =0.75f; // factor recomendado
    public bool applyBreakForceAfterMeasure = false; // si true, aplica automáticamente lo recomendado
    public bool applyOnlyToRoad = true; // aplica solo a barras tipo Road
    public string levelName = "Nivel_1";

    public Image gridImage; // referencia a la imagen de la cuadrícula

    [Header("Car")]
    public car carController; // referencia al script del auto

    public GameObject CreationOptionsUI; // Reference to the creation options UI GameObject

    [Header("Budget per Level")]
    public int levelBudget =1000; // Configurable por nivel en el inspector

    public string backScene = "Niveles";

    // Flags estáticos para omitir panels tras reinicio
    public static bool SkipObjectiveOnce = false;
    public static bool SkipTutorialOnce = false;

    public void Start()
    {
        // Inicializa presupuesto del nivel (si existe BudgetManager en escena)
        if (BudgetManager.Instance != null)
        {
            BudgetManager.Instance.Initialize(levelBudget);
        }

        // Evita invocar si no está asignado y registra el evento solo una vez
        if (RoadButton != null)
        {
            RoadButton.onClick.Invoke();
        }

        // Objetivo
        if (!SkipObjectiveOnce)
        {
            SetupObjectiveForCurrentScene();
        }
        else if (objectivePanel != null)
        {
            objectivePanel.SetActive(false);
        }
        SkipObjectiveOnce = false; // reset

        // Tutorial (solo si no se pidió saltarlo en el reinicio)
        if (!SkipTutorialOnce && tutorialPanel != null && string.Equals(SceneManager.GetActiveScene().name, autoTutorialSceneName, StringComparison.OrdinalIgnoreCase))
        {
            tutorialPanel.Open();
        }
        SkipTutorialOnce = false; // reset
    }

    private void SetupObjectiveForCurrentScene()
    {
        if (objectivePanel == null) return;
        string currentScene = SceneManager.GetActiveScene().name;
        LevelObjective? found = null;
        for (int i =0; i < levelObjectives.Count; i++)
        {
            if (string.Equals(levelObjectives[i].sceneName, currentScene, StringComparison.OrdinalIgnoreCase))
            {
                found = levelObjectives[i];
                break;
            }
        }
        if (found.HasValue)
        {
            if (objectiveImage && found.Value.objectiveSprite) objectiveImage.sprite = found.Value.objectiveSprite;
            if (objectiveTextTMP) objectiveTextTMP.text = string.IsNullOrEmpty(found.Value.objectiveText) ? fallbackObjectiveText : found.Value.objectiveText;
        }
        else
        {
            if (objectiveImage && fallbackObjectiveSprite) objectiveImage.sprite = fallbackObjectiveSprite;
            if (objectiveTextTMP) objectiveTextTMP.text = fallbackObjectiveText;
        }
        objectivePanel.SetActive(true); // mostrará el objetivo; usa CloseOnClick para cerrarlo
    }

    public void StartGame()
    {
        AudioController.instance.ButtonPressed();
        // Cancelar cualquier barra en construcción antes de iniciar
        if (barCreator != null && barCreator.IsCreating)
        {
            barCreator.CancelCurrentBarCreation();
        }

        if (gameManager == null)
        {
            Debug.LogError("UIManager: GameManager no asignado en el inspector.");
            return;
        }

        CreationOptionsUI.SetActive(false); // Oculta las opciones de creación
        if (gridImage != null)
        {
            gridImage.enabled = false; // Oculta la cuadrícula
        }

        // Asegura que el índice esté poblado antes de ocultar
        gameManager.RebuildAllPointsIndex();
        gameManager.HideAllStaticSupports();
        gameManager.HideAllRuntimeSupports();
        gameManager.Resume();

        // Inicia la medición de fuerzas en todas las barras para registrar picos y recomendación
        var bars = FindObjectsOfType<Bar>(true);
        for (int i =0; i < bars.Length; i++)
        {
            var b = bars[i];
            if (b == null) continue;
            if (applyOnlyToRoad && b.kind != Bar.BarKind.Road)
            {
                b.StartForceMeasurement(measurementDuration, measurementFactor, false); // solo medir
            }
            else
            {
                b.StartForceMeasurement(measurementDuration, measurementFactor, applyBreakForceAfterMeasure);
            }
        }

        // Iniciar conducción automática del auto
        if (carController != null)
        {
            carController.StartAutoDrive();
        }
    }

    public void Restart()
    {
        AudioController.instance.ButtonPressed();
        AudioController.instance.CarStopMovementSfx();
        SkipObjectiveOnce = true; // evitar objetivo
        SkipTutorialOnce = true; // evitar tutorial
        SceneManager.LoadScene(levelName);
    }
    public void ChangeBar(int myBarType)
    {
        AudioController.instance.ButtonPressed();
        if (barCreator == null) return;

        // Cancelar cualquier construcción en curso al cambiar de tipo
        if (barCreator.IsCreating)
        {
            barCreator.CancelCurrentBarCreation();
        }

        if (myBarType ==0)
        {
            if (WoodButton != null) WoodButton.GetComponent<Outline>().enabled = false;
            if (RoadButton != null) RoadButton.GetComponent<Outline>().enabled = true;
            barCreator.barToInstantiate = barCreator.RoadBar;
        }
        if (myBarType ==1)
        {
            if (RoadButton != null) RoadButton.GetComponent<Outline>().enabled = false;
            if (WoodButton != null) WoodButton.GetComponent<Outline>().enabled = true;
            barCreator.barToInstantiate = barCreator.WoodBar;
        }
    }

    public void GoBack()
    {

        AudioController.instance.ButtonPressed();
        AudioController.instance.ButtonPressed();
        AudioController.instance.StopLevelMusic();
        AudioController.instance.BackgroundMenuMusic();
        if (!string.IsNullOrEmpty(backScene))
        {
            SceneManager.LoadScene(backScene);
        }
    }

    // Abre el panel de tutorial (asignar este método al botón Tutorial)
    public void Tutorial()
    {
        if (tutorialPanel == null)
        {
            Debug.LogWarning("UIManager: tutorialPanel no asignado.");
            return;
        }
        AudioController.instance.ButtonPressed();
        tutorialPanel.Open();
    }
}
