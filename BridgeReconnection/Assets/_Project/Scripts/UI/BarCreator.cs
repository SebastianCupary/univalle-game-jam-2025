using UnityEngine;
using UnityEngine.EventSystems;

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

    /// <summary>
    /// MÉTODO OnPointerDown CORREGIDO
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Redondea la posición del clic para que coincida con la rejilla.
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 gridPosition = new Vector2(Mathf.Round(mouseWorldPosition.x), Mathf.Round(mouseWorldPosition.y));

        if (BarCreationStarted == false)
        {
            // --- LA CLAVE DE LA SOLUCIÓN ---
            // Solo inicia la creación si el clic se hizo sobre un punto existente.
            if (GameManager.AllPoints.ContainsKey(gridPosition))
            {
                BarCreationStarted = true;
                StartBarCreation(gridPosition);
            }
            // Si no se encuentra un punto en esa posición, no hace nada.
            // Esto evita que se creen puntos nuevos y previene el error.
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
    /// MÉTODO StartBarCreation SIMPLIFICADO Y CORREGIDO
    /// </summary>
    private void StartBarCreation(Vector2 StartPosition)
    {
        // Ahora podemos asumir que el punto de inicio siempre existe,
        // porque la verificación ya se hizo en OnPointerDown.
        CurrentStartPoint = GameManager.AllPoints[StartPosition];

        // Se crea la barra y el punto final temporal como antes.
        CurrentBar = Instantiate(barToInstantiate, barParent).GetComponent<Bar>();
        CurrentBar.StartPosition = StartPosition;
        CurrentEndPoint = Instantiate(PointToInstantiate, StartPosition, Quaternion.identity, PointParent).GetComponent<Point>();
    }

    private void DeleteCurrentBar()
    {
        if (CurrentBar != null) Destroy(CurrentBar.gameObject);
        if (CurrentEndPoint != null) Destroy(CurrentEndPoint.gameObject);

        if (CurrentStartPoint != null && CurrentStartPoint.ConnectedBars.Count == 0 && CurrentStartPoint.Runtime == true)
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
        Vector2 finalPosition = CurrentEndPoint.transform.position;
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
        CurrentBar.startJoint.anchor = CurrentBar.transform.InverseTransformPoint(CurrentBar.StartPosition); ;
        CurrentBar.endJoint.connectedBody = CurrentEndPoint.rbd;    
        CurrentBar.endJoint.anchor = CurrentBar.transform.InverseTransformPoint(CurrentEndPoint.transform.position);


        StartBarCreation(CurrentEndPoint.transform.position);
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
}