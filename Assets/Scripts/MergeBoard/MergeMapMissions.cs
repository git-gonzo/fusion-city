using Assets.Scripts.MergeBoard;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "new mapMissionsConfig", menuName = "Fame/MapMissionsConfig")]
public class MergeMapMissions : ScriptableObject
{
    public List<MergeMissionMapConfig> mapMissions;
}

public class MergeMapMissionsConfigs
{
    public List<MergeMissionMapConfig> mapMissions;

    [JsonConstructor]
    public MergeMapMissionsConfigs(List<MergeMissionMapConfig> mapMissions)
    {
        this.mapMissions = mapMissions;
    }
    public void UpdateDataFromConfig(MergeMapMissions newData)
    {
        mapMissions = newData.mapMissions;
    }
}

[System.Serializable]
public class MergeMissionMapConfig
{
    MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;

    public List<PieceDiscovery> piecesRequest;
    [HorizontalGroup("Split")]
    [BoxGroup("Split/Location")][HideLabel]
    public BuildingType location;
    [BoxGroup("Split/Reward")]
    public List<RewardData> rewardInstant;
    [BoxGroup("Split/Requirement")]
    public int requirePlayerLevel;
    [BoxGroup("Split/Requirement")]
    public int untilPlayerLevel;

    public bool IsValid()
    {
        foreach(var p in piecesRequest)
        {
            if (!mergeConfig.ValidPieceType((int)p.pType)) return false;
        }
        return true;
    }
}

[Serializable]
public class MapMissionCloud
{
    public List<PieceDiscovery> piecesRequest;
    public BuildingType location;
    public List<RewardData> rewards;
}
[Serializable]
public class MapMissionCloudCharacter : MapMissionCloud
{
    [HideInInspector] public string stepId;
    [HideInInspector] public int characterId;
}


[System.Serializable]
public class LimitedMissionMapConfig
{
    MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;
    MergeBoardModel mergeModel => GameManager.Instance.mergeModel;

    public List<PieceDiscovery> piecesRequest;
    public BuildingType location;
    public List<RewardData> rewardInstant;
    public DateTime endTime;
    private List<PieceDiscovery> _maxDiscovered = new List<PieceDiscovery>();

    public bool IsValid()
    {
        foreach (var p in piecesRequest)
        {
            if (!mergeConfig.ValidPieceType((int)p.pType)) return false;
        }
        return true;
    }

    public void CreateLimitedMission()
    {
        endTime = DateTime.Now.AddHours(3 + GameManager.Instance.PlayerLevel*0.5f);
        //endTime = DateTime.Now.AddSeconds(20);
        UIUtils.SaveTimeStamp("LimitedMissionEndTime", endTime);
        var levelWeight = 0;
        //GetMaxLevelDiscovered() - 3;
        _maxDiscovered = GameManager.Instance.mapManager.GetMaxDiscoveries();
        //TODO: Find Random location
        location = GameManager.Instance.mapManager.GetRandomUnlockedBuilding();
        
        //Create Requested Pieces - DONE
        piecesRequest = new List<PieceDiscovery>();
        for (var i = 6; i <= 8; i++) // i = max possible level 
        {
            var itemRequested = GetDiscoveredItem(i);
            if (itemRequested == null) break;
            var genConfig = mergeConfig.GetGeneratorOfPiece(itemRequested.pType);
            var localWeight = (int)((Math.Pow(itemRequested.Lvl,3.1f) + Math.Sqrt(genConfig.coolDown) * genConfig.piecesChances.Count) * .02f) ; // / genConfig.piecesChances.Count
            GameManager.Log("Local weight for " + itemRequested.pType +", lvl = " + itemRequested.Lvl + " = " + localWeight);
            levelWeight += localWeight;
            piecesRequest.Add(itemRequested);
        }
        //var gems = (int)(GetFactorial(levelWeight+1) * 0.01f);
        var coins = Mathf.Max(100,(int)(MergeBoardManager.GetFactorial(levelWeight*2) * 0.1f) * 50);
        var fPoints = (int)(levelWeight * 1.1f);
        Debug.Log("Total level " + levelWeight);
        //TODO: Set rewards
        rewardInstant = new List<RewardData>();
        rewardInstant.Add(new RewardData(RewardType.Coins, coins));
        rewardInstant.Add(new RewardData(RewardType.FamePoints, fPoints));
        var random = UnityEngine.Random.Range(0, 15);

        if (fPoints < 9)
        {
            if (random < 5)
            {
                rewardInstant.Add(new RewardData(new PieceDiscovery(PieceType.Energy, fPoints < 5 ? 0 : 1)));
            }
            else if (random < 10)
            {
                rewardInstant.Add(new RewardData(new PieceDiscovery(PieceType.Gems, fPoints < 5 ? 0 : 1)));
            }
            else
            {
                rewardInstant.Add(new RewardData(new PieceDiscovery(PieceType.XP, fPoints < 5 ? 2 : 3)));
            }
        }
        else
        {
            var mergeItemReward = new RewardData(RewardType.MergeItem, 1);
            if      (fPoints < 10) mergeItemReward.mergePiece = new PieceDiscovery(PieceType.Energy, 2);
            else if (fPoints < 11) mergeItemReward.mergePiece = new PieceDiscovery(PieceType.Scissors, 0);
            else if (fPoints < 16) mergeItemReward.mergePiece = new PieceDiscovery(PieceType.CommonChest, 0);
            else if (random < 2)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterAutoMerge, 0);
            }
            else if (random < 3)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.LevelUP, 0);
            }
            else if (random < 5)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterEnergy, 0);
            }
            else if (random < 7)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.Scissors, 0);
            }
            else if (random < 9)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterGenerators, 0);
            }
            else if (random < 12)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.XP, 4);
            }
            else if (random < 13)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.RouleteTicketSpecial, 1);
            }
            else if (random < 14)
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.RouleteTicketSpecial, 2);
            }
            else
            {
                mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterChest, 0);
            }
            rewardInstant.Add(mergeItemReward);
        }

    }

    private PieceDiscovery GetDiscoveredItem(int maxLevel = 8)
    {
        var isValidChain = false;
        var safeTries = 20;
        while (!isValidChain && safeTries > 0) {
            var chainIndex = UnityEngine.Random.Range(0, _maxDiscovered.Count);
            if (_maxDiscovered[chainIndex].Lvl > 3) //Has discovered at least level 3
            {
                // is candidate already in the request?
                if (piecesRequest.Find(p=>p.pType == _maxDiscovered[chainIndex].pType) == null)
                {
                    var minLevel = Math.Max(2, _maxDiscovered[chainIndex].Lvl - 2);
                    var _maxLevel = Math.Min(maxLevel, _maxDiscovered[chainIndex].Lvl);
                    var itemLevel = UnityEngine.Random.Range(minLevel, _maxDiscovered[chainIndex].Lvl+1);
                    GameManager.Log("Piece Chosen::: " + _maxDiscovered[chainIndex].pType + "," + itemLevel);
                    return new PieceDiscovery(_maxDiscovered[chainIndex].pType,itemLevel);
                }
                safeTries--;
            }
        }
        return null;
    }

    
}