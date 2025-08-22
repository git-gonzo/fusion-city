using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.Scripts.MergeBoard;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    public List<ShopResourcePack> coinsPacks;
    public TextMeshProUGUI txtGetVideo;
    public Button btnWatchVideo;
    public int goldVideoReward;
    public int gemsVideoReward;
    public int energyVideoReward;
    public Transform coinsContainer;
    public Transform gemsContainer;
    public Transform energyDestinationTransform;
    public int videoInterval = 3599;
    public TextMeshProUGUI txtVideoTimeLeft;
    public TextMeshProUGUI txtDailyDealTimeLeft;
    public GameObject videoTimeLeftContainer;
    public GameObject videoLoading;
    public GameObject chestsItemsContainer;
    public GameObject dailyDealItemsContainer;
    public GameObject dailyDealTimeLeftContainer;
    public ShopDailyDealItem dailyDealItemPrefab;
    public Image shopBG;
    public ShopDailyDealConfig dailyDealConfig;
    public ShopDailyDealConfig chestsConfig;

    private bool isFirstTime = true;
    private TimeSpan _lastVideoWatched;
    private TimeSpan _timeToNextDailyDeal;
    private DateTime lastVideoTime;
    private DateTime nextDailyDealTime;
    private bool giveRewardsFromVideo;
    private MergeBoardModel mergeModel => GameManager.Instance.mergeModel;


    public void ResetVideoTimeLeft() {
        lastVideoTime = DateTime.Now;
        UIUtils.SaveTimeStamp("LastVideoWatched", lastVideoTime);
        UpdateLastVideoWatched();
    }
    public void UpdateLastVideoWatched()
    {
        if (PlayerPrefs.HasKey("LastVideoWatched"))
        {
            _lastVideoWatched = UIUtils.GetTimeStampByKey("LastVideoWatched") - System.DateTime.Now;
        }
        else
        {
            _lastVideoWatched = new TimeSpan();
        }
    }
    private void SetLastVideoWatched()
    {
        lastVideoTime = DateTime.Now.AddSeconds(videoInterval);
        UIUtils.SaveTimeStamp("LastVideoWatched", lastVideoTime);
        UpdateLastVideoWatched();
    }
    private void SetNextDailyDeal()
    {
        nextDailyDealTime = DateTime.Now.AddHours(8);
        UIUtils.SaveTimeStamp("nextDailyDeal", nextDailyDealTime);
    }
    private void UpdateNextDailyDealTimeLeft()
    {
        _timeToNextDailyDeal = UIUtils.GetTimeStampByKey("nextDailyDeal") - DateTime.Now;
    }

    public void LazyUpdate()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        RefreshVideoState();
        RefreshDailyDealState();
        UpdateLastVideoWatched();
    }

    public void OnShow()
    {
        ShowBG();
        if (isFirstTime)
        {
            isFirstTime = false;
            btnWatchVideo.onClick.AddListener(OnWatchVideoStart);
        }

        //Update packs price availability
        foreach (var pack in coinsPacks)
        {
            pack.updatePriceState();
        }

        //Load Daily Deals
        GameManager.RemoveChildren(dailyDealItemsContainer);
        if (mergeModel.currentDailyDeal == null)
        {
            //Create daily deal model
            GetNextDailyDealFromConfig();
            SetNextDailyDeal();
        }
        else
        {
            UpdateNextDailyDealTimeLeft();
        }
        FillDealItems();
        FillChestsItems();
        LazyUpdate();
    }

    private void FillDealItems()
    {
        var delay = 0.1f;
        foreach (var item in mergeModel.currentDailyDeal.items)
        {
            var dealItem = Instantiate(dailyDealItemPrefab, dailyDealItemsContainer.transform);
            dealItem.ShowItem(item, delay, transform);
            delay += 0.2f;
        }
    }
    
    private void FillChestsItems()
    {
        var delay = 0.1f;
        GameManager.RemoveChildren(chestsItemsContainer);
        foreach (var item in chestsConfig.itemsSet[0].items)
        {
            var chestItem = Instantiate(dailyDealItemPrefab, chestsItemsContainer.transform);
            chestItem.ShowChest(item, delay, transform);
            delay += 0.2f;
        }
    }

    private void GetNextDailyDealFromConfig()
    {
        var index = PlayerPrefs.GetInt("dealIndex");
        if (PlayerPrefs.HasKey("dealIndex") && index >= dailyDealConfig.itemsSet.Count - 1) index = 0; else index++;
        mergeModel.currentDailyDeal = dailyDealConfig.itemsSet[index];
        //Set the 1st item as the higher tier in missions
        var candidate = mergeModel.GetFirstTopMissionItem();
        if (candidate != null)
        {
            var itemshop = new ItemShop(candidate, new RewardData(RewardType.Gems, 10 * Mathf.Max(1, candidate.Lvl - 2)), 1);
            mergeModel.currentDailyDeal.items[0] = itemshop;
        }
        PlayerPrefs.SetInt("dealIndex", index);
        mergeModel.SaveDailyDeals();
    }

    private void ShowBG()
    {
        shopBG.DOFade(0, 0);
        shopBG.DOFade(1, 0.3f);
    }
    public void HideBG()
    {
        shopBG.DOFade(0, 0.2f);
    }

    private void RefreshVideoState()
    {
        if (!GameManager.Instance.mapManager.playerInputEnable) return;
        //Debug.Log("VideoInterval " + videoInterval + ", _lastVideoWatched.TotalSeconds = " + _lastVideoWatched.TotalSeconds);

        if (!GameManager.Instance.IsShowingShop) return;

        videoLoading.SetActive(false);
        txtGetVideo.gameObject.SetActive(_lastVideoWatched.TotalSeconds <= 0);
        btnWatchVideo.gameObject.SetActive(_lastVideoWatched.TotalSeconds <= 0);
        videoTimeLeftContainer.SetActive(_lastVideoWatched.TotalSeconds > 0);
        if (_lastVideoWatched.TotalSeconds > 0)
        {
            txtVideoTimeLeft.text = UIUtils.FormatTime(_lastVideoWatched.TotalSeconds);
        }
        else if (!GameManager.Instance.videoAdsManager.videoLoaded)
        {
            txtGetVideo.gameObject.SetActive(false);
            btnWatchVideo.gameObject.SetActive(false);
            videoLoading.SetActive(true);
            GameManager.Instance.videoAdsManager.CheckReady();
        }
    }

    private void RefreshDailyDealState()
    {
        UpdateNextDailyDealTimeLeft();
        txtDailyDealTimeLeft.text = "Next Deal in " + UIUtils.FormatTime(_timeToNextDailyDeal.TotalSeconds);
        if (_timeToNextDailyDeal.TotalSeconds <= 0)
        {
            GetNextDailyDealFromConfig();
            GameManager.RemoveChildren(dailyDealItemsContainer);
            FillDealItems();
            SetNextDailyDeal();
        }
    }

    public void OnWatchVideoStart()
    {
        TrackingManager.TrackVideoAdStart();
        GameManager.Instance.videoAdsManager.OnVideoEndSuccess.RemoveAllListeners();
        GameManager.Instance.videoAdsManager.OnVideoEndSuccess.AddListener(OnWatchVideoCompleted);
        GameManager.Instance.videoAdsManager.PlayVideo();

    }

    public void OnWatchVideoCompleted()
    {
        giveRewardsFromVideo = true;
        TrackingManager.TrackVideoAdEnd();
    }

    private void GiveRewardsFromVideo()
    {
        SetLastVideoWatched();
        RefreshVideoState();
        RewardData coins = new RewardData(RewardType.Coins, goldVideoReward);
        RewardData gems = new RewardData(RewardType.Gems, gemsVideoReward);
        RewardData energy = new RewardData(RewardType.Energy, energyVideoReward);
        GameManager.Instance.AddRewardToPlayer(coins, true);
        GameManager.Instance.AddRewardToPlayer(gems, true);
        GameManager.Instance.AddRewardToPlayer(energy, true);
        UIUtils.FlyingParticles(RewardType.Coins, coinsContainer.position, Mathf.Min(goldVideoReward, 10), null);
        UIUtils.FlyingParticles(RewardType.Gems, gemsContainer.position, Mathf.Min(gemsVideoReward, 10), null);
        UIUtils.FlyingParticles(RewardType.Energy, energyDestinationTransform.position, Mathf.Min(energyVideoReward, 10), null);
        //GameManager.Instance.server.SendAction(ActionType.ShopVideoAd, RewardType.Gems, gemsVideoReward);
        //GameManager.Instance.server.SendAction(ActionType.ShopVideoAd, RewardType.Coins, goldVideoReward);
    }

    private void Update()
    {
        if (giveRewardsFromVideo)
        {
            giveRewardsFromVideo = false;
            GiveRewardsFromVideo();
        }
    }

    public void ADMIN_SetNextDealIn10Sec()
    {
        nextDailyDealTime = DateTime.Now.AddSeconds(15);
        UIUtils.SaveTimeStamp("nextDailyDeal", nextDailyDealTime);
    }

}