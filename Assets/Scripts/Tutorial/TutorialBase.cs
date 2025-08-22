using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.Services.Analytics;
using Assets.Scripts.MergeBoard;
using Unity.Cinemachine;

public class TutorialBase : MonoBehaviour
{
    protected enum TutorialState{
        Started,
        Completed,
        Skipped
    }
    public TutorialKeys TutorialKey;
    public List<TutorialKeys> TutorialsCompletedBefore;
    public List<TutorialStep> steps;
    public GameObject targetButton;
    public bool showTutoGirl;
    public int autoCompleteInLevel;
    public GameObject[] buttonsToHide;
    public GameObject[] buttonsToShow;
    public GameObject[] ObjectsToShowOnComplete;
    public CinemachineCamera cameraTuto;
    [Range(0,5)] public float startDelaySeconds = 0f;
    public BuildingIteractive buildingToTap;
    public bool blockUI;
    public bool isRunning = false;
    public bool insideMergeBoard;
    public UnityEvent OnStartMethod;
    public UnityEvent OnCompleteMethod;

    private int _tutoState = -1;

    protected TutorialSequence _tutorialsContainer;
    protected MergeBoardController boardController => GameManager.Instance.MergeManager.boardController;

    public virtual bool CanStartTutorial()
    {
        if (!IsThisTutoCompleted() && !isRunning)
        {
            if(TutorialsCompletedBefore.Count > 0)
            {
                for(var i=0; i<TutorialsCompletedBefore.Count; i++)
                {
                    if (!TutorialManager.IsTutoCompleted(TutorialsCompletedBefore[i]))
                    {
                        return false;
                    }
                }
            }
            if(insideMergeBoard && !GameManager.Instance.MergeManager.IsBoardActive)
            {
                return false;
            }
            if (autoCompleteInLevel > 0 && GameManager.Instance.PlayerLevel >= autoCompleteInLevel)
            {
                CompleteTutorial();
                return false;
            }
            return true;
        }
        return false;
    }

    public virtual void StartTutorial(TutorialSequence TutorialsContainer)
    {
        isRunning = true;
        HideButtons();
        ShowButtons();
        _tutorialsContainer = TutorialsContainer;
        OnStartMethod?.Invoke();
        GameManager.Log("Start TUTORIAL" + TutorialKey.ToString() + " delay = " + startDelaySeconds);
        StartCoroutine(DoStart());
    }

    protected void HideButtons()
    {
        foreach (var bt in buttonsToHide)
        {
            if(bt == null) 
            {
                Debug.LogWarning("There is a null button to hide in tutorial " + TutorialKey);
                continue; 
            }
            bt.SetActive(false);
        }
    }

    protected void ShowButtons()
    {
        if (buttonsToShow != null && buttonsToShow.Length > 0)
        {
            foreach (var bt in buttonsToShow)
            {
                if (bt == null)
                {
                    Debug.LogWarning("There is a null button to show in tutorial " + TutorialKey);
                    continue;
                }
                bt.SetActive(true);
            }
        }
    }

    public IEnumerator DoStart()
    {
        yield return new WaitForSeconds(startDelaySeconds);
        TrackTutorialEvent(TutorialState.Started);
       
        _tutorialsContainer.gameObject.SetActive(true);
        _tutorialsContainer.Init(steps);
        GameManager.Log(TutorialKey.ToString() + " init with Steps " + _tutorialsContainer.gameObject.activeSelf);

        _tutorialsContainer.StartTuto(showTutoGirl, blockUI, CompleteTutorial, targetButton ? targetButton.GetComponent<Button>() : null);
        if (cameraTuto != null)
        {
            cameraTuto.Priority = 100;
        }
        if (targetButton)
        {
            boardController.tutorialPiece = new PieceState(PieceType.Type3,0);
        }
    }

    protected void TrackTutorialEvent(TutorialState state)
    {
        /*AnalyticsService.Instance.CustomData("TutorialEvent", new Dictionary<string, object>
        {
            { "TutorialName", TutorialKey.ToString() },
            { "TutorialState", state.ToString() }
        });*/
    }

    public virtual void CheckTutoEnd()
    {
        
    }

    public virtual void CompleteTutorial()
    {
        CompleteTutorialBase();
        if (TutorialKey == TutorialKeys.HouseWelcome) return;
        if (cameraTuto != null) cameraTuto.Priority = 0;
    }

    protected void CompleteTutorialBase() { 
        GameManager.Log(TutorialKey.ToString() + " Completed ");
        _tutoState = 1;
        isRunning = false;
        boardController.tutorialPiece = null;
        TrackTutorialEvent(TutorialState.Completed);
        PlayerPrefs.SetInt("Tutorial" + TutorialKey, 1);
        ShowObjectsOnCompleteTuto();
        OnCompleteMethod?.Invoke();
    }

    protected void ShowObjectsOnCompleteTuto()
    {
        foreach (var bt in ObjectsToShowOnComplete)
        {
            if (bt == null)
            {
                Debug.LogWarning("There is a null button to ShowObjectsOnCompleteTuto in tutorial " + TutorialKey);
                continue;
            }
            bt.SetActive(true);
        }
    }

    public bool IsThisTutoCompleted()
    {
        if(_tutoState == -1)
        {
            _tutoState = PlayerPrefs.GetInt("Tutorial" + TutorialKey);
        }
        return (_tutoState == 1);
    }

    protected void ResetTutorial()
    {
        MarkTutorialNotDone();
        for (var i = 0; i < TutorialsCompletedBefore.Count; i++)
        {
            GameManager.Instance.tutorialManager.MarkTutorialNotDone(TutorialsCompletedBefore[i]);
            PlayerPrefs.SetInt("Tutorial" + TutorialsCompletedBefore[i], 0);
        }
    }

    public void MarkTutorialNotDone()
    {
        PlayerPrefs.SetInt("Tutorial" + TutorialKey, 0);
        isRunning = false;
        _tutoState = 0;
    }

    public void AddMissionDrinkLevel3()
    {
        GameManager.Instance.MergeManager.boardController.AddOrder(PieceType.Drink1, 2);
    }
}