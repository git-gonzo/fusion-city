using Assets.Scripts.MergeBoard;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonusController : PopupBase
{
    
    public int gemsRestoreBase = 4;
    public List<DailyItemController> items;
    public DailyItemController finalItem;
    public Button btnClaim;
    public Button btnDiscard;
    public ButtonWithPrice btnRestore;
    public GameObject buttonsRestore;

    private bool CanClaim => GameManager.Instance.DailyBonusManager.IsClaimable();
    private bool isExpired => GameManager.Instance.DailyBonusManager.IsExpired();
    private int progress => GameManager.Instance.DailyBonusManager.dailyBonusProgress.progress;
    private DailyItemController currentItem { get 
        {
            if (progress < 6) return items[progress];
            else return finalItem;
        } }
    public void Show(List<PieceDiscovery> hardRewards)
    {
        for (var i = 0; i < 6; i++)
        {
            items[i].InitPiece(i+1,hardRewards[i],i< progress, i == progress,OnClaim);
        }
        finalItem.InitPiece(7, hardRewards[6], 6 < progress, 6 == progress);
        btnClaim.gameObject.SetActive(CanClaim);
        buttonsRestore.SetActive(false);
        btnClose.gameObject.SetActive(false);

        if (CanClaim)
        {
            btnClaim.onClick.AddListener(OnClaim);
        }
        else if(isExpired)
        {
            currentItem.ShowExpired();
            btnRestore.SetPrice(gemsRestoreBase * progress, RewardType.Gems);
            btnRestore.button.onClick.AddListener(OnRestore);
            btnDiscard.onClick.AddListener(OnRestoreCancel);
            buttonsRestore.SetActive(true);
        }
        else 
        {
            currentItem.ShowTomorrow();
            btnClose.gameObject.SetActive(true);
        }
        base.Show();
    }

    internal void OnClaim()
    {
        btnClaim.onClick.RemoveAllListeners();
        currentItem.ClaimReward(OnRewardsComplete);
        GameManager.Instance.DailyBonusManager.ClaimReward();
        GameManager.Instance.sfxClaimEnergy.Play();
    }

    private void OnRewardsComplete()
    {
        if(progress == 0)
        {
            OnClose();
            return;
        }
        UpdateState();
        currentItem.ShowTomorrow();
        btnClaim.gameObject.SetActive(false);
        btnClose.gameObject.SetActive(true);
    }

    private void UpdateState()
    {
        for (var i = 0; i < 6; i++)
        {
            items[i].UpdateState(i < progress, i == progress);
        }
        finalItem.UpdateState(6 < progress, 6 == progress);
    }

    private void OnRestore()
    {
        if (GameManager.TryToSpend(btnRestore.cost))
        {
            GameManager.Instance.DailyBonusManager.Restore();
            UpdateState();
            btnClaim.onClick.RemoveAllListeners();
            btnClaim.onClick.AddListener(OnClaim);
            btnClaim.gameObject.SetActive(true);
            buttonsRestore.SetActive(false);
        }
        else
        {
            OnClose();
        }
    }
    private void OnRestoreCancel()
    {
        buttonsRestore.SetActive(false);
        GameManager.Instance.DailyBonusManager.RestoreCancel();
        UpdateState();
        btnClaim.onClick.RemoveAllListeners();
        btnClaim.onClick.AddListener(OnClaim);
        btnClaim.gameObject.SetActive(true);
    }
}
