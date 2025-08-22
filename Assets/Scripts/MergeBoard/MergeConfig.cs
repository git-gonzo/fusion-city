using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MergeBoard
{
    [CreateAssetMenu(fileName = "MergeConfig",menuName = "Merge/MergeConfig")]
    public class MergeConfig : ScriptableObject
    {
        public int maxEnergy;
        public int energyRefillTime;
        public GameObject LockedPiecePrefab;
        public GameObject Locked2PiecePrefab;
        public List<PieceDef> PiecesDef;
        public MergeMapMissionsConfigs mapMissionsConfig;

        public List<MergeMissionMapConfig> mapMissions => mapMissionsConfig.mapMissions;
        public PieceDef GetDefByPieceType(PieceType pieceType)
        {
            return PiecesDef.Find(x => x.pieceType == pieceType);
        }
        public int GetPieceTypeLevelsCount(PieceType pieceType)
        {
            return PiecesDef.Find(x => x.pieceType == pieceType).levels.Count-1;
        }

        public List<PieceDef> GetPiecesDefOfType(List<PieceType> listTypes)
        {
            List<PieceDef> pieces = new List<PieceDef>();
            {
                foreach(var d in PiecesDef)
                {
                    if (listTypes.Contains(d.pieceType)) pieces.Add(d);
                }
            }
            return pieces;
        }
        public PieceDef GetPieceDefOfType(PieceType pieceType)
        {

            foreach(var d in PiecesDef)
            {
                if (d.pieceType == pieceType) return d;
            }
            return null;
        }
        public PieceDef GetChain(PieceType pieceType)
        {
            foreach(var d in PiecesDef)
            {
                if (pieceType == d.pieceType) return d;
            }
            return null;
        }

        public GameObject GetPiecePrefab(PieceState piece)
        {
            var def = PiecesDef.Find(a => a.pieceType == piece.pieceType);
            return def.levels[Mathf.Min(piece.pieceLevel,def.levels.Count-1)];
        }
        public GameObject GetPiecePrefab(PieceDiscovery piece)
        {
            var def = PiecesDef.Find(a => a.pieceType == piece.pType);
            if (def == null)
            {
                Debug.Log("Piece " + piece.pType + " not found");
            }
            return def.levels[Mathf.Min(piece.Lvl,def.levels.Count-1)];
        }

        public bool ValidPieceType(int value)
        {
            return Enum.IsDefined(typeof(PieceType), value);
        }
        public GeneratorConfig GetGeneratorOfPiece(PieceType pType)
        {
            foreach (var d in PiecesDef)
            {
                foreach (var p in d.levels)
                {
                    var genConfig = p.GetComponent<MovingPiece>()?.genConfig;
                    if (genConfig != null)
                    {
                        if(genConfig.piecesChances.Find(p=>p.pieceType == pType)!=null){
                            return genConfig;
                        }
                    }
                }
            }
            return null;
        }

        public List<PieceType> GetSpawnablePieces()
        {
            var list = new List<PieceType>();

            foreach (var chain in PiecesDef)
            {
                if (chain.levels[0].TryGetComponent<MovingPiece>(out var movingPiece) 
                    && movingPiece.IsGenerator && !movingPiece.IsExpirable )
                {
                    foreach(var item in movingPiece.genConfig.piecesChances)
                    {
                        if(!list.Contains(item.pieceType)) list.Add(item.pieceType);
                    }
                }
            }
            return list;
        }
    }

    [System.Serializable]
    public class PieceDef
    {
        public PieceType pieceType;
        public List<GameObject> levels;
    }
    

    public enum PieceType
    {
        None = 0,
        CommonChest = 1,
        BoosterChest = 48,
        Type2 = 2,
        Type3 = 3,
        /// Boosters
        BoosterGenerators = 44,
        BoosterEnergy = 45,
        BoosterAutoMerge = 46,
        Scissors = 52,
        LevelUP = 54,
        SuperMerge = 72,
        UpgradeGenerator = 73,

        Generator1 = 4,
        GeneratorBurgers = 5,
        GeneratorBakery = 25,
        GeneratorDrinks = 16,
        GeneratorMusic = 15,
        GeneratorMusic2 = 53,
        GeneratorAcademy = 17,
        GeneratorPolice = 20,
        GeneratorPoliceWeapons = 42,
        GeneratorSchool = 23,
        GeneratorSchoolNumbers = 30,
        GeneratorBeach = 35,
        GeneratorFruit = 37,
        GeneratorCards = 55,
        GeneratorShoes = 65,
        GeneratorSport = 67,
        GeneratorPerfum = 68,
        Burger = 6,
        XP = 7,
        XPBig = 43,
        Coins = 8,
        Cash = 71,
        Gems = 9,
        Energy = 24,
        Drink1 = 10,
        Drink2 = 11,
        Drink3 = 12,
        Patatas = 13,
        Music = 14,
        MusicWind = 47,
        Drums = 18,
        Lights = 51,
        Police = 19,
        PoliceHat = 40,
        PoliceWeapon = 41,
        School = 21,
        SchoolNumber = 31,
        Book = 49,
        Pen = 50,
        SchoolChar = 32,
        Grade = 22,
        Bakery1 = 26,
        Bakery2 = 27,
        Bakery3 = 28,
        Bakery4 = 29,
        Beach = 33,
        Surf = 34,
        Sunglass = 36,
        Banana = 38,
        Fruit = 39,
        CardDiamond = 56,
        CardHeart = 57,
        CardSpade = 58,
        CardClub = 59,
        CasinoChip = 60,
        ShoeMan = 61,
        ShoeWoman = 62,
        SportBall = 66,
        Perfum = 63,
        MakeUp = 64,

        //SPECIAL CHAINS
        RouleteTicketCommon = 69,
        RouleteTicketSpecial = 70
        //Next 73
    }
}