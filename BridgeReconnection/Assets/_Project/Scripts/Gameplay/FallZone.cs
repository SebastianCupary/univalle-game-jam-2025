using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class FallZone : MonoBehaviour
{
 [Header("UI")] public GameObject incompletePanel;


 public GameObject CreationUI;

    private bool triggered;

    public Button restartButton;

    public Button levelsButton;

    public string levelMenu = "Niveles";

    private void Reset()
 {
 var col = GetComponent<Collider>();
 if (col) col.isTrigger = true;
 }

    private void Awake()
    {
   

        // Registrar callbacks si hay botones asignados
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (levelsButton != null)
        {
            levelsButton.onClick.RemoveAllListeners();
            levelsButton.onClick.AddListener(LoadLevelsMenu);
        }
    }

    private void OnTriggerEnter(Collider other)
 {
 if (triggered) return;
 var c = other.GetComponentInParent<car>();
 if (c == null) return;
 triggered = true;

 // Detener la conducci�n pero permitir la ca�da
 c.StopAutoDrive();
 if (c.wheel1) { c.wheel1.motorTorque =0f; c.wheel1.brakeTorque =0f; c.wheel1.steerAngle =0f; }
 if (c.wheel2) { c.wheel2.motorTorque =0f; c.wheel2.brakeTorque =0f; c.wheel2.steerAngle =0f; }
 if (c.wheel3) { c.wheel3.motorTorque =0f; c.wheel3.brakeTorque =0f; }
 if (c.wheel4) { c.wheel4.motorTorque =0f; c.wheel4.brakeTorque =0f; }
 if (c.rigid)
 {
 c.rigid.useGravity = true; // asegurarse de que caiga
 c.rigid.linearDamping =0f; // sin resistencia extra
 }

        // Mostrar UI de nivel incompleto
        if (CreationUI) CreationUI.SetActive(false);
        if (incompletePanel) incompletePanel.SetActive(true);
 }


    public void LoadLevelsMenu()
    {
        if (string.IsNullOrEmpty(levelMenu)) return;
        SceneManager.LoadScene(levelMenu);
    }

    public void RestartLevel()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

}
