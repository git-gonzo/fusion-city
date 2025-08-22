using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="DailyDealConfig")]
public class ShopDailyDealConfig : ScriptableObject
{
    public List<DailyDealItemSet> itemsSet;
}


[System.Serializable]
public class DailyDealItemSet
{
    public List<ItemShop> items;
}



[System.Serializable]
public class ItemShop
{
    public PieceType item;
    public int level;
    public RewardData price;
    public int stock;

    public ItemShop() { }
    public ItemShop(PieceDiscovery piece, RewardData price, int totalStock) 
    {
        item = piece.pType;
        level = piece.Lvl;
        this.price = price;
        stock = totalStock;
    }

    public PieceDiscovery pDiscovery => new PieceDiscovery(item, level);
}