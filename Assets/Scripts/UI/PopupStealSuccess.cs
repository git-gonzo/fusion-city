using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Assets.Scripts.MergeBoard;

public class PopupStealSuccess : PopupBase
{
    [SerializeField] private RewardsContainer rewardsContainer;
    [SerializeField] private RewardItemUniversal _stolen;
    [SerializeField] private RewardItemUniversal _weapon;

    public void Init(PieceDiscovery stolen, PieceDiscovery weapon)
    {
        _stolen.InitReward(stolen);
        _stolen.amountText.gameObject.SetActive(false);
        _stolen.SetStateBlue();
        _weapon.InitReward(weapon);
        _weapon.SetStateRed();
        _weapon.amountText.gameObject.SetActive(false);
        transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
        btnClose.onClick.AddListener(OnCloseAndClaim);
    }

    private void OnCloseAndClaim()
    {
        _stolen.ApplyReward();
        OnClose();
    }
}
