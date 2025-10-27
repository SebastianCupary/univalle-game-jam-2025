using UnityEngine;

public class Bar : MonoBehaviour
{
    public Vector2 StartPosition;
    public GameObject BarObject;
    public float maxLength = 1f;
    public BoxCollider bCollider;
    public HingeJoint startJoint;
    public HingeJoint endJoint;

    // Reducir el largo del collider para evitar solapamientos en los extremos (en unidades de mundo por cada lado)
    public float colliderEndMargin = 0.14f;

    public void UpdateCreatingBar(Vector2 ToPosition)
    {
        transform.position = (ToPosition + StartPosition) / 2;

        Vector2 dir = ToPosition - StartPosition;
        float angle = Vector2.SignedAngle(Vector2.right, dir);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float length = dir.magnitude;
        // Escala visual completa
        if (BarObject != null)
        {
            BarObject.transform.localScale = new Vector3(length, BarObject.transform.localScale.y, BarObject.transform.localScale.z);
        }

        // Ajuste del BoxCollider: más corto para no chocar con puntos/baras adyacentes
        if (bCollider != null)
        {
            float lossyX = Mathf.Abs(bCollider.transform.lossyScale.x);
            // Largo del collider en mundo, recortado en ambos extremos
            float colliderWorldLength = Mathf.Max(0f, length - 2f * Mathf.Max(0f, colliderEndMargin));
            if (lossyX > 0.0001f)
            {
                float localX = colliderWorldLength / lossyX;
                bCollider.size = new Vector3(localX, bCollider.size.y, bCollider.size.z);
                bCollider.center = new Vector3(0f, bCollider.center.y, bCollider.center.z); // centrado
            }
        }
    }
}
