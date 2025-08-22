using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts.MergeBoard;
using DG;
using DG.Tweening;

public class PopupWelcomePack : PopupBase
{
    public TextMeshProUGUI txtTimeLeft;
    public RewardsContainer rewardsContainer;

    private int _vehicleID;

    private void Update()
    {
        txtTimeLeft.text = UIUtils.FormatTime(GameManager.Instance.sideBar.GetWPTimeLeft());
    }

    public void OnBuySuccess()
    {
        PlayerPrefs.SetInt("WP_Seen", 3);
        GameManager.Instance.ShowMapLowerBar(true);
        //var v = new Vehicle(GameManager.Instance.gameConfig.vehiclesDefinition[10]);
        GameManager.Instance.playerData.AddVehicleByID(106,false);

        rewardsContainer.ClaimRewardsBySteps(()=> {
            OnClose(); 
            GameManager.Instance.PopupsManager.ShowPopupPurchaseVehicleBuilding(GameManager.Instance.gameConfig.vehiclesDefinition[10].vehicleName);
        });
    }

    public override void Show()
    {
        base.Show();
        //Init rewards
        GameManager.Instance.garaje3D.gameObject.SetActive(true);
        GameManager.Instance.garaje3D.AddVehicle(106);
        rewardsContainer.rewardItems[0].InitSimple(new RewardData(RewardType.Gems, 500));
        rewardsContainer.rewardItems[1].InitSimple(new RewardData(RewardType.MergeItem, 1), new PieceDiscovery(PieceType.Cash, 8), GameManager.Instance.lowerBar.transform.parent);
        rewardsContainer.rewardItems[2].InitSimple(new RewardData(RewardType.MergeItem, 1), new PieceDiscovery(PieceType.BoosterChest,0), GameManager.Instance.lowerBar.transform.parent);
        rewardsContainer.rewardItems[3].InitSimple(new RewardData(RewardType.MergeItem, 1), new PieceDiscovery(PieceType.RouleteTicketSpecial,4), GameManager.Instance.lowerBar.transform.parent);
        Sequence seq = DOTween.Sequence();
        for (var i = 0;i < rewardsContainer.rewardItems.Count; i++)
        {
            seq.Insert(0.1f*i+0.1f, rewardsContainer.rewardItems[i].transform.DOScale(0, 0.4f).From().SetEase(Ease.OutBack));
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
        GameManager.Instance.garaje3D.gameObject.SetActive(false);
    }
}
