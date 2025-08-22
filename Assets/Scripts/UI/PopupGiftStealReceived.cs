using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class PopupGiftStealReceived : PopupBase
{
    [SerializeField] private RewardItemUniversal rewardPrefab;
    [SerializeField] private RewardsContainer rewardsContainer;
    [SerializeField] private TextMeshProUGUI _txtTitle;
    [SerializeField] private TextMeshProUGUI _txtDescrip;
    [SerializeField] private TextMeshProUGUI _txtOtherPlayer;
    [SerializeField] private Color _giftBG;
    [SerializeField] private Color _stealBG;
    [SerializeField] private Image _BG;
    
    public void InitGift(List<RewardData> rewards, List<string> senderName)
    {
        _BG.color = _giftBG;
        InitBase(rewards, senderName);
        GameManager.SetLocString("UI", "GiftReceivedTitle", _txtTitle);
        GameManager.SetLocString("UI", "YouHaveGifts", _txtDescrip);
        onCloseCallback += () =>
        {
            rewardsContainer.ClaimRewards();
        };
    }
    public void InitSteal(List<RewardData> rewards, List<string> senderName)
    {
        _BG.color = _stealBG;
        InitBase(rewards, senderName);
        GameManager.SetLocString("UI", "StolenTitle", _txtTitle);
        GameManager.SetLocString("UI", "StolenDescrip", _txtDescrip);
    }

    private void InitBase(List<RewardData> rewards, List<string> senderName)
    {
        rewardsContainer.FillRewards(rewards, GameManager.Instance.topBar, true);
        transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
        btnClose.onClick.AddListener(OnClose);
        _txtOtherPlayer.text = senderName[0];
        for (int i = 1; i < senderName.Count; i++)
        {
            _txtOtherPlayer.text += ", " + senderName[i];
        }
    }
}
