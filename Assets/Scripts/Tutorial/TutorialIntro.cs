using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TutorialIntro : TutorialBase
{
    [SerializeField]
    PlayableDirector director;
    public override void StartTutorial(TutorialSequence TutorialsContainer)
    {
        isRunning = true;
        foreach (var bt in buttonsToHide)
        {
            bt.SetActive(false);
        }
        _tutorialsContainer = TutorialsContainer;
        GameManager.Log("Start TUTORIAL" + TutorialKey.ToString() + " delay = " + startDelaySeconds);
        director.Play();
        TrackTutorialEvent(TutorialState.Started);
        if (cameraTuto != null)
        {
            cameraTuto.Priority = 100;
        }
    }
}
