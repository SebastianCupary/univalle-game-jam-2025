using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Point : MonoBehaviour
{
    public bool Runtime = true;
    public Vector2 PointID;
    public Rigidbody rbd;
    public List<Bar> ConnectedBars = new List<Bar>();

    private Renderer[] _renderers;

    private void Awake()
    {
        if (rbd == null)
        {
            rbd = GetComponent<Rigidbody>();
        }

        if (Runtime == false)
        {
            // Puntos de apoyo: no deben moverse ni caer
            if (rbd != null)
            {
                rbd.useGravity = false;
                rbd.constraints = RigidbodyConstraints.FreezeAll;
                rbd.isKinematic = true;
            }

            PointID = transform.position;
            if (GameManager.AllPoints.ContainsKey(PointID) == false)
            {
                GameManager.AllPoints.Add(PointID, this);
            }
        }
        else
        {
            // Puntos generados en runtime: dinámicos por defecto
            if (rbd != null)
            {
                rbd.isKinematic = false;
                rbd.useGravity = true;
                rbd.constraints = RigidbodyConstraints.None;
            }
        }
    }

    // Facilita la configuración desde el editor cuando cambias Runtime
    private void OnValidate()
    {
        if (rbd == null) rbd = GetComponent<Rigidbody>();
        if (Runtime == false)
        {
            if (rbd == null) rbd = gameObject.AddComponent<Rigidbody>();
            rbd.useGravity = false;
            rbd.constraints = RigidbodyConstraints.FreezeAll;
            rbd.isKinematic = true;
            // Asegura posicion en grilla para que coincida con AllPoints
            transform.position = Vector3Int.RoundToInt(transform.position);
            PointID = transform.position;
        }
        else
        {
            if (rbd != null)
            {
                rbd.isKinematic = false;
                rbd.useGravity = true;
                rbd.constraints = RigidbodyConstraints.None;
            }
        }
    }

    // Ya no ocultamos automáticamente en Start. La visibilidad se controla desde GameManager al iniciar juego.

    private void CacheRenderersIfNeeded()
    {
        if (_renderers == null)
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    // Controla la visibilidad visual del punto de apoyo (solo aplica si Runtime == false)
    public void SetSupportVisible(bool visible)
    {
        if (Runtime) return; // solo puntos de apoyo estáticos
        CacheRenderersIfNeeded();
        if (_renderers == null) return;
        for (int i =0; i < _renderers.Length; i++)
        {
            var r = _renderers[i];
            if (r != null) r.enabled = visible;
        }
    }

    private void Update()
    {
        // Encajar a la grilla solo en modo edición o mientras el juego está pausado (modo construcción)
        if (!Application.isPlaying || GameManager.IsPaused)
        {
            if (transform.hasChanged == true)
            {
                transform.hasChanged = false;
                transform.position = Vector3Int.RoundToInt(transform.position);
            }
        }
        // En juego activo (no pausado), no forzamos posición para permitir la física
    }

    // Utilidades de editor para configurar soportes estáticos rápidamente
    [ContextMenu("Snap To Grid (Round Position)")]
    private void SnapToGrid()
    {
        transform.position = Vector3Int.RoundToInt(transform.position);
    }

    [ContextMenu("Convert To Static Support (Freeze + Setup)")]
    private void ConvertToStaticSupport()
    {
        Runtime = false;
        if (rbd == null) rbd = GetComponent<Rigidbody>();
        if (rbd == null) rbd = gameObject.AddComponent<Rigidbody>();
        rbd.useGravity = false;
        rbd.constraints = RigidbodyConstraints.FreezeAll;
        rbd.isKinematic = true;
        transform.position = Vector3Int.RoundToInt(transform.position);
        PointID = transform.position;
    }

    [ContextMenu("Register In GameManager Index")]
    private void RegisterInGameManager()
    {
        PointID = (Vector2)transform.position;  
        if (!GameManager.AllPoints.ContainsKey(PointID))
        {
            GameManager.AllPoints.Add(PointID, this);

        }
    }
}
