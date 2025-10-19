using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button RoadButton;
    public Button WoodButton;
    public BarCreator BarCreator;

    private void Start()
    {
        RoadButton.onClick.Invoke();
        Debug.Log($"BarCreator: {BarCreator}, RoadButton: {RoadButton}, WoodButton: {WoodButton}");
    }

    public void Play()
    {
        Time.timeScale = 1;
    }

    public void Restart()
    {
        SceneManager.LoadScene("BridgeTest");
    }
    public void ChangeBar(int myBarType)
    {
        if(myBarType == 0)
        {
            WoodButton.GetComponent<Outline>().enabled = false;
            RoadButton.GetComponent<Outline>().enabled = true;
            BarCreator.barToInstantiate = BarCreator.RoadBar;


        }
        if (myBarType == 1)
        {
            WoodButton.GetComponent<Outline>().enabled = true;
            RoadButton.GetComponent<Outline>().enabled = false;
            BarCreator.barToInstantiate = BarCreator.WoodBar;


        }
    }
}
