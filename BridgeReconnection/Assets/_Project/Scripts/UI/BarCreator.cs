using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class BarCreator : MonoBehaviour, IPointerDownHandler
{
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
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                FinishBarCreation();
            }
            else if(eventData.button == PointerEventData.InputButton.Right)
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
        if (CurrentEndPoint.ConnectedBars.Count == 0) Destroy(CurrentEndPoint.gameObject);
    }

    

    private void StartBarCreation(Vector2 StartPosition)
    {
       CurrentBar = Instantiate(barToInstantiate, barParent).GetComponent<Bar>();
        CurrentBar.StartPosition = StartPosition;
        
        if(GameManager.AllPoints.ContainsKey(StartPosition) == true)
        {
            CurrentStartPoint = GameManager.AllPoints[StartPosition];
        }
        else
        {
            CurrentStartPoint = Instantiate(PointToInstantiate, StartPosition, Quaternion.identity, PointParent).GetComponent<Point>();
            GameManager.AllPoints.Add(StartPosition, CurrentStartPoint);
        }


        CurrentEndPoint = Instantiate(PointToInstantiate, StartPosition, Quaternion.identity, PointParent).GetComponent<Point>();

    }

    private void FinishBarCreation()
    {
        if(GameManager.AllPoints.ContainsKey(CurrentEndPoint.transform.position) == true)
        {
            Destroy(CurrentEndPoint.gameObject);
            CurrentEndPoint = GameManager.AllPoints[CurrentEndPoint.transform.position];
        }
        else
        {
            GameManager.AllPoints.Add(CurrentEndPoint.transform.position, CurrentEndPoint);
        }
        CurrentStartPoint.ConnectedBars.Add(CurrentBar);
        CurrentEndPoint.ConnectedBars.Add(CurrentBar);
        StartBarCreation(CurrentEndPoint.transform.position);

    }


    private void Update()
    {
        if(BarCreationStarted == true)
        {
            Vector2 EndPosition = (Vector2)Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Vector2 Dir = EndPosition - CurrentBar.StartPosition;
            Vector2 ClampedPosition = Vector2.ClampMagnitude(Dir, CurrentBar.maxLength) + CurrentBar.StartPosition;

            CurrentEndPoint.transform.position = (Vector2)Vector2Int.FloorToInt(ClampedPosition);
            CurrentEndPoint.PointID = CurrentEndPoint.transform.position;
            CurrentBar.UpdateCreatingBar(CurrentEndPoint.transform.position);
        }
    }
}
