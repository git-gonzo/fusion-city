using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class PopupChangeName : MonoBehaviour
{
    public TextMeshProUGUI textPopupTitle;
    public TextMeshProUGUI textPopupDescrip;
    public TextMeshProUGUI textPlayerName;
    public GameObject buttonClose;
    public GameObject buttonChange;
    public TextMeshProUGUI textChangeAmount;
    public GameObject gemsIcon;
    public LocalizedString titleKey_firstTime;
    public LocalizedString descripKey_firstTime;
    public LocalizedString titleKey_after;
    public LocalizedString descripKey_after;
    public LocalizedString btnOk_firstTime;

    private int numChanges;
    private Action _onNameChanged;
    RewardData _changeNameCost;

    // Update is called once per frame
    void Update()
    {
        buttonChange.SetActive(textPlayerName.text.Length > 2);
    }

    public void ChangeName()
    {
        if (_changeNameCost != null) {
            if (!GameManager.TryToSpend(_changeNameCost))
            {
                Close();
                return;
            }
        }
        PlayerData.playerName = textPlayerName.text;
        PlayerPrefs.SetInt("playerNameNumChanges", ++numChanges);
        GameManager.Instance.playerData.textPlayerName.text = textPlayerName.text;
        TrackingManager.TrackAndSendChangeName(_changeNameCost);
        Close();
    }

    private void Close()
    {
        _onNameChanged?.Invoke();
        //Hardcoded Tutorial Change Avatar!
        if(PlayerData.characterIndex == 0)
        {
            GameManager.Instance.tutorialManager.StartTutorialChangeAvatar();
        }
    }

    public void Init(Action onNameChanged,float finalScale = 1)
    {
        var s = DOTween.Sequence();
        s.Append(transform.DOScale(0, 0));
        s.Append(transform.DOScale(finalScale, 0.3f).SetEase(Ease.OutBack));
        _onNameChanged = onNameChanged;
        numChanges = PlayerPrefs.GetInt("playerNameNumChanges");
        buttonChange.GetComponent<Button>().onClick.AddListener(ChangeName);
        buttonClose.GetComponent<Button>().onClick.AddListener(Close);
        gemsIcon.SetActive(numChanges > 1);
        buttonClose.SetActive(numChanges > 0);

        var LocTitle = textPopupTitle.GetComponent<LocalizeStringEvent>();
        var LocDesc = textPopupDescrip.GetComponent<LocalizeStringEvent>();
        LocTitle.StringReference = (numChanges == 0) ? titleKey_firstTime: titleKey_after;
        LocDesc.StringReference =  (numChanges == 0) ? descripKey_firstTime: descripKey_after;

        var cost = 0;
        if (numChanges == 2) cost = 100;
        else if (numChanges > 2) cost = 500;
        _changeNameCost = new RewardData(RewardType.Gems,cost);
        if (numChanges <= 1)
        {
            textChangeAmount.GetComponent<LocalizeStringEvent>().StringReference = btnOk_firstTime;
        }
        else
        {
            textChangeAmount.text = $"{(_changeNameCost.amount)}";
        }
        
    }

    
}
