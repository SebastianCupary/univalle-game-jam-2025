using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Dictionary<Vector2, Point> AllPoints = new Dictionary<Vector2, Point>();

    public static bool IsPaused { get; private set; } = true;

    private void Awake()
    {
        AllPoints.Clear();
        Pause();
    }

    // Usa este menú contextual para iniciar el juego desde el editor
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        HideAllStaticSupports();
        HideAllRuntimeSupports();
        Resume();
    }

    // También agrega un menú para mostrar de nuevo (debug)
    [ContextMenu("Show Supports (Debug)")]
    public void ShowAllSupportsDebug()
    {
        SetVisibilityForSupports(true, includeRuntime:true);
    }

    private void HideAllStaticSupports()
    {
        SetVisibilityForSupports(false, includeRuntime:false);
    }

    private void HideAllRuntimeSupports()
    {
        SetVisibilityForSupports(false, includeRuntime:true, onlyRuntime:true);
    }

    private void SetVisibilityForSupports(bool visible, bool includeRuntime, bool onlyRuntime = false)
    {
        foreach (var kvp in AllPoints)  // Recorre todos los puntos
        {
            var point = kvp.Value; // obtiene el punto  
            if (point == null) continue; // punto nulo, salta   
            bool isRuntime = point.Runtime; // verifica si es runtime
            if (onlyRuntime && !isRuntime) continue; // si solo runtime y no lo es, salta
            if (!includeRuntime && isRuntime) continue; // si no incluye runtime y lo es, salta
            if (!isRuntime) // punto estático
            {
                point.SetSupportVisible(visible); // usa API de Point para estáticos
            }
            else
            {
                // Ocultar/mostrar renderers de runtime directamente
                var renderers = point.GetComponentsInChildren<Renderer>(true); // obtiene renderers
                for (int i =0; i < renderers.Length; i++)
                {
                    var r = renderers[i]; // obtiene renderer
                    if (r != null) r.enabled = visible; // establece visibilidad            
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