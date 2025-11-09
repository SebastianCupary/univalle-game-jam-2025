using UnityEngine;

[RequireComponent(typeof(Collider))]
public class coins : MonoBehaviour
{
    [Tooltip("Cantidad de monedas que otorga esta pickup")] public int amount =1;
    [Tooltip("Desactivar en vez de destruir (útil si se usa pooling)")] public bool deactivateInsteadOfDestroy = true;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detecta el auto por el componente 'car' en el objeto o en sus padres
        var carComp = other.GetComponentInParent<car>();
        if (carComp == null) return;

        if (CoinManager.Instance != null)
        {
            AudioController.instance.CoinSfx();
            CoinManager.Instance.Add(amount);
        }

        if (deactivateInsteadOfDestroy) gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
