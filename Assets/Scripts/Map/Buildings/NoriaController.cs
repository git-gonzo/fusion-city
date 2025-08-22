using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NoriaController : MonoBehaviour
{
    public GameObject wheel;
    public GameObject[] cabinas;

    // Start is called before the first frame update
    void Start()
    {
        PlayRotation();
    }

    // Update is called once per frame
    void Update()
    {


    }

    //Todo fix rotation stop, idea: end value = current + desired --- FIXED

    public void PlayRotation()
    {
        var rot = wheel.transform.localEulerAngles + new Vector3(0, 0, 45);
        wheel.transform.DOLocalRotate(rot, 7, RotateMode.FastBeyond360).SetEase(Ease.InBack).OnComplete(() =>
        {
            var rot2 = wheel.transform.localEulerAngles + new Vector3(0, 0, 315);
            wheel.transform.DOLocalRotate(rot2, 10f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(2, LoopType.Incremental)
            .OnUpdate(UpdateCabinas).OnComplete(StopRotation);
        }).OnUpdate(UpdateCabinas);
    }
    public void StopRotation()
    {
        DOTween.Kill(wheel.transform);
        var rot = wheel.transform.localEulerAngles + new Vector3(0,0,45);
        //wheel.transform.DOLocalRotate(new Vector3(0, 0, currentRot), 0);
        wheel.transform.DOLocalRotate(rot, 7, RotateMode.FastBeyond360).SetEase(Ease.OutBack).OnUpdate(UpdateCabinas); ;
        UIUtils.DelayedCallWithTween(10, PlayRotation);
    }

    private void UpdateCabinas()
    {
        var rot = wheel.transform.localRotation.eulerAngles*-1;
        foreach (var cabina in cabinas)
        {
            cabina.transform.DOLocalRotate(rot, 0f);
        }
    }
}
