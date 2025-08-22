using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SeasonEndController : MonoBehaviour
{
    public CinemachineCamera vCam3Pos;
    public CinemachineCamera vCam2Pos;
    public CinemachineCamera vCam1Pos;
    public CinemachineCamera vCamFinal;

    public OtherPlayerVehicle player3;
    public OtherPlayerVehicle player2;
    public OtherPlayerVehicle player1;

    public GameObject listPositions;
    public Transform listContainer;
    public GameObject listItemPrefabTop;
    public GameObject listItemPrefab;
    public Button btnNext;

    private float delay = 2f;
    private int _playerPos;
    private List<LeaderboardPlayer> _winners;
    private Action _onComplete;

    internal void ShowResults(List<LeaderboardPlayer> winners, Action onComplete)
    {
        _onComplete = onComplete;
        _winners = winners;
        vCam3Pos.Priority = winners.Count > 2 ? 200 : 0;
        if (winners.Count > 2)
        {
            DOVirtual.DelayedCall(0.5f, () => { Show1(winners[2]); });
            DOVirtual.DelayedCall(0.7f+delay, ()=> { Show2(winners[1]); });
            DOVirtual.DelayedCall(1+delay*2, ()=> { Show3(winners[0]); });
            DOVirtual.DelayedCall(1+delay*3, ShowFinalPositions);
        }
        else
        {
            vCam2Pos.Priority = 210;
            DOVirtual.DelayedCall(0.7f, ()=> { Show2(winners[1]); });
            DOVirtual.DelayedCall(1+delay, ()=> { Show3(winners[0]); });
            DOVirtual.DelayedCall(1+delay*2, ShowFinalPositions);
        }
    }

    private void Show1(LeaderboardPlayer player)
    {
        player3.Init(player);
        player3.ShowPlayersUI();
        player3.DisableInput();
    }   
    private void Show2(LeaderboardPlayer player)
    {
        player3.HidePlayersUI();
        player2.Init(player);
        player2.ShowPlayersUI();
        player2.DisableInput();
        vCam2Pos.Priority = 210;
        vCam3Pos.Priority = 0;
    }

    private void Show3(LeaderboardPlayer player)
    {
        player2.HidePlayersUI();
        player1.Init(player);
        player1.ShowPlayersUI();
        player1.DisableInput();
        vCam1Pos.Priority = 220;
        vCam2Pos.Priority = 0;
    }

    private void ShowFinalPositions()
    {
        player3.ShowPlayersUI();
        player2.ShowPlayersUI();
        vCamFinal.Priority = 210;
        vCam1Pos.Priority = 0;
        _playerPos = _winners.Find(w => w.isPlayer == true).position;
        btnNext.gameObject.SetActive(false);
        DOVirtual.DelayedCall(1.5f, () => { btnNext.gameObject.SetActive(true); });
        
        int playerItemIndex = 0;
        foreach (var def in _winners)
        {
            var item = Instantiate(def.position < 4?listItemPrefabTop: listItemPrefab, listContainer).GetComponent<LeaderBoardItenFameView>();
            item.Init(def);
            if(def.isPlayer) { playerItemIndex = listContainer.childCount; }
        }
        listPositions.SetActive(true);
        listPositions.transform.DOMoveY(-1000, 0.5f).SetDelay(0.5f).From().OnComplete(() => {
            //listContainer.parent.GetComponent<ScrollRect>().DOVerticalNormalizedPos(11, 0.4f);
            if (_playerPos > 3) {
                listContainer.transform.DOMoveY(playerItemIndex * 190 - 200, 0.5f);
            }
        });
        btnNext.onClick.AddListener(showPopupSeasonRewards);
    }

    private void showPopupSeasonRewards()
    {
        listPositions.SetActive(false);
        Globals.Instance.seasonFirstCheck = true;
        if (_playerPos < 11)
        {
            GameManager.Instance.PopupsManager.ShowPopupLeagueReward(_playerPos, Finish);
        }
        else
        {
            Finish();
        }
    }

    private void Finish()
    {
        vCamFinal.Priority = 0;
        _onComplete.Invoke();
    }
}


/*
 Example for snap Vlayout
protected ScrollRect scrollRect;
protected RectTransform contentPanel;

public void SnapTo(RectTransform target)
{
    Canvas.ForceUpdateCanvases();

    contentPanel.anchoredPosition =
            (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
            - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);
}
 */