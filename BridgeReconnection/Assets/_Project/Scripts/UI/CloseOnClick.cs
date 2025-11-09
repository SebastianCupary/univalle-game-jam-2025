using UnityEngine;
using UnityEngine.EventSystems;

// Cierra el panel al hacer click/tap. Puede apuntar a un objeto objetivo específico.
// Úsalo en el mismo objeto que tiene el componente Image/Button del popup objetivo
public class CloseOnClick : MonoBehaviour, IPointerClickHandler
{
 [Tooltip("Objeto a desactivar al hacer click. Si es null, se desactiva este GameObject")] public GameObject target;
 public bool playClickSfx = true;

 public void OnPointerClick(PointerEventData eventData)
 {
 if (playClickSfx) AudioController.instance?.ButtonPressed();
 var go = target != null ? target : gameObject;
 go.SetActive(false);
 }
}
