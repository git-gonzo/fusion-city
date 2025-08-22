using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "BuildingsConfig", menuName = "Merge/BuildingsConfig")]

public class BuildingsConfig : ScriptableObject
{
    public List<BuildingConfigRaw> data;
    public string GetBuildingLocKey(BuildingType buildingType)
    {
        return data.Find((b) => b.buildingType == buildingType)?.stringKey;
    }
}


[Serializable]
public class BuildingConfigRaw
{
    public BuildingType buildingType;
    public string stringKey;
    public bool forSale;
    [ShowIf("@forSale==true")] public RewardType currency;
    [ShowIf("@forSale==true")] public int price;
    [ShowIf("@forSale==true")] public int profit;

    private const int maxProfitFactor = 10;
    public int Cap => profit * maxProfitFactor;
    string _buildingNameTranslated;
    public string BuildingNameTranslated { get => _buildingNameTranslated; set => _buildingNameTranslated = value; }
    public RewardData BuildingPrice => new RewardData(currency, price);
    public int UnlockLevel => GameManager.Instance.mapManager.GetBuildingDataFromType(buildingType).boardUnlockLevel;
    public int GetCurrentProfit()
    {
        var building = GameManager.Instance.playerData.GetBuildingOwned(buildingType);
        var time = building.LastBuildingClaim;
        var currentProfit = (building.level == 1 ? profit : profit * building.level * 0.75f) * time.TotalSeconds / 3600;
        var cap = building.level == 1 ? Cap : Cap * building.level * 0.75f;
        return Mathf.Min((int)currentProfit, (int)cap);
    }
}