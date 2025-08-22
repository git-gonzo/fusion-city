using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BounceElement : MonoBehaviour
{
    [Range(0f, 1f)]
    public float scale = 0.5f;

    [Range(0f, 1f)]
    public float time = 1;
    // Start is called before the first frame update
    void Start()
    {
        transform.DOPunchScale(Vector3.one * scale, time, 1).SetLoops(-1);
        /*var s = DOTween.Sequence();
        s.Append(transform.DOScale(1.2f, 0.2f));
        s.Append(transform.DOScale(1f, 0.2f));
        s.SetLoops(-1);*/
    }


}
