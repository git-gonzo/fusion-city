using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HelicopterController : MonoBehaviour
{
    public GameObject helices;
    private Sequence seq;

    public bool fly;
    public bool isFlying;

    // Start is called before the first frame update
    void Start()
    {
        //PlayHelices();
    }

    // Update is called once per frame
    void Update()
    {
        if (fly && !isFlying)
        {
            isFlying = true;
            PlayHelices();
        }
        else if (!fly && isFlying)
        {
            isFlying = false;
            StopHelices();
        }
    }

    public void PlayHelices()
    {
        helices.transform.DOLocalRotate(new Vector3(0, 1440, 0), 4, RotateMode.FastBeyond360).SetEase(Ease.InCubic).OnComplete(() =>
        {
            helices.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
        });
    }

    public void StopHelices()
    {
        DOTween.Kill(helices.transform);
        helices.transform.DOLocalRotate(new Vector3(0, 0, 0), 0);
        helices.transform.DOLocalRotate(new Vector3(0, 2880, 0), 6, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);
    }

    public void FlyAndStop(int delay = 3)
    {
        helices.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.4f, RotateMode.FastBeyond360).SetEase(Ease.Linear)
            .SetLoops(delay*2, LoopType.Incremental)
            .OnComplete(StopHelices);
    }
}
