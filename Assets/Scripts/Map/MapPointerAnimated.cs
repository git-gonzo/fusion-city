using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MapPointerAnimated : MonoBehaviour
{
    public Transform target;
    public Vector3 RotateDirection;
    public int speed;

    public TrailRenderer trail;
    public Material materialInProgress;
    public Material materialComplete;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(target.position, RotateDirection, speed * Time.deltaTime);
    }

    public void setMaterialInProgress()
    {
        trail.material = materialInProgress;
    }
    public void setMaterialComplete()
    {
        trail.material = materialComplete;
    }
}
