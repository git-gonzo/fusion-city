using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class SongUI:MonoBehaviour
{
    public ButtonWithPrice btnStartGame;
    public Button btnBackToMap;
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtPerformance;
    public TextMeshProUGUI txtTotalScore;
    public TextMeshProUGUI txtFinalScore;
    public Image icoVideo;
    public GameObject SongScoreContainer;
    public LeaderboardMusic leaderboardScreen;
    public CanvasGroup EndScreen;
    public TopBar topBar;

    private void Start()
    {
        txtScore.DOFade(0, 0f);
        txtPerformance.DOFade(0, 0f);
        EndScreen.DOFade(0, 0f);
        SongScoreContainer.SetActive(false);
        ShowLeaderboard();
    }

    public void OnAddScore(ScorePerformance performance, int amount)
    {
        txtPerformance.text = performance.ToString();
        txtPerformance.transform.DOKill();
        txtPerformance.transform.DOScale(1, 0);
        txtPerformance.DOFade(0, 0f);
        txtPerformance.DOFade(1, 0.2f);
        txtPerformance.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).OnComplete(() =>
        {
            txtPerformance.DOFade(0, 0.2f);
        });

        txtScore.text = amount.ToString();
        txtScore.transform.DOKill();
        txtScore.transform.DOScale(1, 0);
        txtScore.DOFade(0, 0f);
        txtScore.DOFade(1, 0.2f);
        txtScore.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).OnComplete(() =>
        {
            txtScore.DOFade(0, 0.2f);
        });
    }
    
    public void TotalScore(int amount)
    {
        txtTotalScore.text = amount.ToString();
        txtTotalScore.transform.DOKill();
        txtTotalScore.transform.DOScale(1, 0);
        txtTotalScore.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
    }

    public void StartMusicGame()
    {
        if (!topBar.TryToSpendGems(btnStartGame.cost.amount)) return;
        SongController.Instance.StartMusicGame();
        ShowStartButtons(false);
        SongScoreContainer.SetActive(true);
        leaderboardScreen.HideWithFade();
        TotalScore(0);
        topBar.FameContainer.SetActive(false);
    }

    public void ShowFinalScore(int finalScore)
    {
        SongScoreContainer.SetActive(false);
        EndScreen.gameObject.SetActive(true);
        EndScreen.DOFade(1, 0.3f);
        GameManager.AnimateFormatedNumber(txtFinalScore, 0, finalScore, true);
        //DOVirtual.Float(0, finalScore, 1, (v) => txtFinalScore.text = ((int)v).ToString());
        DOVirtual.DelayedCall(2, ShowLeaderboard);
    }

    private async void ShowLeaderboard()
    {
        await leaderboardScreen.LoadData();
        EndScreen.DOFade(0, 0.3f).OnComplete(() =>
        {
            Debug.Log("EndScreen Hidden");
            topBar.FameContainer.SetActive(true);
            EndScreen.gameObject.SetActive(false);
            leaderboardScreen.ShowWithFade();
        });
    }

    internal void ShowStartButtons(bool value = true)
    {
        btnStartGame.SetPrice(SongController.Instance.TodayPrice, RewardType.Gems);
        btnStartGame.gameObject.SetActive(value);
        btnBackToMap.gameObject.SetActive(value);
    }
}
