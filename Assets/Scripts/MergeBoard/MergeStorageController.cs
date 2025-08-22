using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MergeStorageController : MonoBehaviour
{
    public StorageItemController storeItemPrefab;
    public Transform piecesContainer;
    public Transform giftSlot1;
    public Transform giftSlot2;
    public Transform giftSlot3;
    public GameObject giftBubble;
    public TextMeshProUGUI giftBubbleCounter;

    private List<Transform> _giftSlots;
    private List<Button> _giftButtons = new();
    private StorageItemController buyItem;
    
    public List<Button> giftButtons => _giftButtons;

    private int buySlotPrice { get {
            var basePrice = (int)((mergeModel.storageSlots - 2) * 1.5f - 0.5f) * 1000;
            if(mergeModel.storageSlots >= 10)
            {
                basePrice += (mergeModel.storageSlots - 8) * 500;
            }
            if (mergeModel.storageSlots >= 15)
            {
                basePrice += (mergeModel.storageSlots - 14) * 1000;
            }
            if (mergeModel.storageSlots >= 20)
            {
                basePrice += (mergeModel.storageSlots - 19) * 2000;
            }
            if (mergeModel.storageSlots >= 25)
            {
                basePrice += (mergeModel.storageSlots - 24) * 5000;
            }
            return basePrice;
        }}
    private MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
    public void LoadItems(MergeConfig mergeConfig)
    {
        if (piecesContainer.childCount > 0) OnDisable();

        _giftSlots ??= new List<Transform> { giftSlot1, giftSlot2, giftSlot3 };

        for (var i = 0; i< mergeModel.storageSlots; i++)
        {
            var item = Instantiate(storeItemPrefab, piecesContainer);
            item.SetNormalState();
            if (i < mergeModel.storage.Count)
            {
                var prefab = mergeConfig.GetPiecePrefab(mergeModel.storage[i]);
                var isReady = mergeModel.IsItemInMapMission(new PieceState(mergeModel.storage[i].pType, mergeModel.storage[i].Lvl));
                var isMaxed = mergeModel.storage[i].Lvl == mergeConfig.GetPieceTypeLevelsCount(mergeModel.storage[i].pType);
                item.AddItem(prefab, new PieceState(mergeModel.storage[i].pType, mergeModel.storage[i].Lvl), OnTapItem, isReady, isMaxed);
            }
        }
        addBuyItem();

        _giftButtons = new List<Button>();
        //Add gifts
        for (var i = 0; i < 3; i++)
        {
            GameManager.RemoveChildren(_giftSlots[i].gameObject);
            var btn = _giftSlots[i].gameObject.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            if (mergeModel.gifts.Count > i)
            {
                PieceDiscovery currentGift = mergeModel.gifts[i];
                if (currentGift == null) continue;
                var index = i;
                var prefab = mergeConfig.GetPiecePrefab(currentGift);
                Instantiate(prefab, _giftSlots[i]);
                btn.onClick.AddListener(()=> { OnTapGift(currentGift,index); });
                _giftButtons.Add(btn);
            }
        }
        giftBubble.SetActive(mergeModel.gifts.Count > 3);
        if (mergeModel.gifts.Count > 3)
        {
            giftBubbleCounter.text = $"+{mergeModel.gifts.Count - 3}";
        }
    }

    private void OnTapGift(PieceDiscovery piece,int index)
    {
        if (!GameManager.Instance.MergeManager.IsBoardActive) return;
        if (GameManager.Instance.MergeManager.boardController.SpawnPieceFromStorage(new PieceState(piece.pType, piece.Lvl),_giftSlots[index]))
        {
            mergeModel.gifts.RemoveAt(mergeModel.gifts.FindIndex(i => i.pType == piece.pType && i.Lvl == piece.Lvl));
            GameManager.Instance.ShowMergeStorage(false);
            mergeModel.SaveGifts();
        }
    }


    private void addBuyItem()
    {
        if (buyItem!=null)
        {
            buyItem.SetNormalState();
        }
        buyItem = Instantiate(storeItemPrefab, piecesContainer);
        buyItem.SetBuyState(buySlotPrice);
        buyItem.OnBuySlot = TryToBuySlot;
    }

    private void TryToBuySlot()
    {
        if (GameManager.Instance.TryToSpend(buySlotPrice, RewardType.Coins))
        {
            TrackingManager.TrackMergeStorageBuySlot(mergeModel.storageSlots + 1, buySlotPrice);
            mergeModel.storageSlots++;
            PlayerData.storageSlots++;
            addBuyItem();
        }
    }

    private void OnTapItem(PieceState piece, Transform fromPos)
    {
        if (!GameManager.Instance.MergeManager.IsBoardActive)
        {
            ShowToolTip(fromPos.position, "Enter in a board to drop items");
            return;
        }
        if (GameManager.Instance.MergeManager.boardController.SpawnPieceFromStorage(piece, fromPos))
        {
            mergeModel.storage.RemoveAt(mergeModel.storage.FindIndex(i => i.pType == piece.pieceType && i.Lvl == piece.pieceLevel));
            GameManager.Instance.ShowMergeStorage(false);
        }
        else
        {
            ShowToolTip(fromPos.position, "Board is full");
        }
    }

    private void OnDisable()
    {
        while(piecesContainer.childCount > 0)
        {
            DestroyImmediate(piecesContainer.GetChild(0).gameObject);
        }
    }


    public Transform Tooltip;
    public Transform TooltipAnchor;
    public TextMeshProUGUI tooltipText;
    public Vector3 tooltipOffset;
    public Vector3 tooltipAnchorOffset;
    protected bool _isShowingTooltip = false;
    public void ShowToolTip(Vector3 pos, string txt)
    {
        if (Tooltip == null) return;
        //Debug.Log("ShowToolTip");
        if (!_isShowingTooltip)
        {
            _isShowingTooltip = true;
            Tooltip.DOScale(1, 0.3f).SetDelay(0.1f).SetEase(Ease.OutBack).OnStart(() =>
            {
                Tooltip.gameObject.SetActive(true);
                TooltipAnchor.localPosition = tooltipAnchorOffset;
                Tooltip.position = pos + tooltipOffset;
            });
            tooltipText.text = txt;
        }
        else
        {
            HideTooltip();
        }
    }
    public void HideTooltip()
    {
        if (Tooltip != null && _isShowingTooltip)
        {
            _isShowingTooltip = false;
            Tooltip.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }

    protected virtual void Update()
    {
        if (Tooltip != null && Input.GetMouseButtonDown(0))
        {
            HideTooltip();
        }
    }
}
