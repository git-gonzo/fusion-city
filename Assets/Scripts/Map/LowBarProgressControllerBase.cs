using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class LowBarProgressControllerBase : MonoBehaviour
{
    public Button btnTap;
    public Button btnCancel;
    public Button btnFinishNow;
    public TextMeshProUGUI textTitle;
    public ProgressBar progressBar;
    public GameObject moreInfoContainer;
    public GameObject arrowOpenClose;
    public GameObject mainContainer;
    public float _outPositionX;
    public bool hideToLeft;

    protected bool _initialized;
    protected float _startPositionX;
    protected DateTime nextCheck;

    private bool _showingButtons;
    private float _buttonsPositionY;
    protected int ButtonContYDistance = 140;
    protected bool showingProgress;

    public virtual void Init()
    {
        _showingButtons = true;
        nextCheck = DateTime.Now;
        _startPositionX = transform.position.x;
        _outPositionX = mainContainer.GetComponent<RectTransform>().rect.width* (hideToLeft?-2:2);
        transform.DOMoveX(_startPositionX + _outPositionX, 0f);
        _buttonsPositionY = moreInfoContainer.transform.localPosition.y;
        //moreInfoContainer.transform.DOLocalMoveY(_buttonsPositionY - ButtonContYDistance, 0);
        btnCancel.gameObject.SetActive(GameManager.Instance.PlayerLevel > 1);
        btnTap.onClick.AddListener(ShowHideButtons);
        showingProgress = false;
        _initialized = true;
    }

    public void ShowHideButtons()
    {
        if (!DOTween.IsTweening(moreInfoContainer.transform))
        {
            moreInfoContainer.transform.DOLocalMoveY(_showingButtons ? _buttonsPositionY - ButtonContYDistance : _buttonsPositionY, 0.5f).SetEase(_showingButtons ? Ease.InBack : Ease.OutBack);
            var btnRotation = new Vector3(0, 0, _showingButtons ? 180 : 0);
            arrowOpenClose.transform.DOLocalRotate(btnRotation, 0.3f);
            _showingButtons = !_showingButtons;
        }
    }

    public void Hide()
    {
        showingProgress = false;
        transform.DOMoveX(_startPositionX + _outPositionX, 0.7f).SetEase(Ease.InQuad).OnComplete(()=>gameObject.SetActive(false));
    }
}
