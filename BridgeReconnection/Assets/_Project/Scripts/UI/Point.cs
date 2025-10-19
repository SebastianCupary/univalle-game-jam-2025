using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Point : MonoBehaviour
{
    public bool Runtime = true;
    public Vector2 PointID;
    public List<Bar> ConnectedBars = new List<Bar>();

    private void Awake()
    {
        if (Runtime == false)
        {
            PointID = transform.position;
            if(GameManager.AllPoints.ContainsKey(PointID) == false)
            {
                GameManager.AllPoints.Add(PointID, this);
            }
        }
    }
    private void Update()
    {
        if(transform.hasChanged == true)
        {
            transform.hasChanged = false;
            transform.position = Vector3Int.RoundToInt(transform.position);
        }
    }
}
