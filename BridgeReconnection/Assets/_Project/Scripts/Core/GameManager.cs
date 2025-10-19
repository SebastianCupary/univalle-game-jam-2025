using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<Vector2, Point> AllPoints = new Dictionary<Vector2, Point>();

    private void Awake()
    {
        AllPoints.Clear();
    }

    // Agrega este método para dibujar los Gizmos
    private void OnDrawGizmos()
    {
        if (AllPoints.Count == 0) return;

        // Dibuja una esfera roja en la posición de cada punto registrado
        Gizmos.color = Color.red;
        foreach (Vector2 pointPosition in AllPoints.Keys)
        {
            Gizmos.DrawSphere(pointPosition, 0.2f); // Dibuja una esfera de radio 0.2
        }
    }
}