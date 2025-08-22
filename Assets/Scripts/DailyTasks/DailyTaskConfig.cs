using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyTaskConfig
{
    public enum DailyTaskType
    {
        Gift,
        Steal,
        MergeNumberPieces,
        BuyVehicle,
        CompleteMission,
        CompleteOrder,
        UseBooster,
        Travels,
        ChangeAvatar,
        WatchVideo,
        WinFamePoints,
        WinCoins,
        MergeSpecificItem,
        Count
    }

    public DailyTaskType taskType;
    public int currentAmount;
    public int amountTotal;
    public PieceDiscovery piece;
    public bool completed;
    public bool claimed;
    public RewardData reward;
}

[System.Serializable]
public class DailyTaskModel
{
    public int currentDay;
    public List<DailyTaskConfig> dailyTasks;
    public List<RewardData> rewards;
    public bool allClaimed;
}
