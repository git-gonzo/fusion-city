using Assets.Scripts.MergeBoard;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.SocialPlatforms.Impl;
using static Sirenix.OdinInspector.SelfValidationResult;

public class LeaderboardManager : MonoBehaviour
{
    public TopBar topbar;
    public LeaderboardControllerFame FameLeaderboardControllerPrefab;
    public int minutesToRefresh = 3;
    public int SeasonSecondsLeft => Globals.Instance.seasonSecondsLeft;
    public LeaderboardPlayer GetPlayerSeason => _leaderboardPlayers.data_weekly.Find(p => p.isPlayer);
    public List<LeaderboardPlayer> leaderboardMonth => _leaderboardPlayers.data_weekly;
    public bool IsReady => _leaderboardPlayers != null;
    public List<LeaderboardPlayer> winners => _leaderboardPlayers.data_winners;
    public static string lastLeaderboardId
    {
        get => PlayerPrefs.GetString("leaderboardFameYesterdayId");
        set => PlayerPrefs.SetString("leaderboardFameYesterdayId", value);
    }

    private LeaderboardControllerFame _fameLeaderboardController;
    LeaderboardPlayers _leaderboardPlayers;
    bool initialized = false;
    LeaderboardID leaderboardIdAllTime = LeaderboardID.mainAllTime;
    LeaderboardID leaderboardIdWeekly = LeaderboardID.mainWeekly;
    private List<FullLeaderboardItemData> dataWinners;
    private List<FullLeaderboardItemData> dataWeekly;
    private List<FullLeaderboardItemData> dataAlltime;
    int currentSeconds = 0;
    public async void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LeaderboardManager = this;
        }
        await Globals.IsSignedIn();
        if (PlayerData.Instance != null && PlayerData.Instance.level < 2) return;
        await LoadLeaderboard();
    }

    bool doingSuperLazy = true;
    private void Update()
    {
        if (currentSeconds != DateTime.Now.Second)
        {
            currentSeconds = DateTime.Now.Second;
            LazyUpdate();
        }
        else if (currentSeconds % 60 == 0 && !doingSuperLazy)
        {
            doingSuperLazy = true;
            LoadLeaderboard();
        }
        else if (currentSeconds % 60 > 0)
        {
            doingSuperLazy = false;
        }
    }

    private void LazyUpdate()
    {
        topbar.txtSeasonEndContainer.SetActive(Globals.Instance.seasonSecondsLeft >= 0);
        if (Globals.Instance.seasonSecondsLeft >= 0)
        {
            topbar.txtSeasonEnd.text = UIUtils.FormatTime(Globals.Instance.seasonSecondsLeft);
        }
    }

    private async Task LoadLeaderboard()
    {
        Debug.Log("Loading Leaderboard");
        _leaderboardPlayers ??= new();
        dataWeekly = await Leaderboards.GetScoresFull(leaderboardIdWeekly);
        _leaderboardPlayers.FillWeekly(dataWeekly);
        dataAlltime = await Leaderboards.GetScoresFull(leaderboardIdAllTime);
        _leaderboardPlayers.FillAllTime(dataAlltime);
        topbar.UpdateFamePoints();
        var endTime = await Leaderboards.GetLeaderboardTimeLeft(leaderboardIdWeekly);
        var secondsLeft = endTime - DateTime.UtcNow;
        Globals.Instance.seasonSecondsLeft = (int)secondsLeft.TotalSeconds;
        _leaderboardPlayers.seasonSecondsLeft = (int)secondsLeft.TotalSeconds;
        if (_fameLeaderboardController != null)
        {
            InitializeController();
        }
        if(GameManager.Instance != null && !GameManager.Instance.MergeManager.IsBoardActive)
        {
            await GetOtherPlayersVehiclesAndLocations();
            GameManager.Instance.mapManager.AddOtherPlayersVehicles(_leaderboardPlayers.data_weekly);
        }
    }

    public async Task LoadWinners()
    {
        _leaderboardPlayers ??= new();
        dataWinners = await Leaderboards.GetYesterdayScoresFull(leaderboardIdWeekly);
        _leaderboardPlayers.FillWinners(dataWinners);
    }
    public int GetPlayerWinnerPosition()
    {
        return Leaderboards.GetPlayerPosition(dataWinners);
    }

    public async void ShowLeaderboard()
    {
        _fameLeaderboardController ??= Instantiate(FameLeaderboardControllerPrefab, ScreenManager.Instance.transform);
        ScreenManager.Instance.AnimateScreenIN(_fameLeaderboardController.gameObject);

        if (_leaderboardPlayers == null)
        {
            await LoadLeaderboard();
        }
        if(!initialized) InitializeController();
        
        initialized = true;
        _fameLeaderboardController.Init();
        _fameLeaderboardController.ShowMonth();
    }

    public void FillLeaderboardLists()
    {
        //bool playerInServer = false;
        _fameLeaderboardController.listMonth = _leaderboardPlayers.data_weekly;
        _fameLeaderboardController.listAlltime = _leaderboardPlayers.data_allTime;
        _fameLeaderboardController.Sort();
    }

    private void InitializeController()
    {
        initialized = true;
        FillLeaderboardLists();
        _fameLeaderboardController.Init();
    }

    public async void SendScore(int amount = 5)
    {
        await Leaderboards.SendScore(leaderboardIdWeekly, amount, true);
        await Leaderboards.SendScore(leaderboardIdAllTime, amount, true);
        await LoadLeaderboard();
    }
    public async Task GetOtherPlayersVehiclesAndLocations()
    {
        await Globals.IsSignedIn();
        if(GameManager.Instance != null && _leaderboardPlayers != null)
        {
            var result = await CloudCodeService.Instance.CallEndpointAsync<PlayersVehicleAndLocations>("GetPlayersLocation");
            _leaderboardPlayers.UpdateLocations(result.arrLocations);
        }
    }
}

[Serializable]
public class PlayersVehicleAndLocations
{
    public List<PlayerVehicleAndLocation> arrLocations;
}
[Serializable]
public class PlayerVehicleAndLocation
{
    public string playerId;
    public int vehicle;
    public int location;
}
[System.Serializable] public class PlayerActiveMissions { public List<int> list;}
[System.Serializable] public class PlayerStorage{ public List<PieceDiscovery> list; }

[System.Serializable]
public class LeaderboardPlayers
{
    public List<LeaderboardPlayer> data_allTime;
    public List<LeaderboardPlayer> data_weekly;
    public List<LeaderboardPlayer> data_winners;
    public int seasonSecondsLeft;

    public void FillWeekly(List<FullLeaderboardItemData> json)
    {
        FillData(json,out data_weekly);
    }
    public void FillAllTime(List<FullLeaderboardItemData> json)
    {
        FillData(json,out data_allTime);
    }
    public void FillWinners(List<FullLeaderboardItemData> json)
    {
        FillData(json,out data_winners);
    }
    public void UpdateLocations(List<PlayerVehicleAndLocation> results)
    {
        Debug.Log("Updating locations and vehicles");
        for(var i=0; i< results.Count; i++)
        {
            var pl = data_weekly.Find(p => p.playerId == results[i].playerId);
            if(pl != null)
            {
                pl.location = results[i].location > 0 ? results[i].location : pl.location;
                pl.vehicle = results[i].vehicle > 0 ? results[i].vehicle : pl.vehicle;
            }
        }
    }
    
    private void FillData(List<FullLeaderboardItemData> json, out List<LeaderboardPlayer> data)
    {
        data = new List<LeaderboardPlayer>();
        for (var i = 0; i < json.Count; i++)
        {
            LeaderboardPlayer item = new LeaderboardPlayer();
            item.playerId = json[i].playerId;
            item.playername = json[i].playerName;
            item.position = json[i].rank;
            item.score = (int)json[i].score;
            item.level = json[i].level;
            item.xp = json[i].xp;
            item.fame = (int)json[i].score;
            item.merges = json[i].merges;
            item.orders = json[i].orders;
            item.missions = json[i].missions;
            item.vehicles = json[i].vehicles;
            item.travels = json[i].travels;
            item.buildings = json[i].buildings;
            item.gifts = json[i].gifts;
            item.stolen = json[i].stolen;
            item.location = json[i].location;
            item.vehicle = json[i].vehicleId;
            item.material = json[i].carMaterial;
            item.activeMissions = ""; //TODO: pending
            item.storage = JsonConvert.DeserializeObject<PlayerStorage>(json[i].storage).list;
            item.isPlayer = json[i].playerId == PlayerData.playerID;
            item.charIndex = json[i].avatar;
            data.Add(item);
        }
    }
}

public class LeaderboardPlayer
{
    public string playerId;
    public string playername;
    public int position;
    public int score;
    public int level;
    public int xp;
    public int fame;
    public int merges;
    public int orders;
    public int missions;
    public int travels;
    public int vehicles;
    public int buildings;
    public int gifts;
    public int stolen;
    public int location;
    public int vehicle;
    public int material;
    public int charIndex;
    public string activeMissions;
    public List<PieceDiscovery> storage;
    public bool isPlayer;
}

public class SimplePlayer
{
    public string playername;
    public int position;
    public int charIndex;
    public int vehicleID;
    public int vehicleMaterial;
}
