using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce3D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var s = DOTween.Sequence();
        s.Append(transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.2f));
        s.Append(transform.DOScale(new Vector3(1f, 1f, 1), 0.2f));
        s.SetLoops(-1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
