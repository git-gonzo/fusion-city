using Assets.Scripts.MergeBoard;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.CloudSave;
using UnityEngine;
using static DailyTaskConfig;

public class DailyTaskManager
{
    public bool dataLoading = false;
    public bool dataLoaded = false;
    public bool initialized = false;
    public DailyTaskModel model;

    public bool canBeShown => model != null && model.dailyTasks != null & model.dailyTasks.Count > 0;
    public bool allDailyClaimed => model.dailyTasks.All(t => t.claimed) && model.allClaimed;
    public bool isExpired => model != null && model.currentDay < DateTime.Now.DayOfYear;
    private int tasksCount = 3;

    public void Init()
    {
        initialized = true;
        if (model.dailyTasks == null || model.dailyTasks.Count == 0)
        {
            CreateTasks();
        }
    }

    public void OnMerge() => IncreaseTask(DailyTaskType.MergeNumberPieces);
    public void OnGift() => IncreaseTask(DailyTaskType.Gift);
    public void OnSteal() => IncreaseTask(DailyTaskType.Steal);
    public void OnBuyVehicle() => IncreaseTask(DailyTaskType.BuyVehicle);
    public void OnChangeAvatar() => IncreaseTask(DailyTaskType.ChangeAvatar);
    public void OnCompleteOrder() => IncreaseTask(DailyTaskType.CompleteOrder);
    public void OnUseBooster() => IncreaseTask(DailyTaskType.UseBooster);
    public void OnTravel() => IncreaseTask(DailyTaskType.Travels);
    public void OnWatchVideo() => IncreaseTask(DailyTaskType.WatchVideo);
    public void OnWinFame(int amount) => IncreaseTask(DailyTaskType.WinFamePoints, amount);
    public void OnWinCoins(int amount) => IncreaseTask(DailyTaskType.WinCoins, amount);
    public void OnCompleteMission() => IncreaseTask(DailyTaskType.CompleteMission); 


    int playerLevel => GameManager.Instance.PlayerLevel;

    private void IncreaseTask(DailyTaskType type, int amount = 1)
    {
        if (model == null || model.dailyTasks == null) return;
        foreach (var task in model.dailyTasks.Where(t => t.taskType == type))
        {
            task.currentAmount+=amount;
            CheckTask(task);
            SaveModel();
        }
    }
    public void CheckTask(DailyTaskConfig t)
    {
        if (t.currentAmount == t.amountTotal) t.completed = true;
    }

    public void SaveModel()
    {
        //GameManager.Log("Save Economy Sent");
        var data = new Dictionary<string, object> { { "dailyTasks", model } };
        CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    public async void LoadData()
    {
        dataLoading = true;
        var keys = new HashSet<string>() { "dailyTasks" };
        try
        {
            Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAsync(keys);
            model = new DailyTaskModel();
            foreach (var t in savedData)
            {
                if (t.Key == "dailyTasks")
                {
                    var data = JsonConvert.DeserializeObject<DailyTaskModel>(t.Value);
                    model = data;
                }
            }
            if(isExpired)//TODO: Year change
            {
                CreateTasks();
            }
        
            //GameManager.Log("Daily Task loaded");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
        dataLoaded = true;
        dataLoading = false;
    }

    private void CreateTasks()
    {
        model.dailyTasks = new List<DailyTaskConfig>();
        model.rewards = new List<RewardData>();
        for (int i = 0; i < tasksCount; i++)
        {
            DailyTaskType type = DailyTaskType.Gift;
            var isValidType = false;
            while (!isValidType)
            {
                var index = UnityEngine.Random.Range(0, (int)DailyTaskType.Count);
                type = (DailyTaskType)index;
                isValidType = IsValidType(type);
            }
            var task = CreateTaskOfType(type);
            model.dailyTasks.Add(task);   
        }
        model.rewards.Add(new RewardData(GetItemReward(), 1));
        model.rewards.Add(new RewardData(GetItemReward(), 1));
        model.rewards.Add(new RewardData(RewardType.Coins, UnityEngine.Random.Range(10,50) * 100));
        model.currentDay = DateTime.Now.DayOfYear;
        model.allClaimed = false;
        SaveModel();
    }

    private bool IsValidType(DailyTaskType type)
    {
        if(type == DailyTaskType.MergeSpecificItem) return false;
        if(type == DailyTaskType.Steal && playerLevel < 5) return false;
        if(type == DailyTaskType.Gift && playerLevel < 3) return false;
        if (model.dailyTasks.Any(t => t.taskType == type)) return false;
        return true;
    }

    private DailyTaskConfig CreateTaskOfType(DailyTaskType type)
    {
        DailyTaskConfig t = new DailyTaskConfig();
        t.taskType = type;
        t.amountTotal = 1;
        t.reward = new RewardData(RewardType.Gems, 1);

        if (type == DailyTaskType.Gift)
        {
            t.amountTotal = Mathf.Clamp(playerLevel - 3, 1,3);
            t.reward = new RewardData(RewardType.Gems, t.amountTotal+1);
        }
        else if(type == DailyTaskType.Steal)
        {
            t.amountTotal = Mathf.Clamp(playerLevel - 6, 1,2);
            t.reward = new RewardData(RewardType.Gems, t.amountTotal*2);
        }
        else if(type == DailyTaskType.CompleteMission)
        {
            t.amountTotal = Mathf.Clamp(playerLevel - 6, 1,2);
            t.reward = new RewardData(RewardType.Gems, t.amountTotal*2);
        }
        else if(type == DailyTaskType.UseBooster)
        {
            t.amountTotal = Mathf.Clamp(playerLevel - 3, 1,2);
        }
        else if(type == DailyTaskType.WinCoins)
        {
            var baseCoins = (int)Mathf.Pow(Mathf.Min((playerLevel - 2) * 20, 100), 2);
            var buildings = GameManager.Instance.playerData.buildingsOwned.Count;
            if (buildings > 0) baseCoins += (int)(1500 * buildings + MathF.Pow(buildings,2) * 250);
            t.amountTotal = baseCoins;
        }
        else if(type == DailyTaskType.WinFamePoints)
        {
            t.amountTotal = (int)Mathf.Pow(Mathf.Max(playerLevel - 2,2),2);
            t.reward.amount = (int)(t.amountTotal*0.05f) + 1;
        }
        else if(type == DailyTaskType.MergeNumberPieces)
        {
            t.amountTotal = Mathf.Max((int)Mathf.Pow(playerLevel + 6,2),50);
        }
        else if(type == DailyTaskType.WatchVideo)
        {
            t.amountTotal = Mathf.Clamp(playerLevel - 3, 1, 3);
            t.reward.amount = t.amountTotal;
        }
        else if(type == DailyTaskType.Travels)
        {
            t.amountTotal = Mathf.Clamp((playerLevel - 2) * 5, 5, 50);
            t.reward = new RewardData(RewardType.Gems, (int)Mathf.Max(3, t.amountTotal*0.1f));
        }
        else if(type == DailyTaskType.CompleteOrder)
        {
            t.amountTotal = Mathf.Clamp((playerLevel - 2) * 2, 3, 10);
            t.reward.amount = t.amountTotal;
        }
        else if(type == DailyTaskType.MergeSpecificItem)
        {
            var chainCandidates = GameManager.Instance.mapManager.GetMaxDiscoveries();
            var chainIndex = UnityEngine.Random.Range(0, chainCandidates.Count);
            var itemLevel = UnityEngine.Random.Range(2, chainCandidates[chainIndex].Lvl + 1);
            t.piece = new PieceDiscovery(chainCandidates[chainIndex].pType, itemLevel);
        }
        return t;
    }

    private PieceDiscovery GetItemReward()
    {
        PieceDiscovery piece = null;
        do 
        {
            var chainCandidates = GameManager.Instance.mapManager.GetMaxDiscoveries();
            var chainIndex = UnityEngine.Random.Range(0, chainCandidates.Count);
            var itemLevel = UnityEngine.Random.Range(3, chainCandidates[chainIndex].Lvl + 1);
            piece = new PieceDiscovery(chainCandidates[chainIndex].pType, itemLevel);
        }
        while (model.rewards.Any(r=>r.mergePiece.pType == piece.pType));
        return piece;
    }
}
