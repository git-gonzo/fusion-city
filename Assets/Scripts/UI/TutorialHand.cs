using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialHand : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Vector3 handPosOffset;
    Sequence seq;
    Animator _animator;
    

    public void Init(Vector3 pos, bool animateTap = false, bool animateDrag = false, Vector3 to = new Vector3())
    {
        //canvasGroup.DOFade(0, 0.1f).OnComplete(() => {
            transform.position = pos + handPosOffset;
            if (seq != null) seq.Kill();
            canvasGroup.alpha = 0;
            if (animateTap) AnimateTap();
            if (animateDrag) AnimateDrag(to + handPosOffset);
        //});
    }

    public void AnimateTap()
    {
        _animator ??= GetComponent<Animator>();
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, 0.2f).SetDelay(0.1f);
        _animator.SetTrigger("AnimateTap");
    }
    
    public void AnimateDrag(Vector3 targetPos)
    {
        _animator ??= GetComponent<Animator>();
        var dragDuration = Vector3.Magnitude(transform.position - targetPos) *0.0015f;
        if (seq != null) seq.Kill();
        seq = DOTween.Sequence();
        seq.Insert(0, transform.DOLocalMoveZ(0, 0.1f).OnComplete(OnStartDrag));
        seq.Insert(0.9f, transform.DOMove(targetPos, dragDuration).SetEase(Ease.InOutQuad).OnComplete(()=>_animator.SetBool("FinishDrag", true)));
        seq.Insert(1.3f + dragDuration, canvasGroup.DOFade(0, 0.2f).OnComplete(()=> _animator.SetBool("FinishDrag", false)));
        seq.SetLoops(-1).SetDelay(0.5f);
    }

    private void OnStartDrag()
    {
        _animator.SetBool("FinishDrag", false);
        _animator.SetTrigger("StartDrag");
        canvasGroup.DOFade(1, 0.2f);
    }
}
