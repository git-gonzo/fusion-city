using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BouncyPiece : MonoBehaviour
{
    public List<Transform> bouncyElements;
    Sequence seq;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoBounceElementes()
    {
        if (seq != null)
        {
            return;
        }
        seq = seq??DOTween.Sequence();
        for(var i = 0; i < bouncyElements.Count;i++)
        {
            var t = bouncyElements[i];
            seq.Insert(i * 0.02f, t.DOPunchPosition(Vector3.up* Random.Range(5, 6)*(i+1), 0.5f,Random.Range(3,5)));
        }
        seq.OnComplete(() => { seq = null; });
    }
}
