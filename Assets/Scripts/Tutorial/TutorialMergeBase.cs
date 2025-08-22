using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Analytics;
using DG.Tweening;
using Assets.Scripts.MergeBoard;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TutorialMergeBase : TutorialBase
{
    public TutorialMergeStep mergeStep;
    //public GameObject[] buttonsToHide;
    //public GameObject[] buttonsToShow;
    //public GameObject[] ObjectsToShowOnComplete;
    public GameObject handPrefab;
    private GameObject _handPointer;
    private GameObject _arrowPointer;
    private MovingPiece targetPiece;
    private int _timeElapsed = 0;
    private TutorialHand tutorialHand;
    PieceAmount ResultPiece => mergeStep.result;
    TutorialMergeType StepType => mergeStep.stepType;
    PieceState StepTarget => mergeStep.targetPiece;
    int tries = 0;

    public override bool CanStartTutorial()
    {
        var canStartNormal = base.CanStartTutorial();
        if (!canStartNormal) return false;

        if (TutorialKey == TutorialKeys.MergeSelectBox && !GameManager.Instance.MergeManager.IsBoardActive)
        {
            tries++;
            Debug.Log("1 Time for Tutorial:" + TutorialKey + ", but no merge board active");
            if (tries > 10)
            {
                Debug.Log("2 Time for Tutorial:" + TutorialKey + ", but no merge board active");
                tries = 0;
                ResetTutorial();
            }
        }

        if (!GameManager.Instance.MergeManager.IsBoardActive) return false;
        /*if (TutorialKey == TutorialKeys.MergeBoardTapGenerator)
        {
            if (PlayerPrefs.GetInt("PopupMissionClosed") == 0)
            {
                return false;
            }
        }*/
        if (TutorialKey == TutorialKeys.MergeBoardCollectXP)
        {
            if(PlayerData.xp > 0)
            {
                SkipTutorial();
                TutorialKey = TutorialKeys.MergeBoardCollectXP;
                SkipTutorial();
            }
            if (boardController != null && boardController.selectedPiece != null && boardController.selectedPiece.PieceType == mergeStep.targetPiece.pieceType)
            {
                return canStartNormal;
            }
            return false;
        }
        else if (TutorialKey == TutorialKeys.MergeRefillGenerator)
        {
            if (boardController != null) {
                var generator = boardController.SelectGenerator();
                if (!generator.generatorReady)
                {
                    return canStartNormal;
                }
            }
            return false;
        }
        else if (TutorialKey == TutorialKeys.MergeSelectBox)
        {
            if (boardController != null) {
                var target = boardController.FindPieceOfType(new PieceState(PieceType.GeneratorDrinks,0));
                if (target != null && target.PieceState.hidden)
                {
                    return canStartNormal;
                }
            }
            return false;
        }
        else if (TutorialKey == TutorialKeys.MergeSelectBox2)
        {
            if (boardController != null) {
                var target = boardController.FindPieceOfType(new PieceState(PieceType.CommonChest,0));
                if (target != null && target.PieceState.hidden)
                {
                    return canStartNormal;
                }
            }
            return false;
        }
        if (TutorialKey == TutorialKeys.MergeBuyBox)
        {
            Debug.Log("MergeBuyBox. " + canStartNormal);
        }
        return canStartNormal;
    }

    public override void StartTutorial(TutorialSequence TutorialsContainer)
    {
        isRunning = true;
        HideButtons();
        ShowButtons();
        _tutorialsContainer = TutorialsContainer;
        Debug.Log("Start TUTORIAL" + TutorialKey.ToString() + " delay = " + startDelaySeconds);
        StartCoroutine(MergeDoStart());
        if (TutorialKey == TutorialKeys.MergeBuyBox)
        {
            //Todo:if it is merge tutorial check that merge board is open
            if (!GameManager.Instance.MergeManager.IsBoardActive)
            {
                var boardConfig = GameManager.Instance.mapManager.GetBuildingInteractiveFromPlayerLocation().buildingData.boardConfig;
                GameManager.Instance.ShowMergeBoard(true, boardConfig);
                return;
            }
        }
    }

    IEnumerator MergeDoStart()
    {
        yield return new WaitForSeconds(startDelaySeconds);
        while (boardController.IsBusy)
        {
            yield return null;
        }
        _tutorialsContainer.Init(mergeStep);
        _tutorialsContainer.StartTuto(showTutoGirl, blockUI, CompleteTutorial);
        if (mergeStep.targetPiece != null)
        {
            boardController.tutorialPiece = mergeStep.targetPiece;
            targetPiece = GetPieceTarget(mergeStep.targetPiece);
            CreatePointerHand();
            if(mergeStep.stepType == TutorialMergeType.Merge)
            {
                boardController.onMerged = () =>
                {
                    if(mergeStep.targetPiece == null)
                    {
                        boardController.onMerged = null;
                        return;
                    }
                    //StopCoroutine("MergeDoStart");
                    targetPiece = GetPieceTarget(mergeStep.targetPiece);
                    if (targetPiece != null)
                    {
                        CreatePointerHand();
                    }
                    else
                    {
                        boardController.onMerged = null;
                    }
                };
            }
        }

        if (mergeStep.stepType == TutorialMergeType.Mission)
        {
            //CreatePointerArrow();
        }
        if (TutorialKey == TutorialKeys.MergeBoardTapGenerator)
        {
            boardController.SelectGenerator();
            //CreatePointerArrowButton();
        } 
        else if (TutorialKey == TutorialKeys.MergeRefillGenerator)
        {
            var generator = boardController.SelectGenerator();
            CreatePointerArrowButton();
        }
        TrackTutorialEvent(TutorialState.Started);
        isRunning = true;
    }

    public override void CompleteTutorial()
    {
        CompleteTutorialBase();
        boardController.onPieceMoved = null;
        boardController.onMerged = null;
        if (_handPointer != null) DestroyImmediate(_handPointer);
        _timeElapsed = 0;
    }
    public void SkipTutorial()
    {
        Debug.Log("Tutorial " + TutorialKey.ToString() + " completed");
        TrackTutorialEvent(TutorialState.Skipped);

        isRunning = false;
        _timeElapsed = 0;
        PlayerPrefs.SetInt("Tutorial" + TutorialKey, 1);
        foreach (var bt in ObjectsToShowOnComplete)
        {
            bt.SetActive(true);
        }
        //OnCompleteMethod?.Invoke();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            /*if (_handPointer != null && tutorialHand != null)
            {
                tutorialHand.Init(Vector3.zero);
            }*/
            if (_arrowPointer != null)
            {
                DestroyImmediate(_arrowPointer);
            }
            _timeElapsed = 0;
        }
    }

    public override void CheckTutoEnd()
    {
        if (!isRunning) return;

        if (TutorialKey == TutorialKeys.MergeBoardOpenMission)
        {
            if (PlayerPrefs.GetInt("PopupMissionOpen") == 0)
            {
                return;
            }
        }
        else if (TutorialKey == TutorialKeys.MergeBoardCollectXP)
        {
            if (boardController != null)
            {
                var piece = boardController.FindPieceOfType(mergeStep.targetPiece);
                if (piece == null)
                {
                    CompleteTutorial();
                }
            }
            return;
        }
        else if (TutorialKey == TutorialKeys.MergeRefillGenerator)
        {
            if (boardController != null)
            {
                if (boardController.GetGeneratorPiece().generatorReady)
                {
                    CompleteTutorial();
                }
            }
            return;
        }

        if (mergeStep.completeType == TutorialMergeCompleteType.PieceSelected)
        {
            if(boardController!= null && boardController.selectedPiece != null && boardController.selectedPiece.PieceType == mergeStep.result.pieceType.pieceType)
            {
                CompleteTutorial();
            }
            return;
        }
        

        _timeElapsed++;

        var pieceAmount = boardController.FindSamePieces(mergeStep.result.pieceType,true);
        if(pieceAmount.amount >= mergeStep.result.amount){
            CompleteTutorial();
        }
        /*else
        {
            //check if arrow/hand can help the player
            if (_timeElapsed > 3 && _handPointer == null)
            {
                if (mergeStep.stepType == TutorialMergeType.Merge)
                {
                    var tempTarget = new PieceState();
                    tempTarget.pieceType = ResultPiece.pieceType.pieceType;
                    tempTarget.pieceLevel = ResultPiece.pieceType.pieceLevel - 1;
                    var potentialTarget = boardController.FindPieceOfType(tempTarget);
                    if (potentialTarget != null)
                    {
                        targetPiece = potentialTarget;
                    }
                    else
                    {
                        targetPiece = GetPieceTarget(StepTarget);
                    }
                    CreatePointerHand();
                }
                else if (StepType == TutorialMergeType.Mission)
                {
                    targetPiece = GetPieceTarget(StepTarget);
                    CreatePointerHand();
                    //CreatePointerArrow();
                }
                else
                {
                    targetPiece = GetPieceTarget(StepTarget);
                    CreatePointerHand();
                }
            }
            //Debug.Log("Tuto " + TutorialKey + " looking for " + mergeSteps[0].result.amount + " " + mergeSteps[0].result.pieceType);
        }*/
    }

    private void CreatePointerHand()
    {
        if (handPrefab == null || targetPiece == null) return;
        
        _handPointer ??= Instantiate(handPrefab, boardController.transform);
        //handPointer = Instantiate(handPrefab, targetPiece.transform.parent, false);
        var pos = targetPiece.transform.position - Vector3.up * 40;
        //_handPointer.transform.position = pos;
        tutorialHand ??= _handPointer.GetComponentInChildren<TutorialHand>();

        //Debug.Log("Create Pointer hand");
        if (StepType == TutorialMergeType.Generator || StepType == TutorialMergeType.Selection)
        {
            tutorialHand.Init(pos,true,false);
        }
        else if (StepType == TutorialMergeType.Merge)
        {
            var other = boardController.FindClosetsPieceOfTheSameType(targetPiece);
            tutorialHand.Init(pos, false,true, other.transform.position - Vector3.up * 40);
        }
        else if (StepType == TutorialMergeType.Mission)
        {
            var other = boardController.FindMissionOfType(StepTarget);
            tutorialHand.Init(pos, false,true, other.transform.position - Vector3.up * 40);
        }
        _timeElapsed = 0;
        boardController.onPieceMoved = CreatePointerHand;
    }

    public void CreatePointerArrow()
    {
        var t = boardController.FindMissionOfType(StepTarget);
        _arrowPointer = Instantiate(_tutorialsContainer.pointer, t.transform.parent, false);
        _arrowPointer.transform.position = t.transform.position + Vector3.up * 200;
    }

    public void CreatePointerArrowButton()
    {
        if (targetButton == null) return;
        var t = targetButton;
        _tutorialsContainer.AddPointer(t);
        //arrowPointer = Instantiate(_tutorialsContainer.pointer, boardController.transform, false);
        //arrowPointer.transform.position = t.transform.position + Vector3.up * 200;
        t.GetComponent<Button>().onClick.AddListener(CompleteTutorial);
    }


    //prevLevelpiece
    private MovingPiece GetPieceTarget(PieceState targetType)
    {
        /*if (mergeSteps[0].stepType == TutorialMergeType.Mission)
        {
            return GameManager.Instance.MergeManager.boardController.FindMissionOfType(targetType);
        }
        else
        {*/
            var p = boardController.FindPieceOfType(targetType, !targetType.hidden && !targetType.locked);
            
            return p;
        //}
    }
}

