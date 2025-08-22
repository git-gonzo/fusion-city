using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialPointer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var seq = DOTween.Sequence();
        seq.Insert(0,transform.DOLocalMoveY(-70, 0.45f)).SetEase(Ease.InSine);
        seq.Insert(0.2f,transform.DOScaleY(0.8f, 0.3f)).SetEase(Ease.InExpo);
        seq.Insert(0.25f,transform.DOScaleX(1.1f, 0.25f)).SetEase(Ease.InExpo);
        seq.Insert(0.5f,transform.DOLocalMoveY(0, 0.54f)).SetEase(Ease.OutSine);
        seq.Insert(0.5f,transform.DOScaleY(1f, 0.3f)).SetEase(Ease.InSine);
        seq.Insert(0.5f,transform.DOScaleX(1f, 0.25f)).SetEase(Ease.InSine);
        seq.SetLoops(-1);
    }

    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}
