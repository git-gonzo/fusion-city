using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Linq;

namespace Assets.Scripts.MergeBoard
{
    [CreateAssetMenu(fileName = "GeneratorConfig", menuName = "Merge/GeneratorConfig")]
    public class GeneratorConfig : ScriptableObject
    {
        //[OnValueChanged("GetSumChances"), OnStateUpdate("GetSumChances")]
        public List<GeneratorChance> piecesChances;
        public List<PieceDiscovery> guaranteedPieces;
        public int capacity;
        public int coolDown;
        [ShowInInspector]
        float TotalWeight
        {
            get
            {
                var TotalWeight = 0f;
                for (var i = 0; i < piecesChances.Count; i++)
                {
                    TotalWeight += piecesChances[i].weight;
                }
                return TotalWeight;
            }
        }
        MergeBoardManager MergeManager => GameManager.Instance.MergeManager;

        [JsonConstructor]
        public GeneratorConfig(List<GeneratorChance> piecesChances, List<PieceDiscovery> guaranteedPieces, int capacity, int coolDown)
        {
            this.piecesChances = piecesChances;
            this.guaranteedPieces = guaranteedPieces;
            this.capacity = capacity;
            this.coolDown = coolDown;
        }

        internal PieceDiscovery GetObjectToSpawn(int generatorLevel = 0)
        {
            float totalWeight = 0;
           
            for(var i = 0; i < piecesChances.Count; i++)
            {
                totalWeight += piecesChances[i].weight; 
            }
            var targetWeight = UnityEngine.Random.Range(0, totalWeight);

            float cumWeight = 0;
            for (var i = 0; i < piecesChances.Count; i++)
            {
                cumWeight += piecesChances[i].weight;
                if (targetWeight < cumWeight)
                {
                    if (piecesChances[i].pieceDiscovery.pType == PieceType.Cash && GameManager.Instance.PlayerLevel < 3) continue;
                    var pieceToSpawn = piecesChances[i].pieceDiscovery;
                    pieceToSpawn.Lvl = UnityEngine.Random.Range(0,generatorLevel+1);
                    return pieceToSpawn;
                }
            }
            Debug.Log("No item found by weight in Generator");
            return piecesChances[0].pieceDiscovery;
        }
        internal PieceDiscovery GetPreferredObjectToSpawn(int generatorLevel = 0)
        {
            var tempList = piecesChances.Where(p => MergeManager.IsChainRequestedInMission(p.pieceType) || MergeManager.IsPieceRequestedInOrders(p)).ToList();
            if(tempList.Count == 0 || tempList.Count == piecesChances.Count) 
            {
                GameManager.Log($"Cannot get preferred pieces {(tempList.Count == 0?"No candidates":"All are candidates")}");
                return GetObjectToSpawn(generatorLevel);
            }
            var index = UnityEngine.Random.Range(0, tempList.Count);
            var result = tempList[index].pieceDiscovery;
            result.Lvl = UnityEngine.Random.Range(0, generatorLevel + 1);
            return result;
        }
        internal List<GeneratorChance> GetPreferredObjectsToSpawn(int generatorLevel = 0)
        {
            return piecesChances.Where(p => MergeManager.IsChainRequestedInMission(p.pieceType) || MergeManager.IsPieceRequestedInOrders(p)).ToList();
            /*var tempList = piecesChances.Where(p => MergeManager.IsChainRequestedInMission(p.pieceType) || MergeManager.IsPieceRequestedInOrders(p)).ToList();
            if(tempList.Count == 0 || tempList.Count == piecesChances.Count) 
            {
                GameManager.Log($"Cannot get preferred pieces {(tempList.Count == 0?"No candidates":"All are candidates")}");
                return new List<GeneratorChance>();
            }
            var index = UnityEngine.Random.Range(0, tempList.Count);
            var result = tempList[index].pieceDiscovery;
            result.Lvl = UnityEngine.Random.Range(0, generatorLevel + 1);
            return result;*/
        }
        
        internal PieceDiscovery GetSpecificObjectsToSpawn(List<PieceDiscovery> pieces)
        {
            var index = UnityEngine.Random.Range(0, pieces.Count);
            var result = pieces[index];
            return result;
        }
        internal PieceDiscovery GetObjectToSpawnTutorial()
        {
            return piecesChances[0].pieceDiscovery;
        }


        internal float GetItemRarity(int index)
        {
            var rating = piecesChances[index].weight / TotalWeight;
            if (coolDown < 100) rating += 0.2f;
            else if (coolDown < 240) rating += 0.02f;
            else if (coolDown < 400) rating += 0.02f;
            else if (coolDown < 1800) rating -= 0.1f;
            else if (coolDown < 3600) rating -= 0.2f;
            return rating;
        }
        internal float GetChainRarity(int index)
        {
            var rating = (piecesChances[index].weight / TotalWeight) * 100;
            if (coolDown < 100) rating += 200f;
            else if (coolDown < 240) rating += 100f;
            else if (coolDown < 400) rating += 50f;
            else if (coolDown < 1000) rating += 25f;
            else if (coolDown < 1800) rating += 15f;
            else if (coolDown < 3600) rating += 1f;
            else rating -= 20f;

            if (capacity < 10) rating -= 20;
            else if (capacity < 20) rating -= 10;
            else if (capacity < 30) rating -= 1;
            else if (capacity < 40) rating += 5;
            

            if (piecesChances.Count > 1) rating -= 1;
            else if (piecesChances.Count > 2) rating *= 0.9f;
            else if (piecesChances.Count > 3) rating *= 0.7f;
            else if (piecesChances.Count > 4) rating *= 0.4f;
            else if (capacity < 40) rating += 5;

            return rating;
        }

        public void UpdateDataFromConfig(GeneratorConfig newData)
        {
            piecesChances = newData.piecesChances;
            capacity = newData.capacity;
            coolDown = newData.coolDown;
        }

        public List<PieceDiscovery> PossiblePieces(int generatorLevel = 0) 
        {
            List<PieceDiscovery> pieces = new List<PieceDiscovery>();
            foreach(var p in piecesChances)
            {
                if(generatorLevel > 0)
                {
                    for(int i = 1; i < generatorLevel + 1; i++)
                    {
                        pieces.Add(new PieceDiscovery(p.pieceDiscovery.pType,i));
                    }
                }
                pieces.Add(p.pieceDiscovery);
            }
            pieces.Sort((a, b) => b.Lvl.CompareTo(a.Lvl));
            return pieces;
        }
    }

    [System.Serializable]
    public class GeneratorChance
    {
        [HorizontalGroup()]
        public PieceType pieceType;
        [HorizontalGroup(),LabelWidth(150)]
        //[HorizontalGroup(),LabelWidth(150),OnValueChanged("GetSumChances")]
        public int pieceLevel = 0;
        public float weight;

        public PieceDiscovery pieceDiscovery => new PieceDiscovery(pieceType, pieceLevel);
    }
}