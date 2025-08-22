using DG.Tweening;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleInstance))]
public class VehicleWheels : MonoBehaviour
{
    public bool wheelsRotation;
    public float wheelsRotationSpeed;
    public List<Transform> wheels;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void rotateWheelsAppearaing(int duration, float rotation)
    {
        
        foreach (var wheel in wheels)
        {
            var rot = wheel.transform.localRotation.eulerAngles + Vector3.right * rotation * (wheel.transform.localRotation.eulerAngles.y==0?1:-1);
            wheel.DOLocalRotate(rot, duration, RotateMode.FastBeyond360).SetEase(Ease.OutExpo);
        }
        
    }

    public void RotateWheelsConstantly()
    {
        foreach (var wheel in wheels)
        {
            var rot = Vector3.right * wheelsRotationSpeed * (wheel.transform.localRotation.eulerAngles.y == 0 ? -1 : 1);
            wheel.DOLocalRotate(rot, 0, RotateMode.WorldAxisAdd).SetRelative().SetEase(Ease.Linear);
        }
    }
}
