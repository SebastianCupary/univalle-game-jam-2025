using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button RoadButton;
    public Button WoodButton;
    public BarCreator barCreator;
    public GameManager gameManager;
    public BudgetManager budgetManager;

    [Header("Force Measurement Settings")]
    public float measurementDuration =3.0f; // segundos de muestreo
    [Range(0.1f,1f)] public float measurementFactor =0.75f; // factor recomendado
    public bool applyBreakForceAfterMeasure = false; // si true, aplica automáticamente lo reco         endado
    public bool applyOnlyToRoad = true; // aplica solo a barras tipo Road
    public string levelName = "Nivel_1";

    public Image gridImage; // referencia a la imagen de la cuadrícula

    [Header("Car")]
    public car carController; // referencia al script del auto

    public GameObject CreationOptionsUI; // Reference to the creation options UI GameObject

    [Header("Budget per Level")]
    public int levelBudget =1000; // Configurable por nivel en el inspector

    public string backScene = "Niveles";
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
    }

    public void StartGame()
    {
        AudioController.instance.ButtonPressed();
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
        SceneManager.LoadScene(levelName);
    }
    public void ChangeBar(int myBarType)
    {
        AudioController.instance.ButtonPressed();
        if (barCreator == null) return;
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
        if (!string.IsNullOrEmpty(backScene))
        {
            SceneManager.LoadScene(backScene);
        }
    }
}
