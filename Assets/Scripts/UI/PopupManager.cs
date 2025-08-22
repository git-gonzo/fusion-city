using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.Scripts.MergeBoard;
using static GameConfigMerge;

public class PopupManager : MonoBehaviour
{
    public GameObject PopupContainer;
    public CanvasGroup Bg;
    public Image Backgroun;
    public GameObject popupYesNo;
    public GameObject popupPlayerState;
    public GameObject popupChangeName;
    public PopupSettings popupSettings;
    public GameObject popupAvatar;
    public GameObject popupLeagueReward;
    public PopupWelcomePack popupWelcomePack;
    public PopupCarOffer popupCarOffer;
    public PopupGeneratorChest popupGenerator;
    public PopupGeneratorChest popupChest;
    public PopupGiftStealReceived popupGiftSteal;
    public PopupGenericOffer popupGenericOffer;
    public PopupGenericOffer popupGenericOfferGenerator;
    public PopupTutorial popupTutorial;
    public PopupBuildingInfo popupBuilding;
    public DailyBonusController popupDailyBonus;
    public AudioSource popupOpen;
    public AudioSource popupClose;
    public GameObject canvas3d;
    private GameObject popupInstance;
    private List<GameObject> _popups;
    bool _wasLowerBarVisible = false;
    [SerializeField] private PopupPurchaseReward _purchasedPopup;
    [SerializeField] private PopupAssistant _assistantPopup;
    [SerializeField] private PopupPurchaseReward _resetProgressPopup;
    [SerializeField] private PopupStealSuccess _stealsuccessPopup;

    Action _onYesCallback;
    Action _onNoCallback;

    public enum PopupType
    {
        yesno,
        ok
    }

    public void ShowPopupYesNo(string title, string descrip, PopupType popupType = PopupType.yesno, Action onYesCallback = null, Action onNoCallback = null)
    {
        _onNoCallback = onNoCallback;
        _onYesCallback = onYesCallback;
        //PopupContainer.SetActive(true);
        //Bg.DOFade(1, 0.1f);
        var p = Instantiate(popupYesNo, PopupContainer.transform);
        PrepareStack(p.gameObject);
        if (popupType == PopupType.yesno)
        {
            p.GetComponent<PopupYesNo>().Init(title, descrip, ClosePopupYes, ClosePopupNo);
        }
        else if (popupType == PopupType.ok)
        {
            p.GetComponent<PopupYesNo>().InitOk(title, descrip, ClosePopupYes);
        }
    }

    public void ShowMyPlayerStats()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
        }
        PopupContainer.SetActive(true);
        Bg.DOFade(1, 0.1f);
        popupInstance = Instantiate(popupPlayerState, PopupContainer.transform);
        popupInstance.GetComponent<PopupPlayerState>().InitMyPlayer(Close, ShowChangeName);
    }
    
    public void ShowOtherPlayerStats(LeaderboardPlayer data)
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
        }
        PopupContainer.SetActive(true);
        Bg.DOFade(1, 0.1f);
        popupInstance = Instantiate(popupPlayerState, PopupContainer.transform);
        popupInstance.GetComponent<PopupPlayerState>().InitOtherPlayer(Close, data);
    }
    
    public void ShowWelcomePack()
    {
        canvas3d.SetActive(true);
        ShowPopup(popupWelcomePack, true);
    }
    public void ShowCarOffer(CarOffer carOffer)
    {
        var p = Instantiate(popupCarOffer, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Init(carOffer.extraRewards, carOffer.vehicleId);
        p.Show();
    }

    public void ShowSettings()
    {
        var p = Instantiate(popupSettings, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.Init(CloseInStack);
    }
    public void ShowChangeName()
    {
        if (IsPopupOpen<PopupChangeName>()) return;
        var p = Instantiate(popupChangeName, PopupContainer.transform);
        PrepareStack(p);
        p.GetComponent<PopupChangeName>().Init(CloseInStack, 0.7f);            
    }
    public void ShowPopupLeagueReward(int position, Action onClose)
    {
        var p = Instantiate(popupLeagueReward, PopupContainer.transform);
        PrepareStack(p);
        p.GetComponent<PopupLeagueReward>().ShowRewardsForNextLevel(position, () => { 
            CloseInStack();
            onClose.Invoke(); ;
        }) ;      
    }
    public void ShowChangeAvatar(Action OnAvatarChanged)
    {
        var p = Instantiate(popupAvatar, PopupContainer.transform);
        PrepareStack(p);
        p.GetComponent<PopupAvatar>().Init(CloseInStack, OnAvatarChanged);            
    }
    public void ShowPopupTutorial(PopupTutorial popupTuto)
    {
        popupTutorial = Instantiate(popupTuto, PopupContainer.transform);
        PrepareStack(popupTutorial.gameObject);
        popupTutorial.Init(CloseInStack);            
    }
    public void ShowChangeAvatar()
    {
        var p = Instantiate(popupAvatar, PopupContainer.transform);
        PrepareStack(p);
        p.GetComponent<PopupAvatar>().Init(CloseInStack, null);            
    }
    public void ShowChestPopup(PieceDiscovery piece)
    {
        var p = Instantiate(popupChest, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Init(piece);
    }
    public void ShowGeneratorPopup(MovingPiece piece, Action OnLevelUP)
    {
        var p = Instantiate(popupGenerator, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.OnLevelUp = OnLevelUP;
        p.onCloseCallback = CloseInStack;
        p.Init(piece);
    }
    public void ShowDailyBonusPopup(List<PieceDiscovery> hardRewards)
    {
        var p = Instantiate(popupDailyBonus, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Show(hardRewards);
    }
    
    public void ShowBuildingPopup(SO_Building building)
    {
        var p = Instantiate(popupBuilding, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Show(building);
    }
    public void ShowGiftStealReceivedPopup(List<RewardData> pieces, List<string> players, bool isGift)
    {
        var p = Instantiate(popupGiftSteal, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        if (isGift)
        {
            p.InitGift(pieces, players);
        }
        else
        {
            p.InitSteal(pieces, players);
        }
    }
    public void ShowGenericOfferPopup()
    {
        var p = Instantiate(popupGenericOffer, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Show();
    }
    
    public void ShowGenericOfferGeneratorPopup()
    {
        var p = Instantiate(popupGenericOfferGenerator, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Show();
    }

    void PrepareStack(GameObject p)
    {
        _popups ??= new List<GameObject>();
        Bg.DOKill();
        Bg.DOFade(1, 0.1f);
        PopupContainer.SetActive(true);
        if(_popups.Count > 0)
        {
            foreach (var popup in _popups) popup.SetActive(false);
        }
        _popups.Add(p);
        if(popupInstance != null)
        {
            popupInstance.SetActive(false);
        }
    }

    public GameObject ShowPopup(PopupBase popupPrefab, bool hideLowerbar = false)
    {
        Bg.DOFade(1, 0.1f);
        popupOpen.Play();
        PopupContainer.SetActive(true);
        popupInstance = Instantiate(popupPrefab, PopupContainer.transform).gameObject;
        popupInstance.GetComponent<PopupBase>().onCloseCallback += () =>
        {
            Close();
            //canvas3d.SetActive(false);
        };
        popupInstance.GetComponent<PopupBase>().Show();
        if (hideLowerbar)
        {
            if (GameManager.Instance.lowerBar)
            {
                _wasLowerBarVisible = true;
                GameManager.Instance.ShowMapLowerBar(false);
            }
        }
        return popupInstance;
    }

    public void ShowPopupPurchase(List<RewardData> rewards, bool ApplyReward = false)
    {
        var p = Instantiate(_purchasedPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Init(rewards, ApplyReward);
    }
    public void ShowPopupAssistant(string id)
    {
        var p = Instantiate(_assistantPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Init(id);
    }
    public void ShowPopupAssistantSmartGenerators()
    {
        var p = Instantiate(_assistantPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Init("SmartGenerators");
    }

    public void ShowPopupReset(int gemsAmount, Action OnClose)
    {
        var p = Instantiate(_resetProgressPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = OnClose;
        p.txtObjectRewarded.text = $"x{gemsAmount}";
        p.Show();
    }
    public void ShowPopupPurchaseVehicleBuilding(string objectName)
    {
        var p = Instantiate(_purchasedPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.InitWithText(objectName);
    }
    public void ShowPopupPurchaseVehicle(SO_Vehicle v, bool isAdded)
    {
        var p = Instantiate(_purchasedPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.InitWithVehicle(v);
    }
    public void ShowPopupGiftSent(List<RewardData> rewards)
    {
        var p = Instantiate(_purchasedPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.InitWithGift(rewards);
    }
    public void ShowPopupStealSent(PieceDiscovery stolen, PieceDiscovery weapon)
    {
        var p = Instantiate(_stealsuccessPopup, PopupContainer.transform);
        PrepareStack(p.gameObject);
        p.onCloseCallback = CloseInStack;
        p.Init(stolen,weapon);
    }
    public void ShowPopupInstance(PopupBase popupInstance)
    {
        Bg.DOFade(1, 0.1f);
        PopupContainer.SetActive(true);
        popupInstance.GetComponent<PopupBase>().Show();
    }

    private void ClosePopupYes()
    {
        CloseInStack();
        _onYesCallback?.Invoke();
    }
    private void ClosePopupNo()
    {
        CloseInStack();
        _onNoCallback?.Invoke();
    }

    private void Close()
    {
        popupClose.Play();
        if (popupInstance == null) return;
        if (popupInstance.TryGetComponent<PopupBase>(out var popupBase))
        {
            popupBase.onCloseCallback -= Close;
        }
        if (Bg != null)
        {
            Bg.DOFade(0, 0.1f).OnComplete(() => { PopupContainer.SetActive(false); });
        }
        popupInstance.transform.DOScale(0.5f, 0.08f).OnComplete(() =>
        {
            DestroyImmediate(popupInstance);
        });
        if (_wasLowerBarVisible)
        {
            _wasLowerBarVisible = false;
            GameManager.Instance.ShowMapLowerBar(true);
        }
    }

    private void CloseInStack()
    {
        GameManager.Log("Close in Stack");
        popupClose.Play();
        var p = _popups[_popups.Count - 1];
        if (p.TryGetComponent<PopupBase>(out var popupBase))
        {
            popupBase.onCloseCallback -= CloseInStack;
        }
        if (Bg != null && _popups.Count == 1)
        {
            if (popupInstance == null)
            {
                Bg.DOFade(0, 0.1f).OnComplete(() => { PopupContainer.SetActive(false); });
            }
            else
            {
                popupInstance.SetActive(true);
            }
        }
        _popups.Remove(p);
        p.transform.DOScale(0.5f, 0.08f).OnComplete(() =>
        {
            if(_popups.Count > 0)
            {
                _popups[_popups.Count - 1].SetActive(true);
            }
            DestroyImmediate(p);
        });
    }

    private bool IsPopupOpen<T>() where T : Component
    {
        if (_popups == null || _popups.Count == 0)
        {
            return false;
        }

        foreach (var popup in _popups)
        {
            if (popup != null && popup.GetComponent<T>() != null)
            {
                return true;
            }
        }

        return false;
    }
}
