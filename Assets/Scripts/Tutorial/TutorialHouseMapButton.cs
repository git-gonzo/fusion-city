using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialHouseMapButton : TutorialBase
{
    public GameObject MapButton;

    public override void StartTutorial(TutorialSequence TutorialsContainer)
    {
        isRunning = true;
        foreach (var bt in buttonsToHide)
        {
            bt.SetActive(false);
        }
        _tutorialsContainer = TutorialsContainer;
        Debug.Log("Start TUTORIAL" + TutorialKey.ToString() + " delay = " + startDelaySeconds);
        StartCoroutine(DoStart());
    }

    IEnumerator DoStart()
    {
        yield return new WaitForSeconds(startDelaySeconds);
        _tutorialsContainer.Init(steps);
        _tutorialsContainer.StartTuto(false, blockUI, CompleteTutorial, targetButton ? targetButton.GetComponent<Button>() : null, true, ShowMapButton);
        if (cameraTuto != null)
        {
            cameraTuto.Priority = 100;
        }
    }

    private void ShowMapButton()
    {
        
        
        var seq = DOTween.Sequence();
        seq.Append(MapButton.transform.DOScale(0, 0));
        seq.Append(MapButton.transform.DOScale(1, 1).SetEase(Ease.OutBack));
        MapButton.SetActive(true);
    }
}