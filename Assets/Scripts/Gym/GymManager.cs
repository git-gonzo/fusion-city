using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class GymManager : MonoBehaviour
{
    public Animator gymController;
    public Animator introCamara;
    public Animator introCharacter;
    public TutorialSequence tuto;
    public CinemachineCamera cameraTuto;
    public GameObject playerRef;

    private GameObject player;

    

    // Start is called before the first frame update
    void Start()
    {
        introCamara.StopPlayback();
        UIUtils.DelayedCall(0.1f, MyScenesManager.Instance.HideScreen, this);
        UIUtils.DelayedCall(1.5f, StartTutorial, this);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTutorial()
    {
        introCamara.SetTrigger("StartTutorial");
        UIUtils.DelayedCall(2f, TutorialStep2, this);
    }
    public void TutorialStep2()
    {
        UIUtils.DelayedCall(2.4f, () => {tuto.StartTuto(false, false, TutoEnd); }, this);
        introCharacter.SetTrigger("StartTutorial");
    }
    public void TutoEnd()
    {
        cameraTuto.Priority = 1;
        player = playerRef.transform.GetChild(0).gameObject;
        player.GetComponent<Animator>().runtimeAnimatorController = gymController.runtimeAnimatorController;
        UIUtils.DelayedCall(1.5f, InitMinigame, this);
    }

    void InitMinigame()
    {
        GetComponent<GymMiniGame>().Init(player.GetComponent<Animator>());
    }
}
