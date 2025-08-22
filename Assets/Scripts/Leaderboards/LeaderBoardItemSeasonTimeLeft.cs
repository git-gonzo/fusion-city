using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class LeaderBoardItemSeasonTimeLeft : MonoBehaviour
{
    public TextMeshProUGUI txtTimeLeft;
    public Button btnShowRewards;
    public PopupSeasonRewards popupPrizes;

    public void Awake()
    {
        btnShowRewards.onClick.AddListener(ShowRewardsPopup);
    }

    private void ShowRewardsPopup()
    {
        var popupInstance = GameManager.Instance.PopupsManager.ShowPopup(popupPrizes,true).GetComponent<PopupSeasonRewards>();
        popupInstance.Init();
    }
}
