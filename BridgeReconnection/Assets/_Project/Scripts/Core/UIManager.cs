using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button RoadButton;
    public Button WoodButton;
    public BarCreator barCreator;
    public GameManager gameManager;

    [Header("Force Measurement Settings")]
    public float measurementDuration =3.0f; // segundos de muestreo
    [Range(0.1f,1f)] public float measurementFactor =0.75f; // factor recomendado
    public bool applyBreakForceAfterMeasure = false; // si true, aplica automáticamente lo recomendado
    public bool applyOnlyToRoad = true; // aplica solo a barras tipo Road
    public string levelName = "Nivel_1";

    [Header("Car")]
    public car carController; // referencia al script del auto

    public GameObject CreationOptionsUI; // Reference to the creation options UI GameObject

    public void Start()
    {
        // Evita invocar si no está asignado y registra el evento solo una vez
        if (RoadButton != null)
        {
            RoadButton.onClick.Invoke();
        }
    }

    public void StartGame()
    {
        if (gameManager == null)
        {
            Debug.LogError("UIManager: GameManager no asignado en el inspector.");
            return;
        }

        CreationOptionsUI.SetActive(false); // Oculta las opciones de creación

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
        SceneManager.LoadScene(levelName);
    }
    public void ChangeBar(int myBarType)
    {
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
}
