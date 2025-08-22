using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MergeBoard
{
    [System.Serializable]
    public class MergeMissionConfig 
    {
        public PieceState piece;
        [HorizontalGroup("Split")]
        [BoxGroup("Split/Reward Piece")]
        public PieceState rewardPiece;
        [BoxGroup("Split/Reward Resources")]
        public List<RewardData> rewardInstant;
        public int requirePlayerLevel;

        public void GenerateMission(PieceState piece, int maxLevel, float rarity)
        {
            this.piece = piece;
            //2% chest
            var chestChances = Mathf.Max(2, 8 - GameManager.Instance.PlayerLevel);
            if(GameManager.Instance.PlayerLevel>2 && Random.Range(0,100) < chestChances)
            {
                rewardPiece = new PieceState(PieceType.CommonChest, 0);
            }
            else
            {
                rewardPiece = new PieceState(PieceType.XP, piece.pieceLevel > 4 ? 1 : 0);
            }
            rewardInstant = new List<RewardData>();
            //Add Coins
            rewardInstant.Add(new RewardData(RewardType.Coins, 10 * piece.pieceLevel * piece.pieceLevel));
            //Add Fame according to rarity?
            var famePoints = GetFamePointReward(rarity, piece.pieceLevel, maxLevel);
            if(famePoints > 0)
            {
                rewardInstant.Add(new RewardData(RewardType.FamePoints, famePoints));
            }
            requirePlayerLevel = GameManager.Instance.PlayerLevel;
        }

        public void GenerateSpeedBoardMission(PieceState piece)
        {
            this.piece = piece;
            rewardInstant = new List<RewardData>();
            //Add Points
            rewardInstant.Add(new RewardData(RewardType.SpeedBoardPoints, 10 * piece.pieceLevel * piece.pieceLevel));
        }

        private int GetFamePointReward(float rarity, int level, int maxLevel)
        {
            if(maxLevel == 0)
            {
                return 0;
            }
            int levelsLeft = maxLevel - level;
            if(rarity < 0.15f)
            {
                if(maxLevel <= 4)
                {
                    return 3 - levelsLeft;
                }
                return Mathf.RoundToInt((level - 2) * 2.5f) ;
            }
            else if (rarity < 0.3f)
            {
                if (maxLevel <= 4)
                {
                    return 2 - levelsLeft;
                }
                return Mathf.RoundToInt((level - 3) * 2);

            }
            else if (rarity < 0.55f)
            {
                if (maxLevel <= 4)
                {
                    return 1 - levelsLeft;
                }
                return Mathf.RoundToInt((level - 4) * 1.5f);

            }
            if (maxLevel <= 4)
            {
                return 0;
            }
            return Mathf.RoundToInt((level - 3) * 1.5f);

        }
    }
}