using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MergeMapMissionElement : MonoBehaviour
{
    public MergeMapMissionRequestItem requestItem;
    public RewardItemUniversal rewardPrefab;
    public Transform missionsContainter;
    //public Transform rewardsContainer;
    public RewardsContainer rewardsContainer;
    public TextMeshProUGUI txtShowLocation;
    public TextMeshProUGUI txtDeliverIn;
    public TextMeshProUGUI txtLocation;
    public TextMeshProUGUI txtTimeLeft;
    public Button btnLocation;
    public Button btnCompleteMission;
    public Image bgRequest;
    public Color colorPending;
    public Color colorReady;
    public AudioSource missionCompleteSound;

    protected Action _onClose;
    protected BuildingType _missionLocation;
    protected bool _canBeCompleted = false;
    public bool CanBeCompleted => _canBeCompleted;
    bool inLocation => _missionLocation == PlayerData.playerLocation;
    public MergeBoardModel mergeModel => GameManager.Instance.MergeManager.boardModel;

    protected List<MergeMapMissionRequestItem> requestItems;
    protected MapMissionCloud _missionConfig;
    protected LimitedMissionMapConfig _limitedMissionConfig;
    
    public void AddRequests(MapMissionCloud request, Action onClose)
    {
        RemoveObjects();
        _missionLocation = request.location;
        _missionConfig = request;
        _onClose = onClose;
        //Load Request Items
        requestItems = new List<MergeMapMissionRequestItem>();
        for (var j = 0; j < request.piecesRequest.Count; j++) {
            var r = Instantiate(requestItem, missionsContainter);
            r.AddItem(request.piecesRequest[j]);
            requestItems.Add(r);
        }
        rewardsContainer.FillRewards(request.rewards, GameManager.Instance.topBar, true);
        CheckCanbeCompleted();
        CheckLocation();
        btnCompleteMission.onClick.AddListener(CompleteMission);
    }

    public void AddRequests(LimitedMissionMapConfig request, Action onClose)
    {
        RemoveObjects();
        _missionLocation = request.location;
        _limitedMissionConfig = request;
        _onClose = onClose;
        //Load Request Items
        requestItems = new List<MergeMapMissionRequestItem>();
        for (var j = 0; j < request.piecesRequest.Count; j++) {
            var r = Instantiate(requestItem, missionsContainter);
            r.AddItem(request.piecesRequest[j]);
            requestItems.Add(r);
        }
        rewardsContainer.FillRewards(request.rewardInstant,GameManager.Instance.topBar, true);
        CheckCanbeCompleted();
        CheckLocation();
        txtTimeLeft.text = UIUtils.FormatTime((request.endTime - DateTime.Now).TotalSeconds);
        btnCompleteMission.onClick.AddListener(CompleteLimitedMission);
    }
    public void AddRequests2(LimitedMissionMapConfig request, Action onClose)
    {
        RemoveObjects();
        _missionLocation = request.location;
        _limitedMissionConfig = request;
        _onClose = onClose;
        //Load Request Items
        requestItems = new List<MergeMapMissionRequestItem>();
        for (var j = 0; j < request.piecesRequest.Count; j++) {
            var r = Instantiate(requestItem, missionsContainter);
            r.AddItem(request.piecesRequest[j]);
            requestItems.Add(r);
        }
        rewardsContainer.FillRewards(request.rewardInstant,GameManager.Instance.topBar, true);
        CheckCanbeCompleted();
        CheckLocation();
        txtTimeLeft.text = UIUtils.FormatTime((request.endTime - DateTime.Now).TotalSeconds);
        btnCompleteMission.onClick.AddListener(CompleteLimitedMission);
    }

    public void LazyUpdate()
    {
        if(_limitedMissionConfig != null)
        {
            var secondsLeft = (_limitedMissionConfig.endTime - DateTime.Now).TotalSeconds;
            txtTimeLeft.text = UIUtils.FormatTime(secondsLeft);
            if(_limitedMissionConfig.endTime < DateTime.Now)
            {
                mergeModel.limitedMission = null;
                Destroy(gameObject);
            }
        }
    }

    protected void CheckLocation()
    {
        txtShowLocation.gameObject.SetActive(!inLocation);
        txtDeliverIn.gameObject.SetActive(!inLocation);
        txtLocation.gameObject.SetActive(!inLocation);
        btnLocation.gameObject.SetActive(!inLocation);
        btnCompleteMission.gameObject.SetActive(inLocation && _canBeCompleted);
        if (inLocation)
        {
            if (_canBeCompleted)
            {
                //btnCompleteMission.onClick.AddListener(CompleteMission);
            }
        }
        else 
        {
            if (PlayerData.MapMissionsCount < 3)
            {
                txtShowLocation.gameObject.SetActive(_canBeCompleted);
                btnLocation.gameObject.SetActive(_canBeCompleted);
            }
            GameServerConfig.Instance.SetBuildingLocTitle(_missionLocation, txtLocation, "_name");
            //txtLocation.text = _missionLocation.ToString();
            btnLocation.onClick.AddListener(ShowLocation);
        }
    }

    protected void CheckCanbeCompleted()
    {
        _canBeCompleted = true;
        foreach(var r in requestItems)
        {
            if (!r.isReady)
            {
                _canBeCompleted = false;
                break;
            }
        }
        bgRequest.color = _canBeCompleted ? colorReady : colorPending;
        if(PlayerData.MapMissionsCount < 3)
        {
            btnLocation.gameObject.SetActive(_canBeCompleted);
            if (!_canBeCompleted)
            {
                return;
            }
            btnLocation.transform.DOPunchScale(Vector3.one * 0.2f, 0.7f, 1).SetLoops(-1);
        }
    }
    protected void CompleteMission()
    {
        CompleteCommon();
        GameManager.Instance.CompleteMapMission(_missionConfig);
    }
    
    private void CompleteLimitedMission()
    {
        CompleteCommon();
        GameManager.Instance.CompleteLimitedMission(_limitedMissionConfig);
    }
    protected void CompleteCommon()
    {
        if (!_canBeCompleted)
        {
            Debug.Log("CANNOT BE COMPLETED");
        }
        btnCompleteMission.onClick.RemoveAllListeners();
        rewardsContainer.ClaimRewardsBySteps(RemoveMission);
        missionCompleteSound.Play();
    }
    private void RemoveMission()
    {
        transform.DOScale(0, 0.6f).SetEase(Ease.InBack).OnComplete(()=> { Destroy(gameObject); });
    }

    private void ShowLocation()
    {
        if (GameManager.Instance.MergeManager.IsBoardActive)
        {
            GameManager.Instance.ShowMergeBoard(false);
        }
        GameManager.Instance.ShowMergeMapMissions(false, _missionLocation);
        GameManager.Instance.FocusOnBuilding(_missionLocation);
        if(_canBeCompleted)// && PlayerData.MapMissionsCount == 0)
        {
            GameManager.Instance.tutorialManager.StartTutorialGiveVehicle();
        }
    }

    protected void RemoveObjects()
    {
        while (missionsContainter.childCount > 0)
        {
            DestroyImmediate(missionsContainter.GetChild(0).gameObject);
        }
    }
}
