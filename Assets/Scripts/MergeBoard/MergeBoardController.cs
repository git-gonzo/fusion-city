using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;
using Sirenix.OdinInspector;

namespace Assets.Scripts.MergeBoard
{
    public class MergeBoardController : MonoBehaviour
    {
        //public Transform MainContainer;
        #region References

        [SerializeField] Material GreyMaterial;
        [SerializeField] Camera uiCam;
        public Image mainBG;
        public Transform boardContainer;
        public Transform piecesContainer;
        public Transform missionsContainer;
        public BoardTile tilePrefab;
        public MissionTile MissionPrefab;
        public MovingPiece piecePrefab;
        public MergeEnergyController energyController;
        public TextMeshProUGUI txtInfoTitle;
        public TextMeshProUGUI txtInfoDescrip;
        public TextMeshProUGUI txtBoosterTime;
        public TextMeshProUGUI txtBoosterTimeLeft;
        public ButtonBuy btnClaim;
        public Button btnInfo;
        public Button btnRemove;
        public Button btnBooster;
        public ButtonBuy btnSell;
        public ButtonBuy btnUnlock;
        public ButtonBuy btnSkip;
        public ButtonBuy btnBuyBubble;
        public ButtonMergeStore btnStorage;
        public SideBarButton btnMissions;
        public PieceExtraInfo pieceExtraInfo;
        public GameObject boosterActiveContainer;
        public GameObject bottomContainer;
        
        public PopupMergeChain popupChain;
        public PopupEnergy popupEnergy;
        public AudioSource normalMergeSound;
        public AudioSource missionCompleteSound;
        public AudioSource missionMergeSound;
        public AudioSource spawnSound;
        public AudioSource spawnSoundFail;
        public AudioSource boosterAutomergeSound;
        public AudioSource boosterLevelUPSound;
        [Header("Particles")]
        public GameObject movingParticleBlue;
        public GameObject movingParticleGreen;
        public GameObject movingTrail;
        [SerializeField] private AssistanSmartGeneratorController _assistantController;
        PopupMergeChain _popupChainInstance;
        PopupEnergy _popupEnergyInstance;
        [ReadOnly] public BoardConfig boardConfig;

        public MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;
        public MergeBoardModel mergeModel => MergeManager.boardModel;
        MergeBoardManager MergeManager => GameManager.Instance.MergeManager;

        public Action onMerged;
        public Action onPieceMoved;
        public UnityAction _onClose;
        protected Action afterAction;

        protected List<BoardTile> _tiles;
        protected List<MovingPiece> _activeMissionsPieces;
        protected MovingPiece movingPiece;
        private PieceState _tutorialPiece;

        public PieceState tutorialPiece { set => _tutorialPiece = value; }
        public MovingPiece selectedPiece;
        protected BoardState _boardState;
        DateTime time;
        Vector3 _inputStartPos;
        private bool clicOnPiece;
        protected bool isSpeedBoard;
        bool overStoragebutton;
        bool isDragin;
        bool pendingChanges = false;
        bool storageChanged = false;
        protected bool _isAutoMerging = false;
        CanvasGroup _canvasGroup;
        Sequence seqSugest;
        int _timeElapsed = 0;
        private PieceDiscovery _lastMissionItem;
        private Transform _automergeBoosterPiece;
        private bool _isBusy = false;
        public bool canRun = true;
        #endregion

        public int energyRefillTotalSeconds => energyController.energyRefillTotalSeconds;
        private bool FreeStorageSlots => mergeModel.storageSlots > mergeModel.storage.Count;
        private List<BoosterState> activeBoosters => _boardState.activeBoosters;
        public CanvasGroup CanvasGroup {
            get {
                if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            } }
        public bool IsBusy => _isBusy;
        public virtual void Init()
        {
            CanvasGroup.DOFade(0, 0);
            transform.DOScale(0.5f, 0);
            btnStorage.OverButton = (bool val) => { overStoragebutton = val; };
            btnStorage.OnButtonStoreageUP = OnButtonStoreageUP;
        }

        public void Show()
        {
            _timeElapsed = -1;
            _isBusy = true;
            CanvasGroup.DOFade(1, 0.2f);
            transform.DOScale(1.1f, 0.4f).SetEase(Ease.OutBack).OnComplete(() => { _isBusy = false; LazyUpdate(false); });
            if (btnStorage != null) btnStorage.OnAddItem(mergeModel.storage.Count, mergeModel.storageSlots);
        }

        public void Hide()
        {
            var s = DOTween.Sequence();
            s.Append(transform.DOScale(0, 0.4f).SetEase(Ease.InBack));
            s.Insert(0.2f, CanvasGroup.DOFade(0, 0.2f)).OnComplete(() => { gameObject.SetActive(false); });
        }

        //Called OnMouseDown
        public void OnClick(MovingPiece piece)
        {
            if (_tutorialPiece != null && (piece.PieceDiscovery.pType != _tutorialPiece.pieceType || piece.PieceDiscovery.Lvl != _tutorialPiece.pieceLevel))
            {
                return;
            }
            if (piece.PieceState.hidden)
            {
                //Debug.Log("Piece is hidden");
                SelectHiddenTile(piece);
                return;
            }
            else if (piece.PieceState.locked)
            {
                //Debug.Log("Piece is locked");
                SelectLockedTile(piece);
                return;
            }
            movingPiece = piece;
            movingPiece.transform.SetAsLastSibling();
            _inputStartPos = Input.mousePosition;
            clicOnPiece = true;
        }
        public void OnClickMission(MovingPiece piece)
        {
            movingPiece = piece;
            movingPiece.transform.SetAsLastSibling();
            _inputStartPos = Input.mousePosition;
            clicOnPiece = true;
            //ShowPopupChain(movingPiece, true);
        }

        protected virtual void Update()
        {
            if (!canRun) return;
            
            if (Input.GetMouseButtonDown(0) && seqSugest != null)
            {
                KillSugestionAnim();
            }

            if (Input.GetMouseButton(0) && movingPiece != null && movingPiece.missionTile == null) //Not a mission
            {
                if (!isDragin && _inputStartPos != Vector3.zero && Vector3.Distance(_inputStartPos, Input.mousePosition) > 20)
                {
                    isDragin = true;
                }
                if (isDragin && movingPiece != null && CanSelectInTuto()) //TODO: OPTIMIZE, make a var tutorunnig, tutoManager set it
                {
                    movingPiece.transform.position = Vector3.Lerp(movingPiece.transform.position, Input.mousePosition, 0.4f);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (tutorialHand != null)
                {
                    DestroyImmediate(_handPointer.gameObject);
                }
                
                _timeElapsed = 0;
                isDragin = false;

                if (clicOnPiece == false) movingPiece = null;

                clicOnPiece = false;

                if (_tutorialPiece != null && movingPiece != null
                    && (_tutorialPiece.pieceType != movingPiece.PieceType || _tutorialPiece.pieceLevel != movingPiece.PieceLevel)) {
                    Debug.Log("In tutorial, expecting to target piece " + _tutorialPiece.pieceType + "-" + _tutorialPiece.pieceLevel);
                    movingPiece.ReturnToPlace(isSpeedBoard);
                    return;
                }
                if (movingPiece != null && movingPiece.missionTile != null) // TAP ON ORDER(MISSION)
                {
                    //Show popup with the chain and the rewards 
                    ShowPopupChain(movingPiece, true);
                }
                else if (movingPiece != null)
                {
                    // OVER STORAGE
                    if (OverStoragebutton() && !movingPiece.IsGenerator && !movingPiece.IsBoosterActive && !movingPiece.PieceState.IsBubble)
                    {
                        if (FreeStorageSlots)
                        {
                            DragPieceToStore();
                            return;
                        }
                        else
                        {
                            btnStorage.ShowToolTip("Backpack is Full");
                        }
                    }
                    var targetTile = GetTargetTile();
                    if (!CanSelectInTuto())
                    {
                        movingPiece = null;
                        return;
                    }
                    var missionTile = GetMissionTile();
                    //Mission Tile
                    if (missionTile != null)//IF Tap ended over a mission tile
                    {
                        if (MergeBoardManager.PieceStatesMatch(missionTile, movingPiece))
                        {
                            CompleteOrder(missionTile);
                            return;
                        }
                    }
                    //Over a Tile
                    if (targetTile != null && targetTile.piece != movingPiece)
                    {

                        if (targetTile.piece != null) // Tile with content
                        {
                            //Check Combine

                            //Check Merge
                            if (targetTile.piece.PieceType == movingPiece.PieceType
                                && CanLevelUp(targetTile.piece) && !movingPiece.PieceState.IsBubble && !targetTile.piece.PieceState.IsBubble
                                && (GameManager.Instance.PlayerLevel > 1 || !targetTile.piece.PieceState.locked))
                            {
                                PlayerData.mergeCount++;
                                GameManager.Instance.dailyTaskManager.OnMerge();
                                movingPiece = LevelUpPieceToTile(targetTile);
                                movingPiece.PlayMergeAnim();
                                if (!targetTile.piece.PieceState.locked) CheckSpecialReward(targetTile);
                                normalMergeSound.pitch = 1 + movingPiece.PieceLevel / 10f;
                                normalMergeSound.Play();
                                onMerged?.Invoke();
                            }
                            //Check Scissors
                            else if (movingPiece.PieceType == PieceType.Scissors && targetTile.piece.PieceLevel > 0 && targetTile.piece.IsAvailable && !targetTile.piece.PieceState.IsBubble)
                            {
                                movingPiece = LevelDownPieceTile(targetTile);
                                SpawnPiece(movingPiece.PieceDiscovery, movingPiece.GetBoardTileParent());
                            }
                            //Check LevelUp
                            else if (movingPiece.PieceType == PieceType.LevelUP && !IsMaxLevel(targetTile.piece) && targetTile.piece.IsAvailable && !targetTile.piece.PieceState.IsBubble && !targetTile.piece.IsGenerator)
                            {
                                DestroyImmediate(movingPiece.gameObject);
                                movingPiece = LevelUpPiece(targetTile);
                                movingPiece.PlayMergeAnim();
                                boosterLevelUPSound.Play();
                                onMerged?.Invoke();
                                GameManager.Instance.dailyTaskManager.OnUseBooster();
                            }
                            else if (targetTile.piece.PieceState.Interactable)
                            {   //PUSH
                                var secondTile = FindClosestTile();
                                secondTile.SetContent(targetTile.piece, OnClick, true, false, null, isSpeedBoard);
                            }
                            else
                            {
                                movingPiece.ReturnToPlace(isSpeedBoard);
                                CheckMissions();
                                movingPiece = null;
                                return;
                            }
                        }
                        else {
                            if (!TutorialManager.IsTutoCompleted(TutorialKeys.MergeSpawnFromChest))
                            {
                                onPieceMoved?.Invoke();
                            }
                        }

                        targetTile.SetContent(movingPiece, OnClick, true, false, null, isSpeedBoard);
                        SelectTile();
                        pendingChanges = true;
                        afterAction?.Invoke();
                    }
                    else
                    {
                        movingPiece.ReturnToPlace(isSpeedBoard);
                        if (targetTile.piece.IsGenerator)
                        {
                            TryToSpawnFromGenerator(targetTile);
                        }
                        SelectTile();
                    }
                    movingPiece = null;
                }
                CheckMissions();
            }
            /*if (mergeModel != null && mergeModel.energy < mergeConfig.maxEnergy ||
                selectedPiece != null && btnSkip.gameObject.activeSelf)
            {
                LazyUpdate(false);
            }*/
        }

        private void DragPieceToStore()
        {
            PieceDiscovery p = new PieceDiscovery(movingPiece.PieceState);
            GameManager.Instance.soundStorageBounce.Play();
            mergeModel.storage.Add(p);
            btnStorage.OnAddItem(mergeModel.storage.Count, mergeModel.storageSlots);
            movingPiece.Remove();
            storageChanged = true;
            pendingChanges = true;
            UnselectedTiles();
            CheckMissions();
        }

        private bool OverStoragebutton()
        {
            if(btnStorage == null) return false;
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            if (results.Count > 0)
            {
                for (var i = 0; i < results.Count; i++)
                {
                    if (results[i].gameObject == btnStorage.gameObject || results[i].gameObject == btnMissions.gameObject)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void OnButtonStoreageUP()
        {
            //Debug.Log("Put " + selectedPiece + " in storage");
        }

        private void KillSugestionAnim()
        {
            _timeElapsed = 0;
            //Debug.Log("Killing Sugest Sequence Anim");
            seqSugest.Goto(0);
            seqSugest.Kill();
            seqSugest = null;
        }

        protected virtual void CheckSpecialReward(BoardTile destination)
        {

            if (movingPiece.PieceState.pieceType == PieceType.Gems
                || movingPiece.PieceState.pieceType == PieceType.Coins
                || movingPiece.PieceState.pieceType == PieceType.Cash
                && movingPiece.PieceState.pieceLevel > 1)
            {
                if (UnityEngine.Random.Range(0, 250) < 2) //Todo: Set chances in board config
                {
                    SpawnPiece(PieceType.Energy, destination);
                    return;
                }
            }
            else if (movingPiece.PieceState.pieceLevel > 3 && movingPiece.PieceState.pieceType != PieceType.Gems)
            {
                if (UnityEngine.Random.Range(0, 2000) < 2) SpawnPiece(PieceType.BoosterEnergy, destination);
                else if (UnityEngine.Random.Range(0, 2000) < 2) SpawnPiece(PieceType.BoosterAutoMerge, destination);
                else if (UnityEngine.Random.Range(0, 2200) < 2) SpawnPiece(PieceType.BoosterGenerators, destination);
                else if (UnityEngine.Random.Range(0, 700) < 2) SpawnPiece(PieceType.Scissors, destination);
                else if (UnityEngine.Random.Range(0, 700) < 2) SpawnPiece(PieceType.RouleteTicketSpecial, destination);
                else if (UnityEngine.Random.Range(0, 250) < 2) SpawnPiece(PieceType.RouleteTicketCommon, destination);
                else if (CanProduceBubble() && UnityEngine.Random.Range(0, 20) < movingPiece.PieceState.pieceLevel - 3)
                {
                    SpawnPiece(movingPiece.PieceDiscovery, destination, true);//BUBBLE 
                }
                else if (UnityEngine.Random.Range(0, 20) < movingPiece.PieceState.pieceLevel - 4) // GEMS
                    SpawnPiece(PieceType.Gems, destination);
                else if (GameManager.Instance.PlayerLevel > 2 && UnityEngine.Random.Range(0, 12) < movingPiece.PieceState.pieceLevel - 5 + GameManager.Instance.PlayerLevel) // CASH
                    SpawnPiece(PieceType.Cash, destination);
                else //if (UnityEngine.Random.Range(0, 10) < movingPiece.PieceState.pieceLevel - 2)
                    SpawnPiece(PieceType.Coins, destination);
            }
            else if (movingPiece.PieceState.pieceLevel > 2
                && movingPiece.PieceState.pieceType != PieceType.Coins
                && movingPiece.PieceState.pieceType != PieceType.Cash
                && movingPiece.PieceState.pieceType != PieceType.XP)
            {
                SpawnPiece(PieceType.Coins, destination);
            }
        }

        protected virtual bool CanProduceBubble()
        {
            var canProduce = !IsMaxLevel() && GameManager.Instance.PlayerLevel > 2 && GetPieceTypeInBoardCount(movingPiece.PieceState) < 3;
            GameManager.Log("Can produce Bubble " + canProduce + ", piece count " + GetPieceTypeInBoardCount(movingPiece.PieceState));
            return canProduce;
        }
        public int GetPieceTypeInBoardCount(PieceState pieceState)
        {
            var piecesCount = 0;
            foreach (var item in _tiles.Where(t => t != null && t.piece != null))
            {
                if (item.piece.PieceType == pieceState.pieceType && item.piece.PieceLevel == pieceState.pieceLevel) {
                    piecesCount++;
                }
            }
            return piecesCount;
        }

        private void TryToSpawnFromGenerator(BoardTile targetTile)
        {
            if (targetTile.piece == movingPiece
                            && movingPiece.IsGenerator
                            && movingPiece.IsSelected)
            {
                //Check tutorials
                if (!CanSpawInTuto()) return;

                var hasBoosterEnergy = HasActiveBooster(BoosterType.UnlimitedEnergy);
                var hasBoosterSpawners = HasActiveBooster(BoosterType.UnlimitedCharges);
                if (movingPiece.IsReadyToSpawn() || hasBoosterSpawners)
                {
                    if (movingPiece.genConfig.coolDown == 0 || mergeModel.energy > 0 || hasBoosterEnergy)
                    {
                        if (SpawnPiece(movingPiece.GetObjectToSpawnFromGenerator(_assistantController), targetTile) != null)
                        {
                            if (!hasBoosterSpawners) movingPiece.SpawnSuccess(); //Takes 1 energy
                            spawnSound.Play();
                            if (movingPiece.genConfig.coolDown > 0 && (!hasBoosterEnergy || movingPiece.IsExpirable)) 
                                energyController.EnergyChange(-1);
                            return;
                        }
                        else
                        {
                            GameManager.Instance.tutorialManager.StartTutorialStorage();
                        }
                    }
                    else
                    {
                        energyController.transform.parent.DOShakePosition(0.2f, 5).SetLink(energyController.gameObject);
                        //Show NOT Enough Energy
                        ShowEnergyPopup();
                    }
                }
                else
                {
                    GameManager.Instance.tutorialManager.StartTutorialRefill();
                }
                //Show something like Generator not ready
                movingPiece.transform.DOShakePosition(0.3f).SetLink(movingPiece.gameObject);
                spawnSoundFail.Play();
            }
        }

        private bool CanSpawInTuto()
        {
            if (!TutorialManager.IsTutoCompleted(TutorialKeys.MergeBoardDoSecondMerge))
            {
                var burgerCount = 0;
                //If there are 4 or more don't allow
                foreach (var t in _tiles)
                {
                    if (t.piece != null && t.piece.PieceType == PieceType.Burger && t.piece.PieceLevel == 0) burgerCount++;
                }
                if (burgerCount >= 4) return false;
            }
            else if (!TutorialManager.IsTutoCompleted(TutorialKeys.MergeBoardCollectXP))
            {
                return false;
            }
            return true;
        }

        private bool CanSelectInTuto()
        {
            if (GameManager.Instance.tutorialManager.IsTutorialRunning)
            {
                var activeTuto = GameManager.Instance.tutorialManager.ActiveTutorial();

                if (activeTuto.TutorialKey == TutorialKeys.MergeSelectBox || activeTuto.TutorialKey == TutorialKeys.MergeBoardSelectXP)
                {
                    if (movingPiece.PieceType != ((TutorialMergeBase)activeTuto).mergeStep.result.pieceType.pieceType)
                    {
                        return false;
                    }
                }
                else if (activeTuto.TutorialKey == TutorialKeys.Deliveries)
                {
                    return false;
                }
            }
            return true;
        }

        private bool HasActiveBooster(BoosterType booster)
        {
            if(booster == BoosterType.UnlimitedEnergy && mergeModel.energyBooster != null)
            {
                return mergeModel.energyBooster.isActive;
            }
            return _boardState != null && activeBoosters != null && activeBoosters.Find(b => b.boosterType == booster && b.endTime >= DateTime.Now) != null;
        }

        private void ShowPopupChain(MovingPiece piece, bool isMission = false)
        {
            _popupChainInstance = GameManager.Instance.PopupsManager.ShowPopup(popupChain).GetComponent<PopupMergeChain>();
            _popupChainInstance.Init(isMission ? movingPiece.missionConfig : null, OnCancelMission, piece);
        }
        private void ShowPopupGenerator(MovingPiece piece)
        {
            if (piece.IsExpirable)
                GameManager.Instance.PopupsManager.ShowChestPopup(piece.PieceDiscovery);
            else
                GameManager.Instance.PopupsManager.ShowGeneratorPopup(piece, ()=> SaveState(true));
        }
        
        private void ShowEnergyPopup()
        {
            _popupEnergyInstance = GameManager.Instance.PopupsManager.ShowPopup(popupEnergy).GetComponent<PopupEnergy>();
            _popupEnergyInstance.Init(OnEnergyBuySuccess);
        }
        private void OnEnergyBuySuccess(int amount)
        {
            energyController.EnergyChange(amount);
        }

        private void SelectTile()
        {
            if (isSpeedBoard) return;
            //Profiler.BeginSample("SelectTile");
            UnselectedTiles();
            bottomContainer.SetActive(true);
            movingPiece.Bounce();
            movingPiece.SelectTile();
            selectedPiece = movingPiece;

            txtInfoTitle.text = $"{movingPiece.PieceState.pieceType} level {movingPiece.PieceLevel + 1}";
            var isMaxLevel = IsMaxLevel();
            if (isMaxLevel)
            {
                txtInfoDescrip.text = "This item has reached <color=yellow>MAX</color> level";
            }
            else
            {
                txtInfoDescrip.text = "Merge to reach next level";
            }
            btnInfo.gameObject.SetActive(true);
            if (movingPiece.IsGenerator)
            {
                if (movingPiece.IsGeneratorWaiting)
                {
                    btnSkip.gameObject.SetActive(true);
                    btnSkip.button.onClick.AddListener(OnSkipGenerator);
                    btnSkip.Init(movingPiece.GeneratorSkipPrice);
                }
                btnInfo.onClick.AddListener(() => { ShowPopupGenerator(selectedPiece); });
                //Check Smart Generators Assistant
                if (mergeModel.assistants.HasAssistant(AssistantType.SmartGenerators) && movingPiece.genConfig.coolDown > 0)
                {
                    var assistantConfig = GameManager.Instance.gameConfig.GetAssistantConfig(AssistantType.SmartGenerators);
                    _assistantController.gameObject.SetActive(true);
                    _assistantController.Init(movingPiece);
                }
            }
            else {

                if (movingPiece.IsBooster)
                {
                    txtInfoTitle.text = movingPiece.boosterConfig.boosterTitle;
                    txtInfoDescrip.text = movingPiece.boosterConfig.boosterDescrip;
                    btnBooster.onClick.RemoveAllListeners();
                    RefreshBoosterState();

                    if (!selectedPiece.IsBoosterActive)
                    {
                        btnBooster.onClick.AddListener(OnActivateBooster);
                    }
                }
                else if (movingPiece.IsCollectable)
                {
                    btnClaim.gameObject.SetActive(true);
                    btnClaim.Init(movingPiece.reward);
                    txtInfoDescrip.text = isMaxLevel ?
                        $"<color=yellow>MAX</color> level reached, collect {movingPiece.reward.amount} {movingPiece.reward.rewardType} now" :
                        $"Merge to reach next level or collect {movingPiece.reward.amount} {movingPiece.reward.rewardType} now";
                }
                else if (movingPiece.PieceState.IsBubble)
                {
                    btnBuyBubble.gameObject.SetActive(true);
                    btnBuyBubble.button.onClick.AddListener(OnBuyBubble);
                    btnBuyBubble.Init(movingPiece.BubbleBuyPrice);
                    txtInfoDescrip.text = $"You can buy this <color=yellow>Bubble</color> now to get the item";
                }
                else if (TutorialManager.IsTutoCompleted(TutorialKeys.MergeDrinks) || GameManager.Instance.PlayerLevel > 2)
                {
                    btnRemove.gameObject.SetActive(movingPiece.PieceLevel < 2);
                    btnSell.gameObject.SetActive(movingPiece.PieceLevel >= 2);
                    if (movingPiece.PieceLevel < 2)
                    {
                        btnRemove.onClick.AddListener(() => { Debug.Log("Remove Piece"); selectedPiece.Remove(true); UnselectedTiles(); });
                    }
                    else
                    {
                        btnSell.button.onClick.RemoveAllListeners();
                        btnSell.button.onClick.AddListener(OnSell);
                        btnSell.Init(GetSellReward());
                    }
                }
                btnInfo.onClick.AddListener(() => { ShowPopupChain(selectedPiece); });
            }



            //Profiler.EndSample();
        }

        private void RefreshBoosterState()
        {
            if (selectedPiece.IsBooster)
            {
                if (!selectedPiece.boosterConfig.isActionable)
                {
                    btnBooster.gameObject.SetActive(false);
                    boosterActiveContainer.SetActive(false);
                    return;
                }
                btnBooster.gameObject.SetActive(!selectedPiece.IsBoosterActive);
                boosterActiveContainer.SetActive(selectedPiece.IsBoosterActive);
                if (selectedPiece.IsBoosterActive)
                {
                    txtBoosterTimeLeft.text = UIUtils.FormatTime(selectedPiece.BoosterTimeLeft);
                }
                else
                {
                    btnBooster.interactable = !HasActiveBooster(selectedPiece.boosterConfig.boosterType);
                    txtBoosterTime.text = UIUtils.FormatTime(selectedPiece.boosterConfig.duration);
                }
            }
        }

        private void SelectLockedTile(MovingPiece piece)
        {
            UnselectedTiles();
            piece.Bounce();
            //piece.SelectTile();
            bottomContainer.SetActive(true);
            txtInfoTitle.text = $"Locked item";
            txtInfoDescrip.text = "Merge with the same item to unlock it";
            btnUnlock.gameObject.SetActive(false);
        }
        private void SelectHiddenTile(MovingPiece piece)
        {
            UnselectedTiles();
            piece.Bounce();
            piece.SelectTile();
            selectedPiece = piece;
            bottomContainer.SetActive(true);
            txtInfoTitle.text = $"Locked storage";
            if (piece.PieceState.unlockLevel <= GameManager.Instance.PlayerLevel)
            {
                txtInfoDescrip.text = "This box can be unlocked";
                btnUnlock.gameObject.SetActive(true);
            }
            else
            {
                txtInfoDescrip.text = "Reach <color=yellow>Level " + piece.PieceState.unlockLevel + "</color> to unlock this box";
            }
            btnUnlock.button.onClick.RemoveAllListeners();
            btnUnlock.button.onClick.AddListener(OnUnlock);
            btnUnlock.Init(piece.UnlockCost, true);
        }

        private void UnselectedTiles()
        {
            btnInfo.onClick.RemoveAllListeners();
            btnSell.button.onClick.RemoveAllListeners();
            btnSkip.button.onClick.RemoveAllListeners();
            btnRemove.onClick.RemoveAllListeners();
            foreach (var t in _tiles)
            {
                t.Select(false);
            }
            selectedPiece = null;
            txtInfoTitle.text = "";
            btnClaim.gameObject.SetActive(false);
            btnRemove.gameObject.SetActive(false);
            btnSell.gameObject.SetActive(false);
            btnInfo.gameObject.SetActive(false);
            btnSkip.gameObject.SetActive(false);
            btnBuyBubble.gameObject.SetActive(false);
            btnUnlock.gameObject.SetActive(false);
            btnBooster.gameObject.SetActive(false);
            boosterActiveContainer.SetActive(false);
            _assistantController.gameObject.SetActive(false);
        }

        private void OnClaim()
        {
            //Give Reward
            if (selectedPiece == null || selectedPiece.reward == null)
            {
                TrackingManager.TrackMergeClaim(false);
            }
            else
            {
                TrackingManager.TrackMergeClaim(true, selectedPiece.reward);
                GameManager.Instance.AddRewardToPlayer(selectedPiece.reward, true);
                if (selectedPiece.reward.rewardType == RewardType.Coins)
                {
                    UpdateLockedTiles();
                }
                UIUtils.FlyingParticles(selectedPiece.reward.rewardType, selectedPiece.transform.position, Math.Min(selectedPiece.reward.amount, 10), null);
            }
            if (selectedPiece != null)
            {
                selectedPiece.Remove();
            }
            UnselectedTiles();
            pendingChanges = true;
        }
        private void OnSell()
        {
            //Debug.Log("Sell Piece");
            var reward = GetSellReward();
            GameManager.Instance.AddRewardToPlayer(reward, true);
            UIUtils.FlyingParticles(reward.rewardType, selectedPiece.transform.position, Math.Min(reward.amount, 10), null);
            TrackingManager.TrackMergeSell(true, selectedPiece.PieceType + selectedPiece.PieceLevel.ToString(), reward);
            selectedPiece.Remove(true); ;
            UnselectedTiles();
            pendingChanges = true;
        }

        private void OnActivateBooster()
        {
            GameManager.Instance.dailyTaskManager.OnUseBooster();
            var boosterType = selectedPiece.boosterConfig.boosterType;
            if (boosterType == BoosterType.AutoMerge)
            {
                _automergeBoosterPiece = selectedPiece.transform;
            }
            else if (boosterType == BoosterType.UnlimitedEnergy)
            {
                mergeModel.energyBooster = new BoosterState { 
                    boosterType = boosterType, 
                    endTime = DateTime.Now.AddSeconds(selectedPiece.boosterConfig.duration),
                    boosterPiece = selectedPiece.PieceDiscovery
                };
                selectedPiece.AddSmoke();
                selectedPiece.ScalesAndFlyToPosition(0.6f,energyController.boosterEnergyContainer.transform.position,movingTrail,energyController.ShowEnergyBoosterAnimated, true);
                UnselectedTiles();
                return;
            }

            GameManager.Instance.SetMusicBooster(true);

            selectedPiece.ActivateBooster(_boardState.AddBoosterState(selectedPiece.boosterConfig), pieceExtraInfo);
            selectedPiece.onBoosterEnd = () => {
                _boardState.RemoveBooster(boosterType);
                Debug.Log("ON BOOSTER END, activeBoosters left = " + activeBoosters.Count);
                if (activeBoosters.Count == 0) GameManager.Instance.SetMusicBooster(false);
                if (boosterType == BoosterType.UnlimitedCharges)
                    _tiles.FindAll(t => t.piece != null && t.piece.IsAvailableSpawner).ForEach(sp => sp.piece.RemoveBooster());
            };
            if (selectedPiece.boosterConfig.boosterType == BoosterType.UnlimitedCharges)
            {
                _tiles.FindAll(t => t.piece != null && t.piece.IsAvailableSpawner).ForEach(sp => sp.piece.ShowBoosterFX(pieceExtraInfo));
            }
            RefreshBoosterState();
        }

        private void OnUnlock()
        {
            var cost = selectedPiece.UnlockCost;
            if (GameManager.Instance.HasEnoughCurrency(cost))
            {
                GameManager.TryToSpend(cost);
                selectedPiece.Unlock();
                btnUnlock.gameObject.SetActive(false);
                movingPiece = selectedPiece;
                var reward = new RewardData(selectedPiece.PieceDiscovery);
                GameManager.Instance.PopupsManager.ShowPopupPurchase(new List<RewardData>() { reward });
                SelectTile();
            }
            else
            {
                GameManager.Instance.PopupsManager.ShowPopupYesNo(
                    "Not Enough Coins",
                    "You don't have enough Coins. Do you want to go to the Shop?",
                    PopupManager.PopupType.yesno,
                    () => {
                        GameManager.Instance.ShowShop(true);
                        GameManager.Instance.TryToCreateOffer(0.2f);
                    }, 
                    () => {
                        GameManager.Instance.TryToCreateOffer(0.2f);
                    });

            }
        }

        private void OnSkipGenerator()
        {
            var cost = selectedPiece.GeneratorSkipPrice;
            if (GameManager.Instance.HasEnoughCurrency(cost))
            {
                GameManager.TryToSpend(cost);
                selectedPiece.ResetGeneratorCooldown();
                btnSkip.gameObject.SetActive(false);
            }
            else
            {
                GameManager.Instance.PopupsManager.ShowPopupYesNo(
                    "Not Enough Currency",
                    "You don't have enough gems. Do you want to go to the Shop?",
                    PopupManager.PopupType.yesno,
                    () => { GameManager.Instance.ShowShop(true); });
            }
        }

        private void OnBuyBubble()
        {
            var cost = selectedPiece.BubbleBuyPrice;
            if (GameManager.Instance.HasEnoughCurrency(cost, true))
            {
                GameManager.TryToSpend(cost);
                var fromTile = selectedPiece.GetBoardTileParent();
                var piece = selectedPiece.PieceDiscovery;
                selectedPiece.transform.DOKill(); //Cancel the destroy callback
                selectedPiece.Remove();
                movingPiece = SpawnPiece(piece, fromTile, false, false);
                SelectTile();
            }
        }

        private RewardData GetSellReward()
        {
            var coinsAmount = Mathf.Sqrt(RewardsGranter.GetCoinsValue(selectedPiece.PieceState.pieceDiscovery()));
            return new RewardData(RewardType.Coins, (int)coinsAmount);
            //return new RewardData(RewardType.Coins, (int)Math.Pow(selectedPiece.PieceState.pieceLevel - 1, 2));
        }



        bool CheckPieceInMapMission(MovingPiece piece) {
            if (piece.PieceState.locked) return false;
            return MergeManager.IsPieceRequestedInMission(piece);
        }
        bool CheckPieceInStorage(MovingPiece piece) {
            for (var i = 0; i < mergeModel.storage.Count; i++)
            {
                var storeItem = mergeModel.storage[i];
                if (storeItem != null && storeItem.pType == piece.PieceType && storeItem.Lvl == piece.PieceLevel) return true;
            }
            return false;
        }

        public virtual void LazyUpdate(bool increaseTime = true)
        {
            if (mergeModel == null) return;

            energyController.LazyUpdate();

            if (selectedPiece != null && btnSkip.gameObject.activeSelf && selectedPiece.IsGenerator)
            {
                btnSkip.Init(selectedPiece.GeneratorSkipPrice);
            }
            if (selectedPiece != null && selectedPiece.IsBoosterActive)
            {
                txtBoosterTimeLeft.text = UIUtils.FormatTime(selectedPiece.BoosterTimeLeft);
            }
            if (HasActiveBooster(BoosterType.AutoMerge))
            {
                _timeElapsed = 0;
                AutoMerge();
            }
            else if (seqSugest == null && !isDragin && !isSpeedBoard && _timeElapsed > 1)
            {
                AnimateSugestion();
                _timeElapsed = 0;
            }
            //(PlayerPrefs.GetInt("mapmissionsug") == 0 &&
            else if (GameManager.Instance.PlayerLevel > 1 && mergeModel.mapMissionsNew.Count > 0)
            {
                if (PlayerData.MissionsCount == 5 && PlayerPrefs.GetInt("mapmissionsug") == 0)
                {
                    PlayerPrefs.SetInt("mapmissionsug", 1);
                    GameManager.Instance.mergeLowerBar.btnMergeMissions.ShowToolTip("Here you have more rewarding missions");
                }
                else if (PlayerData.MissionsCount > 5 && PlayerPrefs.GetInt("mapmissionsug") == 1)
                {
                    PlayerPrefs.SetInt("mapmissionsug", 2);
                    GameManager.Instance.mergeLowerBar.btnMergeMissions.ShowToolTip("Try to complete these missions");
                }
            }
            LazySave();

            if (increaseTime) _timeElapsed++;
        }

        void AnimateSugestion()
        {
            //Look for Map Missions
            foreach (var t in _tiles)
            {
                if (t.piece != null && CheckPieceInMapMission(t.piece) && !CheckPieceInStorage(t.piece) 
                    && !t.piece.PieceState.IsBubble && !t.piece.PieceState.hidden)
                {
                    CreateSuggestionAnimationSequence(btnStorage.icon.transform, t.piece.transform);
                    if (GameManager.Instance.PlayerLevel < 4 && _handPointer == null)
                    {
                        CreatePointerHandToStorage(t.piece);
                    }
                    return;
                }
            }
            //Look for Requests
            foreach (var t in _tiles)
            {
                if (t.piece == null || t.piece.PieceState.hidden || t.piece.PieceState.locked || t.piece.PieceState.IsBubble) 
                    continue;
                var missionPiece = CheckPieceInMission(t.piece);
                if (missionPiece)
                {
                    CreateSuggestionAnimationSequence(missionPiece.transform, t.piece.transform);
                    return;
                }
            }

            foreach (var t in _tiles)
            {
                if (t.piece == null || t.piece.PieceState.hidden || t.piece.PieceState.locked || t.piece.PieceState.IsBubble) 
                    continue;
                var p = FindSamePieces(t.piece.PieceState);

                if (p.amount > 1 && CanLevelUp(p))
                {
                    //Debug.Log("Threre are " + p.amount + " pieces " + t.piece.PieceState.pieceType);
                    var otherPiece = FindClosestTileSameType(t.piece);
                    if (!otherPiece.piece.PieceState.IsBubble)
                    {
                        CreateSuggestionAnimationSequence(otherPiece.piece.transform, t.piece.transform);
                        break;
                    }
                }
            }
        }

        void CreateSuggestionAnimationSequence(Transform A, Transform B)
        {
            seqSugest = DOTween.Sequence();
            var direction = (A.position - B.position).normalized;
            seqSugest.Insert(0.4f, B.DOMove(B.position + direction * 15, 0.3f).SetEase(Ease.OutCubic));
            seqSugest.Insert(0.4f, A.transform.DOMove(A.transform.position + direction * -15, 0.3f).SetEase(Ease.OutCubic))
                .SetLoops(-1, LoopType.Yoyo);
        }
        void AutoMerge()
        {
            if (_isAutoMerging)
            {
                //Debug.Log("Trying to automerge but it is alredy merging");
                return;
            }
            foreach (var t in _tiles)
            {
                if (t.piece == null || t.piece.PieceState.hidden) continue;
                var p = FindSamePieces(t.piece.PieceState);

                if (p.amount > 1 && CanLevelUp(p))
                {
                    //Debug.Log("AUTO MERGE Threre are " + p.amount + " pieces " + t.piece.PieceState.pieceType);
                    var otherPiece = FindClosestTileSameType(t.piece);
                    DoAutoMerge(otherPiece.piece, t);
                    return;
                }
            }
        }
        void DoAutoMerge(MovingPiece A, BoardTile B)
        {
            _isAutoMerging = true;
            var sound = Instantiate(boosterAutomergeSound, transform);
            Destroy(sound, 1);
            var p1 = Instantiate(movingParticleBlue, transform);
            p1.transform.position = _automergeBoosterPiece.position;
            p1.transform.DOMove(A.transform.position, 0.2f).SetLink(p1.gameObject);
            var p2 = Instantiate(movingParticleBlue, transform);
            p2.transform.position = _automergeBoosterPiece.position;
            p2.transform.DOMove(B.piece.transform.position, 0.2f).SetLink(p2.gameObject);
            var seq = DOTween.Sequence();
            A.isMoving = true;
            B.piece.isMoving = true;
            //var destination = (A.transform.position - B.transform.position)/2;
            seq.Insert(0.1f, A.transform.DOMove(B.transform.position, 0.5f).SetEase(Ease.InBack));
            seq.OnComplete(() => {
                Destroy(p1);
                Destroy(p2);
                _isAutoMerging = false;
                movingPiece = A;
                A = LevelUpPieceToTile(B);
                A.PlayMergeAnim();
                LazyUpdate();
                CheckMissions();
            });
        }

        

        private void LazySave()
        {
            if((DateTime.Now - time).TotalSeconds > 20 && pendingChanges)
            {
                time = DateTime.Now;
                SaveState();
            }
        }
        internal MovingPiece FindPieceOfType(PieceState targetPiece, bool skipLocked = false)
        {
            return _tiles.Find(t => t.piece != null && t.piece.PieceState != null
                    && t.piece.PieceState.pieceType == targetPiece.pieceType
                    && (!skipLocked || (!t.piece.PieceState.locked && !t.piece.PieceState.hidden))
                    && t.piece.PieceState.pieceLevel == targetPiece.pieceLevel)?.piece;
        }

        internal PieceAmount FindSamePieces(PieceState targetPiece, bool onlyActive = false)
        {
            var pAmount = new PieceAmount();
            pAmount.pieceType = targetPiece;
            foreach (var t in _tiles)
            {
                if (t.piece == null || t.piece.PieceState == null || t.piece.PieceState.hidden || t.piece.isMoving) continue;
                if (t.piece.PieceState.pieceType == targetPiece.pieceType
                    && (!onlyActive || t.piece.IsAvailable)
                    && t.piece.PieceState.pieceLevel == targetPiece.pieceLevel)
                {
                    pAmount.amount++;
                }
            }
            return pAmount;
        }
        internal PieceAmount FindSamePieces2(PieceState targetPiece, bool onlyActive = false)
        {
            var pAmount = new PieceAmount();
            pAmount.pieceType = targetPiece;
            for (var i = _tiles.Count - _boardState.sizeX; i > 0; i--)
            {
                var t = _tiles[i];
                if (t.piece == null || t.piece.PieceState == null || t.piece.PieceState.hidden || t.piece.isMoving) continue;
                if (t.piece.PieceState.pieceType == targetPiece.pieceType
                    && (!onlyActive || t.piece.IsAvailable)
                    && t.piece.PieceState.pieceLevel == targetPiece.pieceLevel)
                {
                    pAmount.amount++;
                }
            }
            return pAmount;
        }
        internal MovingPiece FindClosetsPieceOfTheSameType(MovingPiece targetPiece)
        {
            var distance = 1110f;
            MovingPiece result = null;
            foreach (var t in _tiles)
            {
                if (t.piece != null && t.piece.PieceState != null && t.piece != targetPiece
                    && t.piece.PieceState.pieceType == targetPiece.PieceType
                    && t.piece.PieceState.pieceLevel == targetPiece.PieceLevel)
                {
                    var tempDistance = Math.Abs(Vector3.Magnitude(targetPiece.transform.position - t.transform.position));
                    if (tempDistance < distance)
                    {
                        distance = tempDistance;
                        result = t.piece;
                    }
                }
            }
            return result;
        }


        public MovingPiece SelectGenerator(PieceState GeneratorType = null)
        {
            var generator = GetGeneratorPiece(GeneratorType);
            if (generator == null) return null;

            movingPiece = generator;
            SelectTile();
            return generator;
        }

        public MovingPiece GetGeneratorPiece(PieceState GeneratorType = null)
        {
            foreach (var t in _tiles)
            {
                if (t.piece != null && t.piece.IsGenerator)
                {
                    if (GeneratorType != null)
                    {
                        if (t.piece.PieceState.pieceType != GeneratorType.pieceType || t.piece.PieceState.pieceLevel != GeneratorType.pieceLevel)
                        {
                            continue;
                        }
                    }
                    return t.piece;
                }
            }
            return null;
        }

        private BoardTile GetTargetTile()
        {
            
            BoardTile targetTile = null;
            var dist = 10000f;
            foreach (var t in _tiles)
            {
                var d = Vector3.Distance(t.transform.position, Input.mousePosition);
                //var d = Vector3.Distance(t.transform.position, movingPiece.transform.position);
                if (d < dist)
                {
                    dist = d;
                    targetTile = t;
                }
            }
            
            return targetTile;
        }
        protected virtual MovingPiece GetMissionTile()
        {
            MovingPiece mission = null;
            var dist = 100f;
            foreach (var t in _activeMissionsPieces)
            {
                var d = Vector3.Distance(t.transform.position, Input.mousePosition);
                //var d = Vector3.Distance(t.transform.position, movingPiece.transform.position);
                if (d < dist)
                {
                    dist = d;
                    mission = t;
                }
            }
            return mission;
        }
        private BoardTile FindClosestTile()
        {
            var pos = movingPiece.transform;
            BoardTile closestTile = null;
            var dist = 10000f;
            foreach (var t in _tiles)
            {
                if (t.piece != null && t.piece != movingPiece) continue;
                var d = Vector3.Distance(t.transform.position, pos.position);
                if (d < dist)
                {
                    dist = d;
                    closestTile = t;
                }
            }
            return closestTile;
        }
        private BoardTile FindClosestTileSameType(MovingPiece origin)
        {
            var pos = origin.transform;
            BoardTile closestTile = null;
            var dist = 10000f;
            foreach (var t in _tiles)
            {
                //Skip Hidden
                if (t.piece != null && t.piece != origin && !t.piece.PieceState.hidden
                    && t.piece.PieceState.pieceLevel == origin.PieceState.pieceLevel
                    && t.piece.PieceState.pieceType == origin.PieceState.pieceType)
                    
                {
                    var d = Vector3.Distance(t.transform.position, pos.position);
                    if (d < dist)
                    {
                        dist = d;
                        closestTile = t;
                    }
                }
            }
            return closestTile;
        }
        protected List<BoardTile> FindAllTilesSameType(MovingPiece origin)
        {
            var pos = origin.transform;
            List<BoardTile> tiles = new List<BoardTile>();
            
            foreach (var t in _tiles)
            {
                //Skip Hidden
                if (t.piece != null && t.piece != origin && !t.piece.PieceState.hidden
                    && t.piece.PieceState.pieceLevel == origin.PieceState.pieceLevel
                    && t.piece.PieceState.pieceType == origin.PieceState.pieceType)
                    
                {
                    tiles.Add(t);
                    
                }
            }
            return tiles;
        }
        private BoardTile FindClosestTileEmpty(Transform pos)
        {
            BoardTile closestTile = null;
            var dist = 10000f;
            foreach (var t in _tiles)
            {
                if (t.piece != null) continue;
                var d = Vector3.Distance(t.transform.position, pos.position);
                if (d < dist)
                {
                    dist = d;
                    closestTile = t;
                }
            }
            return closestTile;
        }

        public void LoadBoard(BoardState boardState, UnityAction onClose)
        {
            _boardState = boardState;
            CreateBoard();
            _onClose = onClose;
            bottomContainer.SetActive(false);
            btnClaim.button.onClick.RemoveAllListeners();
            btnClaim.button.onClick.AddListener(OnClaim);
            btnClaim.gameObject.SetActive(false);
            energyController.txtEnergy.text = mergeModel.energy.ToString();
            if(mergeModel.energyBooster != null && mergeModel.energyBooster.isActive)
            {
                energyController.ShowEnergyBooster();
            }
        }

        public void CreateBoard()
        {
            time = DateTime.Now;
            var w = _boardState.sizeX * 100 + _boardState.sizeX * 2 + 7;
            var h = _boardState.sizeY * 100 + _boardState.sizeY * 2 + 7;
            GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            ClearBoard();

            _tiles = new List<BoardTile>();
            _activeMissionsPieces = new List<MovingPiece>();
            for (var j = 0; j < _boardState.sizeY; j++)
            {
                for (var i = 0; i < _boardState.sizeX; i++)
                {
                    var tile = Instantiate(tilePrefab, boardContainer);
                    if ((i + j) % 2 == 0)
                    {
                        tile.SetColorBoardNormal(_boardState.GetColorTileNormal());
                    }
                    else
                    {
                        tile.SetColorBoardNormal(_boardState.GetColorTileDark());
                    }
                    tile.name = "tile" + j + "-" + i;
                    _tiles.Add(tile);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(boardContainer.GetComponent<RectTransform>());

        }
        public void FillBoard()
        {
            if (_boardState.pieces != null && _boardState.pieces.Count > 0)
            {
                for (var i = 0; i < _boardState.pieces.Count; i++)
                {
                    var p = _boardState.pieces[i];
                    if (p == null || p.pieceType == PieceType.None) continue;
                    CreatePiece(p, _tiles[i]);
                }
            }
            StartCoroutine(FillMissions());
        }
        
        public void CreatePiece(PieceState piecestate, BoardTile tile)
        {
            if (tile == null) return;
            if(piecestate.Interactable) mergeModel.AddDiscovery(piecestate);
            var pieces = mergeConfig.GetPiecePrefab(piecestate);
            var p = Instantiate(pieces, piecesContainer).GetComponent<MovingPiece>();
            p.transform.position = tile.transform.position;
            p.SetConfig(piecestate);
            tile.SetContent(p, OnClick);
            if (piecestate.locked)
            {
                //Debug.Log("Setting piece locked");
                p.SetGreyState(true,GreyMaterial);
                p.SetExtraInfo(false, false, false, true, pieceExtraInfo);
            }
        }

        private List<BoardTile> GetAllEmptyTiles()
        {
            var l = new List<BoardTile>();
            foreach (var t in _tiles)
            {
                if (t.piece == null) l.Add(t);
            }
            return l;
        }

#region Missions
        IEnumerator FillMissions()
        {
            if (_boardState.activeMissions.missions.Count == 0)
            {
                Debug.LogWarning("No missions in board state");
                yield return null;
            }
            else
            {
                for (var i = 0; i < _boardState.activeMissions.missions.Count; i++)
                {
                    InitMission(_boardState.activeMissions.missions[i]);
                    yield return new WaitForSeconds(0.3f);
                }
            }
            CheckMissionReady();
            CheckPiecesInMissions();
        }
        public void InitMission(MergeMissionConfig mission)
        {
            var piece = mergeConfig.GetPieceDefOfType(mission.piece.pieceType);
            var levelIndex = Math.Min(mission.piece.pieceLevel, piece.levels.Count);
            if (levelIndex >= piece.levels.Count) return; //Prevents loading broken missions

            var tile = Instantiate(MissionPrefab, missionsContainer).GetComponent<MissionTile>();
            tile.SetMissionNormal();
            var rewardFame = mission.rewardInstant.Find(r => r.rewardType == RewardType.FamePoints);
            tile.ShowTrophy(rewardFame != null, rewardFame != null ? rewardFame.amount:0);
            //tile.ShowMission();
            
            var m = Instantiate(piece.levels[levelIndex], tile.pieceContainer).GetComponent<MovingPiece>();
            //m.transform.position = tile.transform.position;
            
            m.SetMissionConfig(mission, tile);
            m.SetTileParent(tile);
            _activeMissionsPieces.Add(m);
            m.OnClick = OnClickMission;
        }
        public void InitMission(ActiveMissionCloudSave mission)
        {
            var mc = new MergeMissionConfig();
            //Rarity
            //Look for avilable generators 
            var rarity = 0f;
            foreach (var t in _tiles)
            {
                if (t.piece && t.piece.IsGenerator && !t.piece.PieceState.hidden)
                {
                    //Take pieces that can be spawned
                    for (var i = 0; i < t.piece.genConfig.piecesChances.Count; i++)
                    { 
                        if(mission.pieceType == t.piece.genConfig.piecesChances[i].pieceType)
                        {
                            rarity = t.piece.genConfig.GetItemRarity(i);
                            break;
                        }
                    }
                }
            }
            mc.GenerateMission(new PieceState(mission.pieceType, mission.pieceLevel), mergeConfig.GetChain(mission.pieceType).levels.Count-1, rarity);
            InitMission(mc);
        }
        protected void CheckMissions()
        {
            CheckMissionReady();
            CheckPiecesInMissions();
            GameManager.Instance.UpdateMapMissions();
            if (mergeModel.HasAnyMissionReady() && PlayerData.MapMissionsCount == 0 && !GameManager.Instance.tutorialManager.IsTutorialRunning)
            {
                GameManager.Instance.tutorialManager.StartTutorialDeliveries();
            }
        }
        protected virtual void CompleteOrder(MovingPiece mission)
        {
            GameManager.Instance.dailyTaskManager.OnCompleteOrder();
            //Debug.Log("MISION CUMPLIDA!");
            TrackingManager.TrackMissionComplete(mission.PieceType, mission.PieceLevel, mission.missionConfig.rewardPiece.pieceType, mission.missionConfig.rewardPiece.pieceLevel);
            movingPiece.Remove();
            missionCompleteSound.Play();
            _lastMissionItem = new PieceDiscovery(mission.PieceState);
            mission.missionTile.CompleteMission(mission,AddMission);
            //Spawn Reward Piece
            SpawnPiece(mission.missionConfig.rewardPiece, mission.GetMissionTileParent());
            //Give Reward Instant
            if(mission.missionConfig.rewardInstant != null && mission.missionConfig.rewardInstant.Count > 0)
            {
                foreach (var r in mission.missionConfig.rewardInstant)
                {
                    //Debug.Log("Adding reward with particles");
                    GameManager.Instance.AddRewardWithParticles(r, mission.transform, null, true);
                }
            }
            RemoveMission(mission);
            UnselectedTiles();
            UpdateLockedTiles();

            //Try to create new map mission delivery
            if (PlayerData.MissionsCount >= TutorialManager.OrdersCompletedForFirstMapMission)
            {
                if(PlayerData.MissionsCount == TutorialManager.OrdersCompletedForFirstMapMission || UnityEngine.Random.Range(0, 100) < 75)
                {
                    if (MergeManager.TryToAddMapMission())
                    {
                        GameManager.Instance.mergeLowerBar.btnMergeMissions.ShowToolTip("New Delivery");
                    }
                    if(PlayerData.MissionsCount == TutorialManager.OrdersCompletedForFirstMapMission)
                    {
                        GameManager.Instance.tutorialManager.StartTutorialDeliveries();
                    }
                }
            }
        }

        private void UpdateLockedTiles()
        {
            foreach(var p in _tiles)
            {
                if (p.piece != null && p.piece.PieceState.hidden) p.piece.UpdateHiddenState();
            }
        }

        private void OnCancelMission(MovingPiece mission)
        {
            mission.missionTile.DiscardMission(mission, AddMission);
            RemoveMission(mission);
        }
        protected void RemoveMission(MovingPiece mission)
        {
            _activeMissionsPieces.Remove(mission);
            _boardState.activeMissions.Remove(mission.missionConfig);
        }
        public void AddMission()
        {
            StartCoroutine(AddMission2());
        }
        IEnumerator AddMission2()
        {
            yield return new WaitForSeconds(0.2f);
            if(GameManager.Instance.PlayerLevel == 1)
            {
                //Create hardcoded Mission - Patatas 2
                var mi = new MergeMissionConfig();
                mi.GenerateMission(new PieceState(PieceType.Patatas, 2), 0,0);
                InitMission(mi);
                _boardState.activeMissions.Add(mi);
                yield break;
            }
            //Get Valid Missions
            List<MergeMissionConfig> validMissions = new List<MergeMissionConfig>();
            WeightedList<MergeMissionConfig> wValidMissions = new WeightedList<MergeMissionConfig>();
            //Possible chains to spawn
            List<PieceType> possibleChains = new List<PieceType>();
            //Look for available generators 
            foreach (var t in _tiles)
            {
                if(t.piece && t.piece.IsGenerator && !t.piece.PieceState.hidden && t.piece.genConfig.coolDown > 0)
                {
                    //Take pieces that can be spawned
                    for(var i = 0; i< t.piece.genConfig.piecesChances.Count; i++)
                    {
                        var pType = t.piece.genConfig.piecesChances[i].pieceType;
                        //Exclude Pieces
                        if(pType == PieceType.RouleteTicketCommon || pType == PieceType.RouleteTicketSpecial)
                        {
                            continue;
                        }
                        possibleChains.Add(pType);
                        var maxLevel = mergeModel.MaxLevelDiscovered(pType)+1;
                        var chainCount = mergeConfig.GetChain(pType).levels.Count-1;
                        //Create Valid Missions
                        if (maxLevel == 1){//Chain not discovered
                            var mi = new MergeMissionConfig();
                            mi.GenerateMission(new PieceState(pType, 2), chainCount, t.piece.genConfig.GetItemRarity(i));
                            //if (!ExistingMission(mi)) validMissions.Add(mi);
                            if (!ExistingMission(mi)) wValidMissions.Add(mi, t.piece.genConfig.piecesChances[i].weight);
                        }
                        else
                        {
                            //for(var j = maxLevel - 2; j<=Mathf.Min(maxLevel, chainCount); j++)//Changed to add more varaity
                            for(var j = 1; j<=Mathf.Min(maxLevel, chainCount); j++)
                            {
                                var mi = new MergeMissionConfig();
                                mi.GenerateMission(new PieceState(pType, j), chainCount, t.piece.genConfig.GetItemRarity(i));
                                //if (!ExistingMission(mi)) validMissions.Add(mi);
                                if (!ExistingMission(mi)) wValidMissions.Add(mi, t.piece.genConfig.piecesChances[i].weight);
                            }
                        }

                    }

                }
            }

            while (_boardState.activeMissions.missions.Count < GameManager.Instance.PlayerLevel + 1 
                && _boardState.activeMissions.missions.Count < boardConfig.maxVisibleMissions
                && wValidMissions.Count > 0) 
            {
                var m = wValidMissions.GetWieghted();
                //Check if it's the same item or chain already exists
                if (possibleChains.Count > _boardState.activeMissions.missions.Count + 1) 
                {
                    if(!IsPieceTypeInMissions(m.item.piece.pieceType) && _lastMissionItem.pType != m.item.piece.pieceType)
                    {
                        _boardState.activeMissions.Add(m.item);
                        InitMission(m.item);
                    } 
                }
                else
                {
                    if (_lastMissionItem.pType != m.item.piece.pieceType && _lastMissionItem.Lvl != m.item.piece.pieceLevel)
                    {
                        _boardState.activeMissions.Add(m.item);
                        InitMission(m.item);
                    }
                }
                wValidMissions.Remove(m);
                
            }
            CheckMissionReady();
        }

        public void AddOrder(PieceType pType, int level)
        {
            var mi = new MergeMissionConfig();
            var chainCount = mergeConfig.GetChain(pType).levels.Count - 1;
            mergeConfig.GetGeneratorOfPiece(pType);
            mi.GenerateMission(new PieceState(pType, level), 0, 0);// chainCount, mergeConfig.GetGeneratorOfPiece(pType).GetItemRarity(0));
            _boardState.activeMissions.Add(mi);
            InitMission(mi);
        }

        public void AddForcedMission(MergeMissionConfig m)
        {
            _boardState.activeMissions.Add(m);
            InitMission(m);
        }

        private bool IsPieceTypeInMissions(PieceType pType)
        {
            foreach(var p in _boardState.activeMissions.missions)
            {
                if (p.pieceType == pType) return true;
            }
            return false;
        }
        private bool ExistingMission(MergeMissionConfig mission)
        {
            foreach(var m in _boardState.activeMissions.missions)
            {
                if(m.pieceType == mission.piece.pieceType && m.pieceLevel == mission.piece.pieceLevel)
                {
                    return true;
                }
            }
            return false;
        }
        internal MovingPiece FindMissionOfType(PieceState targetPiece)
        {
            foreach (var t in _activeMissionsPieces)
            {
                if (t != null
                    && t.PieceState.pieceType == targetPiece.pieceType
                    && t.PieceState.pieceLevel == targetPiece.pieceLevel)
                    return t;
            }
            return null;
        }
        protected virtual void CheckPiecesInMissions()
        {
            foreach (var t in _tiles)
            {
                if (t.piece == null || t.piece.IsBooster) continue;
                bool bMap = false;
                bool bMission = false;
                bool bMaxed = false;
                if (!t.piece.IsGenerator)
                {
                    if (CheckPieceInMission(t.piece))
                    {
                        //t.SetStateMissionReady();
                        bMission = true;
                    }
                    if (CheckPieceInMapMission(t.piece))
                    {
                        //t.SetStateMapMissionReady();
                        bMap = true;
                        if (PlayerPrefs.GetInt("storagetooltip") == 0)
                        {
                            PlayerPrefs.SetInt("storagetooltip", 1);
                            btnStorage.ShowToolTip("Put the requested mission items here");
                        }
                    }
                    bMaxed = IsMaxLevel(t.piece);
                }
                else
                {
                    t.SetStateNormal();
                }
                if (t.piece != null && !t.piece.PieceState.locked && !t.piece.PieceState.hidden && !t.piece.PieceState.IsBubble)
                {
                    t.piece.SetExtraInfo(bMap, bMission, bMaxed, false, pieceExtraInfo);
                    if (GameManager.Instance.PlayerLevel < 3 && bMap && _handPointer == null && !mergeModel.isItemInStorage(t.piece.PieceDiscovery))
                    {
                        CreatePointerHandToStorage(t.piece);
                    }
                }
            }
        }
        protected MovingPiece CheckPieceInMission(MovingPiece piece)
        {
            if (piece.PieceState.locked) return null;
            foreach (var mission in _activeMissionsPieces)
            {
                if(mission.missionCompleted) continue;
                if (MergeBoardManager.PieceStatesMatch(mission, piece))
                {
                    return mission;
                }
            }
            return null;
        }
        List<MovingPiece> CheckMissionReady()
        {
            var missionsReady = new List<MovingPiece>();
            foreach (var mission in _activeMissionsPieces)
            {
                mission.GetMissionTileParent().SetMissionNormal();
                
                foreach (var tile in _tiles)
                {
                    if (tile.piece == null || tile.piece.PieceState.hidden || tile.piece.PieceState.locked) continue;
                    
                    if (MergeBoardManager.PieceStatesMatch(mission,tile.piece))
                    {
                        missionsReady.Add(mission);
                        mission.GetMissionTileParent().SetMissionReady();
                        break;
                    } 
                }
            }
            return missionsReady;
        }
#endregion

#region SpawnPieces
        public bool SpawnPiece(PieceType piecetype, BoardTile fromTile)
        {
            var tile = FindClosestTileEmpty(fromTile.transform);
            if (tile == null) return false;
            var pieces = mergeConfig.GetPiecesDefOfType(new List<PieceType> { piecetype });
            var p = Instantiate(pieces[0].levels[0], piecesContainer).GetComponent<MovingPiece>();
            p.transform.position = fromTile.transform.position;
            tile.SetContent(p, OnClick, true, true,movingTrail,isSpeedBoard);
            p.SetConfig(piecetype, 0);
            mergeModel.AddDiscovery(new PieceState(p));
            pendingChanges = true;
            return true;
        }
        public MovingPiece SpawnPiece(PieceDiscovery piece, BoardTile fromTile, bool isBubble = false, bool animate = true)
        {
            var tile = FindClosestTileEmpty(fromTile.transform);
            if (tile == null) return null;
            var p = Instantiate(mergeConfig.GetPiecePrefab(piece), piecesContainer).GetComponent<MovingPiece>();
            p.transform.position = fromTile.transform.position;
            tile.SetContent(p, OnClick, animate, true,movingTrail,isSpeedBoard);
            p.SetConfig(piece);
            if (isBubble)
            {
                p.SetBubble(pieceExtraInfo);
                p.transform.DOLocalMoveZ(1, 1).SetDelay(60).OnComplete(() => { p.Remove(true); });
            }
            mergeModel.AddDiscovery(new PieceState(p));
            pendingChanges = true;
            return p;
        }
        public MovingPiece SpawnPiece(PieceState pieceState, MissionTile fromTile)
        {
            var tile = FindClosestTileEmpty(fromTile.transform);
            if (tile == null) return null;
            //Debug.Log("Spawning piece " + pieceState.pieceType + ", level " + pieceState.pieceLevel);
            var pieces = mergeConfig.GetPiecesDefOfType(new List<PieceType> { pieceState.pieceType });
            
            var p = Instantiate(pieces[0].levels[pieceState.pieceLevel], piecesContainer).GetComponent<MovingPiece>();
            p.transform.position = fromTile.transform.position;
            tile.SetContent(p, OnClick, true, true, movingTrail, isSpeedBoard);
            p.SetConfig(pieceState);
            pendingChanges = true;
            return p;
        }
        public bool SpawnPieceFromStorage(PieceState pieceState,Transform fromPos)
        {
            var tile = FindClosestTileEmpty(_tiles[0].transform);
            if (tile == null) return false;
            btnStorage.OnAddItem(mergeModel.storage.Count, mergeModel.storageSlots);
            var pieces = mergeConfig.GetPiecesDefOfType(new List<PieceType> { pieceState.pieceType });
            var p = Instantiate(pieces[0].levels[Mathf.Min(pieceState.pieceLevel, pieces[0].levels.Count - 1)], piecesContainer).GetComponent<MovingPiece>();
            p.transform.position = fromPos.position;
            tile.SetContent(p, OnClick, true, true, movingTrail);
            p.SetConfig(pieceState);
            pendingChanges = true;
            return true;
        }
#endregion

        public MovingPiece LevelUpPieceToTile(BoardTile targetTile)
        {
            PieceType pieceType = movingPiece.PieceType;
            var newLevel = movingPiece.PieceLevel + 1;
            DestroyImmediate(targetTile.piece.gameObject);
            DestroyImmediate(movingPiece.gameObject);
            var a = Instantiate(mergeConfig.GetDefByPieceType(pieceType).levels[newLevel], piecesContainer).GetComponent<MovingPiece>();
            targetTile.SetContent(a, OnClick, false,false,null,isSpeedBoard);
            a.SetConfig(pieceType, newLevel);
            pendingChanges = true;
            mergeModel.AddDiscovery(new PieceState(pieceType,newLevel));
            return a;
        }
        public MovingPiece LevelUpPiece(BoardTile targetTile, int toLevel = 0)
        {
            PieceType pieceType = targetTile.piece.PieceType;
            var newLevel = toLevel == 0 ? targetTile.piece.PieceLevel + 1 : toLevel;
            DestroyImmediate(targetTile.piece.gameObject);
            var a = Instantiate(mergeConfig.GetDefByPieceType(pieceType).levels[newLevel], piecesContainer).GetComponent<MovingPiece>();
            targetTile.SetContent(a, OnClick, false);
            a.SetConfig(pieceType, newLevel);
            pendingChanges = true;
            mergeModel.AddDiscovery(new PieceState(pieceType,newLevel));
            return a;
        }
        public MovingPiece LevelDownPieceTile(BoardTile targetTile)
        {
            //Todo VFX cut
            PieceType pieceType = targetTile.piece.PieceType;
            var newLevel = targetTile.piece.PieceLevel - 1;
            DestroyImmediate(targetTile.piece.gameObject);
            DestroyImmediate(movingPiece.gameObject);
            var a = Instantiate(mergeConfig.GetDefByPieceType(pieceType).levels[newLevel], piecesContainer).GetComponent<MovingPiece>();
            targetTile.SetContent(a, OnClick, false);
            a.SetConfig(pieceType, newLevel);
            pendingChanges = true;
            return a;
        }

        public bool CanLevelUp(MovingPiece other)
        {
            return movingPiece.PieceLevel < mergeConfig.GetPieceTypeLevelsCount(movingPiece.PieceType) && movingPiece.PieceLevel == other.PieceLevel;
        }
        public bool CanLevelUp(PieceAmount piece)
        {
            return piece.Level < mergeConfig.GetPieceTypeLevelsCount(piece.Type);
        }
        public bool IsMaxLevel()
        {
            return movingPiece.PieceLevel == mergeConfig.GetPieceTypeLevelsCount(movingPiece.PieceType);
        }
        public bool IsMaxLevel(MovingPiece p)
        {
            return p.PieceLevel == mergeConfig.GetPieceTypeLevelsCount(p.PieceType);
        }

        public void SaveState(bool forceSave = false)
        {
            if (!pendingChanges && !forceSave) return;
            if (_boardState == null || _tiles == null || _tiles.Count == 0) return;
            //_boardState.SaveState(_tiles, forceSave);
            _boardState.UpdatePieces(_tiles);
            mergeModel.SaveEconomy(_boardState);
            pendingChanges = false;
            storageChanged = true;
        }

        public virtual void CloseBoard()
        {
            SaveState(true);
            ClearBoard();
            Hide();
            _onClose?.Invoke();
        }
        protected void ClearBoard()
        {
            Debug.Log("ClearBoard");
            foreach (Transform t in boardContainer) { Destroy(t.gameObject); }
            foreach (Transform t in piecesContainer) { Destroy(t.gameObject); }
            foreach (Transform t in missionsContainer) { Destroy(t.gameObject); }
        }
        void OnApplicationFocus(bool hasFocus)
        {
            if(!hasFocus) SaveState();
        }
        void OnApplicationQuit()
        {
            SaveState();
        }
        void OnApplicationPause()
        {
            SaveState();
        }

        

        //For tutorials
        public GameObject GetGenerator()
        {
            for(var i = 0; i < piecesContainer.childCount; i++)
            {
                var mp = piecesContainer.GetChild(i).GetComponent<MovingPiece>();
                if (mp != null && mp.IsGenerator)
                {
                    return piecesContainer.GetChild(i).gameObject;
                }
            }
            Debug.Log("Generator NOT FOUND");
            return null;
        }

        public void PlaySound()
        {
            boosterAutomergeSound.Play();
        }

        public GameObject handPrefab;
        private GameObject _handPointer;
        private TutorialHand tutorialHand;

        private void CreatePointerHandToStorage(MovingPiece targetPiece)
        {
            if (handPrefab == null || targetPiece == null) return;

            _handPointer = Instantiate(handPrefab, transform);
            tutorialHand = _handPointer.GetComponentInChildren<TutorialHand>();
            DOVirtual.DelayedCall(0.3f,() =>
            {
                var pos = targetPiece.transform.position - Vector3.up * 30 + Vector3.right * 10;
                tutorialHand.Init(pos, false, true, btnStorage.GetCenteredPosition());
            }).SetLink(_handPointer);
        }
    } 
}
