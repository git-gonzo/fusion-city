using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

public class PopupPurchaseReward : PopupBase
{
    [SerializeField] private TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtObjectRewarded;
    [SerializeField] private RewardItemUniversal rewardPrefab;
    [SerializeField] private RewardsContainer rewardsContainer;
    [SerializeField] private VehicleThumbnail vehicleThumbnail;

    public void Init(List<RewardData> rewards, bool applyRewards)
    {
        Base(false,true);
        rewardsContainer.FillRewards(rewards, GameManager.Instance.topBar, true);
        transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
        btnClose.onClick.RemoveAllListeners();
        if (applyRewards)
        {
            btnClose.onClick.AddListener(OnCloseAndClaim);
        }
        else
        {
            btnClose.onClick.AddListener(OnClose);
        }
    }
    public void InitWithGift(List<RewardData> rewards)
    {
        Base(false, true);
        GameManager.SetLocString("UI", "GiftSent", txtTitle);
        rewardsContainer.FillRewards(rewards, GameManager.Instance.topBar, true);
        transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
        btnClose.onClick.AddListener(OnCloseAndClaim);
    }

    public void InitWithText(string objectName)
    {
        Base(true);
        txtObjectRewarded.text = objectName;
        btnClose.onClick.AddListener(OnClose);
        Show();
    }
    public void InitWithVehicle(SO_Vehicle v)
    {
        Base(true,true,true);
        vehicleThumbnail.GenerateThumbnail(v.id);
        txtObjectRewarded.text = v.vehicleName;
        //btnClose.onClick.RemoveAllListeners();
        //btnClose.onClick.AddListener(OnClose);
        Show();
    }

    private void OnCloseAndClaim()
    {
        rewardsContainer.ClaimRewards();
        OnClose();
    }

    private void Base(bool hasTitle = false, bool hasReward = false, bool hasVehicle = false)
    {
        vehicleThumbnail.gameObject.SetActive(hasVehicle);
        rewardsContainer.gameObject.SetActive(hasReward);
        txtObjectRewarded.gameObject.SetActive(hasTitle);
    }
}