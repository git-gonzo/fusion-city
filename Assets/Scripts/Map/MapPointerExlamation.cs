using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPointerExlamation : MonoBehaviour
{
    public GameObject greenExclamation;
    public GameObject yellowExclamation;
    // Start is called before the first frame update
    void Start()
    {
        var s = DOTween.Sequence();
        s.Append(transform.DOScale(new Vector3(1.25f, 0.65f , 1), 0.15f).SetEase(Ease.InOutSine));
        s.Append(transform.DOScale(new Vector3(1f   , 1f    , 1), 0.1f).SetEase(Ease.InSine));
        s.Append(transform.DOScale(new Vector3(1.15f, 1.35f , 1), 0.4f).SetEase(Ease.OutExpo));
        s.Append(transform.DOScale(new Vector3(1    , 1f    , 1), 0.4f).SetEase(Ease.OutBounce));
        s.SetLoops(-1);
        s.SetDelay(0.5f);
    }

    public void SetReady(bool value)
    {
        greenExclamation.SetActive(value);
        yellowExclamation.SetActive(!value);
    }
}
