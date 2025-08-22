using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Coffee.UIEffects;

public class VehicleView : MonoBehaviour
{
    [SerializeField] Color colorNormal;
    [SerializeField] Color colorSelected;
    public Image BGVehicle;
    public Image BGFrame;
    public List<Image> titleBG;
    public TextMeshProUGUI TextTitle;
    public TextMeshProUGUI TextDescrip;
    public TextMeshProUGUI TextStatSpeed;
    public TextMeshProUGUI TextStatSpeedOwned;
    public TextMeshProUGUI TextStatDurability;
    public GameObject ownedContent;
    public GameObject notOwnedContent;
    public Button btnSelect;
    public ButtonBuy buyButton;
    public TextMeshProUGUI TextNoVehicle;
    public ProgressBar durabilityBar;
    public VehicleThumbnail thumbnail;


    //[SerializeField] bool _isMap;

    private CanvasGroup _canvasGroup;
    private SO_Vehicle _v;
    private Action _onSelect;
    private VehicleIconController _icon;
    private Action _onBuy;

    public CanvasGroup canvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();
    private PlayerData playerData => GameManager.Instance.playerData;

    public void InitVehicleInShop(SO_Vehicle v, bool owned, bool inShop, int curDurability = 0, Action OnBuy = null) 
    {
        _onBuy = OnBuy;
        _v = v;
        _icon = GetComponent<VehicleIconController>();
        _icon.SetIcon(v.vehicleType);
        if (TextDescrip != null) TextDescrip.text = v.vehicleDescrip;
        if (notOwnedContent != null) notOwnedContent.SetActive(!owned);
        if (ownedContent != null) ownedContent.SetActive(owned);

        buyButton.gameObject.SetActive(!owned);
        buyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        buyButton.GetComponent<Button>().onClick.AddListener(BuyVehicle);
        buyButton.Init(v.price);
        InitBase();
    }
    public void InitVehicle(SO_Vehicle v, bool owned, bool inShop, int curDurability = 0, Action OnSelect = null)
    {
        if (TextNoVehicle != null) TextNoVehicle.gameObject.SetActive(false);
        _v = v;
        _onSelect = OnSelect;
        InitBase();

        btnSelect.onClick.AddListener(SelectVehicle);
        TextStatDurability.text = curDurability.ToString();

        durabilityBar.UpdateSimple(curDurability, v.durability);
        TextStatSpeedOwned.text = (v.reduceTimePercent).ToString();
        thumbnail.GenerateThumbnail(v.id);

        SetState();
    }

    private void InitBase()
    {
        TextTitle.text = _v.vehicleName;
        TextStatSpeed.text = (_v.reduceTimePercent).ToString();
        TextStatDurability.text = _v.durability.ToString();

    }


    private void BuyVehicle()
    {
        if (!GameManager.Instance.HasEnoughCurrency(_v.price, true)) return;
        GameManager.Instance.PopupsManager.ShowPopupYesNo(
            "Buy " + _v.vehicleType.ToString(),
            $"Are you sure you want to buy {_v.vehicleName}?<br>You will pay <color=yellow>{_v.price.amount}</color> {_v.price.currencyName}.",
            PopupManager.PopupType.yesno,
            () =>
            {
                if (!GameManager.TryToSpend(_v.price)) return;
                GameManager.Instance.playerData.AddVehicle(_v);
                if (ownedContent != null) ownedContent.SetActive(true);
                buyButton.gameObject.SetActive(false);
                GameManager.Instance.dailyTaskManager.OnBuyVehicle();
                TrackingManager.AddTracking(TrackingManager.Track.BuyVehicle, "VehicleName", _v.vehicleName);
                _onBuy?.Invoke();
            });
    }
public void SetState()
    {
        BGVehicle.color = _v.id == PlayerData.vehicleSelected ? colorSelected : colorNormal;
        BGFrame.color = _v.id == PlayerData.vehicleSelected ? colorSelected : Color.white;
        btnSelect.interactable = !playerData.IsTravelling;
        btnSelect.gameObject.SetActive(_v.id != PlayerData.vehicleSelected);
    }

    private void SelectVehicle()
    {
        PlayerData.vehicleSelected = _v.id;
        GameManager.Instance.mapManager.AddPlayerVehicle();
        _onSelect?.Invoke();
    }

    public void ShowScreenInMap()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup != null)
        {
            var sq = DOTween.Sequence();
            sq.Insert(0, _canvasGroup.DOFade(0, 0));
            sq.Insert(1, _canvasGroup.DOFade(1, 0.3f));
        }
        gameObject.SetActive(true);
    }

    

    
}
