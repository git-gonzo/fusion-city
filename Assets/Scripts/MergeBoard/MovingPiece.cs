using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI; 

namespace Assets.Scripts.MergeBoard
{
    public class MovingPiece : MonoBehaviour, IPointerDownHandler
    {
        public RewardData reward;
        public BoosterConfig boosterConfig = new BoosterConfig();
        public GeneratorConfig genConfig;
        [ShowIf("genConfig")] public Image energyIcon;
        [ShowIf("genConfig")] public Image bubble;
        public GameObject imgContent;
        public GameObject clock;
        public GameObject generatorLevelContainer;
        public TextMeshProUGUI txtLevel; 
        public Image clockProgress;
        [HideInInspector] public MergeMissionConfig missionConfig;
        [HideInInspector] public MissionTile missionTile;
        [HideInInspector] public bool missionCompleted; //Used in speed board
        public bool isMoving;
        private BoardTile _tileParentBoard;
        private MissionTile _tileParentMission;
        private PieceState _config;

        public Action<MovingPiece> OnClick;
        public PieceType PieceType => _config.pieceType;
        public int PieceLevel => _config.pieceLevel;
        public bool IsGenerator => genConfig != null;
        public bool IsAvailable => !PieceState.hidden && !PieceState.locked;
        public bool IsAvailableSpawner => genConfig != null && genConfig.coolDown > 0 && IsAvailable;
        public bool IsExpirable => genConfig != null && genConfig.coolDown <= 0;
        public bool IsCollectable => reward != null && reward.amount > 0;
        public bool IsBooster => boosterConfig.boosterType != BoosterType.None;
        public bool IsBoosterActive => _config!=null && _config.booster!=null && _config.booster.boosterType != BoosterType.None && _config.booster.endTime > DateTime.Now;
        public bool IsGeneratorWaiting => genConfig != null && _config.generator != null && _config.generator.capLeft == 0 && _config.generator.nextTime > DateTime.Now;
        public RewardData GeneratorSkipPrice => new RewardData(RewardType.Gems, (int)(_config.generator.nextTime - DateTime.Now).TotalMinutes); 
        public RewardData BubbleBuyPrice => new RewardData(RewardType.Gems, (PieceLevel - 3) * 10); 
        public PieceState PieceState => _config;
        public PieceDiscovery PieceDiscovery => new PieceDiscovery(_config);
        public int BoosterTimeLeft => (int)((_config.booster.endTime - DateTime.Now).TotalSeconds);
        public int PieceScore => (PieceLevel + 1) * (PieceLevel + 1);

        public RewardData UnlockCost => new RewardData(RewardType.Coins, PieceState.unlockPrice);
        public void SetTileParent(BoardTile tile) { _tileParentBoard = tile; }
        public void SetTileParent(MissionTile tile) { _tileParentMission = tile; }
        public MissionTile GetMissionTileParent() { return _tileParentMission; }
        public BoardTile GetBoardTileParent() { return _tileParentBoard; }
        public MergeConfig mergeConfig => GameManager.Instance.MergeManager.boardController.mergeConfig;
        public MergeBoardModel mergeModel => GameManager.Instance.MergeManager.mergeModel;
        private Tween energyTween;
        public bool generatorReady;
        public Action onBoosterEnd;
        private GameObject _unlockBox;
        private PieceExtraInfo _pieceExtraInfoBack;
        private PieceExtraInfo _pieceExtraInfo;
        private bool _isBoostered;
        private bool isSpeedBoard;
        private Vector3 offset => Vector3.up * (isSpeedBoard ? (9 - GameManager.Instance.speedBoardManager.speedBoardController.moveSpeedSeconds) * 3 : 0);
        private void Awake()
        {
            foreach(var im in GetComponentsInChildren<Image>())
            {
                im.raycastTarget = false;
            }
        }
        private void Start()
        {
            if (IsGenerator)
            {
                bubble.transform.DOScale(0, 0);
                bubble.DOFade(0, 0);
            }
        }
        private void Update()
        {
            if (IsGenerator)
            {
                if (_config != null && _config.generator != null && _config.generator.capLeft <= 0)
                {
                    //Check Cooldown
                    if (_config.generator.nextTime <= DateTime.Now)
                    {
                        _config.generator.capLeft = genConfig.capacity;
                        SetGeneratorStateReady();
                    }
                    else
                    {
                        if (_isBoostered)
                        {
                            SetGeneratorStateReady();
                        }
                        else
                        {
                            SetGeneratorStateWaiting();
                        }
                        UpdateClockProgress();
                    }
                }
                else
                {
                    if (genConfig != null && genConfig.coolDown > 0 &&
                        _config != null && _config.generator != null && _config.generator.nextTime <= DateTime.Now)
                    {
                        _config.generator.capLeft = genConfig.capacity;
                        _config.generator.nextTime = DateTime.Now.AddSeconds(genConfig.coolDown+2);
                        GameManager.Log("Generator cooldown passed, setting max charges + new cooldown");
                    }
                    if (!generatorReady) SetGeneratorStateReady();
                }
            }
            else if(IsBooster)
            {
                SetBoosterState(IsBoosterActive);
                if (IsBoosterActive)
                {


                    UpdateClockProgress();
                }
                else
                {
                    if (_config?.booster != null && _config.booster.endTime <= DateTime.Now)
                    {
                        onBoosterEnd?.Invoke();
                        Remove();
                    }
                }
            }
        }

        private void SetGeneratorStateReady()
        {
            generatorReady = true;
            clock.SetActive(false);
            ShowPieceLevel();
            AnimateGeneratorReady();
        }
        private void SetGeneratorStateWaiting()
        {
            generatorReady = false;
            bubble.transform.DOScale(0, 0);
            bubble.DOFade(0, 0);
            energyIcon.DOFade(0, 1);
            clock.SetActive(true);
        }
        public void SetSimpleState()
        {
            bubble.gameObject.SetActive(false);
            energyIcon.gameObject.SetActive(false);
            if (generatorLevelContainer != null)
            {
                generatorLevelContainer.SetActive(false);
            }
        }
        private void SetBoosterState(bool active)
        {
            if(clock!=null)
                clock.SetActive(active);
        }
        private void UpdateClockProgress()
        {
            var timeLeft = (IsGenerator?_config.generator.nextTime:_config.booster.endTime) - DateTime.Now;
            var progress = timeLeft.TotalSeconds / (IsGenerator ? genConfig.coolDown:boosterConfig.duration);
            clockProgress.fillAmount = IsGenerator?1 - (float)progress: (float)progress;
        }
        private void ShowPieceLevel()
        {
            if (generatorLevelContainer == null) return;
            
            generatorLevelContainer.SetActive(_config!=null && PieceLevel>0);
            if(_config == null)
            {
                return;
            }
            txtLevel.text = (PieceLevel+1).ToString();
        }

        private void AnimateGeneratorReady()
        {
            bubble.transform.DOScale(0, 0);
            bubble.DOFade(1, 0);
            energyIcon.DOFade(1, 0);
            energyIcon.transform.DOScale(1.1f, 0);
            seq = DOTween.Sequence();
            seq.SetDelay(0.5f);
            seq.SetLoops(-1, LoopType.Yoyo);
            seq.Insert(0, energyIcon.transform.DOScale(0.9f, 1).SetEase(Ease.InCirc));
            seq.Insert(0, energyIcon.DOFade(0, 1));
            seq.Insert(0, bubble.transform.DOScale(1.6f, 1).SetEase(Ease.OutExpo));
            //seq.Insert(0, bubble.DOFade(0, 1).SetEase(Ease.InExpo));
        }

        public void SetBoardTileParent(BoardTile tile, bool animate = false, bool isSpawn = false, GameObject trail = null, bool isSeep = false)
        {
            isSpeedBoard = isSeep;
            if (_tileParentBoard != null && _tileParentBoard != tile && _tileParentBoard.piece == this)
            {
                _tileParentBoard.SetContent(null, null);
            }
            SetTileParent(tile);
            if (animate)
            {
                
                if (isSpawn)
                {
                    //Debug.Log("DistanceFactor " + distanceFactor);
                    var path = ReturnToPlaceSpawn();
                    var t = Instantiate(trail, transform.parent);
                    t.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
                    t.transform.position = transform.position;
                    //t.transform.DOMove(_tileParentBoard.transform.position, 0.5f).OnComplete(()=>  Destroy(t, 0.5f) );
                    t.transform.DOPath(path, 0.5f,PathType.CatmullRom).OnComplete(() => Destroy(t, 0.4f));
                    transform.DOPunchScale(new Vector3(1f, 1f, 1f), 0.5f, 5);
                }
                else
                {
                    transform.DOMove(_tileParentBoard.transform.position + offset, 0.5f);
                }
                return;
            }
            transform.position = _tileParentBoard.transform.position;
        }
        float distanceFactor => ((_tileParentBoard.transform.position - transform.position).magnitude+5000) * 0.0002f;
        public Vector3[] ReturnToPlaceSpawn()
        {
            var midStep = (_tileParentBoard.transform.position - transform.position) / 2;
            midStep.x += transform.position.x + UnityEngine.Random.Range(60, 30) * (UnityEngine.Random.Range(0, 2) * 2 - 1);// * distanceFactor;
            midStep.y += transform.position.y + UnityEngine.Random.Range(60, 30) * (UnityEngine.Random.Range(0, 2) * 2 - 1);// * distanceFactor;
            Vector3[] path = new Vector3[2];
            path[1] = _tileParentBoard.transform.position;
            path[0] = midStep;
            transform.DOPath(path, 0.5f, PathType.CatmullRom);
            return path;
        }
        public void ReturnToPlace(bool isSpeed = false)
        {
            isSpeedBoard = isSpeed;
            if (_tileParentBoard != null)
            {
                transform.DOMove(_tileParentBoard.transform.position + offset, 0.5f);
            }
        }

        
        public void PlayMergeAnim()
        {
            if (Bounce()) return;            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }

        public void SetConfig(PieceType pieceType, int level)
        {
            _config = new PieceState(pieceType,level);
            ShowPieceLevel();
        }
        public void SetConfig(PieceDiscovery pieceTypeLevel)
        {
            _config = new PieceState(pieceTypeLevel.pType, pieceTypeLevel.Lvl);
            ShowPieceLevel();
        }
        
        public void SetConfig(PieceState pieceState)
        {
            _config = pieceState;
            ShowPieceLevel();
            //Check if it is locked
            if (_config.hidden)
            {
                //Deactivate all transforms and instantiate crate
                foreach(Transform t in transform)
                {
                    t.gameObject.SetActive(false);
                }
                if(_config.unlockLevel > GameManager.Instance.PlayerLevel)
                {
                    _unlockBox = Instantiate(mergeConfig.Locked2PiecePrefab, transform);
                }
                else
                {
                    _unlockBox = Instantiate(mergeConfig.LockedPiecePrefab, transform);
                }
                UpdateHiddenState();
            }
            //Temp Fix
            if (IsGenerator && _config.generator == null)
            {
                if (_config.generator == null)
                {
                    Debug.Log("Generator null");
                    _config.generator = new GeneratorState();
                    _config.generator.nextTime = DateTime.Now;
                }
                _config.generator.capLeft = genConfig.capacity;
            }
        }
        public void UpdateHiddenState()
        {
            if (!_config.hidden) return;
            {
                if (_config.unlockLevel <= GameManager.Instance.PlayerLevel && _unlockBox.TryGetComponent<BoxController>(out var boxController))
                {
                    boxController.SetPrice(UnlockCost);
                    //_unlockBox.GetComponentInChildren<Image>().DOColor(GameManager.Instance.HasEnoughCurrency(UnlockCost) ? Color.white : new Color(1, 0.8f, 0.8f), 0.1f);
                }
            }
        }

        public void SetMissionConfig(MergeMissionConfig mission, MissionTile tile)
        {
            foreach (var im in GetComponentsInChildren<Image>())
            {
                im.raycastTarget = true;
            }
            SetConfig(mission.piece);
            missionConfig = mission;
            missionTile = tile;
            //Debug.Log("MissionReward:" + missionConfig.rewardPiece.pieceType);
        }
        internal bool IsReadyToSpawn()
        {
            return _config.generator.capLeft > 0;
        }
        public PieceDiscovery GetObjectToSpawnFromGenerator(AssistanSmartGeneratorController assistantController)
        {
            if (TutorialManager.IsTutoCompleted(TutorialKeys.MergeBoardCollectXP))
            {
                //This is meant to be only for chest that can contain guaranteed items
                if(genConfig.guaranteedPieces.Count > 0 && _config.generator.capLeft <= genConfig.guaranteedPieces.Count)
                {
                    return genConfig.guaranteedPieces[_config.generator.capLeft - 1];
                }
                //TODO:We can check here if Assistant is hired
                if(GameManager.Instance.PlayerLevel < 5)
                {
                    return genConfig.GetPreferredObjectToSpawn(_config.pieceLevel);
                }
                else if (mergeModel.assistants.HasAssistant(AssistantType.SmartGenerators) && assistantController.SelectedPiecesToSpawn.Count > 0)
                {
                    return genConfig.GetSpecificObjectsToSpawn(assistantController.SelectedPiecesToSpawn);
                }

                return genConfig.GetObjectToSpawn(_config.pieceLevel);
            }
            else
            {
                return genConfig.GetObjectToSpawnTutorial();
            }
        }
        public void SpawnSuccess()
        {
            transform.SetAsLastSibling();
            _config.generator.capLeft--;
            //Debug.Log("Spawed, capleft = " + _config.generator.capLeft);
            if (_config.generator.capLeft == 0)
            {
                if (genConfig.coolDown == 0)
                {
                    Remove(true);
                    return;
                }
                else if(_config.generator.nextTime > DateTime.Now.AddSeconds(genConfig.coolDown))
                {
                    _config.generator.nextTime = DateTime.Now.AddSeconds(genConfig.coolDown);
                    return;
                }
            }
            if(genConfig.coolDown > 0 && _config.generator.nextTime < DateTime.Now)
            {
                _config.generator.nextTime = DateTime.Now.AddSeconds(genConfig.coolDown);
            }
        }

        public void ResetGeneratorCooldown()
        {
            _config.generator.nextTime = DateTime.Now;
        }

        internal void Remove(bool addSmoke = false)
        {
            //SetTileParent(null);
            if (addSmoke)
            {
                AddSmoke();
            }
            _tileParentBoard.piece = null;
            Destroy(gameObject);
        }
        public void AddSmoke()
        {
            _tileParentBoard.AddSmoke();
        }
        public void FlyToPosition(float duration, Vector3 finalPos, GameObject trail, TweenCallback OnComplete, Ease easeType = Ease.InOutQuad)
        {
            var midPoint = transform.position;
            var midStep = (finalPos - transform.position) / 2;
            midPoint.x += midStep.x + UnityEngine.Random.Range(-30, 30) * 10;
            midPoint.y += midStep.y + UnityEngine.Random.Range(-30, 30) * 5;
            Vector3[] path = new Vector3[2];
            path[0] = midPoint;
            path[1] = finalPos;

            //TRAIL
            var t = Instantiate(trail, transform);
            t.transform.SetAsFirstSibling();
            t.transform.localPosition = new Vector3(-10, -20, 0);
            transform.DOPath(path, duration, PathType.CatmullRom).OnComplete(OnComplete).SetEase(easeType);
        }

        public void ScalesAndFlyToPosition(float duration, Vector3 finalPos, GameObject trail, TweenCallback OnComplete, bool destroyOnComplete = false, Ease easeType = Ease.InOutQuad)
        {
            var midPoint = transform.position;
            var midStep = (finalPos - transform.position) / 2;
            midPoint.x += midStep.x + UnityEngine.Random.Range(-30, 30) * 10;
            midPoint.y += midStep.y + UnityEngine.Random.Range(-30, 30) * 5;
            Vector3[] path = new Vector3[2];
            path[0] = midPoint;
            path[1] = finalPos;

            //TRAIL
            var t = Instantiate(trail, transform);
            t.transform.SetAsFirstSibling();
            t.transform.localPosition = new Vector3(-10, -20, 0);
            transform.DOPunchScale(Vector3.one*3, duration, 0, 0).SetEase(Ease.InOutCubic);
            transform.DOPath(path, duration, PathType.CatmullRom).OnComplete(()=>
            {
                OnComplete?.Invoke();
                if(destroyOnComplete) { Destroy(gameObject); }
            }).SetEase(easeType);
        }

        public void ClearAndFlyTo(float duration, Vector3 finalPos, GameObject trail, TweenCallback OnComplete)
        {
            _tileParentBoard.AddSmoke();
            var midPoint = transform.position;
            var midStep = (finalPos - transform.position) / 2;
            midPoint.x += midStep.x + UnityEngine.Random.Range(-30, 30) * 10;
            midPoint.y += midStep.y + UnityEngine.Random.Range(-30, 30) * 5;
            Vector3[] path = new Vector3[2];
            path[0] = midPoint;
            path[1] = finalPos;

            //TRAIL
            var t = Instantiate(trail, _tileParentBoard.transform);
            t.transform.position = transform.position;
            t.transform.DOPath(path, duration, PathType.CatmullRom)
                .OnComplete(() => 
                { 
                    OnComplete?.Invoke(); 
                    Destroy(gameObject); 
                    Destroy(t.gameObject); 
                }).SetEase(Ease.InOutQuad);
            gameObject.SetActive(false);
        }


        public bool Bounce()
        {
            if(TryGetComponent<BouncyPiece>(out var bouncy))
            {
                bouncy.DoBounceElementes();
                return true;
            }
            transform.DOScale(1,0);
            transform.DOPunchScale(new Vector3(0.4f, 0.5f, 0.5f), 0.3f, 7);
            return false;
        }

        internal void SelectTile()
        {
            _tileParentBoard.Select(true);
        }
        public bool IsSelected => _tileParentBoard.IsSelected;

        internal void Unlock()
        {
            PieceState.hidden = false;
            Destroy(_unlockBox);
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(true);
            }
            if (IsGenerator) SetGeneratorStateReady();
        }

        public void SetExtraInfo(bool valueMap, bool valueMission, bool valueMaxLevel, bool locked = false, PieceExtraInfo prefab = null)
        {
            if (!valueMap && !valueMaxLevel && !valueMission && !locked && _pieceExtraInfo == null) return;
            if (PieceState != null && PieceState.IsBubble) return;
            _pieceExtraInfo ??= Instantiate(prefab, transform);
            _pieceExtraInfo.SetState(valueMap, valueMission, valueMaxLevel, locked);
            _pieceExtraInfo.gameObject.SetActive(valueMap || valueMaxLevel || valueMission||locked);
            if (locked)
            {
                _pieceExtraInfoBack ??= Instantiate(prefab, transform);
                _pieceExtraInfoBack.transform.SetAsFirstSibling();
                _pieceExtraInfoBack.SetLockedBack();
            }
            else
            {
                RemoveBackInfo();
            }
        }

        public void RemoveBubble()
        {
            Destroy(_pieceExtraInfo.gameObject);
            PieceState.IsBubble = false;
        }
        public void SetBubble(PieceExtraInfo prefab = null)
        {
            _pieceExtraInfo ??= Instantiate(prefab, transform);
            _pieceExtraInfo.gameObject.SetActive(true);
            _pieceExtraInfo.SetBubble();
            PieceState.IsBubble = true;
        }
        
        public void SetGreyState(bool value, Material greyMat)
        {
            foreach (var im in gameObject.GetComponentsInChildren<Image>()) {
                    im.material = value? greyMat : null;
            }
        }

        public void ShowBoosterFX(PieceExtraInfo prefab)
        {
            if (prefab == null) return;
            AnimateBoostered(true);
            _isBoostered = true;
            _pieceExtraInfoBack ??= Instantiate(prefab, transform);
            _pieceExtraInfoBack.transform.SetAsFirstSibling();
            _pieceExtraInfoBack.ActivateBoosterSpawner();
        }
        public void ActivateBooster(BoosterState boosterState, PieceExtraInfo prefab = null)
        {
            _config.booster = boosterState;
            ShowBoosterFX(prefab);
        }
        public void RemoveBooster()
        {
            _isBoostered = false;
            AnimateBoostered(false);
            RemoveBackInfo();
        }
        public void RemoveBackInfo()
        {

            if (_pieceExtraInfoBack != null)
            {
                //Debug.Log("Removing back info from piece " + _config.pieceType);
                Destroy(_pieceExtraInfoBack.gameObject);
            }
            else
            {
                //Debug.Log("There is no back info to remove");
            }
        }

        public void AnimateBoostered(bool value)
        {
            _sqAnimateBooster ??= CreateBoosteredAnimation();
            if (value)
                _sqAnimateBooster.Play();
            else
                _sqAnimateBooster.Rewind();
        }

        Sequence _sqAnimateBooster;
        private Sequence seq;

        public void SuperDoKill()
        {
            _sqAnimateBooster.Kill();
            transform.DOKill();
        }

        private Sequence CreateBoosteredAnimation()
        {
            var sq = DOTween.Sequence();
            var target = imgContent != null ? imgContent.transform : transform;
            sq.Insert(0, target.DOScaleY(0.7f, 0.3f));
            sq.Insert(0, target.DOScaleX(1.1f, 0.3f));
            sq.Insert(0.3f, target.DOScaleY(1.2f, 0.3f));
            sq.Insert(0.3f, target.DOScaleX(0.9f, 0.3f));
            sq.Insert(0.6f, target.DOScale(1f, 0.3f));
            //sq.Insert(0, transform.DOShakeScale(0.5f, 0.5f, 1, 0).SetDelay(0.5f));
            sq.SetLoops(-1);
            sq.SetDelay(0.5f);
            return sq;
        }

        public bool IsSameStateAs(PieceState target)
        {
            return PieceState.pieceType == target.pieceType && PieceState.pieceLevel == target.pieceLevel;
        }

        public void LevelUP()
        {
            PieceState.pieceLevel++;
            ShowPieceLevel();
        }

        private void OnDestroy()
        {
            if(seq != null &&  seq.IsPlaying()) seq.Kill();
        }
    }
}
