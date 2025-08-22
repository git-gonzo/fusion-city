using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TravellingController : LowBarProgressControllerBase
{
    public VehicleThumbnail thumbnail;
    public VehicleIconController vehicleIcon;
    public Transform walkingTooltip;

    private int duration { get => GameManager.Instance.mapManager.GetTimeToReachBuilding(GameManager.Instance.playerData.TravellingDestination);}
    private int _durationCache;
    private bool _isShowingTooltip = false;
    public override void Init()
    {
        base.Init();
        btnCancel.onClick.AddListener(CancelTravelPopup);
        btnFinishNow.onClick.AddListener(OnFinishNow);
    }
    public void Show()
    {
        if (!_initialized) Init();
        nextCheck = DateTime.Now;
        _durationCache = duration;
        progressBar.UpdateProgressBar(duration, GameManager.Instance.playerData.travellingTimeLeft.TotalSeconds, false);
        transform.DOMoveX(_startPositionX, 1).SetEase(Ease.OutQuint).OnComplete(()=> 
            {
                if (GameManager.Instance.PlayerLevel > 1 && GameManager.Instance.playerData.GetBestVehicle() == null)
                {
                    ShowWalkingTooltip();
                }
            });
        GameManager.Instance.mapManager.PlayerArriveDestionation = OnTravelComplete;
        var destination = GameManager.Instance.mapManager.GetBuildingDataFromType(GameManager.Instance.playerData.TravellingDestination).buildingName;
        textTitle.text = "On the way to <color=yellow>" + destination;
        //Set vehicle Icon
        var vehicle = GameManager.Instance.playerData.GetBestVehicle();
        thumbnail.gameObject.SetActive(vehicle != null && vehicle.vehicleType != VehicleType.None);
        vehicleIcon.gameObject.SetActive(vehicle == null || vehicle.vehicleType == VehicleType.None);
        if(vehicle != null && vehicle.vehicleType != VehicleType.None)
        {
            thumbnail.GenerateThumbnail(PlayerData.vehicleSelected);
        }
        else
        {
            vehicleIcon.SetIcon(0);
        }
        walkingTooltip.DOScale(0, 0);
    }


    public void OnTravelComplete()
    {
        GameManager.Log("Travell completed to " + PlayerData.playerLocation);
        TrackingManager.AddTracking(TrackingManager.Track.TravelComplete, "Destiation", PlayerData.playerLocation.ToString());
        GameManager.Instance.mapManager.PlayerArriveDestionation = null;
        GameManager.Instance.LeaderboardManager.GetOtherPlayersVehiclesAndLocations();
        Hide();
    }
    
    private void Update()
    {
        if (!_initialized) return;

        if (DateTime.Now >= nextCheck && GameManager.Instance.playerData.IsTravelling)
        {
            progressBar.UpdateProgressBar(_durationCache, GameManager.Instance.playerData.travellingTimeLeft.TotalSeconds, true);
            nextCheck.AddMilliseconds(100);
        }
        if (walkingTooltip != null && Input.GetMouseButtonDown(0))
        {
            HideTooltip();
        }
    }

    private void CancelTravelPopup()
    {
        GameManager.Instance.PopupsManager.ShowPopupYesNo(
            "Cancel Travelling",
            "Are you sure you want to <color=yellow>cancel</color> the travelling?",
            PopupManager.PopupType.yesno,
            CancelTravelling
            );
    }

    private void CancelTravelling()
    {
        Debug.Log("Bien, Cancelar");
        GameManager.Instance.CancelTravelling();
    }

    private void OnFinishNow()
    {
        if(GameManager.Instance.TryToSpend(progressBar.finishNowPrice,RewardType.Gems))
        {
            TrackingManager.AddTracking(TrackingManager.Track.SpeedUpTravelling, "Success", true);
            GameManager.Instance.playerData.FinishNowTravelling(true);
            //if(GameManager.Instance.PlayerLevel>1) GameManager.Instance.dailyTaskManager.OnTravel();
        }
        else
        {
            TrackingManager.AddTracking(TrackingManager.Track.SpeedUpTravelling, "NotEnoughGems", true);
        }
    }

    private void ShowWalkingTooltip()
    {
        if (walkingTooltip == null) return;
        if (tooltipShowedTimes > 2) return;
        if (!_isShowingTooltip)
        {
            tooltipShowedTimes++;
            walkingTooltip.gameObject.SetActive(true);
            _isShowingTooltip = true;
            
            walkingTooltip.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            HideTooltip();
        }
    }

    public void HideTooltip()
    {
        if (walkingTooltip != null && _isShowingTooltip)
        {
            _isShowingTooltip = false;
            walkingTooltip.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }
    
    int tooltipShowedTimes
    {
        get => PlayerPrefs.GetInt("tooltipShowedTimes");
        set { PlayerPrefs.SetInt("tooltipShowedTimes", value); }
    }

}
