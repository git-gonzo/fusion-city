using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static NotificationsController;
using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class RewardItemUniversal : MonoBehaviour
{
    public SO_RewardSprites rewardIcons;
    public Image icon;
    public TextMeshProUGUI amountText;
    public GameObject glow;
    public GameObject special;
    public GameObject BGrequirement;
    public GameObject BGRed;
    public GameObject BGNormal;
    public GameObject BGNormal_blue;
    public GameObject BGNormal_green;
    public GameObject particlesCoins;
    public GameObject particlesGems;
    public GameObject iconUp;
    public GameObject iconDown;
    public AudioSource sfxClaimCoins;
    public AudioSource sfxClaimGems;
    public RewardData RewardData => _rewardData;
    [SerializeField] private VehicleThumbnail vehicleThumbnail;
    [SerializeField] private Transform _mergeItemContainer;
    [HideInInspector] public Transform mergeItemFlyingContainer;
    private RewardData _rewardData;
    protected GameObject _pieceReward;
    private ButtonMergeStore _storageButton => GameManager.Instance.BtnStorage;
    private Action _onRewarded;
    private Vector3 scaleOrigin;
    private TopBar _topbar;
    public void InitReward(RewardData rewardData, TopBar topbar, bool readyToClaim = false, bool frameless = false)
    {
        _topbar = topbar;
        glow.SetActive(readyToClaim);
        iconUp.SetActive(false);
        iconDown.SetActive(false);
        vehicleThumbnail.gameObject.SetActive(false);
        _rewardData = rewardData;
        if (rewardData.rewardType == RewardType.MergeItem)
        {
            amountText.text = "x" +rewardData.amount;
            InitReward(rewardData.mergePiece,rewardData.amount);
            return;
        }
        else if(rewardData.rewardType == RewardType.Vehicle)
        {
            InitVehicle(rewardData.vehicleID);
        }
        icon.sprite = rewardIcons.GetSpriteByType(rewardData.rewardType);
        special.SetActive(rewardData.rewardType == RewardType.Gems);
        particlesCoins.SetActive(rewardData.rewardType == RewardType.Coins);
        particlesGems.SetActive(rewardData.rewardType == RewardType.Gems);
        amountText.text = rewardData.amount.ToString();
        BGrequirement.SetActive(false);
        scaleOrigin = transform.localScale;
        StopBounce();
    }

    public void InitReward(PieceDiscovery rewardData, int amount = 1)
    {
        iconUp.SetActive(false);
        iconDown.SetActive(false);
        vehicleThumbnail.gameObject.SetActive(false);
        _rewardData = new RewardData(rewardData, amount);
        var prefab = GameManager.Instance.gameConfig.mergeConfig.GetPiecePrefab(rewardData);
        _pieceReward = Instantiate(prefab, _mergeItemContainer);
        icon.gameObject.SetActive(false);
        special.SetActive(false);
        particlesCoins.SetActive(false);
        particlesGems.SetActive(false);
        BGrequirement.SetActive(true);
        scaleOrigin = transform.localScale;
    }

    public void InitVehicle(int vID)
    {
        amountText.text = "";
        special.SetActive(true);
        vehicleThumbnail.gameObject.SetActive(true);
        vehicleThumbnail.GenerateThumbnail(vID);
        icon.gameObject.SetActive(false);
        scaleOrigin = transform.localScale;
    }

    public void InitSimple(RewardData rewardData)
    {
        scaleOrigin = transform.localScale;
        _rewardData = rewardData;
    }
    public void InitSimple(RewardData rewardData, PieceDiscovery piece, Transform flyingContainer)
    {
        _rewardData = rewardData;
        _rewardData.mergePiece = piece;
        InitReward(piece);
        mergeItemFlyingContainer = flyingContainer;
        scaleOrigin = transform.localScale;
    }

    public void ApplyReward(Action onRewarded = null)
    {
        _onRewarded = onRewarded;
        ApplyReward(transform.position);
    }

    public virtual void ApplyReward(Vector3 pos)
    {
        if (_rewardData != null)
        {
            if (_rewardData.rewardType == RewardType.FamePoints)
            {
                _topbar.AnimateFame(_rewardData.amount, true);
            }
            else if (_rewardData.rewardType == RewardType.Coins)
            {
                sfxClaimCoins.Play();
                _topbar.AnimateCoins(_rewardData.amount, true);
            }
            else if (_rewardData.rewardType == RewardType.Gems)
            {
                sfxClaimGems.Play();
                _topbar.AnimateGems(_rewardData.amount, true);
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.AddRewardToPlayer(_rewardData, true);
            }
            if (_rewardData.rewardType == RewardType.MergeItem)
            {
                mergeItemFlyingContainer ??= GameManager.Instance.mainCanvasTransform;
                if (GameManager.Instance.mergeMapMissionsScreen.activeSelf)
                {
                    mergeItemFlyingContainer = GameManager.Instance.mergeMapMissionsScreen.transform;
                }
                for (var i = 0; i < _rewardData.amount; i++)
                {
                    //var flyingItem = Instantiate(_pieceReward, mergeItemFlyingContainer);
                    var flyingItem = Instantiate(_pieceReward, GameManager.Instance.mainCanvasTransform);
                    flyingItem.transform.position = _pieceReward.transform.position;
                    flyingItem.transform.DOMove(_storageButton.icon.transform.position, 0.8f)
                        .OnComplete(() =>
                            {
                                Destroy(flyingItem);
                                _storageButton.Bounce();
                                GameManager.Instance.soundStorageBounce.Play();
                                GameManager.Instance.UpdateStorageButtons();
                                _onRewarded?.Invoke();
                            })
                        .SetEase(Ease.InSine).SetDelay(0.2f*i);
                    flyingItem.transform.DOPunchScale(Vector3.one * 1.6f, 0.8f, 1, 0.1f).SetDelay(0.2f * i);
                }
            }
            else if(_rewardData.rewardType == RewardType.Vehicle)
            {
                GameManager.Instance.playerData.AddVehicleByID(_rewardData.vehicleID,false);
            }
            else
            {
                UIUtils.FlyingParticles(_rewardData.rewardType, pos, Mathf.Min(_rewardData.amount, 10), null);
            }
        }
    }

    public void SetStateRed()
    {
        BGRed.SetActive(true);
    }
    public void SetStateBlue()
    {
        BGNormal.SetActive(false);
        BGRed.SetActive(false);
        BGrequirement.SetActive(false);
    }
    public void SetStateGreen()
    {
        BGNormal.SetActive(false);
        BGRed.SetActive(false);
        BGrequirement.SetActive(false);
        BGNormal_green.SetActive(true);
    }

    public void SetStateNaked()
    {
        BGNormal.SetActive(false);
        BGNormal_blue.SetActive(false);
        BGRed.SetActive(false);
        BGrequirement.SetActive(false);
        amountText.gameObject.SetActive(false);
    }
    public void SetStateNormal()
    {
        BGNormal.SetActive(true);
        BGNormal_blue.SetActive(false);
        BGRed.SetActive(false);
        BGrequirement.SetActive(false);
        amountText.gameObject.SetActive(false);
    }

    public void Bounce()
    {
        transform.DOScale(scaleOrigin * 1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
    public void StopBounce()
    {
        transform.DOKill();
        transform.DOScale(scaleOrigin, 0);
    }

    public void AddButton(UnityAction onClick = null)
    {
        gameObject.AddComponent<Button>().onClick.AddListener(() =>
        {
            this.ApplyReward();
            onClick?.Invoke();
        });
    }
    public void RemoveButton()
    {
        if (gameObject.TryGetComponent<Button>(out var btn))
        {
            btn.onClick.RemoveAllListeners();
        }
    }

    public void ResetItemScale()
    {
        _mergeItemContainer.transform.DOScale(Vector3.one, 0.1f);
        _mergeItemContainer.transform.localPosition = Vector3.zero;
    }
}
