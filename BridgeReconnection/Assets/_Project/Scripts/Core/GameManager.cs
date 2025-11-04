using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<Vector2, Point> AllPoints = new Dictionary<Vector2, Point>();

    public static bool IsPaused { get; private set; } = true;

    private void Awake()
    {
        AllPoints.Clear();
        // Construye el índice al cargar escena, así los anclajes clonados quedan disponibles
        RebuildAllPointsIndex();
        Pause();
    }

    // Reconstruye el índice con todos los Point existentes en escena (estáticos y runtime)
    [ContextMenu("Rebuild Points Index (Debug)")]
    public void RebuildAllPointsIndex()
    {
        AllPoints.Clear();
        var points = FindObjectsOfType<Point>(true);
        for (int i =0; i < points.Length; i++)
        {
            var p = points[i];
            if (p == null) continue;
            // Redondear posición a grilla para tener claves consistentes
            var rounded = Vector2Int.RoundToInt(p.transform.position);
            p.transform.position = new Vector3(rounded.x, rounded.y, p.transform.position.z);
            p.PointID = rounded;
            if (!AllPoints.ContainsKey(p.PointID))
            {
                AllPoints.Add(p.PointID, p);
            }
        }
    }

    // Usa este menú contextual para iniciar el juego desde el editor
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        RebuildAllPointsIndex();
        HideAllStaticSupports();
        HideAllRuntimeSupports();
        Resume();
    }

    // También agrega un menú para mostrar de nuevo (debug)
    [ContextMenu("Show Supports (Debug)")]
    public void ShowAllSupportsDebug()
    {
        SetVisibilityForSupports(true, includeRuntime: true);
    }

    public void HideAllStaticSupports()
    {
        SetVisibilityForSupports(false, includeRuntime: false);
    }

    public void HideAllRuntimeSupports()
    {
        SetVisibilityForSupports(false, includeRuntime: true, onlyRuntime: true);
    }

    private void SetVisibilityForSupports(bool visible, bool includeRuntime, bool onlyRuntime = false)
    {
        foreach (var kvp in AllPoints)
        {
            var point = kvp.Value;
            if (point == null) continue;
            bool isRuntime = point.Runtime;
            if (onlyRuntime && !isRuntime) continue;
            if (!includeRuntime && isRuntime) continue;
            if (!isRuntime)
            {
                point.SetSupportVisible(visible); // usa API de Point para estáticos
            }
            else
            {
                // Ocultar/mostrar renderers de runtime directamente
                var renderers = point.GetComponentsInChildren<Renderer>(true);
                for (int i =0; i < renderers.Length; i++)
                {
                    var r = renderers[i];
                    if (r != null) r.enabled = visible;
                }
            }
        }
    }

    public void Pause()
    {
        Time.timeScale =0f; // Detiene física, animaciones y lógica basada en deltaTime
        AudioListener.pause = true; // Pausa el audio
        IsPaused = true;
    }

    public void Resume()
    {
        Time.timeScale =1f;
        AudioListener.pause = false;
        IsPaused = false;
    }

    public void TogglePause()
    {
        if (IsPaused) Resume(); else Pause();
    }

    // Gizmos para depuración
    private void OnDrawGizmos()
    {
        if (AllPoints == null || AllPoints.Count ==0) return;
        Gizmos.color = Color.red;
        foreach (Vector2 pointPosition in AllPoints.Keys)
        {
            Gizmos.DrawSphere(new Vector3(pointPosition.x, pointPosition.y,0f),0.2f);
        }
    }
}