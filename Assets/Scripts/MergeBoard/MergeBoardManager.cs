using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using static DailyBonusManager;
using System;
using System.Linq;
using UnityEngine.Purchasing;
using Unity.Services.CloudSave.Models;

namespace Assets.Scripts.MergeBoard
{
    public class MergeBoardManager : MonoBehaviour
    {
        public MergeBoardController boardController;
        public MergeBoardModel boardModel = new MergeBoardModel();
        public BoardConfig tempBoardConfig;

        public MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;
        public MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
        public bool IsMergeBoardActive => boardController != null && boardController.gameObject.activeSelf;
        public bool IsBoardActive => boardController.gameObject.activeSelf && Globals.Instance.gameLoaded;
        private string boardsCloudKey = "boardsTemp24";
        
        private Vector3 originalPosition;

        System.Action _OnClose;

        // Start is called before the first frame update
        void Start()
        {
            originalPosition = boardController.transform.position;
        }

        public IEnumerator Init()
        {
            while (!Globals.Instance.gameLoaded || !Globals.isSignedIn || !Globals.configLoaded) yield return null;
            LoadData();
            /*while (!Globals.Instance.boardsLoaded)
            {
                LoadData();
                yield return new WaitForSeconds(1);
            }*/
        }
        IEnumerator InitBoard()
        {
            while (!Globals.Instance.gameLoaded)
            {
                yield return Globals.Instance.gameLoaded;
            }
            //Debug.Log("Initializing board");
            CreateBoard(tempBoardConfig);
            yield return new WaitForEndOfFrame();
            boardController.FillBoard();
            boardController.Show();
        }
        
        public void ShowMergeBoard(bool value, BoardConfig boardConfig = null, System.Action OnClose = null)
        {
            _OnClose = OnClose;
            
            if (!value)
            {
                if (IsBoardActive) {
                    boardController.SaveState();
                    boardController.Hide();
                }
                return;
            }
            if (!TutorialManager.IsTutoCompleted(TutorialKeys.MergeBoardCollectXP))
            {
                //Board is placed up to let tutorial bottom show properly
                boardController.transform.position = originalPosition + new Vector3(0, 130, 0);
            }
            else 
            {
                //bannerManager.gameObject.SetActive(true);
                //bannerManager.ShowBanner();
                boardController.transform.position = originalPosition;
            }
            boardController.Init();
            boardController.gameObject.SetActive(value);
            boardController.boardConfig = boardConfig;
            tempBoardConfig = boardConfig;
            StartCoroutine(InitBoard());
        }

        public void CreateBoard(BoardConfig boardConfig)
        {
            GameManager.Instance.canvasBackground.color = boardConfig.BGColor;
            boardController.mainBG.color = boardConfig.boardBGColor;
            var boardstate = boardModel.GetBoard(boardConfig);
            boardstate.SetColorTileNormal(boardConfig.boardTileColorNormal);
            boardstate.SetColorTileDark(boardConfig.boardTileColorDark);
            if (boardstate.activeMissions.missions.Count == 0)
            {
                if (boardConfig.missions.Count > 0)
                {
                    for (var i = 0; 
                        i< boardConfig.startingMissions && i<boardConfig.maxVisibleMissions;// && i<GameManager.Instance.PlayerLevel;
                        i++)
                    {
                        //boardstate.activeMissions.Add(GetRandomMissionFromConfig(boardConfig));
                        boardstate.activeMissions.Add(boardConfig.missions[i]);
                    }
                }
                else
                {
                    Debug.Log("No missions in config");
                }
            }
            else
            {
                Debug.Log("board already has mission");
            }
            boardController.LoadBoard(boardstate, 
                () => {
                    _OnClose?.Invoke();
                    GameManager.Instance.ShowMergeBoard(false); 
                });
        }

        public void LazyUpdate()
        {
            boardController.LazyUpdate();
        }

        public async void LoadData()
        {
            bool newDataFound = false;
            try
            {
                var result = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

                Dictionary<string, Item> savedData = result;
                foreach (var t in savedData)
                {
                    if (t.Key == "mergeEconomy")
                    {
                        var mergeEconomy = JsonConvert.DeserializeObject<MergeEconomy>(t.Value.Value.GetAsString());
                        boardModel.energy = mergeEconomy.energy;
                        boardModel.nextEnergy = mergeEconomy.nextEnergy;
                        boardModel.storageSlots = mergeEconomy.storageSlots;
                    }
                    else if (t.Key == "assistants")
                    {
                        boardModel.assistants = JsonConvert.DeserializeObject<AssistantsModel>(t.Value.Value.GetAsString());
                        boardModel.assistants.CheckExpired();
                    }
                    else if (t.Key.StartsWith("discovery"))
                    {
                        var data = JsonConvert.DeserializeObject<MergeDiscovery>(t.Value.Value.GetAsString());
                        boardModel.pieceDiscoveries = data.pieceDiscoveries;
                    }
                    else if (t.Key.StartsWith("CharacterStepsStates"))
                    {
                        boardModel.characterStoryModel.CharacterStepsStates = JsonConvert.DeserializeObject<List<CharacterStoryStepState>>(t.Value.Value.GetAsString());
                    }
                    else if (t.Key.StartsWith("board_"))
                    {
                        var boardstate = JsonConvert.DeserializeObject<BoardState>(t.Value.Value.GetAsString());
                        if (boardModel.BoardInModel(boardstate.boardID))
                        {
                            //the board already exist in the model, so do nothing. Just send an event to know about it (end march2024)
                            TrackingManager.TrackError("Board loaded from model already exists " + t.Key);
                            continue;
                        }
                        else
                        {
                            boardModel.boards.Add(boardstate);
                            var boardConfig = GameManager.Instance.mapManager.GetBoardConfigByID(boardstate.boardID);
                            if (boardConfig != null)
                            {
                                boardstate.CheckGeneratorsMatchWithConfig(boardConfig);
                            }
                            newDataFound = true;
                        }
                    }
                    else if (t.Key.StartsWith("mergeStorage"))
                    {
                        boardModel.storage = JsonConvert.DeserializeObject<List<PieceDiscovery>>(t.Value.Value.GetAsString());
                    }
                    else if (t.Key.StartsWith("gifts"))
                    {
                        boardModel.gifts = JsonConvert.DeserializeObject<List<PieceDiscovery>>(t.Value.Value.GetAsString());
                    }
                    else if (t.Key.StartsWith("mergeMapMissions"))
                    {
                        var missions = JsonConvert.DeserializeObject<List<int>>(t.Value.Value.GetAsString());
                        boardModel.mapMissions = missions;
                        //Migrate to new System
                        if (missions.Count > 0) MigrateOldMissionsToNewSystem();
                    }
                    else if (t.Key.StartsWith("newMergeMapMissions"))
                    {
                        var missions = JsonConvert.DeserializeObject<List<MapMissionCloud>>(t.Value.Value.GetAsString());
                        boardModel.mapMissionsNew = missions;
                    }
                    else if (t.Key.StartsWith("CharacterMissions"))
                    {
                        var missions = JsonConvert.DeserializeObject<List<MapMissionCloudCharacter>>(t.Value.Value.GetAsString());
                        boardModel.mapMissionsCharacters = missions;
                    }
                    else if (t.Key.StartsWith("ownedCharacters"))
                    {
                        var ownedCharacters = JsonConvert.DeserializeObject<List<int>>(t.Value.Value.GetAsString());
                        boardModel.ownedCharacters = ownedCharacters;
                    }
                    else if (t.Key.StartsWith("mergeLimitedMission"))
                    {
                        var mission = JsonConvert.DeserializeObject<LimitedMissionMapConfig>(t.Value.Value.GetAsString());
                        boardModel.limitedMission = mission;
                    }
                    else if (t.Key.StartsWith("dailyDeal"))
                    {
                        var dailyDeal = JsonConvert.DeserializeObject<DailyDealItemSet>(t.Value.Value.GetAsString());
                        boardModel.currentDailyDeal = dailyDeal;
                    }
                    else if (t.Key.StartsWith("dailyBonus"))
                    {
                        var dailyBonus = JsonConvert.DeserializeObject<DailyBonusProgress>(t.Value.Value.GetAsString());
                        GameManager.Instance.DailyBonusManager.dailyBonusProgress = dailyBonus;
                    }
                    else if (t.Key.StartsWith("SpeedBoardTries"))
                    {
                        var speedBoardTries = JsonConvert.DeserializeObject<SpeedBoardTries>(t.Value.Value.GetAsString());
                        GameManager.Instance.speedBoardManager.speedBoardTries = speedBoardTries;
                    }
                }
                if (!newDataFound)
                {   //Check OLD saving system

                    boardModel.energy = 100;

                    if (!savedData.ContainsKey("dailyBonus"))
                    {
                        GameManager.Instance.DailyBonusManager.CreateNewProgress();
                    }
                }
                else
                {
                    GameManager.Instance.UpdateMapMissions();
                }
                Globals.Instance.boardsLoaded = true;
            }
            catch (CloudSaveValidationException e)
            {
                TrackingManager.TrackError("Error Loading model from Unity ValidationException");
                Debug.LogError(e);
            }
            catch (CloudSaveRateLimitedException e)
            {
                TrackingManager.TrackError("Error Loading model from Unity RateLimited");
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                TrackingManager.TrackError("Error Loading model from Unity Exception");
                Debug.LogError(e);
            }
        }
        

        public static bool PieceStatesMatch(MovingPiece A, MovingPiece B)
        {
            return A.PieceState.pieceLevel == B.PieceState.pieceLevel 
                && A.PieceState.pieceType == B.PieceState.pieceType;
        }
        public static bool PieceStatesMatch(PieceDiscovery A, MovingPiece B)
        {
            return A.Lvl == B.PieceState.pieceLevel 
                && A.pType == B.PieceState.pieceType;
        }
        public bool TryToAddMapMission()
        {
            if(boardModel.mapMissionsNew.Count < GameManager.Instance.PlayerLevel - 1 && boardModel.mapMissionsNew.Count < 3)
            {
                CreateMapMissionNEW();
                return true;
            }
            else if (GameManager.Instance.PlayerLevel > 3)
            {
                var lastLimitedMission = PlayerPrefs.HasKey("LimitedMissionEndTime")? UIUtils.GetTimeStampByKey("LimitedMissionEndTime"): System.DateTime.Now.AddSeconds(-1);
                if(System.DateTime.Now> lastLimitedMission)
                {
                    var limitedConfig = new LimitedMissionMapConfig();
                    limitedConfig.CreateLimitedMission();
                    mergeModel.limitedMission = limitedConfig;
                    mergeModel.SaveStorageAndMissions(false, false, true);
                    GameManager.Instance.mergeMissionsController.AddLimited();
                    return true;
                }
            }
            return false;
        }
#if UNITY_EDITOR
        public void resetLimitedDailyCooldown()
        {
            PlayerPrefs.DeleteKey("LimitedMissionEndTime");
            PlayerPrefs.DeleteKey("nextGiftTime");
            PlayerPrefs.DeleteKey("nextStealTime");
            PlayerPrefs.DeleteKey("genericOffer");
            
            mergeModel.limitedMission = null;
        }
        public void ResetLimitedMission()
        {
            PlayerPrefs.DeleteKey("LimitedMissionEndTime");
            mergeModel.limitedMission = null;
        }
#endif

        private void MigrateOldMissionsToNewSystem()
        {
            foreach (var m in boardModel.mapMissions)
            {
                var newMission = new MapMissionCloud();
                var oldMission = mergeConfig.mapMissions[m];
                newMission.location = GameManager.Instance.mapManager.GetBuildingDataFromType(oldMission.location).IsUnlocked ?
                        oldMission.location:
                        GameManager.Instance.mapManager.GetRandomUnlockedBuilding();
                newMission.piecesRequest = oldMission.piecesRequest;
                newMission.rewards = oldMission.rewardInstant;
                boardModel.mapMissionsNew.Add(newMission);
            }
            boardModel.mapMissions.Clear();
            boardModel.SaveStorageAndMissions(true);
        }

        public void CreateMapMissionNEW()
        {
            var levelWeight = 0;
            var maxDiscovered = GameManager.Instance.mapManager.GetMaxDiscoveries();
            var mapMissionsReceived = PlayerPrefs.GetInt("mapMissionsReceived");

            if(mapMissionsReceived < 5 && PlayerData.MapMissionsCount < 5) //Missions from config until level X
            {
                boardModel.AddMission(mergeConfig.mapMissions[mapMissionsReceived++]);
                PlayerPrefs.SetInt("mapMissionsReceived", mapMissionsReceived);
                GameManager.Instance.UpdateMapMissions();
                return;
            }

            //Create Pieces to request
            var piecesRequest = new List<PieceDiscovery>();
            for (var i = 0; i < 3; i++) 
            {
                var itemRequested = GetDiscoveredItemForMission(maxDiscovered,piecesRequest);
                if (itemRequested == null) break;
                var genConfig = mergeConfig.GetGeneratorOfPiece(itemRequested.pType);
                var localWeight = (int)((Math.Pow(itemRequested.Lvl, 3.1f) + Math.Sqrt(genConfig.coolDown) * genConfig.piecesChances.Count) * .02f); // / genConfig.piecesChances.Count
                GameManager.Log("Local weight for " + itemRequested.pType + ", lvl = " + itemRequested.Lvl + " = " + localWeight);
                levelWeight += localWeight;
                piecesRequest.Add(itemRequested);
            }

            //Set rewards
            var coins = Mathf.Max(100, (int)(GetFactorial(levelWeight * 2) 
                * (UnityEngine.Random.Range(10,20)-Mathf.Min(GameManager.Instance.PlayerLevel,8))));
            var fPoints = Mathf.Max(2,(int)(levelWeight * 1.2f));
            //Debug.Log("Total level " + levelWeight);
            var rewardInstant = new List<RewardData>();
            rewardInstant.Add(new RewardData(RewardType.Coins, coins));
            rewardInstant.Add(new RewardData(RewardType.FamePoints, fPoints));

            var random = UnityEngine.Random.Range(0, 15);
            //Last reward
            if (fPoints < 9)
            {
                if (random < 5)
                {
                    rewardInstant.Add(new RewardData(new PieceDiscovery(PieceType.Energy, fPoints<5?0:1)));
                }
                else if(random < 10)
                {
                    rewardInstant.Add(new RewardData(new PieceDiscovery(PieceType.Gems, fPoints < 5 ? 0 : 1)));
                }
                else if(random < 15)
                {
                    rewardInstant.Add(new RewardData(new PieceDiscovery(PieceType.XP, fPoints < 5 ? 2 : 3)));
                }
            }
            else
            {
                //TODO: More variaty
                var mergeItemReward = new RewardData(RewardType.MergeItem, 1);
                if (fPoints < 11) mergeItemReward.mergePiece = new PieceDiscovery(PieceType.Energy, 2);
                else if (fPoints < 13) mergeItemReward.mergePiece = new PieceDiscovery(PieceType.Scissors, 0);
                else if (fPoints < 16) mergeItemReward.mergePiece = new PieceDiscovery(PieceType.CommonChest, 0);
                else
                {
                    if (random < 2)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterAutoMerge, 0);
                    }
                    else if (random < 4)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.LevelUP, 0);
                    }
                    else if (random < 6)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterEnergy, 0);
                    }
                    else if (random < 8)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.BoosterGenerators, 0);
                    }
                    else if (random < 9)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.RouleteTicketCommon, 1);
                    }
                    else if (random < 10)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.RouleteTicketCommon, 2);
                    }
                    else if (random < 11)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.RouleteTicketSpecial, 0);
                    }
                    else if (random < 12)
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.RouleteTicketSpecial, 1);
                    }
                    else 
                    {
                        mergeItemReward.mergePiece = new PieceDiscovery(PieceType.XP, 4);
                    }
                }
                rewardInstant.Add(mergeItemReward);
            }
            var newMission = new MapMissionCloud();
            newMission.piecesRequest = piecesRequest;
            newMission.rewards = rewardInstant;
            newMission.location = PlayerData.MapMissionsCount==0?BuildingType.BicycleShop:
                GameManager.Instance.mapManager.GetRandomUnlockedBuilding();
            boardModel.AddMission(newMission);
            GameManager.Instance.UpdateMapMissions();
        }

        /*private PieceDiscovery GetDiscoveredItem(List<PieceDiscovery> maxDiscovered, List<PieceDiscovery> piecesToSkip)
        {
            var isValidChain = false;
            var safeTries = 20;
            while (!isValidChain && safeTries > 0)
            {
                var chainIndex = UnityEngine.Random.Range(0, maxDiscovered.Count);
                //if (maxDiscovered[chainIndex].Lvl > 3) //Has discovered at least level 3
                //{
                    // is candidate already in the request?
                    if (piecesToSkip.Find(p => p.pType == maxDiscovered[chainIndex].pType) == null)
                    {
                        var minLevel = Math.Max(2, maxDiscovered[chainIndex].Lvl - 2);
                        var _maxLevel = maxDiscovered[chainIndex].Lvl;
                        var itemLevel = UnityEngine.Random.Range(minLevel, maxDiscovered[chainIndex].Lvl + 1);
                        GameManager.Log("Piece Chosen::: " + maxDiscovered[chainIndex].pType + "," + itemLevel);
                        return new PieceDiscovery(maxDiscovered[chainIndex].pType, itemLevel);
                    }
                    safeTries--;
                //}
            }
            return null;
        }*/
        
        private PieceDiscovery GetDiscoveredItemForMission(List<PieceDiscovery> maxDiscovered, List<PieceDiscovery> piecesToSkip)
        {
            var isValidChain = false;
            var safeTries = 20;
            while (!isValidChain && safeTries > 0)
            {
                safeTries--;

                var chainIndex = UnityEngine.Random.Range(0, maxDiscovered.Count);
                //Skip this chains
                if (maxDiscovered[chainIndex].pType == PieceType.RouleteTicketCommon ||
                    maxDiscovered[chainIndex].pType == PieceType.RouleteTicketSpecial)
                {
                    continue;
                }
                // candidate is already in the request?
                if (piecesToSkip.Find(p => p.pType == maxDiscovered[chainIndex].pType) != null)
                {
                    continue;
                }
                // candidate is already in another mission?
                if (IsChainInMapMission(maxDiscovered[chainIndex].pType))
                {
                    continue;
                }
                var minLevel = Math.Max(2, maxDiscovered[chainIndex].Lvl - 2);
                var _maxLevel = maxDiscovered[chainIndex].Lvl;
                var itemLevel = UnityEngine.Random.Range(minLevel, maxDiscovered[chainIndex].Lvl + 1);
                GameManager.Log("Piece Chosen::: " + maxDiscovered[chainIndex].pType + "," + itemLevel);
                return new PieceDiscovery(maxDiscovered[chainIndex].pType, itemLevel);
                
            }
            return null;
        }

        public bool IsChainInMapMission(PieceType piece)
        {
            for (var i = 0; i < mergeModel.mapMissionsNew.Count; i++)
            {
                var mission = mergeModel.mapMissionsNew[i];
                if (mission.piecesRequest.Any(item => item.pType == piece)) 
                    return true;
            }
            if (mergeModel.limitedMission != null && mergeModel.limitedMission.piecesRequest.Any(item => item.pType == piece)) 
                return true;
            return false;
        }


        public void MapMissionComplete() { }

        public int CompletedMissions
        {
            get => PlayerPrefs.GetInt("CompletedMissions");
            set => PlayerPrefs.SetInt("CompletedMissions", value);
        }

        public static int GetFactorial(int num)
        {
            var result = 0;
            for (var i = 1; i <= num; i++)
            {
                result += i;
            }
            //Debug.Log(result);
            return result;
        }

        public bool IsPieceRequestedInMission(MovingPiece piece)
        {
            for (var i = 0; i < mergeModel.mapMissionsNew.Count; i++)
            {
                var mission = mergeModel.mapMissionsNew[i];
                if (mission.piecesRequest.Any(item => PieceStatesMatch(item, piece))) return true;
            }
            for (var i = 0; i < mergeModel.mapMissionsCharacters.Count; i++)
            {
                var mission = mergeModel.mapMissionsCharacters[i];
                if (mission.piecesRequest.Any(item => PieceStatesMatch(item, piece))) return true;
            }
            if (mergeModel.limitedMission != null && mergeModel.limitedMission.piecesRequest.Any(item => PieceStatesMatch(item, piece))) return true;
            return false;
        }
        public bool IsChainRequestedInMission(PieceType pieceType)
        {
            for (var i = 0; i < mergeModel.mapMissionsNew.Count; i++)
            {
                var mission = mergeModel.mapMissionsNew[i];
                foreach (var item in mission.piecesRequest)
                {
                    if(item.pType == pieceType && !mergeModel.isItemInStorage(item))
                    {
                        return true;
                    }
                }
            }
            if (mergeModel.limitedMission != null && mergeModel.limitedMission.piecesRequest.Any(item => item.pType == pieceType)) return true;
            return false;
        }
        public bool IsPieceRequestedInOrders(GeneratorChance targetPiece)
        {
            for (var i = 0; i < mergeModel.boards.Count; i++)
            {
                var board = mergeModel.boards[i];
                var isInBoard = false;
                var isInMission = false;
                foreach (var item in board.activeMissions.missions)
                {
                    if(item.pieceType == targetPiece.pieceType)
                    {
                        isInMission = true;
                        foreach (var piece in board.pieces)
                        {
                            if (piece != null && !piece.locked && !piece.hidden && piece.pieceType == targetPiece.pieceType && piece.pieceLevel == item.pieceLevel)
                            {
                                isInBoard = true;
                            }
                        }
                    }
                }
                if (!isInBoard && isInMission)
                {
                    Debug.Log(targetPiece.pieceType + " is a candidate");
                    return true;
                }
            }
            return false;
        }
    }
}
