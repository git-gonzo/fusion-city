using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Assets.Scripts.MergeBoard;

public class PopupMergeChain : PopupBase
{
    public TextMeshProUGUI txtItemTitle;
    public TextMeshProUGUI txtItemLevel;
    public TextMeshProUGUI txtItemLevel2;
    public Transform selectedItemContainer;
    public Transform chainContainer;
    public GameObject boardTilePrefab;
    public GameObject frameTilePrefab;
    public RewardsContainer rewardsContainer;
    public Button btnDiscardMission;

    Action<MovingPiece> _onCancelMission;
    MovingPiece _missionPiece;

    public void Init(MergeMissionConfig missionConfig, Action<MovingPiece> onCancelMission, MovingPiece missionPiece)
    {
        _onCancelMission = onCancelMission;
        _missionPiece = missionPiece;
        AddSelectedItem(missionPiece.PieceState);
        //btnDiscardMission.gameObject.SetActive(missionConfig != null);
        if (missionConfig != null)
        {
            AddRewards(missionConfig.rewardInstant);
            //btnDiscardMission.onClick.AddListener(RemoveMission);
        }
        else
        {
            rewardsContainer.gameObject.SetActive(false);
        }
        AddChainItems(chainContainer, boardTilePrefab, frameTilePrefab, missionPiece.PieceDiscovery);
        onCloseCallback += PopupMissionClosed;
        PlayerPrefs.SetInt("PopupMissionOpen",1);
    }
    
    public void Init(PieceState piece)
    {
        AddSelectedItem(piece);
        rewardsContainer.gameObject.SetActive(false);

        AddChainItems(chainContainer, boardTilePrefab, frameTilePrefab, piece.pieceDiscovery());
        onCloseCallback += PopupMissionClosed;
        PlayerPrefs.SetInt("PopupMissionOpen",1);
    }

    private void PopupMissionClosed()
    {
        PlayerPrefs.SetInt("PopupMissionClosed", 1);
        onCloseCallback -= PopupMissionClosed;
    }

    private void AddRewards(List<RewardData> rewards)
    {
        rewardsContainer.FillRewards(rewards, GameManager.Instance.topBar);
    }

    public void AddSelectedItem(PieceState piece)
    {
        var prefab = _mergeConfig.GetPiecePrefab(piece);
        GameManager.RemoveChildren(selectedItemContainer.gameObject);
        Instantiate(prefab, selectedItemContainer);
        txtItemTitle.text = piece.pieceType.ToString();
        txtItemLevel.text = (piece.pieceLevel+1).ToString();
        txtItemLevel2.text = "Level " + (piece.pieceLevel +1);
    }

    public void RemoveMission()
    {
        Debug.Log("Remove mission 1");
        _onCancelMission?.Invoke(_missionPiece);
        OnClose();
    }

    public void AddFrame()
    {

    }
}
