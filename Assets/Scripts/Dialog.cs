using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    public GameObject BlockUI_image;
    public Button buttonNext;
    public TextMeshProUGUI txtSpeech;
    public Image character;
    private Action OnDialogEnd;
    private List<LocalizedString> localizedKeys;
    private int _currentStep = 0;
    private CharacterStoryStep _storyStep;
    public MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
    public void StartDialog(CharacterStoryStep storySteps, Action onEnd = null, bool blockAllUI = true)
    {
        _storyStep = storySteps;
        localizedKeys = storySteps.localizedKeys;
        if (BlockUI_image != null)
        {
            BlockUI_image.SetActive(blockAllUI);
        }
        buttonNext.gameObject.SetActive(false);
        OnDialogEnd = onEnd;
        character.sprite = storySteps.CharacterImage;
        txtSpeech.DOFade(0, 0);
        txtSpeech.DOFade(1, 0.3f).SetDelay(0.25f).OnComplete(ShowButtonNext);
        if (_storyStep.hasMission && mergeModel.characterStoryModel.StepExists(_storyStep.id))
        {
            txtSpeech.GetComponent<LocalizeStringEvent>().StringReference = storySteps.waitingForMissionlocalizedKeys[_currentStep];
        }
        else
        {
            txtSpeech.GetComponent<LocalizeStringEvent>().StringReference = localizedKeys[_currentStep];
        }

        var tempStep = _currentStep;
        _currentStep++;
        
        gameObject.SetActive(true);
    }

    private void ShowButtonNext()
    {
        buttonNext.gameObject.SetActive(true);
        buttonNext.transform.localScale = new Vector3(0, 0, 0);
        buttonNext.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack).OnComplete(()=> buttonNext.onClick.AddListener(NextStep));
    }

    public void NextStep()
    {
        buttonNext.gameObject.SetActive(false);
        buttonNext.onClick.RemoveAllListeners();
        txtSpeech.DOFade(0, 0);

        //Finish Tutorial sequence?
        if (_currentStep == localizedKeys.Count)
        {
            DialogEnd();
            return;
        }

        txtSpeech.GetComponent<LocalizeStringEvent>().StringReference = localizedKeys[_currentStep];
        txtSpeech.DOFade(1, 0.3f).OnComplete(ShowButtonNext);
        var tempStep = _currentStep;
        _currentStep++;
    }

    private void DialogEnd()
    {
        if (_storyStep.hasRewards)
        {
            GameManager.Instance.GiveRewardsWithPopup(_storyStep.rewards,true);
        }
        
        if(_storyStep.hasMission && !mergeModel.characterStoryModel.StepExists(_storyStep.id))
        {
            _storyStep.mission.stepId = _storyStep.id;
            _storyStep.mission.characterId = _storyStep.character.characterId;
            mergeModel.characterStoryModel.StepState(_storyStep.id, false);
            mergeModel.AddMission(_storyStep.mission);
        }
        if (_storyStep.isUnique)
        {
            mergeModel.characterStoryModel.StepState(_storyStep.id, true);
        }
        OnDialogEnd?.Invoke();
        Destroy(gameObject);
    }
}