using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Unity.Cinemachine;

[Serializable]
public class CharacterWithCamera
{
    public CinemachineCamera cameraPlayer;
    public GameObject character;
    public String characterName;
    public String characterAge;
    
}

public class PlayerSelectionManager : MonoBehaviour
{
    public int LimitCharacters = 2;
    public List<CharacterWithCamera> characters;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtAge;
    public CinemachineCamera cameraTuto;
    public GameObject Screen2;
    public Button btnNext;
    public Button btnPrev;
    public Button btnSelect;
    public TutorialManager tutoManager;
    public GameObject playerNamePopup;

    private bool firstTime = true;
    private int _current;

    private List<GameObject> charactersInstances;
    // Start is called before the first frame update
    void Start()
    {
        _current = 0;
        CamarasToLowPrio();
        characters[_current].cameraPlayer.Priority = 10;
        cameraTuto.Priority = 50;
        Screen2.SetActive(false);
        playerNamePopup.SetActive(false);
        /*if (tuto != null)
        {
            tuto.gameObject.SetActive(false);
        }*/
        if (MyScenesManager.Instance != null)
        {
            MyScenesManager.Instance.HideScreen();
        }
    }

   async void Update()
    {
        if (firstTime)
        {
            firstTime = false;
            //CheckInstances();
            //HideCharacters();
           /* UIUtils.DelayedCall(2, ()=> {
                tuto.StartTuto(false, TutorialStep3);
            }, this);*/
        }
        if (tutoManager != null)
        {
            await tutoManager.CheckTutorials();
        }
    }

    public void TutorialStep3()
    {
        txtName.enabled = false;
        txtAge.enabled = false;
        cameraTuto.Priority = 0;
        StartCoroutine(WaitForSelection());
    }

    public void NextCharacter()
    {
        //ActivateButtons(false);
        CamarasToLowPrio();
        ++_current;
        if (_current >= LimitCharacters) _current = 0;
        //CheckInstances();
        //charactersInstances[_current].SetActive(true);
        characters[_current].cameraPlayer.Priority = 10;
        txtName.enabled = false;
        txtAge.enabled = false;
        //UIUtils.DelayedCall(0.8f,SetName,this);
    }

    public void PrevCharacter()
    {
        //ActivateButtons(false);
        CamarasToLowPrio();
        --_current;
        //CheckInstances();
        if (_current < 0) _current = LimitCharacters-1;
        //charactersInstances[_current].SetActive(true);
        characters[_current].cameraPlayer.Priority = 10;
        txtName.enabled = false;
        txtAge.enabled = false;
        //UIUtils.DelayedCall(0.8f, SetName, this);
    }

    public void SelectCharacter()
    {
        ActivateButtons(false);
        PlayerPrefs.SetInt("playerCharacterIndex",_current);
        PlayerPrefs.SetInt("gems", 50);
        PlayerPrefs.SetInt("coins", 100);
        ShowPlayerNamePopup();
    }

    public void ShowPlayerNamePopup()
    {
        playerNamePopup.SetActive(true);
        playerNamePopup.GetComponent<PopupChangeName>().Init(LoadNextScene);
    }

    public void LoadNextScene()
    {
        StartCoroutine(WaitForNextScene());
    }

    IEnumerator WaitForSelection()
    {
        yield return new WaitForSeconds(1);
        //SetName();
        Screen2.SetActive(true);
    }
    IEnumerator WaitForNextScene()
    {
        MyScenesManager.Instance.ShowScreen();
        yield return new WaitForSeconds(0.34f);
        SceneManager.LoadScene("HouseScene2");
    }
    private void ActivateButtons(bool value)
    {
        btnNext.gameObject.SetActive(value);
        btnPrev.gameObject.SetActive(value);
        btnSelect.gameObject.SetActive(value);
    }
    private void SetName()
    {
        ActivateButtons(true);
        HideCharacters();
        return;
    }

    void HideCharacters()
    {
        CheckInstances();
        for (var i=0; i < characters.Count; i++)
        {
            charactersInstances[i].SetActive(i==_current);
        }
    }

    void CheckInstances()
    {
        if (charactersInstances == null)
        {
            charactersInstances = new List<GameObject>();
            for (var i = 0; i < characters.Count; i++)
            {
                charactersInstances.Add(GameObject.Find(characters[i].character.name));
            }
        }
    }

    private void CamarasToLowPrio()
    {
        for(var i = 0; i < characters.Count; i++)
        {
            characters[i].cameraPlayer.Priority = 1;
        }
    }

}
