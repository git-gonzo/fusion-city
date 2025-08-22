using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class PopupYesNo : PopupBase
{
    public Button btnYes;
    public Button btnNo;
    public Button btnOK;
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtDescrip;

    Action _onNoCallback;
    Action _onYesCallback;

    public void Init(string title, string descrip, Action onYesCallback, Action onNoCallback)
    {
        txtTitle.text = title;
        txtDescrip.text = descrip;
        btnNo.onClick.AddListener(OnClose);
        btnYes.onClick.AddListener(OnYes);
        btnOK.gameObject.SetActive(false);
        _onNoCallback = onNoCallback;
        _onYesCallback = onYesCallback;
        transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
    }
    public void InitOk(string title, string descrip, Action onYesCallback)
    {
        txtTitle.text = title;
        txtDescrip.text = descrip;
        btnNo.gameObject.SetActive(false);
        btnYes.gameObject.SetActive(false);
        btnOK.onClick.RemoveAllListeners();
        btnOK.onClick.AddListener(OnYes);
        _onYesCallback = onYesCallback;
        transform.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.2f);
    }

    private void OnClose()
    {
        _onNoCallback?.Invoke();
    }

    private void OnYes()
    {
        _onYesCallback?.Invoke();
    }

    private void AnimIn()
    {
        this.transform.DOScale(0.5f, 0);
        var seq = DOTween.Sequence();
       
    }
}
