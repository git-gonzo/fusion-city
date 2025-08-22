using Assets.Scripts.MergeBoard;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class RewardsGranter
{
    public static List<RewardData> GetRewardsOfGift(PieceDiscovery piece)
    {
        List<RewardData> rewards = new List<RewardData>();
        if (piece.pType == PieceType.Scissors)
        {
            rewards.Add(new RewardData(RewardType.XP, 5));
            rewards.Add(new RewardData(RewardType.Coins, 250));
        }
        else if (piece.pType == PieceType.LevelUP)
        {
            rewards.Add(new RewardData(RewardType.XP, 100));
            rewards.Add(new RewardData(RewardType.Gems, 5));
            rewards.Add(new RewardData(new PieceDiscovery(PieceType.RouleteTicketSpecial, 3)));
        }
        else if (piece.pType == PieceType.BoosterAutoMerge)
        {
            rewards.Add(new RewardData(RewardType.XP, 10));
            rewards.Add(new RewardData(RewardType.Coins, 500));
        }
        else if (piece.pType == PieceType.BoosterEnergy)
        {
            rewards.Add(new RewardData(RewardType.XP, 15 * (piece.Lvl + 1)));
            rewards.Add(new RewardData(RewardType.Coins, 200 * (piece.Lvl + 1)));
        }
        else if (piece.pType == PieceType.BoosterGenerators)
        {
            rewards.Add(new RewardData(RewardType.XP, 20));
            rewards.Add(new RewardData(RewardType.Coins, 1000));
        }
        else if (piece.pType == PieceType.Energy)
        {
            rewards.Add(new RewardData(RewardType.XP, 6 * (piece.Lvl + 1) * (piece.Lvl + 1)));
            rewards.Add(new RewardData(RewardType.Coins, 40 * (piece.Lvl + 1) * (piece.Lvl + 1)));
        }
        else if (piece.pType == PieceType.RouleteTicketCommon)
        {
            rewards.Add(new RewardData(RewardType.XP, 7 * (piece.Lvl + 1) * (piece.Lvl + 1)));
            rewards.Add(new RewardData(RewardType.Coins, 40 * (piece.Lvl + 1) * (piece.Lvl + 1)));
        }
        else if (piece.pType == PieceType.RouleteTicketSpecial)
        {
            rewards.Add(new RewardData(RewardType.XP, 8 * (piece.Lvl + 1) * (piece.Lvl + 1)));
            rewards.Add(new RewardData(RewardType.Coins, 60 * (piece.Lvl + 1) * (piece.Lvl + 1)));
            rewards.Add(new RewardData(RewardType.Gems, (piece.Lvl + 1) * (piece.Lvl > 0 ? piece.Lvl : 1)));
        }
        else
        {
            rewards.Add(new RewardData(RewardType.XP, piece.Lvl * 2));
            if (piece.Lvl > 4)
            {
                rewards.Add(new RewardData(RewardType.FamePoints, 1 + (piece.Lvl - 5) * 2));
            }
            if (piece.Lvl > 5)
            {
                if (piece.Lvl > 7)
                {
                    rewards.Add(new RewardData(new PieceDiscovery(PieceType.RouleteTicketSpecial, Math.Min(2, piece.Lvl - 8))));
                }
                else
                {
                    rewards.Add(new RewardData(new PieceDiscovery(PieceType.RouleteTicketCommon, Math.Min(2, piece.Lvl - 6))));
                }
            }
            else
            {
                rewards.Add(new RewardData(RewardType.Coins, (int)(GetCoinsValue(piece))));
            }
        }
        return rewards;
    }
    public static int GetCoinsValue(PieceDiscovery piece)
    {
        if(!GameConfigMerge.instance) return 0;
        var genConfig = GameConfigMerge.instance.mergeConfig.GetGeneratorOfPiece(piece.pType);
        var f1 = Math.Pow(piece.Lvl, 2.5f);
        var f2 = Math.Sqrt(genConfig.coolDown);
        var f3 = genConfig.piecesChances.Count;
        var f4 = f2 * f3;
        var f5 = f1 * f4;
        var weight = (int)(f5 * .1f);
        return Mathf.Max(1, weight);
    }
}