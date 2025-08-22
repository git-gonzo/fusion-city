using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SideBarButton : ButtonWithTooltip
{
    public GameObject bubleBackGreen;
    public GameObject bubleBackRed;
    public Image bgImage;
    public Image foreImage;
    public Color shinecolor;
    public Color shinecolor2;
    public Color normalcolor;
    public Color normalcolor2;
    private int _bubbleAmount;
    private bool _isShining;

    public void SetBubbleCounter(int number, bool isGreen)
    {
        bubleContainer.SetActive(number > 0);
        bubleBackGreen.SetActive(isGreen);
        bubleBackRed.SetActive(!isGreen);
        textBubleCounter.text = number.ToString();
        if(_bubbleAmount < number)
        {
            Punch();
        }
        _bubbleAmount = number;

    }
    public void SetBubbleExclamation(bool isGreen = true)
    {
        textBubleCounter.text = "!";
        bubleBackGreen.SetActive(isGreen);
        bubleBackRed.SetActive(!isGreen);
    }

    public void Shine(bool value)
    {
        bgImage.color = value ? shinecolor : normalcolor;
        foreImage.color = value ? shinecolor2 : normalcolor2;
        if(!_isShining && value)
        {
            bg.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        } 
        else if (_isShining && !value)
        {
            bgImage.transform.DOKill();
            bgImage.transform.DOScale(Vector3.one, 0.2f);
        }
        _isShining = value;
    }
}
