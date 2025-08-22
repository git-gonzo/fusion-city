using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Unity.Cinemachine;

public class HouseCameraTracker : MonoBehaviour
{
    public static HouseCameraTracker Instance
    {
        get
        {
            return _instance;
        }
    }
    private static HouseCameraTracker _instance;
    public CinemachineCamera vCam;
    public GameObject camaraTracker;
    public GameObject camaraTrackerView;

    Vector3 startPoint;
    Vector3 finalPoint;
    Vector3 startTrackerPos;
    bool bDragging { get { return finalPoint.magnitude > minDrag; } }
    //float modX = 1;
    float minDrag = 0.01f;
    //float modY = 1;
    float strength = 3;


    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //lastTrackerPos = Vector3.zero;
            startTrackerPos = camaraTracker.transform.position;
            startPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(0) )
        {

            finalPoint = startPoint - Camera.main.ScreenToViewportPoint(Input.mousePosition) ;
            if (bDragging && !UIUtils.IsPointerOverUIObject())
            {
                Vector3 finalPos = new Vector3(
                    Mathf.Max(1.21f, Mathf.Min(4.45f, startTrackerPos.x + finalPoint.x * strength)), //finalPoint.x * strength,
                    camaraTracker.transform.position.y,
                    camaraTracker.transform.position.z
                );
                //camaraTracker.transform.position = startTrackerPos + finalPos;
                camaraTracker.transform.position = finalPos;
                Vector3 finalPosView = new Vector3(
                    Mathf.Max(-7.59f, Mathf.Min(15.17f, startTrackerPos.x + (finalPos.x - 3) * strength * strength)), //finalPoint.x * strength,
                    camaraTrackerView.transform.position.y,
                    camaraTrackerView.transform.position.z
                );
                camaraTrackerView.transform.position = finalPosView;
                
            }

        }

        /*
        if (Input.GetMouseButtonUp(0))
        {
            if (GameManager.Instance.IsJobScreenActive() || GameManager.Instance.IsCareersScreenActive()) return;
            finalPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition) - startPoint;
            if (!bDragging)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f) && !EventSystem.current.IsPointerOverGameObject() && !UIUtils.IsPointerOverUIObject())
                {
                    Debug.Log("Tap on " + hit.transform.gameObject.name);
                    HouseDecoSellPoint sellPoint = hit.transform.gameObject.GetComponentInParent<HouseDecoSellPoint>();
                    if (sellPoint)
                    {
                        sellPoint.OnOpen();
                        ChangePointOfView(hit.transform);
                        GameManager.Instance.ShowLowerBarShop(ResetPointOfView);
                    }
                }
            }
            else
            {
                //if (!showingBuilding)
                  //camaraTracker.transform.position = lastTrackerPos;
            }
        }*/
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        if (results.Count > 0) Debug.Log("Anda mira, hay " + results.Count); 
        return results.Count > 0;
    }

    public void ChangePointOfView(Transform target)
    {
        vCam.LookAt = target;
    }

    public void ResetPointOfView()
    {
        vCam.LookAt = camaraTrackerView.transform;
        Vector3 finalPos = new Vector3(
                   2.71f, 
                    camaraTracker.transform.position.y,
                    camaraTracker.transform.position.z
                );
        camaraTracker.transform.DOMove(finalPos,0.2f);
        Vector3 finalPosView = new Vector3(2, camaraTrackerView.transform.position.y, camaraTrackerView.transform.position.z);
        camaraTrackerView.transform.position = finalPosView;
    }
}
