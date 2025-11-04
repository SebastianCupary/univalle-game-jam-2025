using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class FallZone : MonoBehaviour
{
 [Header("UI")] public GameObject completePanel;
 public TMP_Text messageTMP;
 public string fallMessage = "Nivel incompleto: te ca�ste";

 [Header("Hide On Show")] public GameObject CoinCounterUI;
 public GameObject CreationButtonsUI;

 [Header("Buttons (opcional)")] public Button restartButton;
 public Button nextLevelButton; // ser� oculto al caer

 private bool triggered;

 private void Reset()
 {
 var col = GetComponent<Collider>();
 if (col) col.isTrigger = true;
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
 if (CoinCounterUI) CoinCounterUI.SetActive(false);
 if (CreationButtonsUI) CreationButtonsUI.SetActive(false);
 if (messageTMP) messageTMP.text = fallMessage;
 if (completePanel) completePanel.SetActive(true);
 if (restartButton) restartButton.gameObject.SetActive(true);
 if (nextLevelButton) nextLevelButton.gameObject.SetActive(false);
 }
}
