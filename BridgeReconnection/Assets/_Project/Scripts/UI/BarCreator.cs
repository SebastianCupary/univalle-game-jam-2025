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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (BarCreationStarted == false)
        {
            BarCreationStarted = true;
            StartBarCreation(Camera.main.ScreenToWorldPoint(eventData.position));
        }
        else
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                FinishBarCreation();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                BarCreationStarted = false;
                DeleteCurrentBar();
            }
        }
    }

    private void DeleteCurrentBar()
    {
        Destroy(CurrentBar.gameObject);
        if (CurrentStartPoint.ConnectedBars.Count == 0) Destroy(CurrentStartPoint.gameObject);
        // The endpoint is temporary and will be destroyed anyway if not connected.
        Destroy(CurrentEndPoint.gameObject);
    }

    private void StartBarCreation(Vector2 StartPosition)
    {
        CurrentBar = Instantiate(barToInstantiate, barParent).GetComponent<Bar>();
        CurrentBar.StartPosition = (Vector2)Vector2Int.RoundToInt(StartPosition);

        // Snap to existing point if close enough
        if (GameManager.AllPoints.ContainsKey(CurrentBar.StartPosition))
        {
            CurrentStartPoint = GameManager.AllPoints[CurrentBar.StartPosition];
        }
        else
        {
            CurrentStartPoint = Instantiate(PointToInstantiate, CurrentBar.StartPosition, Quaternion.identity, PointParent).GetComponent<Point>();
            GameManager.AllPoints.Add(CurrentBar.StartPosition, CurrentStartPoint);
        }

        CurrentEndPoint = Instantiate(PointToInstantiate, CurrentBar.StartPosition, Quaternion.identity, PointParent).GetComponent<Point>();
    }

    private void FinishBarCreation()
    {
        Vector2 finalEndpointPos = (Vector2)Vector2Int.RoundToInt(CurrentEndPoint.transform.position);

        if (GameManager.AllPoints.ContainsKey(finalEndpointPos))
        {
            Destroy(CurrentEndPoint.gameObject);
            CurrentEndPoint = GameManager.AllPoints[finalEndpointPos];
        }
        else
        {
            CurrentEndPoint.transform.position = finalEndpointPos;
            GameManager.AllPoints.Add(finalEndpointPos, CurrentEndPoint);
        }

        // Finalize connections
        CurrentStartPoint.ConnectedBars.Add(CurrentBar);
        CurrentEndPoint.ConnectedBars.Add(CurrentBar);

        CurrentBar.StartJoint.connectedBody = CurrentStartPoint.rbd;
        CurrentBar.StartJoint.anchor = CurrentBar.transform.InverseTransformPoint(CurrentStartPoint.transform.position);

        CurrentBar.EndJoint.connectedBody = CurrentEndPoint.rbd;
        CurrentBar.EndJoint.anchor = CurrentBar.transform.InverseTransformPoint(CurrentEndPoint.transform.position);

        CurrentBar.gameObject.name = "Bar from " + CurrentStartPoint.gameObject.name + " to " + CurrentEndPoint.gameObject.name;

        // Start creating the next bar from the endpoint of the last one
        StartBarCreation(CurrentEndPoint.transform.position);
    }

    private void Update()
    {
        if (BarCreationStarted == true)
        {
            Vector2 EndPosition = (Vector2)Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector2 Dir = EndPosition - CurrentBar.StartPosition;
            Vector2 ClampedPosition = Vector2.ClampMagnitude(Dir, CurrentBar.maxLength) + CurrentBar.StartPosition;

            CurrentEndPoint.transform.position = (Vector2)Vector2Int.RoundToInt(ClampedPosition);
            CurrentBar.UpdateCreatingBar(CurrentEndPoint.transform.position);
        }
    }
}