using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.Events;

public class DailyItemController : MonoBehaviour
{
    [SerializeField] private RewardItemUniversal item;
    [SerializeField] private GameObject completed;
    [SerializeField] private GameObject expired;
    [SerializeField] private GameObject focused;
    [SerializeField] private GameObject tomorrow;
    [SerializeField] private TextMeshProUGUI txtDay;

    private Vector3 _originalItemScale;
    private Action _onClaim;

    private bool CanClaim => GameManager.Instance.DailyBonusManager.IsClaimable();

    public void InitPiece(int index, PieceDiscovery piece, bool isCompleted = false, bool isFocused = false, Action onClaim = null)
    {
        item.InitReward(piece);
        Init(index, isCompleted, isFocused);
        _onClaim = onClaim;
    }
    public void InitResource(int index, RewardData reward, bool isCompleted = false, bool isFocused = false)
    {
        item.InitReward(reward, GameManager.Instance.topBar);
        Init(index, isCompleted, isFocused);
    }

    private void Init(int index, bool isCompleted, bool isFocused) 
    {
        UpdateState(isCompleted, isFocused);
        txtDay.text = $"Day {index}";
        item.SetStateNaked();
        
    }

    public void UpdateState(bool isCompleted, bool isFocused)
    {
        expired.SetActive(false);
        tomorrow.SetActive(false);
        completed.SetActive(isCompleted);
        focused.SetActive(isFocused && CanClaim);
        if (focused.activeSelf)
        {
            _originalItemScale = item.transform.localScale;
            gameObject.AddComponent<Button>().onClick.AddListener(TapItem);
            Bounce();
        }
    }

    private void TapItem()
    {
        _onClaim?.Invoke();
    }

    public void ShowTomorrow()
    {
        tomorrow.SetActive(true);
    }
    public void ShowExpired()
    {
        expired.SetActive(true);
    }

    public void ClaimReward(Action OnRewarded)
    {
        item.transform.DOKill();
        item.ApplyReward(OnRewarded);
    }

    private void Bounce()
    {
        item.transform.DOScale(_originalItemScale * 1.35f, 0.5f).SetDelay(0.5f).SetLoops(-1,LoopType.Yoyo);
    }
}
