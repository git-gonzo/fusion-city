using System.Collections;
using System.Reflection;
using UnityEngine;

public class TutorialStorageRewards : TutorialBase
{
    public override bool CanStartTutorial()
    {
        var canStartNormal = base.CanStartTutorial();
        if (!canStartNormal) return false;

        if (!GameManager.Instance.mergeStorageScreen.activeSelf)
        {
            return false;
        }

        if (GameManager.Instance.StorageController.giftButtons.Count == 0)
        {
            return false;
        }

        return base.CanStartTutorial();
    }

    public override void StartTutorial(TutorialSequence TutorialsContainer)
    {
        isRunning = true;
        HideButtons();
        ShowButtons();
        _tutorialsContainer = TutorialsContainer;
        StartCoroutine(DoStartGifts());
    }

    IEnumerator DoStartGifts()
    {
        yield return new WaitForSeconds(startDelaySeconds);
        var btn = GameManager.Instance.StorageController.giftButtons[0];
        _tutorialsContainer.SetTargetButton(btn);
        _tutorialsContainer.AddArrowPointer(btn, true);
        btn.onClick.AddListener(CompleteTutorial);
    }

    public override void CompleteTutorial()
    {
        _tutorialsContainer.RemoveCanvasComponent(GameManager.Instance.StorageController.giftButtons[0].gameObject);
        CompleteTutorialBase();
        GameManager.Instance.StorageController.giftButtons[0].onClick.RemoveListener(CompleteTutorial);
    }
}