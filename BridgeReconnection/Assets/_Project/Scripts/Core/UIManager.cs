using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button RoadButton;
    public Button WoodButton;
    public BarCreator barCreator;
    public GameManager gameManager;

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

        // Asegura que el índice esté poblado antes de ocultar
        gameManager.RebuildAllPointsIndex();
        gameManager.HideAllStaticSupports();
        gameManager.HideAllRuntimeSupports();
        gameManager.Resume();
    }

    public void Restart()
    {
        SceneManager.LoadScene("BridgeTest");
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
