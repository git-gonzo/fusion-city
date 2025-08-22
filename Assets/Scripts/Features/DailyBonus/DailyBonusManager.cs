using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;

public class DailyBonusManager : MonoBehaviour
{
    public List<PieceDiscovery> hardRewards;
    public List<PieceDiscovery> hardRewards1;
    public List<PieceDiscovery> hardRewards2;
    public List<PieceDiscovery> hardRewards3;
    public List<PieceDiscovery> hardRewards4;
    public List<PieceDiscovery> currentRound { 
        get {
            if(dailyBonusProgress.round == 0)
            {
                return hardRewards;
            }
            var r = dailyBonusProgress.round > 4?dailyBonusProgress.round % 4: dailyBonusProgress.round - 1;
            if (r == 0)
                return hardRewards1;
            else if (r == 1)
                return hardRewards2;
            else if (r == 2)
                return hardRewards3;
            else if (r == 3)
                return hardRewards4;
            return hardRewards;
        } 
    }
    public DailyBonusProgress dailyBonusProgress = new DailyBonusProgress();
    public bool IsClaimable()
    {
        if (dailyBonusProgress == null) return false;
        var yearDay = DateTime.Now.DayOfYear;
        if (dailyBonusProgress.progress == 0 && yearDay != dailyBonusProgress.yearDay) 
            return true;
        else if (yearDay == dailyBonusProgress.yearDay + 1)
            return true;
        else if (yearDay == 1 && dailyBonusProgress.yearDay == 365)
            return true;
        return false;
    }

    public bool CanBeShown => IsClaimable() || IsExpired();
    public bool IsExpired()
    {
        var yearDay = DateTime.Now.DayOfYear;
        if (dailyBonusProgress.yearDay < yearDay - 1) return true;
        return false;
    }

    public void ShowDailyBonusPopup()
    {
        GameManager.Instance.PopupsManager.ShowDailyBonusPopup(currentRound);
    }

    public async void CloudSaveDay()
    {
        var data = new Dictionary<string, object> { { "dailyBonus", dailyBonusProgress } };
        await SaveData.ForceSaveAsync(data);
    }

    [Serializable]
    public class DailyBonusProgress
    {
        public int yearDay;
        public int progress;
        public int round;
    }

    internal void CreateNewProgress()
    {
        dailyBonusProgress = new DailyBonusProgress();
        dailyBonusProgress.yearDay = DateTime.Now.DayOfYear;
    }

    public void ClaimReward()
    {
        dailyBonusProgress.yearDay = DateTime.Now.DayOfYear;
        if (dailyBonusProgress.progress == 6)
        {
            dailyBonusProgress.progress = 0;
            dailyBonusProgress.round++;
        }
        else
        {
            dailyBonusProgress.progress++;
        }
        CloudSaveDay();
    }

    public void Restore()
    {
        dailyBonusProgress.yearDay = DateTime.Now.DayOfYear - 1;
        CloudSaveDay();
    }
    public void RestoreCancel()
    {
        dailyBonusProgress.yearDay = DateTime.Now.DayOfYear;
        dailyBonusProgress.progress = 0;
        CloudSaveDay();
    }
}
