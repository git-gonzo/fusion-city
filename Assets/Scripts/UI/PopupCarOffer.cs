using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PopupCarOffer : PopupBase
{
    public TextMeshProUGUI txtCarName;
    public RewardsContainer rewardsContainer;
    public Text txtPrice;

    private int _vehicleID;
    private SO_Vehicle _vehicle;
    private List<RewardData> _rewards;

    bool purchased;

    public void Init(List<RewardData> rewards, int vehicleId)
    {
        _vehicleID = vehicleId; ;
        _rewards = rewards;
        //Init rewards
        GameManager.Instance.garaje3D.gameObject.SetActive(true);
        GameManager.Instance.garaje3D.AddVehicle(_vehicleID);
        _vehicle = GameConfigMerge.instance.vehiclesDefinition.Find(v => v.id == _vehicleID);
        txtCarName.text = _vehicle?_vehicle.vehicleName:"No vehicle name";
        rewardsContainer.ClearRewards();
        rewardsContainer.FillRewards(_rewards, GameManager.Instance.topBar, true);
    }

    public void OnBuySuccess()
    {
        purchased = true;
        GameManager.Instance.ShowMapLowerBar(true);
        //var v = new Vehicle(GameManager.Instance.gameConfig.vehiclesDefinition[10]);
        GameManager.Instance.playerData.AddVehicleByID(_vehicleID, false);

        rewardsContainer.ClaimRewardsBySteps(() => {
            OnClose();
            GameManager.Instance.PopupsManager.ShowPopupPurchaseVehicle(_vehicle,true);
        });
    }

    public override void Show()
    {
        base.Show();
        Sequence seq = DOTween.Sequence();
        for (var i = 0; i < rewardsContainer.rewardItems.Count; i++)
        {
            seq.Insert(0.1f * i + 0.1f, rewardsContainer.rewardItems[i].transform.DOScale(0, 0.4f).From().SetEase(Ease.OutBack));
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
        GameManager.Instance.garaje3D.gameObject.SetActive(false);
        TrackingManager.TrackCarOffer(_vehicleID, purchased);
    }


    public void OnProductFetched(Product product)
    {
        txtPrice.text = product.metadata.localizedPriceString;
    }
}
