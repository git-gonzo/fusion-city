using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MergeBoard
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Merge/BoardConfig")]
    public class BoardConfig : ScriptableObject
    {
        public string boardID;
        public int sizeX;
        public int sizeY;
        public bool isSpeedBoard;
        public List<PieceState> pieces;
        public List<MergeMissionConfig> missions;
        public int startingMissions;
        public int maxVisibleMissions;
        public int missionsCooldown;
        public Color BGColor = Color.white;
        public Color boardBGColor = Color.white;
        public Color boardTileColorNormal;
        public Color boardTileColorDark;
        public Color boardTileColorSelected;
    }
}
