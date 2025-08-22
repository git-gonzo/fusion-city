using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PopupTutorial : PopupBase
{
    public List<Transform> animatedObjects;
    // Start is called before the first frame update
    public void Init(Action OnClose)
    {
        onCloseCallback = OnClose;
        Show();
        AnimObjects();
    }

    private void AnimObjects()
    {
        var seq = DOTween.Sequence();
        for(var i = 0;i <animatedObjects.Count; i++)
        {
            seq.Insert(i * 0.15f, animatedObjects[i].DOScale(0, 0.3f).From());
        }
        seq.SetDelay(0.3f);
    }
}
