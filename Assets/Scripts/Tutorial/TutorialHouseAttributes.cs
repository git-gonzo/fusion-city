using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Localization;

public class TutorialHouseAttributes : TutorialBase
{
    public Button btnOpenAttributes;
    public Button btnCloseAttributes;
    public Button btnAttributesRandomizeAttributes;
    public List<string> textsSequenceRandomize;
    public List<string> textsSequenceRandomize2;
    public List<TutorialStep> Att2;
    public List<TutorialStep> Att3;

    public override void StartTutorial(TutorialSequence TutorialsContainer)
    {
        isRunning = true;
        _tutorialsContainer = TutorialsContainer;
        Debug.Log("Start TUTORIAL" + TutorialKey.ToString() + " delay = " + startDelaySeconds);
        StartCoroutine(DoStartAtt());
    }

    IEnumerator DoStartAtt()
    {
        yield return new WaitForSeconds(startDelaySeconds);
        _tutorialsContainer.Init(steps);
        _tutorialsContainer.StartTuto(false, blockUI, null, targetButton ? targetButton.GetComponent<Button>() : null,true, ShowLowerBar);
        if (cameraTuto != null)
        {
            cameraTuto.Priority = 100;
        }
    }

    private void ShowLowerBar()
    {
        foreach (var bt in buttonsToHide)
        {
            bt.SetActive(false);
        }
        var lowerbar = GameManager.Instance.lowerBar;
        var barPos = lowerbar.transform.localPosition;
        var seq = DOTween.Sequence();
        seq.Append(lowerbar.transform.DOLocalMoveY(lowerbar.transform.localPosition.y - 100, 0));
        seq.Append(lowerbar.transform.DOLocalMoveY(barPos.y, 1.5f).SetEase(Ease.OutBack));
        lowerbar.gameObject.SetActive(true);
        btnOpenAttributes.onClick.AddListener(OnAttributesScrennOpen);
    }

    private void OnAttributesScrennOpen()
    {
        btnOpenAttributes.onClick.RemoveListener(OnAttributesScrennOpen);
        btnCloseAttributes.gameObject.SetActive(false);
        StartCoroutine(StartRandomizeWithDelay(1));
    }

    IEnumerator StartRandomizeWithDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        _tutorialsContainer.Init(Att2);
        _tutorialsContainer.StartTuto(false, blockUI, OnRandomizeAttributes, btnAttributesRandomizeAttributes,false);
    }


    private void OnRandomizeAttributes()
    {
        Debug.Log("Start Randomize tuto 1 with delay 1");
        StartCoroutine(StartRandomizeWithDelay2(1));
    }

    IEnumerator StartRandomizeWithDelay2(int delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Start Randomize tuto 2");
        _tutorialsContainer.Init(Att3);
        _tutorialsContainer.StartTuto(false, blockUI, ShowCloseAttributesButton, null, false);
    }

    private void ShowCloseAttributesButton()
    {
        btnCloseAttributes.gameObject.SetActive(true);
        btnCloseAttributes.onClick.AddListener(OnAttributesScrennClose);
    }

    private void OnAttributesScrennClose()
    {
        CompleteTutorial();
        btnCloseAttributes.onClick.RemoveListener(OnAttributesScrennClose);
    }

}
