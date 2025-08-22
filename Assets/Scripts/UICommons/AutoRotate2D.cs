using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AutoRotate2D : MonoBehaviour
{
    public float duration = 1;

    private void Awake()
    {
        transform.DOLocalRotate(new Vector3(0,0, 360), duration, RotateMode.FastBeyond360).SetLoops(-1,LoopType.Incremental).SetEase(Ease.Linear);
    }
}
