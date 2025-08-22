using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Assets.Scripts.MergeBoard;
using System.Linq;
using Unity.Cinemachine;

public class MapManager : MonoBehaviour
{
    public Action OnDrag;
    public Action PlayerArriveDestionation;
    public Action CancelPlayers;
    public Action<BuildingType> OnBuildingTap;  
    public float mainFOV = 40;
    public float maxFOV = 100;
    public float minFOV = 10;
    public float modX;
    public float modY;
    public float minDrag;
    public float strength;
    private bool bDragging { get { return finalPoint.magnitude > minDrag; } }
    private bool _bDragging2;
    public CinemachineCamera cameraMap;
    public CinemachineCamera cameraOtherPlayer;
    public CinemachineCamera cameraBuilding;
    public CinemachineCamera cameraInteractive;
    public Collider confiner1;
    public Collider confiner2;
    public Collider confiner3;
    [SerializeField]
    [Range(1,5)]
    private float distanceToTimeExpoFactor = 2f;
    [SerializeField]
    [Range(1, 10)]
    private float distanceToTimeMultiplier = 5f;
    [SerializeField] private Vector3 CamOtherPlayerOffset = new Vector3(0.3f,1,9);
    public Popup_OutsideBuilding PlayerOutsidePopup;
    public Popup_InsideBuilding PlayerInsidePopup;
    public Popup_BuildingLocked popupBuildingLocked;
    public PopupOtherPlayer popupOtherPlayer;

    public bool playerInputEnable = true;

    private Vector3 touch1StartPoint;
    private Vector3 touch2StartPoint;
    private Vector3 touchStartDistance;
    private Vector3 startPoint;
    private Vector3 finalPoint;
    private Vector3 finalPos;
    private Vector3 startTrackerPos;
    private Vector3 lastTrackerPos;
    private bool wasTwoTouches;
    private bool showingBuilding = false;
    private bool showingPlayer = false;
    //public CameraTrackerConfiner cameraTrackerConfiner;

    private float _time;
    private float _smoothTime = 5;
    private Vector3 _velocity;
    private Vector3 _targetBuildingPosition;
    private bool _underInertia = false;
    private bool _moveToBuilding = false;
    private float mainCamY;
    public BuildingIteractive tutorialHouse;
    public List<BuildingIteractive> allMapPointers;
    private LineRenderer _line;
    private bool initialized = false;
    private MergeBoardModel mergeModel => GameManager.Instance.MergeManager.boardModel;
    private MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;

    public Collider confiner => GameManager.Instance.PlayerLevel < 4 ? 
        confiner1 : GameManager.Instance.PlayerLevel < 5 ? confiner2:confiner3;

    // Start is called before the first frame update
    void Start()
    {
        /*var mapPointers = GameObject.FindObjectsOfType<BuildingIteractive>();
        allMapPointers = new List<BuildingIteractive>();
        foreach (var mapPointer in mapPointers)
        {
            if(mapPointer.buildingData != null)
                allMapPointers.Add(mapPointer);
        }*/

        if (MyScenesManager.Instance == null) return;
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 0;
        UIUtils.DelayedCall(0.1f, MyScenesManager.Instance.HideScreen, this);
    }

    public void Init()
    {
        PlayerOutsidePopup.gameObject.SetActive(false);
        PlayerInsidePopup.gameObject.SetActive(false);
        popupOtherPlayer.gameObject.SetActive(false);
        popupBuildingLocked.Init();
    }

    
    // Update is called once per frame
    void LateUpdate()
    {
        if (_line == null) _line = GetComponent<LineRenderer>();
        if (mainCamY == 0)
        {
            mainCamY = cameraMap.transform.position.y;
            //Debug.Log("Start, mainCamY = " + mainCamY);
        }
        if (GameManager.Instance.playerData.IsTravelling && _line.positionCount < 2)
        {
            DrawTravellingLine();
        }


        //Check Inputs
        /*if (UIUtils.IsPointerOverUIObject()) {
            if (GameManager.Instance.playerData.CheckJustArrived())
            {
                initialized = true;
                ResetTravelling();
                PlayerArriveDestionation?.Invoke();
            }
            return; 
        }*/

        if (IsZooming()) return;
        
        cameraMap.Lens.FieldOfView = Mathf.Lerp(cameraMap.Lens.FieldOfView, mainFOV, 0.1f);

        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began)
            {
                //Debug.Log("Touch Began");
                startTrackerPos = cameraMap.transform.position;
                _underInertia = false;
                wasTwoTouches = false;
                startPoint = Camera.main.ScreenToViewportPoint(touch.position);
            }
            else if (wasTwoTouches) return;

        } 
        else if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Map - click down");
            //lastTrackerPos = Vector3.zero;
            startTrackerPos = cameraMap.transform.position;
            startPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            _underInertia = false;
        }
        if (Input.GetMouseButton(0))
        {
            finalPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition) - startPoint;
            //if ((bDragging || _bDragging2) && !UIUtils.IsPointerOverUIObject() && playerInputEnable && GameManager.Instance.playerData.level > 1)
            if ((bDragging || _bDragging2) && playerInputEnable && GameManager.Instance.playerData.level > 1 && !GameManager.Instance.tutorialManager.IsTutorialRunning)
            {
                _bDragging2 = true;
                //Debug.Log("Map - isDragging");
                //DRAGGING if not running tutorial
                //OnDrag?.Invoke();
                //OnInteractionCancel();
                PlayerOutsidePopup.Hide();
                popupOtherPlayer.Hide();
                CancelPlayers?.Invoke();
                showingBuilding = false;
                CamPriority(cameraMap);
                Vector3 finalPos = GetFinalPos();
                if (confiner.bounds.Contains(finalPos))
                {
                    lastTrackerPos = cameraMap.transform.position;
                }
                else
                {
                    finalPos = confiner.ClosestPoint(finalPos);
                    //finalPos.x -= finalPos.x 
                    //Debug.Log("Outside Confiner");
                }
                var before = cameraMap.transform.position;
                cameraMap.transform.position=Vector3.Lerp(cameraMap.transform.position , finalPos,Time.deltaTime*5);
                cameraBuilding.transform.position = finalPos;
            }
            
        }

        if (Input.GetMouseButtonUp(0))
        {
            _bDragging2 = false;
            if (UIUtils.IsPointerOverUIObject()) return;
            //Debug.Log("Map - click up");
            finalPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition) - startPoint;
            if (!bDragging && (!showingBuilding || IsTutorialRunning()))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    var target = hit.transform.gameObject;
                    GameManager.Log("Click On " + target.name);
                    BuildingIteractive building;
                    building = target.GetComponentInChildren<BuildingIteractive>();
                    
                    //CHECK TUTORIAL MODE
                    if (IsTutorialRunning())
                    {
                        if (building == null)
                        {
                            Debug.Log("Null building");
                        }
                        else if (!IsTutorialTarget(building))
                        {
                            if(GameManager.Instance.tutorialManager.ActiveTutorial()?.buildingToTap == null)
                            {
                                GameManager.Log(GameManager.Instance.tutorialManager.ActiveTutorial().TutorialKey.ToString());
                                GameManager.Log("Tap on " + building.buildingData.buildingName + " but is on tuto NOT expecting tap on building");
                            }
                            else
                            {
                                GameManager.Log("Tap on " + building.buildingData.buildingName + " but is on tuto expecting " + GameManager.Instance.tutorialManager.ActiveTutorial().buildingToTap.buildingData.buildingName);
                            }
                            return;
                        }
                    }

                    if ((!showingBuilding || IsTutorialRunning()) && !_moveToBuilding && playerInputEnable)
                    {
                        if (building != null)
                        {
                            if (showingPlayer && popupOtherPlayer.gameObject.activeSelf)
                            {
                                CancelPlayers?.Invoke();
                                showingPlayer = false;
                                popupOtherPlayer.Hide();
                            }
                            showingBuilding = OnTapBuilding(building.buildingData);
                            if (!showingBuilding)
                            {
                                //UnlockBuilding(BuildingType.Circus);
                                return;
                            }
                            OnDrag += building.Cancel;
                            FocusOnBuilding(building);
                            OnBuildingTap?.Invoke(building.buildingData.buildingType);

                        }
                        else if(target.TryGetComponent<OtherPlayerVehicle>(out var otherPlayer))
                        {
                            if (otherPlayer.playerData != null)
                            {
                                FocusOnOtherPlayer(otherPlayer);
                            }
                        }
                        else
                        {
                            OnDrag?.Invoke();
                            GameManager.Log("NOT Interactive " + hit.transform.gameObject.name);
                            _moveToBuilding = true;
                            _targetBuildingPosition = new Vector3(
                                hit.transform.position.x - 0.3f,
                                mainCamY,
                                hit.transform.position.z - 11.6f
                            );
                            cameraBuilding.LookAt = hit.transform;
                            CamPriority(cameraBuilding);
                        }
                    }
                    else
                    {
                        if (!IsTutorialRunning() && target.TryGetComponent<OtherPlayerVehicle>(out var otherPlayer))
                        {
                            if (otherPlayer.playerData != null)
                            {
                                Debug.Log("otherPlayer " + otherPlayer.playerData.playername);
                                FocusOnOtherPlayer(otherPlayer);
                            }
                            else
                            {
                                Debug.Log("otherPlayer empty");
                            }
                        }
                        GameManager.Log("Cannot Interact with building " + hit.transform.gameObject.name + " Showing = " + showingBuilding);
                    }
                }
                else
                {
                    if (!IsTutorialRunning())
                    {
                        CamPriority(cameraMap);
                    }
                }
            }
            else
            {
                if (!IsTutorialRunning())// && GameManager.Instance.playerData.level > 1)
                {
                    /*RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (!showingPlayer && Physics.Raycast(ray, out hit, 100.0f))
                    {
                        var target = hit.transform.gameObject;
                        if (target.TryGetComponent<OtherPlayerVehicle>(out var otherPlayer))
                        {
                            FocusOnOtherPlayer(otherPlayer);
                        }
                        finalPos = GetFinalPos() + (GetFinalPos() - cameraMap.transform.position) * 1.8f;
                        if (!confiner.bounds.Contains(finalPos))
                        {
                            GameManager.Log("End Drag and It was outside Confiner");
                            finalPos = confiner.ClosestPoint(finalPos);
                            //finalPos = lastTrackerPos;
                        }
                        _underInertia = true;
                    }
                    else
                    {*/
                        finalPos = GetFinalPos() + (GetFinalPos() - cameraMap.transform.position) * 1.8f;
                        if (!confiner.bounds.Contains(finalPos))
                        {
                            GameManager.Log("End Drag and It was outside Confiner");
                            finalPos = confiner.ClosestPoint(finalPos);
                            //finalPos = lastTrackerPos;
                        }
                        _underInertia = true;
                    //}
                }
                else
                {
                    Debug.Log("Is in tutorial mode but no input working bDragging=" + bDragging + " showingBuilding=" + showingBuilding);
                }
            }
        }

        if (_moveToBuilding)
        {
            cameraMap.transform.DOMove(_targetBuildingPosition, 0.8f);
            _moveToBuilding = false;
        }
        else if (_underInertia && _time <= _smoothTime)
        {
            cameraMap.transform.position = Vector3.Lerp(cameraMap.transform.position, finalPos, Time.deltaTime * 3);
        }
        else if (!IsTutorialRunning())
        {
            _underInertia = false;
            _moveToBuilding = false;
            _time = 0.0f;
        }
        if (((PlayerData.TravellingVehicle > 0 && GameManager.Instance.playerData.vehiclesOwned != null && GameManager.Instance.playerData.vehiclesOwned.Count > 0)
            || PlayerData.TravellingVehicle <= 0) &&
            GameManager.Instance.playerData.CheckJustArrived())
        {
            initialized = true;
            ResetTravelling();
            AddPlayerVehicle();
            PlayerArriveDestionation?.Invoke();
        }
        if (!initialized)
        {
            initialized = true;
            FocusOnPlayerLocation();
            AddPlayerVehicle();
        }
    }

    private bool IsZooming()
    {
        if (Input.touchCount == 2)
        {
            var touch1 = Input.GetTouch(0);
            var touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                //Debug.Log("Touch2 Began");
                touch2StartPoint = Camera.main.ScreenToViewportPoint(touch2.position);
                touchStartDistance = startPoint - touch2StartPoint;
                wasTwoTouches = true;
            }
            if (startPoint != Vector3.zero && touch2StartPoint != Vector3.zero)
            {
                var newDistance = Camera.main.ScreenToViewportPoint(touch1.position) - Camera.main.ScreenToViewportPoint(touch2.position);
                var dif = touchStartDistance.magnitude - newDistance.magnitude;
                //Debug.Log("Touch distance " + dif);
                cameraMap.Lens.FieldOfView = Mathf.Min(maxFOV, Mathf.Max(minFOV, mainFOV + dif * 30));
            }
            return true;
        }
        return false;
    }

    private void CamPriority(CinemachineCamera cam)
    {
        cameraBuilding.Priority = 1;
        cameraInteractive.Priority = 1;
        cameraOtherPlayer.Priority = 1;
        cameraMap.Priority = 14;
        cam.Priority = 15;
    }

    public void FocusOnOtherPlayer(OtherPlayerVehicle otherPlayer)
    {
        showingPlayer = true;
        showingBuilding = false;
        CancelPlayers?.Invoke();
        var pos = new Vector3(
            otherPlayer.transform.position.x - CamOtherPlayerOffset.x,
            mainCamY - CamOtherPlayerOffset.y,
            otherPlayer.transform.position.z - CamOtherPlayerOffset.z
        );
        cameraOtherPlayer.transform.DOMove(pos, 0.8f);
        cameraOtherPlayer.Follow = otherPlayer.transform;
        cameraOtherPlayer.LookAt = otherPlayer.transform;
        CamPriority(cameraOtherPlayer);
        otherPlayer.canvasPlayerMap.gameObject.SetActive(true);
        CancelPlayers -= otherPlayer.HidePlayersUI;
        CancelPlayers += otherPlayer.HidePlayersUI;
        if(!popupOtherPlayer.gameObject.activeSelf) HidePopups();
        popupOtherPlayer.Show(otherPlayer.playerData);
        popupOtherPlayer.OnCancel += OnInteractionCancel;
    }

    public void FocusOnPlayerLocation()
    {
        HidePopups();
        FocusOnBuilding(PlayerData.playerLocation);
        //HideAndShowPlayersUI();
    }
    public void FocusOnPlayerHouse()
    {
        HidePopups();
        FocusOnBuilding(BuildingType.TutorialHouse);
    }

    private void HidePopups()
    {
        PlayerOutsidePopup.Hide();
        PlayerInsidePopup.Hide();
        popupOtherPlayer.Hide();
        showingBuilding = false;
    }

    private bool OnTapBuilding(SO_Building building)
    {
        var isPlayerHere = PlayerData.playerLocation == building.buildingType;
        
        if (isPlayerHere)
        {
            PlayerInsidePopup.Show(building);
            PlayerInsidePopup.OnCancel += OnInteractionCancel;
        }
        else
        {
            bool hasMergeMissions = GameManager.Instance.MergeManager.boardModel.HasMissionsInLocation(building.buildingType);
            if (building.unlockLevel > GameManager.Instance.PlayerLevel && !hasMergeMissions 
                && (building.buildingType != BuildingType.Casino || (building.buildingType == BuildingType.Casino && GameManager.Instance.PlayerLevel < 3)))
            {
                if ((building.buildingType == BuildingType.Casino && GameManager.Instance.PlayerLevel < 3))
                {
                    popupBuildingLocked.ShowPopupLocked(3);
                }
                else
                {
                    popupBuildingLocked.ShowPopupLocked(building.unlockLevel);
                }
                return false;
            }
            PlayerOutsidePopup.Show(building);
            PlayerOutsidePopup.OnCancel -= OnInteractionCancel;
            PlayerOutsidePopup.OnCancel += OnInteractionCancel;
        }
        return true;
    }

    private bool IsTutorialRunning()
    {
        return GameManager.Instance.playerData.level < 3 && GameManager.Instance.tutorialManager.IsTutorialRunning;
            //|| GameManager.Instance.playerData.level == 1 // Why?
            //|| !playerInputEnable; //Why?
    }
    private bool IsTutorialTarget(BuildingIteractive building)
    {
        return (GameManager.Instance.tutorialManager.ActiveTutorial() != null && GameManager.Instance.tutorialManager.ActiveTutorial().buildingToTap == building);
    }

    Vector3 GetFinalPos()
    {
        return new Vector3(
                    -finalPoint.x * strength,
                    0,
                    -finalPoint.y * strength * 1.8f
                ) + startTrackerPos;
        
    }
    public void FocusOnBuilding(BuildingIteractive building)
    {
        if (building.transform == null) return;
        MoveCameraToTransform(building.transform);
    }

    public void FocusOnBuilding(BuildingType buildingType)
    {
        Debug.Log("FocusOnBuilding " + buildingType);
        var building = GetBuildingFromType(buildingType);        
        if (building == null) return;
        HidePopups();
        MoveCameraToTransform(building.transform);
    }

    public Transform GetBuildingTransformFromType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.TutorialHouse) return tutorialHouse.transform.parent;
        foreach (var pointer in allMapPointers.Where(p=>p!=null))
        {

            if (pointer.buildingData.buildingType == buildingType)
            {
                return pointer.transform.parent;
            }
        }
        return null;
    }

    public MapPointer GetBuildingMapPointerFromType(BuildingType buildingType)
    {
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer.buildingData.buildingType == buildingType)
            {
                return pointer.mapPointer;
            }
        }
        return null;
    }
    
    public BuildingIteractive GetBuildingFromType(BuildingType buildingType)
    {
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer.buildingData.buildingType == buildingType)
            {
                return pointer;
            }
        }
        return null;
    }

    public SO_Building GetBuildingDataFromType(BuildingType buildingType)
    {
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer.buildingData.buildingType == buildingType)
            {
                return pointer.buildingData;
            }
        }
        Debug.Log("building Type Not found - " + buildingType);
        return null;
    }

    public BuildingIteractive GetBuildingInteractiveFromType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.TutorialHouse) return tutorialHouse;
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer.buildingData.buildingType == buildingType)
            {
                return pointer;
            }
        }
        return null;
    }

    public BuildingIteractive GetBuildingInteractiveFromPlayerLocation()
    {
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer.buildingData.buildingType == PlayerData.playerLocation)
            {
                return pointer;
            }
        }
        return null;
    }

    public BuildingType GetBuildingSourceOfMergeItem(PieceType pieceType)
    {
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer.buildingData.boardConfig !=null)
            {
                foreach(var p in pointer.buildingData.boardConfig.pieces)
                {
                    if(p.pieceType != PieceType.None)
                    {
                        var genConfig = mergeConfig.GetDefByPieceType(p.pieceType).levels[0].GetComponent<MovingPiece>()?.genConfig;
                        if (genConfig != null)
                        {
                            foreach(var genPiece in genConfig.piecesChances)
                            {
                                if(genPiece.pieceType == pieceType)
                                {
                                    return pointer.buildingData.buildingType;
                                }
                            }
                        }
                    }
                }
            }
        }
        return BuildingType.None;
    }

    public List<BoardConfig> GetUnlockedBoards()
    {
        var boards = new List<BoardConfig>();
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {
            if (pointer.buildingData.boardConfig != null)
            {
                //Is Unlocked?
                if (pointer.buildingData.IsUnlocked)
                {
                    boards.Add(pointer.buildingData.boardConfig);
                }
            }
        }
        return boards;
    }
    
    public BuildingType GetRandomBuilding()
    {
        return allMapPointers[UnityEngine.Random.Range(0, allMapPointers.Count)].buildingData.buildingType;
    }
    public BuildingType GetRandomUnlockedBuilding()
    {
        var isUnlocked = false;
        var safeTries = 0;
        while (!isUnlocked && safeTries < 50)
        {
            var building = allMapPointers[UnityEngine.Random.Range(0, allMapPointers.Count)]?.buildingData;
            if (building == null) continue;
            isUnlocked = building.IsUnlocked;
            if (isUnlocked)
            {
                return building.buildingType;
            }
            safeTries++;
        }
        GameManager.Log("ATTENTION, no Unlocked building found ");
        return BuildingType.None;
    }

    public BoardConfig GetBoardConfigByID(string boardID)
    {
        foreach (var pointer in allMapPointers.Where(p => p != null))
        {

            if (pointer !=null && pointer.buildingData.boardConfig != null && pointer.buildingData.boardConfig.boardID == boardID)
            {
                return pointer.buildingData.boardConfig;
            }
        }
        return null;
    }

    public List<PieceDiscovery> GetUnlockedChains(bool includeHidden = false)
    {
        var _chains = new List<PieceDiscovery>();
        var unlockedBoards = GetUnlockedBoards();
        foreach (var board in mergeModel.boards.Where(b => unlockedBoards.Find(unlockedBoard => unlockedBoard.boardID == b.boardID) != null))
        {
            foreach (var piece in board.pieces)
            {
                if (piece != null && !includeHidden && piece.hidden) continue;
                if (piece != null && piece.generator != null)
                {
                    var prefab = mergeConfig.GetPiecePrefab(piece);

                    var genConfig = prefab.GetComponent<MovingPiece>().genConfig;
                    if (genConfig != null && genConfig.coolDown > 0)
                    {
                        var itemsProduced = genConfig.PossiblePieces();
                        foreach (var pieceChain in itemsProduced)
                        {
                            //GameManager.Log("Piece Chain candidate " + pieceChain.pType);
                            _chains.Add(pieceChain);
                        }
                    }
                }
            }
        }
        return _chains;
    }
    
    public List<PieceType> GetPiecesGeneratedInBuilding(BuildingType buildingType)
    {
        var pointer = GetBuildingInteractiveFromType(buildingType);
        var pieces = new List<PieceType>();

        if (pointer.buildingData.boardConfig != null)
        {
            foreach (var p in pointer.buildingData.boardConfig.pieces)
            {
                if (p.pieceType != PieceType.None)
                {
                    if (mergeConfig.GetDefByPieceType(p.pieceType).levels[0].TryGetComponent<MovingPiece>(out var movingPiece))
                    {
                        if (movingPiece.IsGenerator && !movingPiece.IsExpirable)
                        {
                            foreach (var genPiece in movingPiece.genConfig.piecesChances)
                            {
                                pieces.Add(genPiece.pieceType);
                            }
                        }
                    }
                }
            }
        }
        return pieces;
    }

    public List<GeneratorConfig> GetUnlockedGenerators(bool includeHidden = false)
    {
        var generators = new List<GeneratorConfig>();
        var unlockedBoards = GetUnlockedBoards();
        foreach (var board in mergeModel.boards.Where(b => unlockedBoards.Find(unlockedBoard => unlockedBoard.boardID == b.boardID) != null))
        {
            foreach (var piece in board.pieces)
            {
                if (!includeHidden && piece.hidden) continue;
                if (piece != null && piece.generator != null)
                {
                    var prefab = mergeConfig.GetPiecePrefab(piece);

                    var genConfig = prefab.GetComponent<MovingPiece>().genConfig;
                    if (genConfig != null && genConfig.coolDown > 0)
                    {
                        generators.Add(genConfig);
                    }
                }
            }
        }
        return generators;
    }

    public List<PieceDiscovery> GetMaxDiscoveries(int limitedToLevel = 8)
    {
        var _maxDiscovered = new List<PieceDiscovery>();
        foreach (var chain in GetUnlockedChains())
        {
            var max = 0;
            foreach (var d in mergeModel.pieceDiscoveries.Where(p => p.pType == chain.pType && p.Lvl < limitedToLevel))
            {
                if (d.Lvl > max) max = d.Lvl;
            }
            var disc = mergeModel.pieceDiscoveries.Find(d => d.Lvl == max && d.pType == chain.pType);
            if (disc != null)
            {
                _maxDiscovered.Add(disc);
            }
            else
            {
                //Adding unlocked but not discovered chain
                chain.Lvl = 3;
                _maxDiscovered.Add(chain);
            }
            //GameManager.Log("Chain " + chain.pType + " Max Discovery  " + max);
        }
        return _maxDiscovered;
    }


    private void MoveCameraToTransform(Transform target)
    {
        Debug.Log("Move Camera to transform " + target);
        cameraBuilding.transform.position = target.position + (Vector3.up * 3);
        cameraBuilding.Follow = target;
        cameraBuilding.LookAt = target;
        CamPriority(cameraBuilding);

        /*cameraBuilding.Priority = 15;
        cameraBuilding.Follow = target;
        cameraBuilding.LookAt = target;
        _moveToBuilding = true;
        _targetBuildingPosition = new Vector3(
            target.position.x - 0.3f,
            mainCamY,
            target.position.z - 11.6f
        );*/
    }

    public void OnInteractionCancel()
    {
        CancelPlayers?.Invoke();
        PlayerOutsidePopup.OnCancel -= OnInteractionCancel;
        PlayerInsidePopup.OnCancel -= OnInteractionCancel;
        popupOtherPlayer.OnCancel -= OnInteractionCancel;
        showingBuilding = false;
        CamPriority(cameraMap);
        _targetBuildingPosition = new Vector3(
                            cameraMap.transform.position.x,
                            mainCamY,
                            cameraMap.transform.position.z);
        cameraMap.transform.DOMove(_targetBuildingPosition, 0.2f).SetLink(cameraMap.gameObject);
        //Debug.Log("exit Building, mainCamY = " + mainCamY);
        showingPlayer = false;
    }


    public int GetTimeToReachBuilding(BuildingType buildingType)
    {
        return GetTimeToReachBuilding(GetBuildingTransformFromType(buildingType));
    }

    public int GetTimeToReachBuilding(Transform target)
    {
        //Now is using best vehicle.. TODO: Select vehicle to go with
        var vehicle = GameManager.Instance.playerData.GetBestVehicle();
        var origin = GetBuildingTransformFromType(PlayerData.playerLocation);
        var distance = (target.position - origin.position).magnitude;
        var time = Mathf.Pow(distance,distanceToTimeExpoFactor) * distanceToTimeMultiplier;
        
        if (vehicle != null)
        {
            //Debug.Log("vehicleMod = " + vehicle.speedMod);
            time /= (vehicle.speedMod*3);
        }
        return (int)time;
    }

    public void DrawTravellingLine()
    {
        var destination = GetBuildingTransformFromType(GameManager.Instance.playerData.TravellingDestination).position;
        var origin = GetBuildingTransformFromType(PlayerData.playerLocation).position;
        origin = new Vector3(origin.x,1,origin.z);
        destination = new Vector3(destination.x,1, destination.z);
        
        _line.positionCount = 2;
        _line.SetPosition(1, origin);
        _line.SetPosition(0, destination);
        _line.material.DOOffset(_line.material.mainTextureOffset + Vector2.right, 1).SetLoops(-1).SetEase(Ease.Linear).SetLink(_line.gameObject);
        //_line.material.DOColor(Color.red,1);
        //TODO: Set speed and color based on vehicle
    }

    public void ResetTravelling()
    {
        _line.positionCount = 0;
        FocusOnBuilding(PlayerData.playerLocation);
        //HideAndShowPlayersUI();
    }

    private void HideAndShowPlayersUI()
    {
        foreach (var b in allMapPointers.Where(p => p != null))
        {
            b.ShowPlayersUI(false);
        }
        var building = GetBuildingInteractiveFromType(PlayerData.playerLocation);
        building.ShowPlayersUI(true);
    }

    public void UnlockBuilding(SO_Building building)
    {
        popupBuildingLocked.ShowPopupUnLocked(building);
        FocusOnBuilding(building.buildingType);
        var mapPointer = GetBuildingMapPointerFromType(building.buildingType);
        UIUtils.DelayedCall(0.5f,mapPointer.PlayUnlockAnim,this);
    }

    public List<SO_Building> GetUnlockedBuildingsAtLevel(int level)
    {
        List<SO_Building> list = new List<SO_Building>();
        foreach(var mp in allMapPointers.Where(p => p != null))
        {
            if(mp.buildingData.unlockLevel == level)
            {
                list.Add(mp.buildingData);
            }
        }
        return list;
    }

    public void LazyUpdate()
    {
        if (PlayerInsidePopup.gameObject.activeSelf)
        {
            PlayerInsidePopup.LazyUpdate();
        }
    }

    public void UpdateMapMissions()
    {
        //if (!mergeConfig || mergeConfig.mapMissions == null) return;

        foreach(var mission in mergeModel.mapMissionsNew)
        {
            var mapPointer = GetBuildingMapPointerFromType(mission.location);
            if (mapPointer != null)
            {
                mapPointer.SetPointerMission(mergeModel.IsMapMissionReady(mission));
            }
        }
        if(mergeModel.limitedMission != null)
        {
            GetBuildingMapPointerFromType(mergeModel.limitedMission.location).SetPointerMission(mergeModel.IsLimitedMissionReady());
        }
        if (PlayerInsidePopup.gameObject.activeSelf)
            PlayerInsidePopup.UpdateButtons();
    }
    
    public void CompleteMapMission(MergeMissionMapConfig mission)
    {
            GetBuildingMapPointerFromType(mission.location).ReCheckPointer();
    }

    public void AddPlayerVehicle()
    {
        if (!GameManager.Instance.playerData.HasAvailableVehicle) return;
        var vehicle = GameManager.Instance.playerData.GetBestVehicle();
        var building = GetBuildingInteractiveFromType(PlayerData.playerLocation);
        if(vehicle.vehiclePrefab != null)
            building.LoadPlayerVehicle(vehicle);
    }

    public void AddOtherPlayersVehicles(List<LeaderboardPlayer> players)
    {
        foreach (var item in allMapPointers.Where(p => p != null))
        {
            item.CleanOthersVehiclesHolders(players.Where(p => p != null).ToList());
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].vehicle == 0 || players[i].location == 0 || players[i].isPlayer) continue;
            
            var building = GetBuildingInteractiveFromType((BuildingType)players[i].location);
            if (building == null || building._playersInBuilding.Contains(players[i])) continue;
            var v = GameConfigMerge.instance.vehiclesDefinition.Find(v => v.id == players[i].vehicle);
            if (v != null)
            {
                building.LoadOtherPlayerVehicle(players[i]);
            }
        }
    }

    internal void RefeshVehicle()
    {
        if (PlayerOutsidePopup.isActiveAndEnabled)
        {
            PlayerOutsidePopup.UpdateTravelling();
        }
    }
}
