using System.Collections;
using UnityEngine;

namespace Assets.Scripts.MergeBoard
{
    [System.Serializable]
    public class PieceConfig
    {
        public PieceType pieceType;
        public int level;

        public PieceConfig(PieceType pieceType, int level)
        {
            this.pieceType = pieceType;
            this.level = level;
        }
    }

    
}