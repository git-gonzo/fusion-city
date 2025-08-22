using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class DailyTaskScreen : MonoBehaviour
{
    public TextMeshProUGUI txtTimeLeft;
    public TextMeshProUGUI txtCompleteAllTasks;
    public TextMeshProUGUI txtComeBackTomorrow;
    public DailyTaskItem task1;
    public DailyTaskItem task2;
    public DailyTaskItem task3;
    public RewardsContainer rewards;
    public Button btnClaim;

    private bool allCompleted => dailyTaskModel.dailyTasks[0].completed &&
            dailyTaskModel.dailyTasks[1].completed &&
            dailyTaskModel.dailyTasks[2].completed;
    private bool readyToClaim => allCompleted && !dailyTaskModel.allClaimed;
    private DailyTaskModel dailyTaskModel => GameManager.Instance.dailyTaskManager.model;
    
    public void Show()
    {
        btnClaim.transform.DOKill();
        btnClaim.transform.DOScale(1,0);
        task1.Init(dailyTaskModel.dailyTasks[0], CheckStatus);
        task2.Init(dailyTaskModel.dailyTasks[1], CheckStatus);
        task3.Init(dailyTaskModel.dailyTasks[2], CheckStatus);
        CheckStatus();
    }

    private void CheckStatus()
    {
        if (readyToClaim)
        {
            btnClaim.onClick.RemoveAllListeners();
            btnClaim.onClick.AddListener(OnClaim);
            ClaimBounce();
        }
        if (dailyTaskModel.allClaimed)
        {
            rewards.gameObject.SetActive(false);
        }
        else
        {
            rewards.gameObject.SetActive(true);
            rewards.FillRewards(dailyTaskModel.rewards,GameManager.Instance.topBar);
        }
    }

    internal void LazyUpdate()
    {
        var now = DateTime.Now;
        var next = DateTime.Now.AddDays(1).Date;
        var x = (next - now).TotalSeconds;
        txtTimeLeft.text = UIUtils.FormatTime(x);
        txtComeBackTomorrow.gameObject.SetActive(dailyTaskModel.allClaimed);
        txtCompleteAllTasks.gameObject.SetActive(!allCompleted && !dailyTaskModel.allClaimed);
        btnClaim.gameObject.SetActive(readyToClaim);
        //Check new day
        if(dailyTaskModel.currentDay < DateTime.Now.DayOfYear)
        {
            GameManager.Instance.CreateDailyTasks();
            Show();
        }
    }

    private void OnClaim()
    {
        btnClaim.gameObject.SetActive(false);
        dailyTaskModel.allClaimed = true;
        GameManager.Instance.dailyTaskManager.SaveModel();
        rewards.ClaimRewardsBySteps(LazyUpdate);
    }

    private void ClaimBounce()
    {
        btnClaim.transform.DOScale(1.1f, 0.5f).SetLoops(-1,LoopType.Yoyo);
    }

    public void Admin_FakeDayBack()
    {
        dailyTaskModel.currentDay--;
    }

    /*public void AdminCompleteAll()
    {
        dailyTaskModel.dailyTasks[0].completed = true;
        dailyTaskModel.dailyTasks[1].completed = true;
        dailyTaskModel.dailyTasks[2].completed = true;
    }*/
}
