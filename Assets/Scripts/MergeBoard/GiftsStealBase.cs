using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using Unity.Services.CloudCode;

public class GiftsStealBase : MonoBehaviour
{
    public GiftStealItemController itemPrefab;
    public GameObject giftsSelector;
    public GameObject stealSelector;
    public Transform piecesToGiftContainer;
    public RewardsContainer gitRewardsContainer;
    public GameObject stealContainer;
    public Transform piecesContainer1;
    public Transform piecesContainer2;
    public Button btnSendGift;
    public Button btnSteal;
    public Button btnClose;
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtDescrip;
    public TextMeshProUGUI txtPlayerName;
    public TextMeshProUGUI txtPlayerName2;
    public TextMeshProUGUI txtTitleUp;
    public TextMeshProUGUI txtTitleDown;
    public TextMeshProUGUI txtSendingInfo;
    public TextMeshProUGUI txtStorageEmpty;
    public GameObject txtNoWeapons;
    public TextMeshProUGUI txtNoNeedItems;
    public int giftCooldownMinutes = 10;
    public int stealCooldownMinutes = 60;

    private MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
    private MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;
    private List<GiftStealItemController> items;
    private List<GiftStealItemController> _weapons;
    private PieceState _weaponSelected;
    private PieceState _selected;
    private LeaderboardPlayer _playerData;
    private bool _isStealing = false;
    private bool _isProcessing = false;

    DateTime NextGift { get => UIUtils.GetTimeStampByKey("nextGiftTime"); }
    DateTime NextSteal { get => UIUtils.GetTimeStampByKey("nextStealTime"); }
    bool CanSendGift => NextGift <= DateTime.Now;
    bool CanSteal => NextSteal <= DateTime.Now;


    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (_isProcessing) return;
            if (_isStealing)
            {
                btnSteal.gameObject.SetActive(CanSteal);
                txtSendingInfo.gameObject.SetActive(!CanSteal);
                if (!CanSteal)
                {
                    txtSendingInfo.text = UIUtils.FormatTime((NextSteal - DateTime.Now).TotalSeconds);
                }
            }
            else
            {
                btnSendGift.gameObject.SetActive(CanSendGift);
                txtSendingInfo.gameObject.SetActive(!CanSendGift);
                if (!CanSendGift)
                {
                    txtSendingInfo.text = UIUtils.FormatTime((NextGift - DateTime.Now).TotalSeconds);
                }
            }
        }
    }
    public void LoadItemsFromStorage(MergeConfig mergeConfig, LeaderboardPlayer playerData)
    {
        InitCommon(false, playerData);
        btnSendGift.onClick.AddListener(SendGift);
        btnSendGift.gameObject.SetActive(CanSendGift);
        GameManager.SetLocString("UI", "MakeGift", txtTitle);
        GameManager.SetLocString("UI", "MakeGiftDescrip", txtDescrip);
        GameManager.SetLocString("UI", "GiftStorageEmpty", txtStorageEmpty);
        GameManager.SetLocString("UI", "islookingfor", txtTitleUp);
        GameManager.SetLocString("UI", "AvailableItems", txtTitleDown);
        GenerateItemsToGive(out var availableGifts, out var itemsInMissions, playerData);
        if (piecesToGiftContainer.childCount > 0) GameManager.RemoveChildren(piecesToGiftContainer.gameObject);
        Fill(availableGifts, piecesToGiftContainer, true);
        txtStorageEmpty.gameObject.SetActive(availableGifts.Count == 0);
        stealContainer.SetActive(false);
    }

    public void LoadItemsToSteal(MergeConfig mergeConfig, LeaderboardPlayer playerData)
    {
        InitCommon(true, playerData);
        
        btnSteal.onClick.AddListener(Steal);
        btnSteal.gameObject.SetActive(CanSteal);
        
        GameManager.SetLocString("UI", "Steal", txtTitle);
        GameManager.SetLocString("UI", "StealDescrip", txtDescrip);
        GameManager.SetLocString("UI", "ChooseItemToSteal", txtTitleUp);
        GameManager.SetLocString("UI", "WeaponsAvailable", txtTitleDown);
        GenerateItemsToSteal(out var availableWeapons, out var itemsInOtherStorage, playerData);
        //txtTitleUp.gameObject.SetActive(itemsInOtherStorage.Count > 0);
        txtNoNeedItems.gameObject.SetActive(itemsInOtherStorage.Count == 0);
        for (var i = 0; i < itemsInOtherStorage.Count && i < 10; i++)
        {
            if (itemsInOtherStorage[i].pType == PieceType.XP
                || itemsInOtherStorage[i].pType == PieceType.Coins
                || itemsInOtherStorage[i].pType == PieceType.Gems) continue;
            if (items.Find(it => it.pieceState.pieceType == itemsInOtherStorage[i].pType && it.pieceState.pieceLevel == itemsInOtherStorage[i].Lvl) != null)
                continue;
            var itemGift = Instantiate(itemPrefab, piecesContainer1);
            itemGift.SetNormalState();
            var available = availableWeapons.Any(w => w.Lvl >= itemsInOtherStorage[i].Lvl);
            var prefab = mergeConfig.GetPiecePrefab(itemsInOtherStorage[i]);
            var levelsCount = mergeConfig.GetPieceTypeLevelsCount(itemsInOtherStorage[i].pType);
            var isMaxed = itemsInOtherStorage[i].Lvl == levelsCount && levelsCount > 0;
            itemGift.AddItem(prefab, new PieceState(itemsInOtherStorage[i].pType, itemsInOtherStorage[i].Lvl), SelectItemToSteal, isMaxed, available);
            if (available) items.Add(itemGift);
        }
        _weapons = new List<GiftStealItemController>();
        txtNoWeapons.SetActive(availableWeapons.Count == 0);
        if (availableWeapons.Count > 0)
        {
            for (var i = 0; i < availableWeapons.Count; i++)
            {
                var itemGift = Instantiate(itemPrefab, piecesContainer2);
                var prefab = mergeConfig.GetPiecePrefab(availableWeapons[i]);
                var levelsCount = mergeConfig.GetPieceTypeLevelsCount(availableWeapons[i].pType);
                var isMaxed = availableWeapons[i].Lvl == levelsCount && levelsCount > 0;
                itemGift.AddItem(prefab, new PieceState(availableWeapons[i].pType, availableWeapons[i].Lvl), null, isMaxed, false);
                itemGift.SetStateWeapon();
                _weapons.Add(itemGift);
            }
        }
        stealContainer.SetActive(true);
    }

    private void InitCommon(bool stealing, LeaderboardPlayer playerData)
    {
        gitRewardsContainer.transform.parent.gameObject.SetActive(false);
        _playerData = playerData;
        _isStealing = stealing;
        giftsSelector.SetActive(!_isStealing);
        stealSelector.SetActive(_isStealing);
        btnSendGift.interactable = false;
        btnSteal.interactable = false;
        btnSteal.gameObject.SetActive(false);
        btnSendGift.gameObject.SetActive(false);
        txtSendingInfo.gameObject.SetActive(false);
        txtNoNeedItems.gameObject.SetActive(false);
        if (piecesContainer1.childCount > 0) GameManager.RemoveChildren(piecesContainer1.gameObject);
        if (piecesContainer2.childCount > 0) GameManager.RemoveChildren(piecesContainer2.gameObject);
        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(OnClose);
        btnSteal.onClick.RemoveAllListeners();
        btnSendGift.onClick.RemoveAllListeners();
        items = new List<GiftStealItemController>();
        txtPlayerName.text = playerData.playername;
        txtPlayerName2.text = playerData.playername;
    }

    private void Fill(List<PieceDiscovery> pieces, Transform container, bool available)
    {
        for (var i = 0; i < pieces.Count; i++)
        {
            var itemGift = Instantiate(itemPrefab, container);
            itemGift.SetNormalState();

            var prefab = mergeConfig.GetPiecePrefab(pieces[i]);
            var isReady = mergeModel.IsItemInMapMission(new PieceState(pieces[i].pType, pieces[i].Lvl));
            var levelsCount = mergeConfig.GetPieceTypeLevelsCount(pieces[i].pType);
            var isMaxed = pieces[i].Lvl == levelsCount && levelsCount > 0;
            itemGift.AddItem(prefab, new PieceState(pieces[i].pType, pieces[i].Lvl), SelectItemToGift, isMaxed, available);
            if (available) items.Add(itemGift);
            else if (mergeModel.isItemInStorage(itemGift.pieceState))
            {
                itemGift.SetNormalState();
            }
        }
    }

    private void OnClose()
    {
        GameManager.Instance.ShowSendGifts(false, null);
    }

    public void SelectItemToGift(PieceState pieceState, GiftStealItemController item)
    {
        SelectItemBase(pieceState, item);
        btnSendGift.interactable = true;
        //ShowRewards
        var rewards = RewardsGranter.GetRewardsOfGift(pieceState.pieceDiscovery());
        gitRewardsContainer.transform.parent.gameObject.SetActive(rewards.Count > 0);
        gitRewardsContainer.FillRewards(rewards, GameManager.Instance.topBar, true);
    }
    
    public void SelectItemToSteal(PieceState pieceState, GiftStealItemController item)
    {
        SelectItemBase(pieceState,item);
        btnSteal.interactable = true;
        foreach (var i in _weapons) i.Select(false);
        _weapons.Sort((a, b) => a.pieceState.pieceLevel.CompareTo(b.pieceState.pieceLevel));
        var weapon = _weapons.First(w => w.pieceState.pieceLevel >= pieceState.pieceLevel);
        weapon.Select(true);
        _weaponSelected = weapon.pieceState;
    }

    private void SelectItemBase(PieceState pieceState, GiftStealItemController item)
    {
        foreach (var i in items)
        {
            i.Select(false);
        }
        _selected = pieceState;
        item.Select(true);
    }

    public void SendGift()
    {
        _isProcessing = true;
        btnSendGift.gameObject.SetActive(false);
        txtSendingInfo.gameObject.SetActive(true);
        GameManager.SetLocString("UI", "SendingGiftWait", txtSendingInfo);
        SendGift(_playerData.playerId, (int)_selected.pieceType, _selected.pieceLevel);
    }
    private async void SendGift(string playerId, int itemType, int itemLevel)
    {
        var args = new Dictionary<string, object>();
        args.Add("senderName", PlayerData.playerName);
        args.Add("otherPlayerId", playerId);
        args.Add("itemtype", itemType);
        args.Add("itemLevel", itemLevel);
        var result = await CloudCodeService.Instance.CallEndpointAsync<bool>("SendGift", args);
        SendGiftResult(result);
    }
    public void Steal()
    {
        _isProcessing = true;
        btnSteal.gameObject.SetActive(false);
        txtSendingInfo.gameObject.SetActive(true);
        GameManager.SetLocString("UI", "SendStelaingWait", txtSendingInfo);
        SendSteal(_playerData.playerId, (int)_selected.pieceType, _selected.pieceLevel);
    }

    private async void SendSteal(string playerId, int itemType, int itemLevel)
    {
        var args = new Dictionary<string, object>();
        args.Add("robbedBy", PlayerData.playerName);
        args.Add("otherPlayerId", playerId);
        args.Add("itemtype", itemType);
        args.Add("itemLevel", itemLevel);
        var result = await CloudCodeService.Instance.CallEndpointAsync<bool>("StealItem", args);
        SendStealResult(result);
    }

    public void GenerateItemsToGive(out List<PieceDiscovery> availableGifts, out List<PieceDiscovery> itemsInMissions, LeaderboardPlayer playerData)
    {
        availableGifts = new List<PieceDiscovery>();
        itemsInMissions = new List<PieceDiscovery>();

        /*if (playerData.activeMissions != null)
        {
            foreach (var m in playerData.activeMissions)
            {
                foreach (var item in mergeConfig.mapMissions[m].piecesRequest)
                {
                    if (!itemsInMissions.Contains(item))
                    {
                        itemsInMissions.Add(item);

                        if (mergeModel.isItemInStorage(item) && availableGifts.Count < 10)
                        {
                            availableGifts.Add(item);
                        }
                    }
                }
            }
        }
        //Add Boosters & Energy & coins & cash
        foreach (var item in mergeModel.storage)
        {
            if (availableGifts.Contains(item)) continue;

            if ((item.pType == PieceType.BoosterAutoMerge
                || item.pType == PieceType.BoosterEnergy
                || item.pType == PieceType.BoosterGenerators
                || item.pType == PieceType.LevelUP
                || item.pType == PieceType.Scissors
                || (item.pType == PieceType.Cash && item.Lvl > 2)
                || (item.pType == PieceType.Coins && item.Lvl > 3)
                || item.pType == PieceType.Energy)
                && availableGifts.Count < 10)
            {
                    availableGifts.Add(item);
            }
        }*/

        foreach (var item in mergeModel.storage)
        {
            if (!availableGifts.Any(x=> x.pType == item.pType && x.Lvl == item.Lvl) && availableGifts.Count < 15)
            {
                if ((item.pType == PieceType.Cash && item.Lvl <= 2)
                || (item.pType == PieceType.Coins && item.Lvl <= 3)) continue;
                else if ((item.pType == PieceType.BoosterAutoMerge
                || item.pType == PieceType.BoosterEnergy
                || item.pType == PieceType.BoosterGenerators
                || item.pType == PieceType.LevelUP
                || item.pType == PieceType.Scissors
                || item.pType == PieceType.RouleteTicketCommon
                || item.pType == PieceType.RouleteTicketSpecial
                || item.pType == PieceType.Energy))
                {
                   // GameManager.Log("a");
                }
                else if (item.Lvl <= 2) continue;
                availableGifts.Add(item);
            }
        }
    }

    public void GenerateItemsToSteal(out List<PieceDiscovery> availableWeapons, out List<PieceDiscovery> itemsInOtherStorage, LeaderboardPlayer playerData)
    {
        availableWeapons = new List<PieceDiscovery>();
        //Add Weapons
        foreach (var item in mergeModel.storage)
        {
            if (item.pType == PieceType.PoliceWeapon && item.Lvl > 3 && availableWeapons.Count < 10 && !availableWeapons.Contains(item))
            {
                availableWeapons.Add(item);
            }
        }

        itemsInOtherStorage = new List<PieceDiscovery>();
        if (playerData.storage != null)
        {
            foreach (var item in playerData.storage)
            {
                if (!itemsInOtherStorage.Contains(item) && itemsInOtherStorage.Count < 10)
                {
                    itemsInOtherStorage.Add(item);
                }
            }
        }
    }

    public void SendGiftResult(bool result)
    {
        UIUtils.SaveTimeStamp("nextGiftTime", DateTime.Now.AddMinutes(giftCooldownMinutes));
        _isProcessing = false;
        PlayerData.giftsSent++;
        GameManager.Instance.SendGiftSuccess(result,_selected.pieceDiscovery(),_playerData.playername);
    } 
    
    public void SendStealResult(bool result)
    {
        _isProcessing = false;
        if (result)
        {
            PlayerData.itemsStolen++;
            UIUtils.SaveTimeStamp("nextStealTime", DateTime.Now.AddMinutes(stealCooldownMinutes));
            //remove weapon from storage
            mergeModel.storage.Remove(mergeModel.storage.Find(i => i.pType == _weaponSelected.pieceType && i.Lvl == _weaponSelected.pieceLevel));
            //Give item
            //mergeModel.AddGift(_selected);
            //mergeModel.SaveGifts();
            //mergeModel.SaveStorageAndMissions();
            GameManager.Instance.dailyTaskManager.OnSteal();
            //Item is given in Popup
            GameManager.Instance.PopupsManager.ShowPopupStealSent(_selected.pieceDiscovery(),_weaponSelected.pieceDiscovery());
            OnClose();
        }
        else
        {
            GameManager.Instance.PopupsManager.ShowPopupYesNo("Failed to Steal", "Failed to Steal item", PopupManager.PopupType.ok);
        }
    }
}
