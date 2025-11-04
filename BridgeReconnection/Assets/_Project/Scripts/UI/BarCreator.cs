using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BarCreator : MonoBehaviour, IPointerDownHandler
{
    public GameObject RoadBar;
    public GameObject WoodBar;
    bool BarCreationStarted = false;
    public Bar CurrentBar;
    public GameObject barToInstantiate;
    public Transform barParent;
    public Point CurrentStartPoint;
    public GameObject PointToInstantiate;
    public Transform PointParent;
    public Point CurrentEndPoint;

    // Costos por tipo de barra
    [Header("Budget Costs")] public int roadCost =100; public int woodCost =50;

    // Historial por material (para botón de deshacer)
    private readonly List<Bar> _roadHistory = new List<Bar>();
    private readonly List<Bar> _woodHistory = new List<Bar>();

    /// <summary>
    /// MÉTODO OnPointerDown CORREGIDO + Control de presupuesto
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Redondea la posición del clic para que coincida con la rejilla.
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 gridPosition = new Vector2(Mathf.Round(mouseWorldPosition.x), Mathf.Round(mouseWorldPosition.y));

        if (BarCreationStarted == false)
        {
            // Solo inicia la creación si el clic se hizo sobre un punto existente y hay presupuesto
            if (GameManager.AllPoints.ContainsKey(gridPosition))
            {
                if (!CanAffordSelectedKind())
                {
                    Debug.LogWarning("No hay presupuesto suficiente para iniciar esta barra.");
                    return;
                }
                BarCreationStarted = true;
                StartBarCreation(gridPosition);
            }
            // Si no se encuentra un punto en esa posición, no hace nada.
        }
        else
        {
            // La lógica para finalizar o cancelar la creación sigue igual.
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                FinishBarCreation();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                DeleteCurrentBar();
            }
        }
    }

    /// <summary>
    /// MÉTODO StartBarCreation SIMPLIFICADO Y CORREGIDO + guardas de presupuesto para cadenas
    /// </summary>
    private void StartBarCreation(Vector2 StartPosition)
    {
        // Si no hay presupuesto para crear otra barra del tipo seleccionado, no continúes con la cadena
        if (!CanAffordSelectedKind())
        {
            BarCreationStarted = false;
            CurrentBar = null;
            CurrentEndPoint = null;
            return;
        }

        // Asegura clave exacta de grilla y recuperación segura
        Vector2 key = new Vector2(Mathf.Round(StartPosition.x), Mathf.Round(StartPosition.y));
        if (!GameManager.AllPoints.TryGetValue(key, out CurrentStartPoint))
        {
            // Fallback robusto: si el punto no existe (p.ej. por clon/copiar), créalo y regístralo
            var p = Instantiate(PointToInstantiate, key, Quaternion.identity, PointParent).GetComponent<Point>();
            p.PointID = key;
            if (!GameManager.AllPoints.ContainsKey(key)) GameManager.AllPoints.Add(key, p);
            CurrentStartPoint = p;
        }

        // Se crea la barra y el punto final temporal como antes.
        CurrentBar = Instantiate(barToInstantiate, barParent).GetComponent<Bar>();
        CurrentBar.StartPosition = key;
        CurrentEndPoint = Instantiate(PointToInstantiate, key, Quaternion.identity, PointParent).GetComponent<Point>();
    }

    private void DeleteCurrentBar()
    {
        if (CurrentBar != null) Destroy(CurrentBar.gameObject);
        if (CurrentEndPoint != null) Destroy(CurrentEndPoint.gameObject);

        if (CurrentStartPoint != null && CurrentStartPoint.ConnectedBars.Count ==0 && CurrentStartPoint.Runtime == true)
        {
            if (GameManager.AllPoints.ContainsKey(CurrentStartPoint.PointID))
            {
                GameManager.AllPoints.Remove(CurrentStartPoint.PointID);
            }
            Destroy(CurrentStartPoint.gameObject);
        }

        BarCreationStarted = false;
        CurrentBar = null;
        CurrentStartPoint = null;
        CurrentEndPoint = null;
    }

    private void FinishBarCreation()
    {
        if (CurrentBar == null || CurrentEndPoint == null) { BarCreationStarted = false; return; }

        // Verificar presupuesto antes de confirmar la creación
        int cost = GetCostForKind(CurrentBar.kind);
        if (BudgetManager.Instance != null && !BudgetManager.Instance.CanAfford(cost))
        {
            // Cancelar esta barra sin eliminar el punto inicial
            if (CurrentBar != null) Destroy(CurrentBar.gameObject);
            if (CurrentEndPoint != null) Destroy(CurrentEndPoint.gameObject);
            CurrentBar = null; CurrentEndPoint = null;
            BarCreationStarted = false;
            Debug.LogWarning("Presupuesto insuficiente para colocar la barra.");
            return;
        }

        // Recalcular y forzar posición de grilla para usar exactamente la misma clave
        Vector2 finalPosition = new Vector2(
            Mathf.Round(CurrentEndPoint.transform.position.x),
            Mathf.Round(CurrentEndPoint.transform.position.y)
        );
        CurrentEndPoint.transform.position = finalPosition;

        if (GameManager.AllPoints.ContainsKey(finalPosition))
        {
            Destroy(CurrentEndPoint.gameObject);
            CurrentEndPoint = GameManager.AllPoints[finalPosition];
        }
        else
        {
            CurrentEndPoint.PointID = finalPosition;
            GameManager.AllPoints.Add(finalPosition, CurrentEndPoint);
        }

        CurrentStartPoint.ConnectedBars.Add(CurrentBar);
        CurrentEndPoint.ConnectedBars.Add(CurrentBar);

        CurrentBar.startJoint.connectedBody = CurrentStartPoint.rbd;
        CurrentBar.startJoint.anchor = CurrentBar.transform.InverseTransformPoint(CurrentBar.StartPosition);
        CurrentBar.endJoint.connectedBody = CurrentEndPoint.rbd;
        CurrentBar.endJoint.anchor = CurrentBar.transform.InverseTransformPoint(CurrentEndPoint.transform.position);

        // Cobrar presupuesto una vez creada
        if (BudgetManager.Instance != null)
        {
            BudgetManager.Instance.Spend(cost);
        }

        // Registrar en historial por material
        if (CurrentBar.kind == Bar.BarKind.Road)
        {
            _roadHistory.Add(CurrentBar);
        }
        else // WoodSupport
        {
            _woodHistory.Add(CurrentBar);
        }

        // Iniciar siguiente barra desde la exacta clave usada (solo si hay presupuesto para otra)
        if (CanAffordSelectedKind())
        {
            StartBarCreation(finalPosition);
        }
        else
        {
            BarCreationStarted = false;
            CurrentBar = null;
            CurrentEndPoint = null;
        }
    }

    private void Update()
    {
        if (BarCreationStarted)
        {
            // Esta comprobación de seguridad previene el error si algo sale mal.
            if (CurrentBar == null || CurrentEndPoint == null)
            {
                BarCreationStarted = false; // Detiene el proceso para evitar más errores.
                return;
            }

            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 startPosition = CurrentBar.StartPosition;
            Vector2 direction = mouseWorldPosition - startPosition;
            Vector2 clampedDirection = Vector2.ClampMagnitude(direction, CurrentBar.maxLength);
            Vector2 idealPosition = startPosition + clampedDirection;
            Vector2 finalPosition = new Vector2(Mathf.Round(idealPosition.x), Mathf.Round(idealPosition.y));

            CurrentEndPoint.transform.position = finalPosition;
            CurrentEndPoint.PointID = finalPosition;
            CurrentBar.UpdateCreatingBar(finalPosition);
        }
    }

    // ========== UNDO API ==========

    public void UndoLastRoadBar() => UndoLastFromList(_roadHistory);
    public void UndoLastWoodBar() => UndoLastFromList(_woodHistory);

    // Usa el prefab actualmente seleccionado para decidir qué material deshacer.
    public void UndoLastBarOfCurrentKind()
    {
        var prefab = barToInstantiate;
        Bar.BarKind kind = Bar.BarKind.Road;
        if (prefab != null)
        {
            var b = prefab.GetComponent<Bar>();
            if (b != null) kind = b.kind;
        }
        UndoLastFromList(kind == Bar.BarKind.Road ? _roadHistory : _woodHistory);
    }

    private void UndoLastFromList(List<Bar> list)
    {
        if (list == null || list.Count ==0) return;
        var last = list[list.Count -1];
        list.RemoveAt(list.Count -1);
        if (last == null) return;

        // Quitar referencias en puntos conectados
        var sp = last.startJoint != null && last.startJoint.connectedBody != null ? last.startJoint.connectedBody.GetComponent<Point>() : null;
        var ep = last.endJoint != null && last.endJoint.connectedBody != null ? last.endJoint.connectedBody.GetComponent<Point>() : null;
        if (sp != null) sp.ConnectedBars.Remove(last);
        if (ep != null) ep.ConnectedBars.Remove(last);

        // Reembolsar presupuesto según tipo
        if (BudgetManager.Instance != null)
        {
            int refund = GetCostForKind(last.kind);
            BudgetManager.Instance.Refund(refund);
        }

        // Eliminar puntos runtime huérfanos
        CleanupPointIfOrphan(sp);
        CleanupPointIfOrphan(ep);

        Destroy(last.gameObject);
    }

    private void CleanupPointIfOrphan(Point p)
    {
        if (p == null) return;
        // Limpia nulos por seguridad
        p.ConnectedBars.RemoveAll(b => b == null);
        if (p.Runtime && p.ConnectedBars.Count ==0)
        {
            if (GameManager.AllPoints.ContainsKey(p.PointID))
            {
                GameManager.AllPoints.Remove(p.PointID);
            }
            Destroy(p.gameObject);
        }
    }

    // ======== Presupuesto helpers ========
    private int GetCostForKind(Bar.BarKind kind)
    {
        return kind == Bar.BarKind.WoodSupport ? woodCost : roadCost;
    }

    private Bar.BarKind GetSelectedKind()
    {
        var prefab = barToInstantiate;
        var kind = Bar.BarKind.Road;
        if (prefab != null)
        {
            var b = prefab.GetComponent<Bar>();
            if (b != null) kind = b.kind;
        }
        return kind;
    }

    private bool CanAffordSelectedKind()
    {
        if (BudgetManager.Instance == null) return true; // sin gestor => sin límite
        int cost = GetCostForKind(GetSelectedKind());
        return BudgetManager.Instance.CanAfford(cost);
    }
}