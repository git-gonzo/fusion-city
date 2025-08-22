using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using Assets.Scripts.MergeBoard;

public class PopupEnergy : PopupBase
{
    public Transform energyIcon;
    public ButtonBuy btnbuy;
    public Button btnVideo;
    public TextMeshProUGUI txtEnergyRefillWithGemsAmount;
    public TextMeshProUGUI txtEnergyRefillWithVideoAmount;
    RewardData energyRefillCost => GameConfigMerge.instance.energyRefillCost;
    MergeBoardModel mergeModel => GameManager.Instance.MergeManager.boardModel;
    public MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;

    Action<int> _onBuyCallback;
    int energyGems => GameConfigMerge.instance.energyRefillAmountWithGems;
    int energyVideo => GameConfigMerge.instance.energyRefillAmountWithVideo;
    int energyVideoCooldownMinutes = 5;
    DateTime NextVideoTime { 
        get => UIUtils.GetTimeStampByKey("nextEnergyVideoTime");
    }
    bool CanViewAdd => NextVideoTime <= DateTime.Now;

    public void Init(Action<int> onBuyCallback = null)
    {
        _onBuyCallback = onBuyCallback;
        //transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
        btnbuy.Init(energyRefillCost);
        btnbuy.OnBuyAction = OnBuy;
        AnimIn();
        btnVideo.onClick.RemoveAllListeners();
        btnVideo.onClick.AddListener(WatchVideo);
        txtEnergyRefillWithVideoAmount.text = energyVideo.ToString();
        txtEnergyRefillWithGemsAmount.text = energyGems.ToString();
        btnbuy.button.interactable = mergeModel.energy < mergeConfig.maxEnergy - energyGems;
        btnVideo.interactable = CanViewAdd && mergeModel.energy < mergeConfig.maxEnergy - energyVideo;
    }

    private void OnBuy()
    {
        if (GameManager.Instance.HasEnoughCurrency(energyRefillCost, true))
        {
            //TODO: Cool sound
            GameManager.TryToSpend(energyRefillCost);
            _onBuyCallback?.Invoke(GameConfigMerge.instance.energyRefillAmountWithGems);
            TrackingManager.TrackRefillEnergy();
            OnClose();
        }
        else
        {
            GameManager.Log("Not enough gems to buy energy");
        }
    }

    private void WatchVideo()
    {
        UIUtils.SaveTimeStamp("nextEnergyVideoTime", DateTime.Now.AddMinutes(energyVideoCooldownMinutes));
        GameManager.Instance.PlayVideoAd(OnVideoEnd);
    }

    private void OnVideoEnd()
    {
        UIUtils.FlyingParticles(RewardType.Energy, energyIcon.position, 8, ()=> { _onBuyCallback?.Invoke(GameConfigMerge.instance.energyRefillAmountWithVideo); });
        OnClose();
    }
    private void AnimIn()
    {
        //this.transform.DOScale(0.5f, 0);
        var seq = DOTween.Sequence();
        seq.Insert(0, btnbuy.transform.DOScale(0, 0.4f).SetEase(Ease.OutBack).From());
        seq.Insert(0.2f, btnVideo.transform.DOScale(0, 0.4f).SetEase(Ease.OutBack).From());
    }
}
