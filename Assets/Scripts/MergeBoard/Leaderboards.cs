using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using UnityEngine;

public class Leaderboards
{
    public static async Task<string> SendScore(LeaderboardID leaderboardId, int amount = 5, bool fullMetadata = false)
    {
        var metadata = new Dictionary<string, string>();
        metadata.Add("level", PlayerPrefs.GetInt("playerLevel").ToString());
        metadata.Add("location", ((int)PlayerData.playerLocation).ToString());
        metadata.Add("vehicleID", PlayerData.vehicleSelected.ToString());
        metadata.Add("avatar", PlayerData.characterIndex.ToString());
        if (fullMetadata)
        {
            var v = GameManager.Instance.playerData.vehiclesOwned.Find(v => v.id == PlayerData.vehicleSelected);
            metadata.Add("coins", PlayerData.coins.ToString());
            metadata.Add("gems", PlayerData.gems.ToString());
            metadata.Add("xp", PlayerData.xp.ToString());
            metadata.Add("merges", PlayerData.mergeCount.ToString());
            metadata.Add("orders", PlayerData.MissionsCount.ToString());
            metadata.Add("travels", PlayerData.travelCount.ToString());
            metadata.Add("missions", PlayerData.MapMissionsCount.ToString());
            metadata.Add("carmaterial", (v != null ? v.mat : 0).ToString());
            metadata.Add("buildings", GameManager.Instance.playerData.buildingsOwned.Count.ToString());
            metadata.Add("vehicles", GameManager.Instance.playerData.vehiclesOwned.Count.ToString());
            metadata.Add("gifts", PlayerData.giftsSent.ToString());
            metadata.Add("stolen", PlayerData.itemsStolen.ToString());
            metadata.Add("storage", GameManager.Instance.mergeModel.StorageJSON());
        }
        var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId.ToString(), amount,
            new AddPlayerScoreOptions { Metadata = metadata });
        return (JsonConvert.SerializeObject(scoreResponse));
    }

    public static async Task<List<BasicLeaderboardItemData>> GetScoresBasic(LeaderboardID leaderboardId)
    {
        var scoresResponse =
            await LeaderboardsService.Instance.GetScoresAsync(leaderboardId.ToString(), new GetScoresOptions { Limit = 50, IncludeMetadata = true });
        return (JsonConvert.DeserializeObject<List<BasicLeaderboardItemData>>(JsonConvert.SerializeObject(scoresResponse.Results)));
    }
    public static async Task<List<FullLeaderboardItemData>> GetScoresFull(LeaderboardID leaderboardId)
    {
        var scoresResponse =
            await LeaderboardsService.Instance.GetScoresAsync(leaderboardId.ToString(), new GetScoresOptions { Limit = 50, IncludeMetadata = true });
        return (JsonConvert.DeserializeObject<List<FullLeaderboardItemData>>(JsonConvert.SerializeObject(scoresResponse.Results)));
    }

    public static async Task<List<BasicLeaderboardItemData>> GetYesterdayScores(LeaderboardID leaderboardId)
    {
        var versionId = await GetYesterdayId(leaderboardId);

            var scoresResponse = await LeaderboardsService.Instance.GetVersionScoresAsync(
                leaderboardId.ToString(),
                versionId,
                new GetVersionScoresOptions { IncludeMetadata = true, Limit = 25 }
            );
            return JsonConvert.DeserializeObject<List<BasicLeaderboardItemData>>(JsonConvert.SerializeObject(scoresResponse.Results));
    }
    public static async Task<List<FullLeaderboardItemData>> GetYesterdayScoresFull(LeaderboardID leaderboardId)
    {
        var versionId = await GetYesterdayId(leaderboardId);

            var scoresResponse = await LeaderboardsService.Instance.GetVersionScoresAsync(
                leaderboardId.ToString(),
                versionId,
                new GetVersionScoresOptions { IncludeMetadata = true, Limit = 25 }
            );
            return JsonConvert.DeserializeObject<List<FullLeaderboardItemData>>(JsonConvert.SerializeObject(scoresResponse.Results));
    }
    
    public static async Task<DateTime> GetLeaderboardTimeLeft(LeaderboardID leaderboardId)
    {
        var result = await LeaderboardsService.Instance.GetVersionsAsync(leaderboardId.ToString());
        return result.NextReset;
    }

    public static async Task<string> GetYesterdayId(LeaderboardID leaderboardId)
    {
        try
        {
            var versionsResponse = await LeaderboardsService.Instance.GetVersionsAsync(leaderboardId.ToString());
            // Get the ID of the most recently archived Leaderboard version
            if (versionsResponse.Results.Count == 0)
            {
                return "previousIdFound";
            }
            return versionsResponse.Results[0].Id;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return "Error";
        }
    }

    public static int GetPlayerPosition(List<BasicLeaderboardItemData> data)
    {
        foreach (var item in data)
        {
            if (item.playerId == PlayerData.playerID)
            {
                return item.rank;
            } 
        }
        return -1;
    }
    public static int GetPlayerPosition(List<FullLeaderboardItemData> data)
    {
        foreach (var item in data)
        {
            if (item.playerId == PlayerData.playerID)
            {
                return item.rank;
            } 
        }
        return -1;
    }
}

public enum LeaderboardID
{
    songLeaderboard = 0,
    speedBoardLeaderboard = 1,
    mainAllTime = 2,
    mainWeekly = 3
}

[Serializable]
public class BasicLeaderboardItemData
{
    public string playerId;
    public string playerName;
    public int rank;
    public float score;
    public int level;
    public int location;
    public int vehicleId;
    public int avatar;

    [JsonConstructor]
    public BasicLeaderboardItemData(string playerId, string playerName, int rank, float score, string metadata)
    {
        this.playerId = playerId;
        var indexofAlmohadilla = playerName.IndexOf("#");
        if (indexofAlmohadilla <= 0)
        {
            indexofAlmohadilla = playerName.Length;
        }
        this.playerName = playerName.Substring(0, indexofAlmohadilla); 
        this.rank = rank + 1;
        this.score = score;
        var meta = JsonConvert.DeserializeObject<BasicPlayerMetadata>(metadata);
        level = meta.level;
        location = meta.location;
        vehicleId = meta.vehicleId;
        avatar = meta.avatar;
    }
}

[Serializable]
public class BasicPlayerMetadata
{
    public int level;
    public int location;
    public int vehicleId;
    public int avatar;
}

[Serializable]
public class FullLeaderboardItemData
{
    public string playerId;
    public string playerName;
    public int rank;
    public float score;
    public int level;
    public int location;
    public int vehicleId;
    public int avatar;
    public int coins;
    public int gems;
    public int xp;
    public int merges;
    public int orders;
    public int travels;
    public int missions;
    public int carMaterial;
    public int buildings;
    public int vehicles;
    public int gifts;
    public int stolen;
    public string storage;

    [JsonConstructor]
    public FullLeaderboardItemData(string playerId, string playerName, int rank, float score, string metadata)
    {
        this.playerId = playerId;
        this.playerName = UIUtils.GetCleanName(playerName);
        this.rank = rank + 1;
        this.score = score;
        var meta = JsonConvert.DeserializeObject<FullPlayerMetadata>(metadata);
        level = meta.level;
        location = meta.location;
        vehicleId = meta.vehicleId;
        avatar = meta.avatar;
        coins = meta.coins;
        gems = meta.gems;
        xp = meta.xp;
        merges = meta.merges;
        orders = meta.orders;
        travels = meta.travels;
        missions = meta.missions;
        carMaterial = meta.carMaterial;
        buildings = meta.buildings;
        vehicles = meta.vehicles;
        gifts = meta.gifts;
        stolen = meta.stolen;
        storage = meta.storage;
    }
}

[Serializable]
public class FullPlayerMetadata
{
    public int level;
    public int location;
    public int vehicleId;
    public int avatar;
    public int coins;
    public int gems;
    public int xp;
    public int merges;
    public int orders;
    public int travels;
    public int missions;
    public int carMaterial;
    public int buildings;
    public int vehicles;
    public int gifts;
    public int stolen;
    public string storage;
}