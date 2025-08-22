using Assets.Scripts.MergeBoard;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public enum RewardType
{
    XP = 0,
    Coins = 1,
    Gems = 2,
    FamePoints = 3,
    Time = 4,
    RealMoney = 5,
    Energy = 6,
    MergeItem = 7,
    Vehicle = 8,
    SpeedBoardPoints = 9
};
[System.Serializable]
public class RewardData 
{
    public RewardType rewardType;
    public int amount;
    [ShowIf("rewardType",RewardType.Vehicle)]
    [ValidateInput("CheckVehicleID", "$validationMSG")]
    [InfoBox("$validationMSG")]
    public int vehicleID;
    [ShowIf("rewardType",RewardType.MergeItem)] 
    public PieceDiscovery mergePiece;

    GameConfigMerge _gameConfig;
    GameConfigMerge gameConfig => _gameConfig??=GameObject.Find("GameConfig").GetComponent<GameConfigMerge>();

    private string validationMSG ="Test";
    //public float chance;
    [JsonConstructor]
    public RewardData(RewardType rewardType, int amount)
    {
        this.rewardType = rewardType;
        this.amount = amount;
    }
    public RewardData(PieceDiscovery piece, int amount = 1)
    {
        this.rewardType = RewardType.MergeItem;
        mergePiece = piece;
        this.amount = amount;
    }
    public RewardData(int vehicleID)
    {
        this.rewardType = RewardType.Vehicle;
        this.vehicleID = vehicleID;
        this.amount = 1;
    }

    public string currencyName
    {
        get
        {
            if (rewardType == RewardType.Coins) return "Coins";
            if (rewardType == RewardType.Gems) return "Gems";
            if (rewardType == RewardType.FamePoints) return "Fame Points";
            if (rewardType == RewardType.Energy) return "Energy";
            if (rewardType == RewardType.MergeItem) return "Merge Item";
            if (rewardType == RewardType.Vehicle) return "Vehicle";
            return "";
        }
    }

    private bool CheckVehicleID()
    {
        if (vehicleID == 0)
        {
            validationMSG = "Insert Vehicle ID";
            return false;
        }
        else
        {
            var v = gameConfig.GetVehicleById(vehicleID);
            if(v == null)
            {
                validationMSG = "Vehicle not found";
                return false;
            }
            else
            {
                validationMSG = v.vehicleName;
            }
        }
        return true;
    }

    public void ApplyReward(Transform fromTransform)
    {
        GameManager.Instance.AddRewardWithParticles(this, fromTransform,null, true);
    }
}

[System.Serializable]
public class RewardDataOLD
{
    public int amount;
    public string currencyName;
    public RewardType rewardType;
}
