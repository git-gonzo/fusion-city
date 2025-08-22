using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using UnityEngine.Events;
using Newtonsoft.Json;
using System;
using Sirenix.OdinInspector;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.MergeBoard
{
    public class MergeBoardModel
    {
        public int energy;
        public int storageSlots = 3;
        public BoosterState energyBooster;
        public List<BoardState> boards = new List<BoardState>();
        public DateTime nextEnergy;
        public List<PieceDiscovery> pieceDiscoveries = new List<PieceDiscovery>();
        public List<PieceDiscovery> storage = new List<PieceDiscovery>();
        public List<PieceDiscovery> gifts = new List<PieceDiscovery>();
        public List<int> mapMissions = new List<int>();
        public List<MapMissionCloud> mapMissionsNew = new List<MapMissionCloud>();
        public List<MapMissionCloudCharacter> mapMissionsCharacters = new List<MapMissionCloudCharacter>();
        public List<int> ownedCharacters = new List<int>();
        public AssistantsModel assistants = new AssistantsModel();
        public LimitedMissionMapConfig limitedMission;
        public DailyDealItemSet currentDailyDeal;
        public CharacterStoryModel characterStoryModel = new CharacterStoryModel();

        private bool saveDiscovery = false;

        public int MissionsCount => mapMissionsNew.Count + mapMissionsCharacters.Count + (limitedMission != null ? 1 : 0);

        private MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;

        public BoardState GetBoard(BoardConfig boardConfig)
        {
            if(boards.Exists(b=>b.boardID == boardConfig.boardID))
            {
                Debug.Log("Getting Board Model " + boardConfig.boardID);
                var targetBoard = boards.Find(b => b.boardID == boardConfig.boardID);
                //Check if board is corrupted
                targetBoard.CheckNullPieces();
                targetBoard.sizeX = boardConfig.sizeX;
                targetBoard.sizeY = boardConfig.sizeY;
                return targetBoard;
            }
            Debug.Log("Getting Random new Board Model ");
            var bState = new BoardState(boardConfig);
            boards.Add(bState);
            return bState;
        }

        public bool BoardInModel(string boardId)
        {
            return boards.Exists(b => b.boardID == boardId);
        }

        public void AddDiscovery(PieceState piecestate)
        {
            if (piecestate.hidden) return;
            if (pieceDiscoveries.Exists(a=> a.pType == piecestate.pieceType && a.Lvl == piecestate.pieceLevel)) return;
            PieceDiscovery p = new PieceDiscovery(piecestate);
            pieceDiscoveries.Add(p);
            PlayerData.discoveries++;
            saveDiscovery = true;
            //Debug.Log("Adding " + piecestate.pieceType + " level " + piecestate.pieceLevel + " To discovery. Total discovered " + pieceDiscoveries.Count);
        }
        public void AddGift(PieceDiscovery piece, int amount = 1)
        {
            //Clear Null gifts
            for(var i = 0; i<gifts.Count; i++)
            {
                if (gifts[i] == null)
                {
                    gifts.RemoveAt(i);
                    i--;
                }
            }
            for (var i = 0; i < amount; i++)
            {
                gifts.Add(piece);
            }
            SaveGifts();
        }
        public void AddGift(PieceType piece, int level)
        {
            AddGift(new PieceDiscovery(piece,level));
        }
        public void AddGift(PieceState piece)
        {
            AddGift(new PieceDiscovery(piece.pieceType,piece.pieceLevel));
            GameManager.Instance.UpdateStorageButtons();
        }

        public void AddMission(MergeMissionMapConfig mission)
        {
            var newMission = new MapMissionCloud();
            newMission.location = mission.location;
            newMission.piecesRequest = mission.piecesRequest;
            newMission.rewards = mission.rewardInstant;
            AddMission(newMission);
        }
        public void AddMission(MapMissionCloud mission)
        {
            mapMissionsNew.Add(mission);
            SaveStorageAndMissions(true);
        }
        public void AddMission(MapMissionCloudCharacter mission)
        {
            mapMissionsCharacters.Add(mission);
            SaveStorageAndMissions(false,true);
        }

        public void AddAssistant(AssistantType assistantType, int days)
        {
            assistants.AddAssistantDays(assistantType, days);
        }

        public bool IsDiscovered(PieceState piecestate)
        {
            return pieceDiscoveries.Exists(a => a.pType == piecestate.pieceType && a.Lvl == piecestate.pieceLevel);
        }
        public bool IsDiscovered(PieceType piece, int level)
        {
            return pieceDiscoveries.Exists(a => a.pType == piece && a.Lvl == level);
        }
        public int MaxLevelDiscovered(PieceType piece)
        {
            var maxLevel = 0;
            foreach(var p in pieceDiscoveries)
            {
                if(p.pType == piece)
                {
                    if (p.Lvl > maxLevel) maxLevel = p.Lvl;
                }
            }
            return maxLevel;
        }
        public bool isItemInStorage(PieceState pieceState, bool includeGifts = false)
        {
            return isItemInStorage(new PieceDiscovery(pieceState), includeGifts);
        }
        public bool isItemInStorage(PieceDiscovery pieceState, bool includeGifts = false)
        {
            return CountItemInStorage(pieceState,includeGifts) > 0;
        }
        public int CountItemInStorage(PieceType pieceType, int level = 0, bool includeGifts = false)
        {
            var piece = new PieceDiscovery(PieceType.UpgradeGenerator, 0);
            return CountItemInStorage(piece, includeGifts);
        }
        public int CountItemInStorage(PieceDiscovery pieceState, bool includeGifts = false)
        {
            int count = 0;
            foreach (var s in storage)
            {
                if (s.pType == pieceState.pType && s.Lvl == pieceState.Lvl)
                {
                    count++;
                }
            }
            if (includeGifts)
            {
                foreach (var s in gifts)
                {
                    if (s.pType == pieceState.pType && s.Lvl == pieceState.Lvl)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public bool HasMissionsInLocation(BuildingType location)
        {
            foreach(var m in mapMissionsNew)
            {
                if(m.location == location) return true;
            }
            if (limitedMission != null && limitedMission.location == location) return true;
            return false;
        }
        public int MissionsCountInLocation(BuildingType location)
        {
            var count = 0;
            foreach(var m in mapMissionsNew)
            {
                if(m.location == location) count++;
            }
            foreach(var m in mapMissionsCharacters)
            {
                if(m.location == location) count++;
            }
            if (limitedMission != null && limitedMission.location == location) count++;
            return count;
        }

        public bool IsMapMissionReady(int missionID)
        {
            var m = mergeConfig.mapMissions[missionID];
            foreach(var piece in m.piecesRequest)
            {
                if (!isItemInStorage(piece)) return false;
            }
            return true;
        }
        public bool IsMapMissionReady(MapMissionCloud mission)
        {
            foreach(var piece in mission.piecesRequest)
            {
                if (!isItemInStorage(piece)) return false;
            }
            return true;
        }

        public bool IsLimitedMissionReady()
        {
            if (limitedMission != null)
            {
                foreach (var piece in limitedMission.piecesRequest)
                {
                    if (!isItemInStorage(piece)) return false;
                }
                return true;
            }
            return false;
        }
        
        public bool IsCharacterMissionReady()
        {
            if (mapMissionsCharacters != null && mapMissionsCharacters.Count > 0)
            {
                foreach (var characterMission in mapMissionsCharacters)
                {
                    foreach (var piece in characterMission.piecesRequest)
                    {
                        if (!isItemInStorage(piece)) return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool HasAnyMissionReady()
        {
            foreach(var mission in mapMissionsNew)
            {
                if (IsMapMissionReady(mission)) return true;

            }
            if (IsLimitedMissionReady()) return true;
            return false;
        }

        public bool IsItemInMapMission(PieceState piece)
        {
            foreach(var mission in mapMissionsNew)
            {
                foreach (var p in mission.piecesRequest)
                {
                    if (p.Lvl == piece.pieceLevel && p.pType == piece.pieceType) return true;
                }
                //if (isItemInStorage(piece)) return true;
            }
            if(limitedMission!= null && limitedMission.piecesRequest.Any(item =>item.Lvl == piece.pieceLevel && item.pType == piece.pieceType)) return true;
            return false;
        }

        public PieceDiscovery GetFirstTopMissionItem() //Not in storage
        {
            PieceDiscovery p = null;
            var maxTier = 0;
            foreach (var mission in mapMissionsNew)
            {
                foreach(var missionItem in mission.piecesRequest)
                {
                    if(missionItem.Lvl > maxTier && !isItemInStorage(missionItem)) //Not in storage
                    {
                        p = missionItem;
                        maxTier = missionItem.Lvl;
                    }
                }
            }
            return p;
        }

        public void CompleteMapMission(MapMissionCloud missionConfig)
        {
            foreach (var piece in missionConfig.piecesRequest)
            {
                RemoveFromStorage(piece);
            }
            mapMissionsNew.Remove(missionConfig);
            SaveStorageAndMissions(true);
        }
        public void CompleteCharacterMission(MapMissionCloudCharacter missionConfig)
        {
            foreach (var piece in missionConfig.piecesRequest)
            {
                RemoveFromStorage(piece);
            }
            mapMissionsCharacters.Remove(missionConfig);
            characterStoryModel.StepState(missionConfig.stepId,true,false,false);
            var data = new Dictionary<string, object> { { "CharacterStepsStates", characterStoryModel.CharacterStepsStates } };
            SaveStorageAndMissions(false,true,false,data);
        }
        public void CompleteLimitedMission(LimitedMissionMapConfig missionConfig)
        {
            foreach (var piece in missionConfig.piecesRequest)
            {
                RemoveFromStorage(piece);
            }
            limitedMission = null;
            SaveStorageAndMissions(false,false,true);
        }
        public void RemoveFromStorage(PieceDiscovery piece, bool save = false, bool includeGifts = false)
        {
            var index = storage.FindIndex(i => i.pType == piece.pType && i.Lvl == piece.Lvl);
            if (index >= 0)
            {
                storage.RemoveAt(index);
                //Debug.Log("Removed " + piece.pType + " from storage");
            }
            else
            {
                index = gifts.FindIndex(i => i.pType == piece.pType && i.Lvl == piece.Lvl);
                gifts.RemoveAt(index);
                //Debug.Log("Removed " + piece.pType + " from Gifts");
            }
            if(save) SaveStorageAndMissions();
        }

        /*public void SaveDiscovery()
        {
            var data = new MergeDiscovery();
            data.pieceDiscoveries = pieceDiscoveries;
            CloudSaveDiscovery(data);
        }
        public async void CloudSaveDiscovery(MergeDiscovery ecoData)
        {
            //Debug.Log("Save discovery Sent");
            var data = new Dictionary<string, object> { { "discovery", ecoData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }*/

        public void SaveEconomy(BoardState boardState)
        {
            //ECONOMY
            var ecoData = new MergeEconomy();
            ecoData.energy = energy;
            ecoData.nextEnergy = nextEnergy;
            ecoData.storageSlots = storageSlots;

            var data = new Dictionary<string, object> { 
                { "mergeEconomy", ecoData }, 
                { "board_" + boardState.boardID, boardState },
                { "assistants", assistants}
            };

            if (saveDiscovery)
            {
                var dataDiscovery = new MergeDiscovery();
                dataDiscovery.pieceDiscoveries = pieceDiscoveries;
                data.Add("discovery", dataDiscovery);
                saveDiscovery = false;
            }
            SaveStorageAndMissions(false, false, false, data);
        }

        public void SaveStorageAndMissions(bool saveMissions = false, bool saveCharacterMissions = false, bool saveLimitedMissions = false, Dictionary<string, object> extraKeys = null)
        {
            var data = new Dictionary<string, object>
            {
                { "mergeStorage", storage },
                { "gifts", gifts }
            };
            if (saveMissions) {
                data.Add("mergeMapMissions", mapMissions);
                data.Add("newMergeMapMissions", mapMissionsNew);
            }
            if (saveCharacterMissions) {
                data.Add("CharacterMissions", mapMissionsCharacters);
            }
            if (saveLimitedMissions) { 
                data.Add("mergeLimitedMission", limitedMission); 
            }
            if(extraKeys != null)
            {
                foreach (var key in extraKeys)
                {
                    data.Add(key.Key, key.Value);
                }
            }
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
            GameManager.Log("Save Storage And Missions Sent");
        }
        public void SaveGifts()
        {
            var data = new Dictionary<string, object> { { "gifts", gifts } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        public void SaveDailyDeals()
        {
            var data = new Dictionary<string, object> { { "dailyDeal", currentDailyDeal } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }

        internal void Import(MergeBoardModelOLD boardModelOLD)
        {
            energy = boardModelOLD.energy;
            nextEnergy = boardModelOLD.nextEnergy;
            pieceDiscoveries = boardModelOLD.pieceDiscoveries;
            foreach(BoardStateOLD bStateOld in boardModelOLD.boards)
            {
                var newBState = new BoardState(bStateOld);
                boards.Add(newBState);
            }
        }

        public string MapMissionsJSON()
        {
            var seriList = new SerializableList<int>();
            seriList.list = new List<int>();
            foreach (var item in mapMissions)
            {
                seriList.list.Add(item);
            }
            return JsonUtility.ToJson(seriList);
        }
        public string StorageJSON()
        {
            var seriList = new SerializableList<PieceDiscovery>();
            seriList.list = new List<PieceDiscovery>();
            foreach (var item in storage)
            {
                seriList.list.Add(item);
            }
            return JsonUtility.ToJson(seriList);
        }

        public bool CharacterOwned(int id)
        {
            return ownedCharacters.Contains(id);
        }
        public void OnBuyCharacter(int id)
        {
            ownedCharacters.Add(id);
            SaveCharacters();
        }
        private void SaveCharacters()
        {
            var data = new Dictionary<string, object> { { "ownedCharacters", ownedCharacters } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        
#if UNITY_EDITOR
        public void ClearAvatars()
        {
            ownedCharacters.Clear();
            SaveCharacters();
        }
#endif
    }


        [System.Serializable]
    public class BoardState
    {
        public string boardID;
        public int sizeX;
        public int sizeY;
        public List<PieceState> pieces;
        public MissionsModel activeMissions;
        public List<BoosterState> activeBoosters = new List<BoosterState>();
        private Color _colorTileNormal;
        private Color _colorTileDark;

        [JsonConstructor]
        public BoardState(string boardID, int sizeX, int sizeY, List<PieceState> pieces, MissionsModel activeMissions)
        {
            this.boardID = boardID;
            this.sizeX = sizeX;
            this.sizeY = sizeY;
            this.pieces = pieces;
            this.activeMissions = activeMissions;
            activeBoosters = new List<BoosterState>(); //Todo get real active boosters
        }
        public BoardState(BoardConfig boardConfig)
        {
            this.boardID = boardConfig.boardID;
            var piecesForBoard = new List<PieceState>();
            foreach(var p in boardConfig.pieces) 
            {
                var newP = new PieceState(p);
                piecesForBoard.Add(newP);
            }
            sizeX = boardConfig.sizeX;
            sizeY = boardConfig.sizeY;
            pieces = piecesForBoard;
            activeMissions = new MissionsModel();
            activeBoosters = new List<BoosterState>();
        }
        public BoardState(BoardStateOLD oldState)
        {
            this.boardID = oldState.boardID;
            sizeX = oldState.sizeX;
            sizeY = oldState.sizeY;
            pieces = oldState.pieces;
            activeMissions = new MissionsModel();
            activeBoosters = new List<BoosterState>();
        }

        //If after an App Update there are more generators, add the new ones in the config to old Player State
        public void CheckGeneratorsMatchWithConfig(BoardConfig boardConfig)
        {
            foreach(var p in boardConfig.pieces)
            {
                if (p.pieceType == PieceType.None || p.pieceType == PieceType.CommonChest) continue;
                var genConfig = GameConfigMerge.instance.mergeConfig.GetDefByPieceType(p.pieceType).levels[0].GetComponent<MovingPiece>()?.genConfig;
                if (genConfig != null)
                {
                    if (genConfig.coolDown > 0 && pieces.Find(x => x != null && x.pieceType == p.pieceType) == null)
                    {
                        AddGeneratorToPlayerState(p);
                    }
                }
            }
        }

        private void AddGeneratorToPlayerState(PieceState generator)
        {
            for(var i = 0; i < pieces.Count;i++)
            {
                if (pieces[i] == null)
                {
                    pieces[i] = generator;
                    return;
                }
            }
        }

        public BoosterState AddBoosterState(BoosterConfig booster)
        {
            if (HasBoosterActive(booster.boosterType)) return null;
            var bState = new BoosterState{ boosterType=booster.boosterType,endTime=DateTime.Now.AddSeconds(booster.duration)};
            activeBoosters.Add(bState);
            return bState;
        }
        public void RemoveBooster(BoosterType boosterType)
        {
            var booster = activeBoosters.Find(b => b.boosterType == boosterType);
            if (booster != null && booster.endTime <= DateTime.Now)
            {
                Debug.Log("Removing booster,total were = " + activeBoosters.Count);
                activeBoosters.Remove(booster);
                Debug.Log("Booster " + boosterType + " removed, boosters left = " + activeBoosters.Count);
            }
        }

        public bool HasBoosterActive(BoosterType boosterType)
        {
            //Temp fix: Clear OLD boosters
            RemoveBooster(boosterType);
            activeBoosters.ForEach((b) =>
            {
                if (b.endTime < DateTime.Now)
                {
                    activeBoosters.Remove(b);
                }
            });
            ////////////////////////////
            return activeBoosters.Any(b => b.boosterType == boosterType && b.endTime > DateTime.Now);
        }
        
        public void UpdatePieces(List<BoardTile> tiles)
        {
            pieces = new List<PieceState>();
            for(var i=0; i< tiles.Count; i++)
            {
                if (tiles[i].piece != null && !tiles[i].piece.PieceState.IsBubble)
                {
                    //var pState = new PieceState(tiles[i].piece);
                    pieces.Add(tiles[i].piece.PieceState);
                }
                else
                {
                    pieces.Add(null);
                }
            }
        }

        public void SetColorTileNormal(Color color)
        {
            _colorTileNormal = color;
        }
        public void SetColorTileDark(Color color)
        {
            _colorTileDark = color;
        }
        public Color GetColorTileNormal()
        {
            return _colorTileNormal;
        }
        public Color GetColorTileDark()
        {
            return _colorTileDark;
        }
        
        /*public void LocalSaveBoard()
        {
            if(pieces.Count == 0 || pieces.All(p=>p == null))
            {
                Debug.LogException(new Exception("Attention: Local Save Skipped for Board" + boardID + ", all pieces are NULL"));
                return;
            }
            GameManager.Log("Local Save Boards Sent - mergeBoard" + boardID);
            var jsSettings = new JsonSerializerSettings();
            var data = new Dictionary<string, object> { { boardID, this } };
            //var data = new Dictionary<string, object> { { boardID, t } };
            
            PlayerPrefs.SetString("mergeBoard"+boardID, JsonConvert.SerializeObject(pieces, Formatting.None, jsSettings));
        }*/

        public void CheckNullPieces()
        {
            foreach(var p in pieces)
            {
                if (p != null) return;
            }
            Debug.Log("ATTENTION: all pieces in board missing");
            //Look for local save
            if (PlayerPrefs.HasKey("mergeBoard" + boardID))
            {
                Debug.Log("Local Data Found, trying to fill");
                pieces = JsonConvert.DeserializeObject<List<PieceState>>(PlayerPrefs.GetString("mergeBoard" + boardID));
                foreach (var p in pieces)
                {
                    if (p != null) return;
                }
                Debug.Log("ATTENTION: all pieces in Local data are missing too");
            }
            else
            {
                Debug.Log("No Local Data Found, using board config");
                pieces = GameManager.Instance.mapManager.GetBuildingDataFromType(PlayerData.playerLocation).boardConfig.pieces;
            }
        }
        
    }



    [Serializable]
    public class ActiveMissionCloudSave
    {
        public PieceType pieceType;
        public int pieceLevel;
        [JsonConstructor]
        public ActiveMissionCloudSave(int pieceType, int pieceLevel) 
        {
            this.pieceType = (PieceType)pieceType;
            this.pieceLevel = pieceLevel;
        }
        public ActiveMissionCloudSave(MergeMissionConfig config) 
        {
            pieceType = config.piece.pieceType;
            pieceLevel = config.piece.pieceLevel;
        }
    }


    [Serializable]
    public class MissionsModel
    {
        public List<ActiveMissionCloudSave> missions;


        public MissionsModel(List<ActiveMissionCloudSave> missions)
        {
            this.missions = missions;
        }
        
        public MissionsModel(List<PieceStateOLD> missionsOld)
        {
            missions = new List<ActiveMissionCloudSave>();
            for (var i = 0; i < missionsOld.Count; i++)
            {
                missions.Add(new ActiveMissionCloudSave((int)missionsOld[i].pieceType, missionsOld[i].pieceLevel));
            }
        }
        public MissionsModel()
        {
            missions = new List<ActiveMissionCloudSave>();
        }
        public MissionsModel(List<MergeMissionConfig> activeMissions)
        {
            missions = new List<ActiveMissionCloudSave>();
            foreach(var m in activeMissions)
            {
                Add(m);
            }
        }
        public void Add(MergeMissionConfig m)
        {
            missions.Add(new ActiveMissionCloudSave(m));
        }
        public void Remove(MergeMissionConfig m)
        {
            missions.Remove(missions.Find(a => a.pieceType == m.piece.pieceType && a.pieceLevel == m.piece.pieceLevel));
        }
    }

    [Serializable]
    public class MergeEconomy
    {
        public int energy;
        public DateTime nextEnergy;
        public int storageSlots = 3;
    }
    
    [Serializable]
    public class MergeDiscovery
    {
        public List<PieceDiscovery> pieceDiscoveries;
    }

    [Serializable]
    public class PieceState
    {
        [HorizontalGroup("PieceState", Width = 140), LabelText("Type"), LabelWidth(35)] public PieceType pieceType;
        [HorizontalGroup("PieceState", Width = 65), LabelText("Level"), LabelWidth(35)] public int pieceLevel;
        [HorizontalGroup("PieceState", Width = 60), LabelWidth(43)] public bool locked;
        [HorizontalGroup("PieceState", Width = 60), LabelWidth(43)] public bool hidden;
        [HorizontalGroup("PieceState", 100), LabelWidth(80), ShowIf("hidden")] public int unlockLevel;
        [HorizontalGroup("PieceState", 150), LabelWidth(80), ShowIf("hidden")] public int unlockPrice;
        [HideInInspector] public GeneratorState generator;
        [HideInInspector] public BoosterState booster;

        [NonSerialized] private bool _isBubble;
        public bool Interactable => !locked && !hidden;
        public bool IsBubble { get => _isBubble; set => _isBubble = value; }

        public PieceState(MovingPiece piece)
        {
            pieceType = piece.PieceType;
            pieceLevel = piece.PieceLevel;
        }
        public PieceState(PieceType pieceType, int level)
        {
            this.pieceType = pieceType;
            pieceLevel = level;
        }
        public PieceState()
        {
            pieceType = PieceType.None;
            pieceLevel = 0;
        }

        public PieceState(PieceState p)
        {
            pieceType = p.pieceType;
            pieceLevel = p.pieceLevel;
            locked = p.locked;
            hidden = p.hidden;
            unlockLevel = p.unlockLevel;
            unlockPrice = p.unlockPrice;
            generator = p.generator;
            booster = p.booster;
        }

        public PieceState GetPreviousLevel() { return new PieceState(pieceType, Mathf.Max(1, pieceLevel - 1)); }

        public PieceDiscovery pieceDiscovery() => new PieceDiscovery(this);
    }


    [System.Serializable]
    public class PieceDiscovery
    {
        public PieceType pType;
        public int Lvl;

        [JsonConstructor]
        public PieceDiscovery(PieceType piece, int level)
        {
            pType = piece;
            Lvl = level;
        }

        public PieceDiscovery(PieceState piece)
        {
            pType = piece.pieceType;
            Lvl = piece.pieceLevel; 
        }
    }

    [System.Serializable]
    public class GeneratorState
    {
        public int capLeft;
        public DateTime nextTime;
    }



    [Serializable]
    public class MergeBoardModelOLD
    {
        public List<BoardStateOLD> boards;
        public int energy;
        public DateTime nextEnergy;
        public List<PieceDiscovery> pieceDiscoveries = new List<PieceDiscovery>();
    }
    [Serializable]
    public class BoardStateOLD
    {
        public string boardID;
        public int sizeX;
        public int sizeY;
        public List<PieceState> pieces;
        public List<MissionsModelOLD> activeMissions;
    }
    [Serializable]
    public class MissionsModelOLD
    {
        public PieceStateOLD piece;
        public int requirePlayerLevel;
        public List<RewardDataOLD> rewardInstant;
        public PieceStateOLD rewardPiece;
    }
    [Serializable]
    public class PieceStateOLD
    {
        public GeneratorState generator;
        public PieceType pieceType;
        public int pieceLevel;
    }

    [System.Serializable]
    public class SerializableList<T>
    {
        public List<T> list;
    }

    
}