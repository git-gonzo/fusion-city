using Assets.Scripts.MergeBoard;
using DG.Tweening;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

public class SpeedBoardManager : MonoBehaviour
{
    public MergeBoardModel boardModel = new MergeBoardModel();
    public MergeBoardChallenge speedBoardController;
    public BoardConfig tempBoardConfig; //For size and color
    public Transform leaderboardContainer;
    public LeaderBoardItenFameView leaderboardItem;
    public GameObject loadingLeaderboard;
    public List<SpeedBoardPieces> pieceTypesByDay = new List<SpeedBoardPieces>();
    public List<int> playPrices;
    
    //public List<List<PieceType>> pieceTypesByDay = new List<List<PieceType>>();
    [HideInInspector] public SpeedBoardTries speedBoardTries = new SpeedBoardTries();

    private BoardState boardState;
    private Vector3 originalPosition;
    private bool yesterdayChecked = false;
    private int _currentDay;
    private List<BasicLeaderboardItemData> data;
    private string yesterdayLeaderboardId;
    private int playerPosition;
    Action _OnClose;
    private LeaderboardID leaderboardId = LeaderboardID.speedBoardLeaderboard;

    public static string yesterdayId
    {
        get => PlayerPrefs.GetString("speedBoardYesterdayId");
        set => PlayerPrefs.SetString("speedBoardYesterdayId", value);
    }
    public bool IsSpeedBoardActive => speedBoardController.gameObject.activeSelf && Globals.Instance.gameLoaded;


    // Start is called before the first frame update
    void Start()
    {
        originalPosition = speedBoardController.transform.position;
    }

    public void ShowSpeedBoard(bool value, System.Action OnClose = null)
    {
        _OnClose = OnClose;
        if (!value)
        {
            if (IsSpeedBoardActive)
            {
                speedBoardController.Hide();
            }
            return;
        }
        LoadLeaderboard();
        CreateBoard(tempBoardConfig);
        speedBoardController.transform.position = originalPosition;
        speedBoardController.CanvasGroup.DOFade(0, 0);

        InitSpeedBoard();

        speedBoardController.Show();
        speedBoardController.gameObject.SetActive(value);
    }

    public void InitSpeedBoard()
    {
        speedBoardController.FirstInitSpeedBoard(boardState, () => {
            _OnClose?.Invoke();
            GameManager.Instance.ShowSpeedMergeBoard(false);
        });
    }

    public async void LoadLeaderboard()
    {
        loadingLeaderboard.SetActive(true);

        GameManager.RemoveChildren(leaderboardContainer.gameObject);

        yesterdayLeaderboardId = await Leaderboards.GetYesterdayId(leaderboardId);
        data = new List<BasicLeaderboardItemData>();

        //FOR TESTING
        //data = await Leaderboards.GetYesterdayScores(leaderboardId);
        //speedBoardController.ShowYesterdayReward(true, 3);
        
        if (yesterdayId != yesterdayLeaderboardId)
        {
            data = await Leaderboards.GetYesterdayScores(leaderboardId);
            playerPosition = GetPlayerPosition();
            if (playerPosition == -1)
            {
                yesterdayId = yesterdayLeaderboardId;
                data = await Leaderboards.GetScoresBasic(leaderboardId);
                speedBoardController.ShowStartScreen();
            }
            else
            {
                speedBoardController.ShowYesterdayReward(true, playerPosition);
            }
        }
        else
        {
            data = await Leaderboards.GetScoresBasic(leaderboardId);
            speedBoardController.ShowStartScreen();
        }


        for (int i = 0; i < data.Count; i++)
        {
            var item = Instantiate(leaderboardItem, leaderboardContainer);
            item.Init(data[i], data[i].playerId == PlayerData.playerID);
        }
        loadingLeaderboard.SetActive(false);
        speedBoardController.GetDaySecondsLeft();
    }


    public async void ClaimYesterday()
    {
        yesterdayId = await Leaderboards.GetYesterdayId(leaderboardId);
        speedBoardController.txtTitleLeaderboardYesterday.gameObject.SetActive(false);
        var reward = new RewardData(RewardType.FamePoints, (11 - playerPosition)*2);
        reward.ApplyReward(speedBoardController.YesterdayRewardContainer);
        yesterdayChecked = true; 
        LoadLeaderboard() ;
        loadingLeaderboard.SetActive(true);
        speedBoardController.ShowStartScreen();
    }


    public void CreateBoard(BoardConfig boardConfig)
    {
        //GameManager.Instance.canvasBackground.color = boardConfig.BGColor;
        speedBoardController.mainBG.color = boardConfig.boardBGColor;
        boardState = boardModel.GetBoard(boardConfig);
        boardState.SetColorTileNormal(boardConfig.boardTileColorNormal);
        boardState.SetColorTileDark(boardConfig.boardTileColorDark);
    }

    public void OnGamePlayed()
    {
        speedBoardTries.IncreasePlayedToday();
        CloudSaveSpeedBoardTries();
    }

    public async void CloudSaveSpeedBoardTries()
    {
        var data = new Dictionary<string, object> { { "SpeedBoardTries", speedBoardTries } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }

    internal List<PieceType> GetRandomSetPieces()
    {
        return pieceTypesByDay[UnityEngine.Random.Range(0, pieceTypesByDay.Count)].pieces;
    }
    private int GetPlayerPosition()
    {
        foreach (var item in data)
        {
            if (item.playerId == PlayerData.playerID)
            {
                return item.rank + 1;
            }
        }
        return -1;
    }
}

[Serializable]
public class SpeedBoardPieces
{
    public List<PieceType> pieces;
}

[Serializable]
public class SpeedBoardTries
{
    public List<SpeedBoardTriesByDay> tries = new List<SpeedBoardTriesByDay>();

    public int todayTries { get {
            var tri = tries.Find(t => t.day == DateTime.UtcNow.DayOfYear && t.year == DateTime.UtcNow.Year);
            return tri != null ? tri.tries : 0;
        } }

    public void IncreasePlayedToday()
    {
        var tri = tries.Find(t => t.day == DateTime.UtcNow.DayOfYear && t.year == DateTime.UtcNow.Year);
        if (tri != null)
        {
            tri.tries++;
        }
        else
        {
            tries.Add(new SpeedBoardTriesByDay(DateTime.Now.DayOfYear, DateTime.UtcNow.Year, 1));
        }
    }
}
public class SpeedBoardTriesByDay
{
    public int day;
    public int year;
    public int tries;

    public SpeedBoardTriesByDay(int day, int year, int tries)
    {
        this.day = day;
        this.year = year;
        this.tries = tries;
    }
}
