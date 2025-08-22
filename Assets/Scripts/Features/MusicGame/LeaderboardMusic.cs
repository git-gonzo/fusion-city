using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardMusic : LeaderboardBase
{
    public GameObject loadingLeaderboard;
    public Transform Container;
    public LeaderBoardItenFameView listItemPrefab;
    public TextMeshProUGUI txtTitleToday;
    public TextMeshProUGUI txtTitleYesterday;
    public TextMeshProUGUI txtSeasonEnd;
    public RewardsContainer rewardsContainer;
    public GameObject timeLeftContainer;
    public GameObject claimContainer;
    public Button btnClaimRewards;
    public Button btnStartGame;
    public static string yesterdayId
    {
        get => PlayerPrefs.GetString("musicYesterdayId");
        set => PlayerPrefs.SetString("musicYesterdayId", value);
    }

    private List<BasicLeaderboardItemData> data;
    private int playerPosition;
    private int secondsLeft;
    private CanvasGroup canvasGroup => GetComponent<CanvasGroup>();
    bool leaderboardLoaded;
    int currentSeconds = 0;
    private void Update()
    {
        if (!leaderboardLoaded)
        {
            return;
        }
        if (currentSeconds != DateTime.Now.Second)
        {
            currentSeconds = DateTime.Now.Second;
            LazyUpdate();
        }
    }

    private void LazyUpdate()
    {
        if (secondsLeft >= 0)
        {
            txtSeasonEnd.text = UIUtils.FormatTime(secondsLeft);
        }
        else
        {
            LoadData();
        }
        secondsLeft--;
    }


    public async Task LoadData()
    {
        leaderboardLoaded = false;
        loadingLeaderboard.SetActive(true);

        await Globals.CheckUnityService();

        await GetLeaderboard();
        leaderboardLoaded = true;
        loadingLeaderboard.SetActive(false);
    }

    private async Task GetLeaderboard()
    {
        GameManager.RemoveChildren(Container.gameObject);

        yesterdayLeaderboardId = await Leaderboards.GetYesterdayId(LeaderboardID.songLeaderboard);
        //yesterdayId = "sdfg"; //TO TEST REWARDS
        data = new List<BasicLeaderboardItemData>();

        if (yesterdayId != yesterdayLeaderboardId)
        {
            data = await Leaderboards.GetYesterdayScores(LeaderboardID.songLeaderboard);
            playerPosition = Leaderboards.GetPlayerPosition(data);
            if (playerPosition < 0)
            {
                yesterdayId = yesterdayLeaderboardId;
                data = await Leaderboards.GetScoresBasic(LeaderboardID.songLeaderboard);
            }
        }
        else
        {
            data = await Leaderboards.GetScoresBasic(LeaderboardID.songLeaderboard);
        }
        playerPosition = Leaderboards.GetPlayerPosition(data);
        SetStateYesterday(yesterdayId != yesterdayLeaderboardId && playerPosition > 0);
        for (int i = 0; i < data.Count; i++)
        {
            var item = Instantiate(listItemPrefab, Container);
            item.Init(data[i]);
        }
        // L O A D   T I M E  L E F T
        var endTime = await Leaderboards.GetLeaderboardTimeLeft(LeaderboardID.songLeaderboard);
        var remaining = endTime - DateTime.UtcNow;
        secondsLeft = (int)remaining.TotalSeconds;
        txtSeasonEnd.text = UIUtils.FormatTime(secondsLeft);

        await Task.CompletedTask;
    }

    private void SetStateYesterday(bool isYesterday)
    {
        timeLeftContainer.gameObject.SetActive(!isYesterday);
        txtTitleToday.gameObject.SetActive(!isYesterday);
        txtTitleYesterday.gameObject.SetActive(isYesterday);
        SongController.Instance.songUI.ShowStartButtons(!isYesterday);
        claimContainer.SetActive(isYesterday);
        if(isYesterday)
        {
            btnClaimRewards.onClick.AddListener(ClaimRewards);
            var rewards = new List<RewardData>();
            rewards.Add(new RewardData(RewardType.FamePoints, (11 - playerPosition) * 3));
            rewardsContainer.FillRewards(rewards, SongController.Instance.songUI.topBar);
        }
    }

    private async void ClaimRewards()
    {
        yesterdayId = yesterdayLeaderboardId;
        rewardsContainer.ClaimRewards();
        rewardsContainer.ClearRewards();
        await LoadData();
    }

    public void ShowWithFade()
    {
        Debug.Log("Show leaderboar with fade");
        gameObject.SetActive(true);
        canvasGroup.DOFade(1, 0.5f);
    }
    public void HideWithFade()
    {
        canvasGroup.DOFade(0, 0.5f).OnComplete(()=> gameObject.SetActive(false));
    }
}
