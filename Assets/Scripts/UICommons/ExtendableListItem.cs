using System.Collections;
using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class ExtendableListItem : MonoBehaviour
{
    public Button BtnExpand;
    public Button BtnClaim;
    public TextMeshProUGUI textTitle;
    public TextMeshProUGUI textDescrip;
    public Transform expandContent;
    protected bool isOpen;
    protected Vector2 sizeClosed;
    public Vector2 sizeOpen;
    public Vector2 sizeContent;

    protected Action _onItemOpen;
    protected RectTransform t;

    protected void BaseInit()
    {
        sizeClosed = GetComponent<RectTransform>().sizeDelta;
        sizeContent = expandContent.GetComponent<RectTransform>().sizeDelta;
        sizeOpen = new Vector2(sizeClosed.x, sizeClosed.y + sizeContent.y);
        expandContent.DOLocalMoveY(sizeContent.y, 0f);
        CloseExpanded(true);
        AddListeners();
        isOpen = false;
    }

    public void CloseIfOpen()
    {
        if (isOpen)
        {
            OnClickExpand();
        }
    }

    protected virtual void OnClickExpand()
    {
        if (isOpen)
        {
            CloseExpanded();
        }
        else
        {
            _onItemOpen?.Invoke();
            expandContent.DOLocalMoveY(0, 0.2f).OnStart(() => expandContent.gameObject.SetActive(true));
        }
        t.DOSizeDelta(isOpen ? sizeClosed : sizeOpen, 0.2f).OnUpdate(() => {
            transform.parent.GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
        });
        isOpen = !isOpen;
    }

    protected virtual Tween CloseExpanded(bool forced = false)
    {
        return expandContent.DOLocalMoveY(sizeContent.y, forced ? 0 : 0.2f).OnComplete(() => expandContent.gameObject.SetActive(false));
    }

    protected virtual void AddListeners()
    {
        BtnExpand.onClick.RemoveAllListeners();
        BtnClaim.onClick.RemoveAllListeners();
        
        BtnExpand.onClick.AddListener(OnClickExpand);
    }

}
