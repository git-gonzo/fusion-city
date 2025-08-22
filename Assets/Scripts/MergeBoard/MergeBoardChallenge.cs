
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.MergeBoard
{
    public class MergeBoardChallenge : MergeBoardController
    {
        //public Transform grid;
        public Transform interactionContent;
        public int moveSpeedSeconds;
        public int gameDuration;
        public CronoPieceController cronoPieceController;
        public TextMeshProUGUI textScore;
        public Transform scoreTransform;
        public ButtonWithPrice btnStartGame;
        public ButtonWithPrice btnBuySecons;
        public ButtonWithPrice btnBuyBooster;
        public Button btnClaimYesterday;
        public Button btnBuySeconsAd;
        public Button btnContinueToGame;
        public Button btnSuperAutomerge;
        public Button btnShowFinalRewards;
        public Button btnClose;
        public GameObject startScreen;
        public GameObject boostersScreen;
        public GameObject scoreContainer;
        public GameObject timeLeftContainer;
        public GameObject yesterdayContainer;
        public GameObject GameplayContent;
        public CanvasGroup endScreen;
        public TextMeshProUGUI txtDayEndsIn;
        public TextMeshProUGUI txtTimeEnd;
        public TextMeshProUGUI txtExtraPoints;
        public TextMeshProUGUI txtFinalPoints;
        public TextMeshProUGUI txtExtraPointsAmount;
        public TextMeshProUGUI txtFinalPointsAmount;
        public TextMeshProUGUI txtSecondsBeforePlay;
        public TextMeshProUGUI txtBoostersBeforePlay;
        public TextMeshProUGUI txtBoostersAmount;
        public TextMeshProUGUI txtTitleLeaderboardYesterday;
        public TextMeshProUGUI txtTitleLeaderboardToday;
        public CanvasGroup rewardsScreen;
        public Transform endRewardsContainer;
        public Transform YesterdayRewardContainer;
        public RewardItemUniversal rewardPrefab;
        [SerializeField] private FloatingPoints _pointsAmount;

        private CronoPieceController _crono;
        private List<List<BoardTile>> rows;
        private List<PieceType> candidates;
        private int rowCounter = 0;
        private int score = 0;
        private int finalScore = 0;
        private Vector3 initialPosition = Vector3.zero;
        Vector2 lastPosition;
        private List<Vector2Int> _chainsAppearanceCount = new List<Vector2Int>();
        private List<PieceDiscovery> _possibleRewards;
        private List<RewardItemUniversal> _rewardsOffered;
        private int boosterCount;
        private int secondsCount;
        private int daySecondsLeft;

        private SpeedBoardManager speedManager => GameManager.Instance.speedBoardManager;
        private int TodayTries => speedManager.speedBoardTries.todayTries;
        private List<int> playPrices => speedManager.playPrices;

        private void Start()
        {
            //initialPosition = interactionContent.transform.localPosition;
        }
        public void FirstInitSpeedBoard(BoardState boardState, UnityAction onClose)
        {
            txtTitleLeaderboardToday.gameObject.SetActive(false);
            txtTitleLeaderboardYesterday.gameObject.SetActive(false);
            yesterdayContainer.SetActive(false);
            GameplayContent.SetActive(false);
            initialPosition = Vector3.zero;
            _boardState = boardState;
            transform.DOScale(0.5f, 0);
            isSpeedBoard = true;
            _onClose = onClose;
            InitSpeedBoard();
            afterAction = CheckGaps;
            timeLeftContainer.SetActive(false);
            btnStartGame.gameObject.SetActive(false);
            btnClaimYesterday.gameObject.SetActive(false);
            btnClaimYesterday.onClick.RemoveAllListeners();
            btnClaimYesterday.onClick.AddListener(speedManager.ClaimYesterday);
            btnStartGame.button.onClick.RemoveAllListeners();
            btnStartGame.button.onClick.AddListener(ShowBuyBoosters);
        }

        public void InitSpeedBoard()
        {
            rows = new List<List<BoardTile>>();
            canRun = false;
            endScreen.gameObject.SetActive(false);
            rewardsScreen.gameObject.SetActive(false);
            startScreen.SetActive(true);
            btnClose.gameObject.SetActive(true);
            GameplayContent.SetActive(false);
            boostersScreen.SetActive(false);
            bottomContainer.gameObject.SetActive(false);
            interactionContent.gameObject.SetActive(false);
            scoreContainer.SetActive(false);
            ClearBoard();
            CreateSpeedBoard();
            

            boosterCount = 1;
        }

        public void ShowYesterdayReward(bool value, int pos = 0)
        {
            txtTitleLeaderboardToday.gameObject.SetActive(!value);
            txtTitleLeaderboardYesterday.gameObject.SetActive(value);
            yesterdayContainer.SetActive(value && pos >= 0);
            btnClaimYesterday.gameObject.SetActive(value);
            timeLeftContainer.SetActive(!value);
            btnStartGame.gameObject.SetActive(!value);
            if (value && pos >= 0)
            {
                var rewardData = new RewardData(RewardType.FamePoints, (11 - pos)*2);
                var reward = Instantiate(rewardPrefab, YesterdayRewardContainer);
                reward.InitReward(rewardData, GameManager.Instance.topBar);
                reward.SetStateGreen();
            }
        }

        public void ShowStartScreen()
        {
            ShowYesterdayReward(false);
            btnStartGame.SetPrice(playPrices[TodayTries], RewardType.Gems);
        }

        public void ShowBuyBoosters()
        {
            if (!GameManager.TryToSpend(btnStartGame.cost)) return;

            startScreen.SetActive(false);
            boostersScreen.SetActive(true);
            btnContinueToGame.onClick.RemoveAllListeners();
            btnBuyBooster.button.onClick.RemoveAllListeners();
            btnBuySecons.button.onClick.RemoveAllListeners();
            btnBuySeconsAd.onClick.RemoveAllListeners();

            btnContinueToGame.onClick.AddListener(StartGame);
            btnBuySeconsAd.onClick.AddListener(BuyTimeAd);
            btnBuyBooster.button.onClick.AddListener(BuyBooster);
            btnBuySecons.button.onClick.AddListener(BuyTime);
            btnBuyBooster.SetPrice(20, RewardType.Gems);
            btnBuySecons.SetPrice(20, RewardType.Gems);
            btnBuySecons.button.interactable = true;
            btnBuyBooster.button.interactable = true;
            btnBuySeconsAd.interactable = true;

            secondsCount = gameDuration;
            UpdateBoosters();
        }

        private void BuyBooster()
        {
            if (GameManager.TryToSpend(btnBuyBooster.cost))
            {
                boosterCount++;
                btnBuyBooster.button.interactable = false;
                UpdateBoosters();
            }
        }
        private void BuyTime()
        {
            if (GameManager.TryToSpend(btnBuySecons.cost))
            {
                GameManager.AnimateFormatedNumber(txtSecondsBeforePlay, secondsCount, 15, false);
                secondsCount += 15;
                btnBuySecons.button.interactable = false;
            }
        }
        private void BuyTimeAd()
        {
            btnBuySeconsAd.interactable = false;
            GameManager.Instance.PlayVideoAd(AdComplete);
        }
        private void AdComplete()
        {
            GameManager.AnimateFormatedNumber(txtSecondsBeforePlay, secondsCount, 10,true);
            secondsCount += 10;
        }

        private void UpdateBoosters()
        {
            txtSecondsBeforePlay.text = secondsCount.ToString();
            txtBoostersBeforePlay.text = boosterCount.ToString();
            //GameManager.Instance.AnimateFormatedNumber(txtSecondsBeforePlay
        }

        private void StartGame()
        {
            boostersScreen.SetActive(false);
            interactionContent.gameObject.SetActive(true);
            GameplayContent.SetActive(true);

            StartCoroutine(StartSpeedBoard());
        }

        IEnumerator StartSpeedBoard()
        {
            while (!Globals.Instance.gameLoaded)
            {
                yield return Globals.Instance.gameLoaded;
            }
            yield return new WaitForEndOfFrame();
            canRun = true;
            LoadSpeedBoard(speedManager.GetRandomSetPieces());
            txtBoostersAmount.text = boosterCount.ToString();
            FillFirstRow();
            AnimateSpeedBoard();
        }


        public void AnimateSpeedBoard(int delay = 0)
        {
            if (canRun)
            {
                interactionContent.transform.DOLocalMoveY(interactionContent.transform.localPosition.y + 100, moveSpeedSeconds).SetDelay(delay).SetEase(Ease.Linear)
                    .OnComplete(() => AnimateSpeedBoard(0));
                CreateNewRow();
            }
        }
        public void LoadSpeedBoard(List<PieceType> pieces )
        {
            candidates = pieces;
            CreateNewRow();
            AddCrono();
            AddMissions(4);
            startScreen.SetActive(false);
            btnClose.gameObject.SetActive(false);
            bottomContainer.gameObject.SetActive(true);
            scoreContainer.SetActive(true);
            _possibleRewards = new List<PieceDiscovery>();
            btnSuperAutomerge.onClick.RemoveAllListeners();
            btnSuperAutomerge.onClick.AddListener(SuperAutomerge);
            btnSuperAutomerge.interactable = true;
        }

        private void AddCrono()
        {
            _crono = Instantiate(cronoPieceController, missionsContainer);
            _crono.Init(secondsCount, GameEnd);
            _crono.Run();
        }

        private void AddMissions(int missionsAmount)
        {
            for(var i = 0; i < missionsAmount; i++)
            {
                CreateMission();
            }
        }

        private void CreateMission()
        {
            var mi = new MergeMissionConfig();
            var added = false;
            var safe = 100;
            while (!added && safe > 0)
            {
                var candidate = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                if (_activeMissionsPieces.Find(m => m.PieceType == candidate) == null)
                {
                    mi.GenerateSpeedBoardMission(new PieceState(candidate, GetLevelPiece((int)candidate)));
                    InitMission(mi);
                    added = true;
                    return;
                }
                safe--;
            }
            CheckPiecesInMissions();
        }

        private int GetLevelPiece(int pieceType)
        {
            var newPiece = new Vector2Int(pieceType, 1);
            foreach (var piece in _chainsAppearanceCount)
            {
                if(piece.x == pieceType)
                {
                    _chainsAppearanceCount.Remove(piece);
                    newPiece = new Vector2Int(piece.x, piece.y+1);
                    _chainsAppearanceCount.Add(newPiece);
                    return newPiece.y;
                }
            }
            _chainsAppearanceCount.Add(newPiece);
            return newPiece.y;
        }

        private void FillFirstRow() //With pieces
        {
            var r = rows[rows.Count-2];
            //for (var i = _tiles.Count-1; i > _tiles.Count - _boardState.sizeX; i--)
            for (var i = 0; i < r.Count ; i++)
            {
                var tile = r[i];   
                var piece = new PieceState(candidates[UnityEngine.Random.Range(0, candidates.Count)],0);
                var prefab = mergeConfig.GetPiecePrefab(piece);
                var p = Instantiate(prefab, piecesContainer).GetComponent<MovingPiece>();
                p.transform.position = tile.transform.position;
                //DOVirtual.DelayedCall(0.05f, () => { p.transform.position = tile.transform.position; });
                p.SetConfig(piece);
                tile.SetContent(p, OnClick);
            }

        }
        private void CreateNewRow() //With pieces
        {
            lastPosition.x = 60;
            var r = new List<BoardTile>();
            for (var i = 0; i < _boardState.sizeX; i++)
            {
                var tile = Instantiate(tilePrefab, boardContainer);
                tile.transform.localPosition = lastPosition;
                lastPosition.x += 100;
                if ((i + rowCounter) % 2 == 1)
                {
                    tile.SetColorBoardNormal(_boardState.GetColorTileNormal());
                }
                else
                {
                    tile.SetColorBoardNormal(_boardState.GetColorTileDark());
                }
                r.Add(tile);
                //tile.name = "tile" + _tiles.Count;
                //Fill with random item
                var pieceType = UnityEngine.Random.Range(0, candidates.Count);
                var maxLevel = Mathf.Max(0,UnityEngine.Random.Range(0, GetMaxLevelOfNewPiece((int)candidates[pieceType]) -1));
                var piece = new PieceState(candidates[pieceType], maxLevel);
                CreatePiece(piece, tile);
                //Debug.Log("create piece Level" + maxLevel);
            }
            rows.Add(r);
            lastPosition.y -= 100;
            rowCounter++;
            if(rowCounter>2)
            {
                RemoveRow();
            }
            CheckGaps();
        }

        private int GetMaxLevelOfNewPiece(int pieceType)
        {
            if(_chainsAppearanceCount.Find(a => a.x ==  pieceType) == null) 
                return 0;
            return _chainsAppearanceCount.Find(a => a.x == pieceType).y;
        }

        private void RemoveRow()
        {
            foreach (var t in rows[0])
            {
                if (t != null)
                {
                    _tiles.Remove(t);
                    if (t.piece != null) Destroy(t.piece.gameObject);
                    Destroy(t.gameObject);
                }
            }
            rows.RemoveAt(0);
        }

        public void CreatePiece(PieceState piecestate, BoardTile tile)
        {
            if (tile == null) return;
            _tiles.Add(tile);
            var pieces = mergeConfig.GetPiecePrefab(piecestate);
            var p = Instantiate(pieces, piecesContainer).GetComponent<MovingPiece>();
            DOVirtual.DelayedCall(0.05f,()=> { p.transform.position = tile.transform.position; });
            p.SetConfig(piecestate);
            tile.SetContent(p, OnClick);
        }

        private void CheckGaps()
        {
            for (int j = 0; j < _boardState.sizeY; j++)
            {
                for (var i = _tiles.Count - _boardState.sizeX; i >= 0; i--)
                {
                    var tile = _tiles[i];
                    if (tile != null && tile.piece == null) //Check piece on the tile above it
                    {
                        CheckPieceAbove(i);
                    }
                }
            }
        }

        private void CheckPieceAbove(int i)
        {
            var index = i - _boardState.sizeX;
            if (index >= 0)
            {
                var tileAbove = _tiles[index];
                if(tileAbove == null) return;
                if (tileAbove.piece != null)
                {
                    _tiles[i].SetContent(tileAbove.piece, OnClick, true,false,movingTrail,isSpeedBoard);
                }
            }
        }
        int currentSeconds = 0;
        protected override void Update()
        {
            base.Update();  
            if (currentSeconds != DateTime.Now.Second)
            {
                currentSeconds = DateTime.Now.Second;
                LazyUpdate();
            }
        }

        public void LazyUpdate()
        {
            if (txtDayEndsIn != null && txtDayEndsIn.gameObject.activeSelf)
            {
                txtDayEndsIn.text = UIUtils.FormatTime(daySecondsLeft);
                daySecondsLeft--;
            }
        }
        public override void LazyUpdate(bool increaseTime = true)
        {
            //base.LazyUpdate(true);
        }

        public async void GetDaySecondsLeft()
        {
            var endTime = await Leaderboards.GetLeaderboardTimeLeft(LeaderboardID.speedBoardLeaderboard);
            var remaining = endTime - DateTime.UtcNow;
            daySecondsLeft = (int)remaining.TotalSeconds;
        }

        public override void CloseBoard()
        {
            //onPieceMoved -= CheckGaps;
            interactionContent.transform.DOKill();
            ClearBoard();
            Hide();
            _onClose?.Invoke();
        }
        protected override void CheckPiecesInMissions()
        {
            foreach (var t in _tiles)
            {
                if (t.piece == null || t.piece.IsBooster) continue;
                if (!t.piece.IsGenerator)
                {
                    var mission = CheckPieceInMission(t.piece);
                    if (mission)
                    {
                        var mPiece = t.piece;
                        AutocompleteMission(mPiece,mission);
                        t.piece = null;
                    }
                }
            }
        }

        private void AutocompleteMission(MovingPiece p,MovingPiece mission)
        {
            mission.missionCompleted = true;
            p.transform.parent = transform;
            var duration = 0.3f + (p.transform.position - mission.transform.position).magnitude * 0.0005f;
            AddPossibleReward(mission.PieceDiscovery);
            p.FlyToPosition(duration, mission.transform.position, movingTrail, () =>
            {
                p.transform.DOKill();
                Destroy(p.gameObject);
                missionCompleteSound.Play();
                mission.missionTile.CompleteSpeedMission(mission, CreateMission);
                Instantiate(_pointsAmount, transform).ShowPlusPoints(mission.transform.position, p.PieceScore);
                RemoveMission(mission);
                //Give Score
                AddScore(p.PieceScore);
            });
        }

        private void AddPossibleReward(PieceDiscovery r)
        {
            var prevReward = _possibleRewards.Find(p => p.pType == r.pType);
            if(prevReward == null) 
            {
                _possibleRewards.Add(r);
            }
            else
            {
                prevReward.Lvl = Mathf.Max(prevReward.Lvl, r.Lvl);
            }
        }

        protected override void CheckSpecialReward(BoardTile destination)
        {
            
        }
        protected override bool CanProduceBubble()
        {
            return false;
        }
        protected override MovingPiece GetMissionTile()
        {
            return null;
        }

        public void CreateSpeedBoard()
        {
            _chainsAppearanceCount = new List<Vector2Int>();
            score = 0;
            textScore.text = "0";
            rowCounter = 0;
            if (initialPosition == Vector3.zero)
            {
                initialPosition = interactionContent.transform.position;
                Debug.Log("Initial position set");
            }
            interactionContent.transform.localPosition = Vector3.zero;// initialPosition;
            var w = _boardState.sizeX * 100 + _boardState.sizeX * 2 + 7;
            var h = _boardState.sizeY * 100 + _boardState.sizeY * 2 + 7;
            GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            lastPosition = new Vector2(0,-60);
            _tiles = new List<BoardTile>();
            
            for (var j = 0; j < _boardState.sizeY; j++)
            {
                var r = new List<BoardTile>();
                lastPosition.x = 60;
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
                    tile.transform.localPosition = lastPosition;
                    lastPosition.x += 100;
                    r.Add(tile);
                }
                rows.Add(r);
                lastPosition.y -= 100;
            }
            //LayoutRebuilder.ForceRebuildLayoutImmediate(boardContainer.GetComponent<RectTransform>());
            _activeMissionsPieces = new List<MovingPiece>();
        }

        private void AddScore(int value)
        {
            //Debug.Log("AddScore " + value);
            GameManager.Instance.AnimateText(textScore, score, value, false);
            score += value;
        }

        void SuperAutomerge() // Only for speed merge for now
        {
            boosterCount--;
            txtBoostersAmount.text = boosterCount.ToString();
            if(boosterCount == 0)
            {
                btnSuperAutomerge.interactable = false;
            }
            var te = interactionContent.transform.DOPause();
            Debug.Log("Tween paused " + te);
            var rounds = 0;
            List<PieceState> pieceTypes;
            //do{
                pieceTypes = new List<PieceState>();
                //for (var i = 0; i < _tiles.Count - _boardState.sizeX; i++)
                for (var i = _tiles.Count - _boardState.sizeX; i > 0; i--)
                {
                    var t = _tiles[i];
                    if (t.piece == null || t.piece.PieceState.hidden) continue;
                    var p = FindSamePieces2(t.piece.PieceState);

                    if (p.amount > 1 && CanLevelUp(p))
                    {
                        var otherPieces = FindAllTilesSameType(t.piece);
                        if (otherPieces == null || otherPieces.Count < 1) continue;
                        if (pieceTypes.Find(a => a.pieceType == p.pieceType.pieceType && a.pieceLevel == p.Level) == null)
                        {
                            pieceTypes.Add(p.pieceType);
                            Debug.Log("Adding piece Type " + p.pieceType.pieceType.ToString());
                        }
                        //Debug.Log($"SUPER AUTO MERGE Round {rounds} Threre are {p.amount}/{otherPieces.Count} pieces {t.piece.PieceState.pieceType} index {i}");
                        //DoSuperAutoMerge(otherPieces);
                        //return;
                    }
                }
                //Debug.Log("Automerging " + rounds + " - piecesTypes " +  pieceTypes.Count);
                SuperAutomergeThisPieces(pieceTypes);
                rounds++;
            //} while (pieceTypes.Count > 0 && rounds < 2);
            interactionContent.transform.DOPlay();
        }

        //async Task SuperAutomergeThisPieces(List<PieceState> pieceTypes)
        void SuperAutomergeThisPieces(List<PieceState> pieceTypes)
        {
            foreach (var pieceType in pieceTypes)
            {
                //Debug.Log("Checkgin Automerge for " + pieceType.pieceType.ToString());
                var tilesToMerge = new List<BoardTile>();
                for (var i = _tiles.Count - _boardState.sizeX; i > 0; i--)
                {
                    var t = _tiles[i];
                    if (t.piece == null || t.piece.PieceState.hidden) continue;
                    if (t.piece.IsSameStateAs(pieceType))
                    {
                        tilesToMerge.Add(t);
                        //Debug.Log("Adding tile " + i + " piece Type " + t.piece.PieceType.ToString());
                    }
                }
                if (tilesToMerge.Count > 1)
                {
                    Debug.Log("Automerging " + tilesToMerge.Count + " piece Type " + pieceType.pieceType.ToString());
                    DoSuperAutoMerge(tilesToMerge);
                    //await Task.Delay(500);
                }
            }
            //return Task.CompletedTask;
        }

        void DoSuperAutoMerge(List<BoardTile> tiles)
        {
            _isAutoMerging = true;
            var sound = Instantiate(boosterAutomergeSound, transform);
            Destroy(sound, 1);
            //Select final tile target
            var finalTile = tiles[(int)Mathf.Floor(tiles.Count / 2f)];
            var seq = DOTween.Sequence();
            for (var i = 0; i < tiles.Count; i++)
            {
                var t = tiles[i];
                if (t != finalTile && t.piece != null)
                {
                    t.piece.isMoving = true;
                    var particle = Instantiate(movingParticleBlue, transform);
                    particle.transform.position = t.piece.transform.position;
                    seq.Insert(0 + 0.05f * i, t.piece.transform.DOMove(finalTile.piece.transform.position, 0.5f).SetEase(Ease.InBack).OnComplete(() => { Destroy(t.piece.gameObject); }));
                    seq.Insert(0 + 0.05f * i, particle.transform.DOMove(finalTile.piece.transform.position, 0.5f).SetEase(Ease.InBack).OnComplete(() => { Destroy(particle); }));
                }
            }
            
            
            seq.OnComplete(() => {
                _isAutoMerging = false;
                var finalLevel = finalTile.piece.PieceLevel;
                if (tiles.Count < 3) finalLevel++;
                else if (tiles.Count < 5) finalLevel += 2;
                else if (tiles.Count < 9) finalLevel += 3;
                else finalLevel += 4;
                LevelUpPiece(finalTile, finalLevel);
                finalTile.piece.PlayMergeAnim();
                LazyUpdate();
                CheckMissions();
                CheckPiecesInMissions();
                CheckGaps();
            });
        }

        private void GameEnd()
        {
            bottomContainer.gameObject.SetActive(false);
            btnShowFinalRewards.gameObject.SetActive(false);
            canRun = false;
            interactionContent.transform.DOKill();
            CleanBoardAndGivePoints();
            ShowFinalText();
            Debug.Log("Game END! score="+score+", final score="+finalScore);
            Leaderboards.SendScore(LeaderboardID.speedBoardLeaderboard, finalScore);
        }

        private void CleanBoardAndGivePoints()
        {
            finalScore = score;
            txtExtraPointsAmount.text = "0";
            txtFinalPointsAmount.text = finalScore.ToString() ;
            var bonusPoints = 0;
            for (var i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].Select(false);
                var t = _tiles[i];
                if (t.piece != null)
                {
                    finalScore += (int)Mathf.Ceil(t.piece.PieceScore * 0.5f);
                    
                    DOVirtual.DelayedCall(UnityEngine.Random.Range(0f, 3)+2.5f, () =>
                    {
                        var parcialScore = (int)Mathf.Ceil(t.piece.PieceScore * 0.5f);
                        t.piece.ClearAndFlyTo(1, scoreTransform.position, movingTrail,
                            () => {
                                GameManager.Instance.AnimateText(txtFinalPointsAmount, score, parcialScore, false, true);
                                AddScore((int)Mathf.Ceil(t.piece.PieceScore * 0.5f));
                                scoreTransform.DOKill();
                                scoreTransform.DOScale(1,0);
                                scoreTransform.DOPunchScale(Vector3.one*0.4f,0.4f,3);
                            });
                        GameManager.Instance.AnimateText(txtExtraPointsAmount, bonusPoints, parcialScore, false,true);
                        bonusPoints += parcialScore;
                    });
                }
            }
            DOVirtual.DelayedCall(5.5f, () => {

                GameManager.Instance.speedBoardManager.LoadLeaderboard();
                GetDaySecondsLeft();
                btnShowFinalRewards.gameObject.SetActive(true);
                btnShowFinalRewards.transform.DOScale(0, 0);
                btnShowFinalRewards.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
            });
        }

        private void ShowFinalText()
        {
            txtExtraPoints.gameObject.SetActive(false);
            txtExtraPointsAmount.gameObject.SetActive(false);
            endScreen.gameObject.SetActive(true);
            endScreen.DOFade(0, 0);
            endScreen.DOFade(1, 0.3f);
            var seq = DOTween.Sequence();
            txtTimeEnd.gameObject.SetActive(true);
            seq.Insert(0,txtTimeEnd.transform.DOLocalMoveY(txtTimeEnd.transform.position.y + 100, 0.3f).From());
            seq.Insert(2, txtFinalPoints.transform.DOLocalMoveY(txtFinalPoints.transform.position.y + 100, 0.3f).From()
                .OnStart(() => { txtTimeEnd.gameObject.SetActive(false); txtFinalPoints.gameObject.SetActive(true); })) ;
            seq.Insert(2.2f,txtFinalPointsAmount.transform.DOLocalMoveY(txtFinalPointsAmount.transform.position.y - 100, 0.3f).From()
                .OnStart(() => { txtFinalPointsAmount.gameObject.SetActive(true); }));
            seq.Insert(3, txtExtraPoints.transform.DOLocalMoveY(txtExtraPoints.transform.position.y + 100, 0.3f).From()
                .OnStart(() => { txtTimeEnd.gameObject.SetActive(false); txtExtraPoints.gameObject.SetActive(true); })) ;
            seq.Insert(3.2f,txtExtraPointsAmount.transform.DOLocalMoveY(txtExtraPointsAmount.transform.position.y - 100, 0.3f).From()
                .OnStart(() => { txtExtraPointsAmount.gameObject.SetActive(true); }));
        }

        public void ShowEndRewards() //Called from Unity
        {
            endScreen.DOFade(0, 0.2f).OnComplete(()=>endScreen.gameObject.SetActive(false));
            rewardsScreen.gameObject.SetActive(true);
            rewardsScreen.DOFade(0, 0);
            rewardsScreen.DOFade(1, 0.3f).SetDelay(0.2f);
            CreateFinalReward();
            GameManager.Instance.ShowMapBtnStorageOnly(true);
        }

        private void CreateFinalReward() 
        {
            GameManager.RemoveChildren(endRewardsContainer.gameObject);
            _rewardsOffered = new List<RewardItemUniversal>();
            for (int i = 0; i < 3; i++)
            {
                if (_possibleRewards.Count == 0) continue;
                var index = UnityEngine.Random.Range(0, _possibleRewards.Count);
                var piece = mergeConfig.GetPieceDefOfType(_possibleRewards[index].pType);
                if (_possibleRewards[index].Lvl >= piece.levels.Count) continue; //Prevents loading broken missions
                else if (_possibleRewards[index].Lvl + 1 < piece.levels.Count) _possibleRewards[index].Lvl++; //One More level
                var reward = Instantiate(rewardPrefab, endRewardsContainer);
                reward.InitReward(_possibleRewards[index]);
                reward.SetStateBlue();
                reward.amountText.gameObject.SetActive(false);
                reward.AddButton(RewardChosen);
                reward.transform.DOScale(Vector3.zero, 0);
                _rewardsOffered.Add(reward);
                _possibleRewards.RemoveAt(index);
                reward.gameObject.SetActive(false);
            }
            //Animate Rewards
            for (int i = 0; i < _rewardsOffered.Count; i++)
            {
                var r = _rewardsOffered[i];
                r.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.3f + i*0.2f).OnStart(()=> r.gameObject.SetActive(true));
                r.transform.DORotate(Vector3.back * 40, 0.3f).From().SetEase(Ease.OutBack).SetDelay(0.3f + i * 0.2f);
            }
        }

        private void RewardChosen()
        {
            for (int i = 0; i < _rewardsOffered.Count; i++)
            {
                _rewardsOffered[i].RemoveButton();
                _rewardsOffered[i].transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutBack).SetDelay(i * 0.1f);
            }
            DOVirtual.DelayedCall(0.5f, GiveRewardAndBack);
        }

        private void GiveRewardAndBack()
        {
            if(_possibleRewards.Count >= 2)
            {
                CreateFinalReward();
            }
            else
            {
                speedManager.OnGamePlayed();
                InitSpeedBoard();
                ShowStartScreen();
                GameManager.RemoveChildren(missionsContainer.gameObject);
                rewardsScreen.DOFade(0, 0.2f).OnComplete(() => {
                    rewardsScreen.gameObject.SetActive(false); 
                    GameManager.Instance.ShowMapBtnStorageOnly(false);
                });
            }
        }
    }
}
