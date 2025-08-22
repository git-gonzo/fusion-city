using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBuildingInfo : PopupBase
{
    public TextMeshProUGUI Title;
    public TextMeshProUGUI txtLevel;
    public TextMeshProUGUI txtBuildingProfit;
    public TextMeshProUGUI txtBuildingCap;
    public TextMeshProUGUI txtBuildingPrice;
    public TextMeshProUGUI txtOwnedProfit;
    public TextMeshProUGUI txtNextLevel;
    public PriceWithIcon upgradePice;
    public Button btnBuyBuilding;
    public Button btnClaimProfit;
    public Button btnUpgrade;
    public GameObject buyContainer;
    public GameObject ownedContainer;
    public GameObject buyIconGems;
    public GameObject buyIconCoins;
    PlayerData playerData => GameManager.Instance.playerData;
    BuildingOwned buildingData => playerData.GetBuildingOwned(_building.buildingType);
    SO_Building _building;
    BuildingConfigRaw buildingConfig;
    RewardData _buildingPrice;
    RewardData upgradeCost => new RewardData(_buildingPrice.rewardType, _buildingPrice.amount * (buildingData.level + 1));
    bool refreshProfit = false;

    void Update()
    {
        if (refreshProfit)
        {
            RefreshProfitTxt();
        }
    }
    public void Show(SO_Building building)
    {
        base.Show();
        _building = building;
        ownedContainer.SetActive(false);
        buildingConfig = GameServerConfig.Instance.GetBuildingConfig(building.buildingType);
        if (GameServerConfig.Instance.ConfigHasBuilding(_building.buildingType))
        {
            GameServerConfig.Instance.SetBuildingLocTitle(_building.buildingType, Title, "_name");
        }
        else
        {
            Title.text = _building.buildingName;
        }
        buildingConfig.BuildingNameTranslated = Title.text;
        txtBuildingProfit.text = buildingConfig.profit.ToString();
        txtBuildingCap.text = buildingConfig.Cap.ToString();
        _buildingPrice = GameServerConfig.Instance.GetBuildingPrice(building.buildingType);

        if (playerData.HasBuilding(building.buildingType) && buildingConfig != null) //Building Owned
        {
            SetStateOwned();
        }
        else
        {
            refreshProfit = false;
            //Debug.Log("Building price = " + _buildingPrice.amount);
            if (_buildingPrice.amount > 0)
            {
                txtBuildingPrice.text = "<color=yellow>" + UIUtils.FormatNumber(_buildingPrice.amount);
                btnBuyBuilding.onClick.RemoveAllListeners();
                btnBuyBuilding.onClick.AddListener(TryBuyBuilding);
                buyIconGems.SetActive(_buildingPrice.rewardType == RewardType.Gems);
                buyIconCoins.SetActive(_buildingPrice.rewardType == RewardType.Coins);
                buyContainer.SetActive(true);
            }
        }
    }
    
    private void SetStateOwned()
    {
        buyContainer.SetActive(false);
        ownedContainer.SetActive(true);
        refreshProfit = true;
        RefreshProfitTxt();
        btnClaimProfit.onClick.RemoveAllListeners();
        btnUpgrade.onClick.RemoveAllListeners();
        btnClaimProfit.onClick.AddListener(ClaimProfit);
        btnUpgrade.onClick.AddListener(TryUpgradeBuilding);
        btnUpgrade.interactable = GameManager.Instance.HasEnoughCurrency(upgradeCost);
        txtLevel.text = buildingData.level.ToString();
        txtNextLevel.text = "Level " + (buildingData.level + 1);
        var profit = buildingData.level == 1 ? buildingConfig.profit : buildingConfig.profit * buildingData.level * 0.75f;
        var cap = buildingData.level == 1 ? buildingConfig.Cap : buildingConfig.Cap * buildingData.level * 0.75f;
        var increaseA = (buildingConfig.profit * (buildingData.level + 1) * 0.75f) - profit;
        var increaseB = (buildingConfig.Cap * (buildingData.level + 1) * 0.75f) - cap;
        txtBuildingProfit.text = $"{(int)profit} <color=green>+{(int)increaseA}";
        txtBuildingCap.text = $"{(int)cap} <color=green>+{(int)increaseB}";
        upgradePice.Init(upgradeCost);
    }

    private void TryBuyBuilding()
    {
        if (GameManager.Instance.HasEnoughCurrency(_buildingPrice))
        {
            GameManager.Instance.PopupsManager.ShowPopupYesNo(
                "Are you sure?",
                "Do you want to buy <color=yellow>" + _building.buildingName +
                "</color>?<br>You will pay <color=yellow>" + UIUtils.FormatNumber(_buildingPrice.amount) + " " + _buildingPrice.currencyName,
                PopupManager.PopupType.yesno,
                () => { //Exito
                    if (!GameManager.TryToSpend(_buildingPrice)) return;
                    playerData.AddBuilding(_building.buildingType);
                    GameManager.Instance.server.SendAction(ActionType.BuyBuilding, _buildingPrice.rewardType, _buildingPrice.amount, (int)_building.buildingType);
                    SetStateOwned();
                }
            );
        }
        else
        {
            GameManager.Instance.PopupsManager.ShowPopupYesNo(
                "Not Enough Currency",
                "You don't have enough money to buy <color=yellow>" + _building.buildingName + "</color> Do you want to go to the Shop?",
                PopupManager.PopupType.yesno,
                () => {
                    GameManager.Instance.ShowShop(true);
                    GameManager.Instance.TryToCreateOffer(0.2f);
                }
            );
        }
    }

    private void TryUpgradeBuilding()
    {
        if (GameManager.Instance.HasEnoughCurrency(upgradeCost))
        {
            GameManager.Instance.PopupsManager.ShowPopupYesNo(
                "Are you sure?",
                "Do you want to Upgrade <color=yellow>" + _building.buildingName +
                "</color>?<br>You will pay <color=yellow>" + UIUtils.FormatNumber(upgradeCost.amount) + " " + upgradeCost.currencyName,
                PopupManager.PopupType.yesno,
                () =>
                { //Exito
                    if (!GameManager.TryToSpend(upgradeCost)) return;
                    playerData.UpgradeBuilding(_building.buildingType);
                    GameManager.Instance.server.SendAction(ActionType.BuyBuilding, _buildingPrice.rewardType, _buildingPrice.amount, (int)_building.buildingType);
                    
                    SetStateOwned();
                }
            );
        }
        else
        {
            GameManager.Instance.PopupsManager.ShowPopupYesNo(
               "Not Enough Currency",
               "You don't have enough money to upgrade <color=yellow>" + _building.buildingName + "</color> Do you want to go to the Shop?",
               PopupManager.PopupType.yesno,
               () =>
               {
                   GameManager.Instance.ShowShop(true);
                   GameManager.Instance.TryToCreateOffer(0.2f);
               }
           );
        }
    }

    private void ClaimProfit()
    {
        if (buildingConfig == null) return;
        RewardData r = new RewardData(RewardType.Coins, buildingConfig.GetCurrentProfit());
        GameManager.Instance.AddRewardToPlayer(r, true);
        UIUtils.FlyingParticles(RewardType.Coins, btnClaimProfit.transform.position, Mathf.Min(r.amount, 10), null);
        buildingData.SetLastBuildingClaim();
        txtOwnedProfit.text = "0";
    }

    private void RefreshProfitTxt()
    {
        var profit = buildingConfig.GetCurrentProfit();
        txtOwnedProfit.text = profit.ToString();
        btnClaimProfit.interactable = (profit > buildingConfig.profit / 10);
    }
}
