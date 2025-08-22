using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ContentResize : MonoBehaviour
{
    private List<Transform> children;
    private float totalHeight;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*GetChildren();
        for (var i = 0; i < children.Count; i++)
        {

        }*/
    }

    private void GetChildren()
    {
        if (transform.childCount == children.Count) return;
        children = new List<Transform>();
        for(var i= 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
       
    }

    public void SetHeight(float value, Action onCompleteHeight)
    {
        var rt = GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = value;
        rt.DOSizeDelta(size,0.2f).OnComplete(()=> { onCompleteHeight?.Invoke(); });
    }
}
