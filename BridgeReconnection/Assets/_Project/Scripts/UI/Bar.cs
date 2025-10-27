using UnityEngine;

public class Bar : MonoBehaviour
{
    public Vector2 StartPosition;
    public GameObject BarObject;
    public float maxLength = 1f;
    public BoxCollider bCollider;
    public HingeJoint startJoint;
    public HingeJoint endJoint;

    public void UpdateCreatingBar(Vector2 ToPosition)
    {
        transform.position = (ToPosition + StartPosition) / 2;

        Vector2 dir = ToPosition - StartPosition;
        float angle = Vector2.SignedAngle(Vector2.right, dir); 
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float length = dir.magnitude;
        BarObject.transform.localScale = new Vector3(length, BarObject.transform.localScale.y, BarObject.transform.localScale.z); // Adjust the x scale to match the length

     
        float lossyX = Mathf.Abs(bCollider.transform.lossyScale.x);
        if (lossyX > 0.0001f)
        {
            bCollider.size = new Vector3(length / lossyX, bCollider.size.y, bCollider.size.z);
        }


    }
}
