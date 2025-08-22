using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class MuebleIndex : MonoBehaviour
{
    public GameObject currentIndex;
    public GameObject selected;
    public TextMeshProUGUI label;
    public TextMeshProUGUI labelOver;
    bool _owned;

    private Vector3 isIndexPosition = Vector3.zero;
    private Vector3 normalPosition;

    private void Start()
    {
        normalPosition = transform.localPosition;
    }

    public bool IsIndex
    {
        set
        {
            /*if(normalPosition == Vector3.zero)
            {
                normalPosition = transform.localPosition;
                isIndexPosition = normalPosition + Vector3.up*10;
            }*/
            currentIndex.SetActive(value);
            //transform.DOLocalMove(normalPosition + (value ? Vector3.up * 10 : Vector3.zero), 0.2f).SetEase(Ease.OutExpo);
        }
    }
    public bool IsSelected
    {
        set
        {
            selected.SetActive(value);
        }
    }
    public bool Locked
    {
        set
        {
            if (value)
            {
                label.text = "X";
                label.color = Color.grey;
            }
        }
    }
    public bool Owned
    {
        set
        {
            if (value)
            {
                label.text = "V";
                label.color = Color.green;
                _owned = value;
            }
            
        }
        get
        {
            return _owned;
        }
    }
    public int Index
    {
        set
        {
            label.text = value.ToString();
            label.color = Color.white;
            labelOver.text = value.ToString();
        }
    }

}
