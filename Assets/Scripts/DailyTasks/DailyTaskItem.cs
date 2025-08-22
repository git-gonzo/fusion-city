using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static DailyTaskConfig;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class DailyTaskItem : MonoBehaviour
{
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtProgress;
    public RewardItemUniversal reward;
    public ProgressBar progressBar;
    public Button btnClaim;
    
    public void Init(DailyTaskConfig task, Action OnClaim)
    {
        txtTitle.text = task.taskType.ToString();
        if(task.taskType == DailyTaskType.UseBooster)
        {
            GameManager.SetLocString("DailyTasks", "UseBoosters", txtTitle);
        }
        else if(task.taskType == DailyTaskType.WinCoins)
        {
            GameManager.SetLocString("DailyTasks", "WinCoins", txtTitle);
        }
        else if(task.taskType == DailyTaskType.Gift)
        {
            GameManager.SetLocString("DailyTasks", "SendGift", txtTitle);
        }
        else if(task.taskType == DailyTaskType.Steal)
        {
            GameManager.SetLocString("DailyTasks", "Steal", txtTitle);
        }
        else if(task.taskType == DailyTaskType.CompleteOrder)
        {
            GameManager.SetLocString("DailyTasks", "CompleteOrder", txtTitle);
        }
        else if(task.taskType == DailyTaskType.CompleteMission)
        {
            GameManager.SetLocString("DailyTasks", "CompleteMissions", txtTitle);
        }
        else if(task.taskType == DailyTaskType.ChangeAvatar)
        {
            GameManager.SetLocString("DailyTasks", "ChangeAvatar", txtTitle);
        }
        else if(task.taskType == DailyTaskType.MergeSpecificItem)
        {
            GameManager.SetLocString("DailyTasks", "MergeItem", txtTitle);
        }
        else if(task.taskType == DailyTaskType.MergeNumberPieces)
        {
            GameManager.SetLocString("DailyTasks", "MergePieces", txtTitle);
        }
        else if(task.taskType == DailyTaskType.Travels)
        {
            GameManager.SetLocString("DailyTasks", "Travels", txtTitle);
        }
        else if(task.taskType == DailyTaskType.WinFamePoints)
        {
            GameManager.SetLocString("DailyTasks", "WinFame", txtTitle);
        }
        else if(task.taskType == DailyTaskType.WatchVideo)
        {
            GameManager.SetLocString("DailyTasks", "WatchVideos", txtTitle);
        }
        else if(task.taskType == DailyTaskType.BuyVehicle)
        {
            GameManager.SetLocString("DailyTasks", "BuyVehicle", txtTitle);
        }
        reward.gameObject.SetActive(!task.claimed);
        var current = Mathf.Min(task.currentAmount, task.amountTotal);
        var complete = current == task.amountTotal;
        txtProgress.text = current + "/" + task.amountTotal;
        if (task.claimed) return;

        progressBar.UpdateAnimated(current, task.amountTotal, false);
        reward.InitReward(task.reward, GameManager.Instance.topBar, complete);
        btnClaim.gameObject.SetActive(complete);
        if (complete)
        {
            btnClaim.onClick.RemoveAllListeners();
            btnClaim.onClick.AddListener(()=>
            {
                reward.ApplyReward();
                reward.gameObject.SetActive(false);
                btnClaim.gameObject.SetActive(false);
                btnClaim.onClick.RemoveAllListeners();
                task.claimed = true;
                task.completed = true;
                GameManager.Instance.dailyTaskManager.SaveModel();
                OnClaim?.Invoke();
            });
            reward.Bounce();
        }
    }
}
