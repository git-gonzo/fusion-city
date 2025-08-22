using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class ShopDailyDealItem : MonoBehaviour
{
    public ButtonBuy btnBuy;
    public Transform itemContainer;
    public TextMeshProUGUI txtStock;
    public TextMeshProUGUI txtOutOfStock;
    public Button btnItem;
    private ItemShop _itemShop;
    private Action _afterBuy;
    private MergeConfig mergeConfig => GameManager.Instance.gameConfig.mergeConfig;
    private MergeBoardModel mergeModel => GameManager.Instance.mergeModel;

    private GameObject _itemInstance;
    private Transform _shopContainer;
    public void ShowItem(ItemShop itemShop,float delay, Transform shopcontainer)
    {
        _shopContainer = shopcontainer;
        _itemShop = itemShop;
        GameManager.RemoveChildren(itemContainer.gameObject);
        _itemInstance = Instantiate(mergeConfig.GetPiecePrefab(new PieceState(itemShop.item, itemShop.level)),itemContainer);
        btnBuy.Init(itemShop.price,true);
        btnBuy.OnBuyAction = OnBuy;
        itemContainer.DOScale(0, 0.5f).From().SetEase(Ease.OutBack).SetDelay(delay);
        if (_itemShop.stock > 0)
        {
            txtOutOfStock.gameObject.SetActive(false);
            btnBuy.transform.DOScale(0, 0.5f).From().SetEase(Ease.OutBack).SetDelay(delay + 0.2f).OnComplete(() => txtOutOfStock.gameObject.SetActive(true));
        }
        Refresh();
    }
    public void ShowChest(ItemShop itemShop,float delay, Transform shopcontainer)
    {
        _shopContainer = shopcontainer;
        _itemShop = itemShop;
        GameManager.RemoveChildren(itemContainer.gameObject);
        _itemInstance = Instantiate(mergeConfig.GetPiecePrefab(new PieceState(itemShop.item, itemShop.level)),itemContainer);
        btnItem.onClick.AddListener(() => { Debug.Log("ok"); GameManager.Instance.PopupsManager.ShowChestPopup(itemShop.pDiscovery); });
        btnBuy.Init(itemShop.price,true);
        btnBuy.OnBuyAction = OnBuyChest;
        itemContainer.DOScale(0, 0.5f).From().SetEase(Ease.OutBack).SetDelay(delay);
        txtStock.gameObject.SetActive(false);
        txtOutOfStock.gameObject.SetActive(false);
        btnBuy.gameObject.SetActive(true);
        btnBuy.transform.DOScale(0, 0.5f).From().SetEase(Ease.OutBack).SetDelay(delay + 0.2f);
    }

    private void OnBuy()
    {
        if (_itemShop.stock > 0 && GameManager.TryToSpend(_itemShop.price))
        {
            AddItem();
            mergeModel.currentDailyDeal.items.Find(i => i.item == _itemShop.item && i.level == _itemShop.level).stock--;
            mergeModel.SaveDailyDeals();
            Refresh(true);
            if(_itemShop.stock > 0)
            {
                btnBuy.button.interactable = false;
                btnBuy.transform.DOScale(0.5f, 0.5f).OnComplete(()=> { btnBuy.button.interactable = true;  });
            }
            _afterBuy?.Invoke();
        }
    }
    
    private void OnBuyChest()
    {
        if (GameManager.TryToSpend(_itemShop.price))
        {
            AddItem();
            btnBuy.button.interactable = false;
            btnBuy.transform.DOScale(0.5f, 0.5f).OnComplete(()=> { btnBuy.button.interactable = true;  });
            _afterBuy?.Invoke();
        }
    }

    private void AddItem()
    {
        var flyingItem = Instantiate(_itemInstance, _shopContainer);
        flyingItem.transform.position = _itemInstance.transform.position;
        flyingItem.transform.DOMove(Vector3.zero, 0.6f).OnComplete(() => Destroy(flyingItem)).SetEase(Ease.InSine);
        flyingItem.transform.DOPunchScale(Vector3.one * 1.6f, 0.6f, 1, 0.1f);
        mergeModel.AddGift(_itemShop.item, _itemShop.level);
    }
    
    private void Refresh(bool bounce = false)
    {
        txtStock.text = "Stock: " + _itemShop.stock;
        btnBuy.gameObject.SetActive(_itemShop.stock > 0);
        if (bounce) txtStock.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
    }
}
