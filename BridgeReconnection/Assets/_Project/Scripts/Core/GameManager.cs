using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Usa Vector3 como clave para las posiciones en el espacio 3D
    public static Dictionary<Vector3, Point> AllPoints = new Dictionary<Vector3, Point>();

    private void OnDisable()
    {
        AllPoints.Clear();
    }
}