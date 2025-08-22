using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class TutorialSequence : MonoBehaviour
{
    public Action<int> OnNextStep; 
    public Action OnTutoEnd;
    public GameObject BlockUI_image;
    public Button buttonNext;
    public GameObject frame;
    public TextMeshProUGUI txtSpeech;
    public List<TutorialStep> steps;
    public GameObject pointer;
    public GameObject hand;
    public GameObject tutoGirl;
    private Button _targetButton;
    private GameObject _targetPointer;
    private int _currentStep = 0;
    private float initialScale = 1;
    private TutorialHand tutorialHand;

    public void Init(List<TutorialStep> stepsParam)
    {
        buttonNext.gameObject.SetActive(false);
        steps = stepsParam;
        _currentStep = 0;
        _targetButton = null;
        OnTutoEnd = null;
    }
    public void Init(TutorialMergeStep stepsParam)
    {
        steps = new List<TutorialStep>();
        
        var n = new TutorialStep();
        n.localizedKey = stepsParam.localizedKey;
        n.hideContinueButton = stepsParam.hideContinueButton;
        n.hideTextBubble= stepsParam.hideTextBubble;
        //n.targetToTap = GameManager.Instance.MergeManager.boardController.FindPieceOfType(p.targetPiece);
        steps.Add(n);
        
        buttonNext.gameObject.SetActive(false);
        _currentStep = 0;
        _targetButton = null;
        OnTutoEnd = null;
    }

    public void StartTuto(bool showTutoGirl, bool blockAllUI, Action onEnd, Button targetButton = null,bool pointerOnTop = true, Action OnFinishAnimatingText = null)
    {
        if (BlockUI_image != null)
        {
            BlockUI_image.SetActive(blockAllUI);
        }
        SetButtons(targetButton);
        OnTutoEnd = onEnd;
        txtSpeech.DOFade(0, 0);
        tutoGirl.SetActive(showTutoGirl);
        if (steps[_currentStep].hideTextBubble)
        {
            gameObject.SetActive(false);
            if (steps[_currentStep].targetToTap != null)
            {
                SetButtons(steps[_currentStep].targetToTap.GetComponent<Button>());
                AddPointer(steps[_currentStep]);
            }
            _currentStep++;
        }
        else
        {
                txtSpeech.DOFade(1, 0.3f);
                txtSpeech.GetComponent<LocalizeStringEvent>().StringReference = steps[_currentStep].localizedKey;

                var tempStep = _currentStep;
                _currentStep++;
                if (HasSteps()) // New System STEPS
                {
                    if (steps[tempStep].targetToTap == null)
                    {
                        if (!steps[tempStep].hideContinueButton)
                        {
                            ShowButtonNext();
                        }
                    }
                    else
                    {
                        Debug.Log("Show arrow pointer, step " + tempStep);
                        var btn = steps[tempStep].targetToTap.GetComponent<Button>();
                        SetButtons(btn);
                        AddArrowPointer(btn, steps[tempStep].pointerBounceOnTop);
                    }
                }
                else //Old System
                {
                    GameManager.Log($"<color={Color.cyan}>Atention: Tutorial using old system, consider updating it");
                    if (targetButton == null)
                    {
                        if (!steps[tempStep].hideContinueButton)
                        {
                            ShowButtonNext();
                        }
                    }
                    else
                    {
                    //SetButtons(steps[_currentStep].targetToTap.GetComponent<Button>());
                    AddArrowPointer(targetButton, pointerOnTop);
                    }
                }
            gameObject.SetActive(true);
        }
    }

    private bool HasSteps()
    {
        return steps != null && steps.Count > 0;
    }

    private void SetButtons(Button targetButton = null)
    {
        if (targetButton == null)
        {
            if (buttonNext == null)
            {
                if (gameObject.GetComponent<Button>() == null)
                {
                    _targetButton = gameObject.AddComponent<Button>();
                }
                else
                {
                    _targetButton = gameObject.GetComponent<Button>();
                }
            }
            else
            {
                _targetButton = buttonNext;
                buttonNext.gameObject.SetActive(false);
                _targetButton.onClick.RemoveAllListeners();
            }
        }
        else
        {
            _targetButton = targetButton;
            _targetButton.onClick.RemoveListener(NextStep);
            _targetButton.gameObject.SetActive(true);
        }
        _targetButton.onClick.AddListener(NextStep);
    }

    public void SetTargetButton(Button button)
    {
        _targetButton = button;
    }

    private void ShowButtonNext()
    {
        buttonNext.gameObject.SetActive(true);
        buttonNext.transform.localScale = new Vector3(0, 0, 0);
        buttonNext.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
    }
    public void AddArrowPointer(Button targetButton, bool pointerOnTop)
    {
        Debug.Log("AddArrowPointer");
        //Add arrow pointer
        if (_targetPointer != null) DestroyImmediate(_targetPointer.gameObject);
        _targetPointer = Instantiate(pointer, targetButton.transform,false);
        AddCanvasComponent(_targetButton.gameObject);
        MoveArrowPointer(pointerOnTop);
    }    

    private void AddCanvasComponent(GameObject gameObject)
    {

        var canvascomponent = gameObject.AddComponent<Canvas>();
        canvascomponent.enabled = true;
        gameObject.AddComponent<GraphicRaycaster>();
        canvascomponent.overrideSorting = true;
        canvascomponent.sortingOrder = 100;
        GameManager.Instance.tutorialManager.ShowOverlay();
    }

    public void RemoveCanvasComponent(GameObject gameObject)
    {
        Debug.Log("Destroy Component");
        Destroy(gameObject.GetComponent<GraphicRaycaster>());
        Destroy(gameObject.GetComponent<Canvas>());
        GameManager.Instance.tutorialManager.HideOverlay();
    }

    public void AddPointer(GameObject target)
    {
        //Add arrow pointer
        if (_targetPointer != null) DestroyImmediate(_targetPointer.gameObject);
        _targetPointer = Instantiate(pointer, target.transform, false);
        MoveArrowPointer(true);
    }
    public void AddPointer(TutorialStep step)
    {
        Debug.Log("AddPointer Step");
        //Add handPointer
        if (step.useHandPointer)
        {
            CreatePointerHand(step.targetToTap.transform, step.handPosition);
            return;
        }
        //AddArrowPointer(_targetButton, step.pointerBounceOnTop);
        if (_targetPointer != null) DestroyImmediate(_targetPointer.gameObject);
        _targetPointer = Instantiate(pointer, step.targetToTap.transform, false);
        AddCanvasComponent(step.targetToTap);
        MoveArrowPointer(step.pointerBounceOnTop);
    }

    private void MoveArrowPointer(bool pointerOnTop)
    {
        var pos = new Vector3(0, pointerOnTop ? 170 : -120, 0);
        _targetPointer.transform.DOLocalMoveY(pos.y, 0f);
        if (!pointerOnTop)
        {
            _targetPointer.transform.DORotate(new Vector3(180, 0, 0), 0);
        }
    }

    public void NextStep()
    {
        //Finish Tutorial sequence?
        if (_targetPointer != null)
        {
            RemoveCanvasComponent(_targetButton.gameObject);
            Destroy(_targetPointer);
        }
        if (tutorialHand != null) DestroyImmediate(tutorialHand.gameObject);
        if (steps != null && _currentStep == steps.Count) 
        {
            _targetButton.onClick.RemoveListener(NextStep);
            //frame.transform.DOScale(initialScale * 0.8f, 0.1f).SetEase(Ease.InBack).OnComplete(() => { 
                OnTutoEnd?.Invoke(); 
                gameObject.SetActive(false); 
                txtSpeech.DOFade(0, 0); 
            //});
            return;
        }

        if(HasSteps() && steps[_currentStep].targetToTap == null)
        {
            buttonNext.gameObject.SetActive(false);
        }

        if (!steps[_currentStep].localizedKey.IsEmpty)
        {
            txtSpeech.GetComponent<LocalizeStringEvent>().StringReference = steps[_currentStep].localizedKey;
        }

        if (steps[_currentStep].hideTextBubble)
        {
            SetButtons(steps[_currentStep].targetToTap.GetComponent<Button>());
            AddPointer(steps[_currentStep]);
            gameObject.SetActive(false); 
            _currentStep++;
            OnNextStep?.Invoke(_currentStep);
        }
        else
        {
            if (HasSteps())
            {
                if (steps[_currentStep].targetToTap == null)
                {
                    SetButtons();
                }
                else
                {
                    SetButtons(steps[_currentStep].targetToTap.GetComponent<Button>());
                }
            }
            var tempStep = _currentStep;
            _currentStep++;
            //UIUtils.AnimateText(txtSpeech, this, () =>
            //{
                // OnFinishAnim - Stopped animating text
                if (HasSteps())
                {
                    if (steps[tempStep].targetToTap == null)
                    {
                        SetButtons();
                        if (!steps[tempStep].hideContinueButton)
                        {
                            ShowButtonNext();
                        }
                    }
                    else
                    {
                        SetButtons(steps[tempStep].targetToTap.GetComponent<Button>());
                        AddArrowPointer(steps[tempStep].targetToTap.GetComponent<Button>(), steps[tempStep].pointerBounceOnTop);
                    }
                }
                OnNextStep?.Invoke(tempStep);
        }
    }

    private void CreatePointerHand(Transform target,Vector3 position)
    {
        if (hand == null) return;

        tutorialHand = Instantiate(hand, target).GetComponentInChildren<TutorialHand>();
        var pos = tutorialHand.transform.position + position;
        tutorialHand.Init(pos, true);
        //var pos = targetPiece.transform.position - Vector3.up * 40;
        //_handPointer.transform.position = pos;
        //tutorialHand ??= _handPointer.GetComponentInChildren<TutorialHand>();
    }
}