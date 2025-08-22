using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Purchasing;

public class TrackingManager
{
    public enum Track
    {
        StartActivity,
        CancelActivity,
        SpeedUpActivity,
        ActivityComplete,
        StartTravelling,
        CancelTravelling,
        TravelComplete,
        SpeedUpTravelling,
        ShowVideoAd,
        BuyVehicle,
        StartJob,
        JobComplete,
        BuyBuilding,
        ShowBanner,
        ViewWelcomePack,
        BuyWelcomePack,
        PromoPurchase,
        GiftSent,
        GiftReceived,
        TrackError
    }

    public static void AddTracking(Track Key, string param, object target, bool addPlayerID = false)
    {
        GameManager.Log("Adding tracking Key " + Key + ", param " + param);
        /*AnalyticsService.Instance.CustomData(Key.ToString(), new Dictionary<string, object>
        {
            { param, target }
        });*/

        if (Key == Track.TravelComplete)
        {
            PlayerData.travelCount++;
        }
    }

    public static void TrackMergeClaim(bool isValid, RewardData reward = null)
    {
        /*AnalyticsService.Instance.CustomData("OnMergeClaim", new Dictionary<string, object>
                {
                    { "MergeClaimResult", isValid },
                    { "MergeClaimType", reward!=null?reward.rewardType.ToString():"unknown" },
                    { "MergeClaimValue", reward!=null?reward.amount:0 }
                });*/
    }
    public static void TrackError(string errorDescrip)
    {
        /*AnalyticsService.Instance.CustomData("TrackError", new Dictionary<string, object>
        {
            { "ErrorDescrip", errorDescrip },
            { "ErrorPlayerID", PlayerData.playerID }
        });*/
    }
    public static async void TrackTokenLost(string tokenState)
    {
        await Globals.IsSignedIn();
        /*AnalyticsService.Instance.CustomData("TrackTokenLost", new Dictionary<string, object>
        {
            { "TrackTokenState", tokenState },
            { "ErrorPlayerID", PlayerData.playerID }
        });*/
    }
    public static void TrackRoulette(string rouletteType)
    {
        /*AnalyticsService.Instance.CustomData("RouletteTicket", new Dictionary<string, object>
        {
            { "RouletteTicketType", rouletteType }
        });*/
    }
    public static void TrackGiftSent(PieceDiscovery piece, string toPlayer)
    {
        var e = new CustomEvent("GiftSent");
        e.AddParameter("GiftFromUser", PlayerData.playerID);
        e.AddParameter("GiftToUser", toPlayer);
        e.AddParameter("GiftSentPieceType", piece.pType.ToString());
        e.AddParameter("GiftSentPieceLevel", piece.Lvl);
        AnalyticsService.Instance.RecordEvent(e);
    }

    public static void TrackMergeSell(bool isValid, string typeLevel, RewardData reward = null)
    {
        var e = new CustomEvent("OnMergeSell");
        e.AddParameter("MergeSellResult", isValid);
        e.AddParameter("MergeSellTypeLevel", typeLevel);
        e.AddParameter("MergeSellRewardType", reward != null ? reward.rewardType.ToString() : "unknown");
        e.AddParameter("MergeSellRewardValue", reward != null ? reward.amount : 0);
        AnalyticsService.Instance.RecordEvent(e);
    }
    public static void TrackMergeStorageBuySlot(int totalSlots, int price)
    {
        var e = new CustomEvent("MergeStorageBuySlot");
        e.AddParameter("BuySlotTotalSlots", totalSlots);
        e.AddParameter("BuySlotPrice", price);
        AnalyticsService.Instance.RecordEvent(e);
    }
    public static void TrackMissionComplete(PieceType pType, int pLevel, PieceType rType, int rLevel)
    {
        PlayerData.MissionsCount++;
        /*AnalyticsService.Instance.CustomData("MergeMissionComplete", new Dictionary<string, object>
                {
                    { "MergeMissionItemType", pType.ToString() },
                    { "MergeMissionItemLevel", pLevel },
                    { "MergeMissionRewardType", rType.ToString() },
                    { "MergeMissionRewardLevel", rLevel },
                    { "MergeMissionRewardInstantType", 0 },
                    { "MergeMissionRewardInstantAmount", 0 }
                });*/
    }
    public static void TrackMapMissionComplete()
    {
        PlayerData.MissionsCount++;
    }

    public static void TrackAndSendChangeName(RewardData cost)
    {
        GameManager.Instance.server.ChangeName();
        var e = new CustomEvent("MergeChangeName");
        e.AddParameter("changeNameCost", cost == null ? 0 : cost.amount);
        AnalyticsService.Instance.RecordEvent(e);
    }
    public static void TrackAndSendChangeAvatar(int avatarIndex)
    {
        var e = new CustomEvent("MergeChangeAvatar");
        e.AddParameter("changeAvatarIndex", avatarIndex);
        AnalyticsService.Instance.RecordEvent(e);
    }
    public static void TrackVideoAdStart() { AnalyticsService.Instance.RecordEvent(new CustomEvent("VideoAdStart")); }
    public static void TrackVideoAdEnd() { AnalyticsService.Instance.RecordEvent(new CustomEvent("VideoAdRewarded")); }
    public static void TrackAndSendLevelUp()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("MergeLevelUp"));
        AnalyticsService.Instance.RecordEvent(new CustomEvent("VideoAdRewarded"));
    }

    public static void TrackRefillEnergy()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("MergeRefillEnergy"));
    }

    public static void TrackPurchase(string productID, string source)
    {
        var e = new CustomEvent("ProcessPurchase");
        e.AddParameter("ProcessPurchase_Product", productID);
        e.AddParameter("ProcessPurchase_playerID", PlayerData.playerID);
        e.AddParameter("ProcessPurchase_source", source);
        AnalyticsService.Instance.RecordEvent(e);
    }
    public static void TrackCarOffer(int carId, bool didPurchase)
    {
        var e = new CustomEvent("CarOffer");
        e.AddParameter("carId", carId);
        e.AddParameter("UnityPlayerID", PlayerData.playerID);
        e.AddParameter("carOfferPurchased", didPurchase);
        AnalyticsService.Instance.RecordEvent(e);
    }


    public class CustomEvent : Unity.Services.Analytics.Event
    {
        public CustomEvent(string name) : base(name)
        {

        }
        public void AddParameter(string key, string value) 
        { 
            base.SetParameter(key, value); 
        }
        public void AddParameter(string key, bool value) 
        {
            base.SetParameter(key, value); 
        }
        public void AddParameter(string key, int value) 
        {
            base.SetParameter(key, value); 
        }
    }
}
