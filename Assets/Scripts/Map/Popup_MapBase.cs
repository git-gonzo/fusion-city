using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;

public class Popup_MapBase : MonoBehaviour
{
    public Action OnCancel;
    public Button btnClose;
    protected CanvasGroup content;
    protected bool isActive;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Descrip;
    public TextMeshProUGUI txtMissionsCount;
    public TextMeshProUGUI txtJobsCount;
    public TextMeshProUGUI txtTravelTime;
    public TextMeshProUGUI txtPeopleCount;
    public GameObject missionReady;
    public GameObject missionPending;
    public GameObject jobsContainer;
    public GameObject peopleContainer;
    public GameObject missionsContainer;
    public GameObject vehiclesContainer;
    public VehicleIconController icon;

    protected SO_Building buildingData;
    protected List<SO_Vehicle> vehicles;
    protected List<int> missions;
    protected PlayerData playerData => GameManager.Instance.playerData;


    [SerializeField] private LocalizeStringEvent locTitle;
    [SerializeField] private LocalizeStringEvent locDescrip;

    public virtual void Show(SO_Building building)
    {
        if (!isActive || !gameObject.activeSelf)
        {
            //Debug.Log("Showing popup was not active");
            //Animate In
            if (content == null) { content = GetComponent<CanvasGroup>(); }
            content.GetComponent<CanvasGroup>().alpha = 0;
            gameObject.SetActive(true);
            content.DOFade(1, 0.3f);
            isActive = true;
        }
        else
        {
            if(content.GetComponent<CanvasGroup>().alpha < 1)
            {
                content.DOFade(1, 0.3f);
            }
            else
            {
                Debug.Log("It was active, so no animation");
            }
        }
        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(Cancel);
        /*if (buildingData == building)
        {
            FillJobs();
            return;
        }*/
        buildingData = building;
        FillData();
    }

    protected virtual void FillData()
    {
        FillTexts();
        FillMissions();
        FillJobs();
        FillVehicles();
    }
    protected void FillTexts()
    {
        if (GameServerConfig.Instance.ConfigHasBuilding(buildingData.buildingType))
        {
            GameServerConfig.Instance.SetBuildingLocTitle(buildingData.buildingType, Title, "_name");
            GameServerConfig.Instance.SetBuildingLocTitle(buildingData.buildingType, Descrip, "_descrip");
        }
        else
        {
            Title.text = buildingData.buildingName;
            Descrip.text = buildingData.buildingDescrip;
        }
    }
    protected int FillVehicles()
    {
        vehicles = GetVehicles();
        return vehicles != null ? vehicles.Count : 0;
    }
    protected int FillMissions()
    {
        var missionsCount = GetMissionsCountInBuilding();
        if (txtMissionsCount)
        {
            txtMissionsCount.text = missionsCount.ToString();
        }
        var hasMissionReady = false;
        var missionsInLocation = GameManager.Instance.mergeModel.mapMissionsNew.Where(m => m.location == buildingData.buildingType);
        foreach ( var mission in missionsInLocation)
        {
            if (GameManager.Instance.mergeModel.IsMapMissionReady(mission)) hasMissionReady = true;
        }
        if (GameManager.Instance.mergeModel.IsLimitedMissionReady()) hasMissionReady = true;
        if (GameManager.Instance.mergeModel.IsCharacterMissionReady()) hasMissionReady = true;

        missionReady.SetActive(hasMissionReady);
        missionPending.SetActive(!hasMissionReady);
        return missionsCount;
    }
    protected int FillJobs()
    {
        int jobsCount = GetPossibleJobsCount();
        if (txtJobsCount)
        {
            txtJobsCount.text = jobsCount.ToString();
        }
        return jobsCount;
    }
    
    protected int FillPeople()
    {
        int peopleCount = GetPeopleInside();
        if (txtPeopleCount)
        {
            txtPeopleCount.text = peopleCount.ToString();
        }
        return peopleCount;
    }

    public void Cancel()
    {
        Hide();
        Title.DOFade(1, 0.1f).OnComplete(() =>
        {
            OnCancel?.Invoke();
        });
    }

    public void Hide(bool withAnim = true)
    {
        if (isActive || gameObject.activeSelf)
        {
            content.DOFade(0, withAnim ? 0.3f:0f).OnComplete(() => { gameObject.SetActive(false); });
            //Debug.Log("Hide PopUP");
        }
        isActive = false;
    }

    protected int GetPossibleJobsCount()
    {
        return buildingData.boardConfig != null ? 1 : 0;
    }

    protected int GetPeopleInside()
    {
        var building = GameManager.Instance.mapManager.GetBuildingInteractiveFromType(buildingData.buildingType);
        return building._playersInBuilding.Count;
    }

    private List<SO_Vehicle> GetVehicles()
    {
        if (buildingData.vehiclesInShop != null && buildingData.vehiclesInShop.Count > 0)
        {
            return buildingData.vehiclesInShop;
        }
        return null;
    }
    private int GetMissionsCountInBuilding()
    {
        return GameManager.Instance.MergeManager.boardModel.MissionsCountInLocation(buildingData.buildingType);
    }


    public void StartMoving()
    {
        //If there is only a mission and no board show msg 'don't go for nothing'
        if (missions != null && jobsContainer!=null && missionPending != null && vehiclesContainer != null 
            && missions.Count > 0 && !jobsContainer.activeSelf && missionPending.activeSelf && !vehiclesContainer.activeSelf)
        {
            GameManager.Instance.PopupsManager.ShowPopupYesNo("Collect items", "You don't have all the items needed to complete the mission here", PopupManager.PopupType.ok);
            return;
        }
        //TODO: Check if is already travelling
        if (GameManager.Instance.IsTravelling)
        {
            Cancel();
            GameManager.Instance.PopupsManager.ShowPopupYesNo(
            "Cancel Travell",
            "You are on the way to <color=green>" + GameManager.Instance.TravellDestination + "</color><br> Do you want to <color=yellow>change</color> destination?",
            PopupManager.PopupType.yesno,
            CancelTravellAndGo
            );
            return;
        }
        StartMovingEnd();
    }

    public virtual void StartMovingEnd()
    {
        Cancel();
        GameManager.Instance.StartTravelling(buildingData.buildingType);
    }

    private void CancelActivityAndGo()
    {

        StartMovingEnd();
    }
    private void CancelTravellAndGo()
    {
        GameManager.Instance.CancelTravelling();
        StartMovingEnd();
    }

    protected int GetTimeToGetBuilding(BuildingType building)
    {
        return GameManager.Instance.mapManager.GetTimeToReachBuilding(building);
    }
    protected void SetIconBestVehicle()
    {
        var bestVehicle = GameManager.Instance.playerData.GetBestVehicle();
        if (icon != null) icon.SetIcon(bestVehicle != null ? bestVehicle.vehicleType : 0);
    }
}
