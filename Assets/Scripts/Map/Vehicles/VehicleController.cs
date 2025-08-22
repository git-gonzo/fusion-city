using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Utilities;
using DG.Tweening;

public class VehicleController : MonoBehaviour
{
    [Range(0,5)]
    public float speed;
    public float rotationSpeedFactorBasedOnSpeed = 20f ;
    public float rotationSpeed= 0.2f ;
    public float rotationOffset;
    public List<Transform> destinations;
    public bool bouncyBoat = false;
    [Range(0,2)]
    public float bouncyDuration = 1f;
    [Range(0, 1)]
    public float jumpHeight = 0.1f;
    [Range(0, 10)]
    public float rotationStrenght = 6f;
    public bool ignoreY = true;
    public GameObject vehicle;

    public bool wheelsRotation;
    public float wheelsRotationSpeed;
    public List<Transform> wheels;

    private VehicleWheels _vehicleWheels;

    int currentDestIndex = 0;
    Vector3 currentPosition;
    Vector3 nextPosition;

    // Start is called before the first frame update
    void Start()
    {
        if(destinations!=null && destinations.Count > 0)
            nextPosition = destinations[0].position;
        if (bouncyBoat)
        {
            vehicle.transform.DOLocalMoveY(jumpHeight, bouncyDuration).SetEase(Ease.InOutQuad).SetLoops(-1,LoopType.Yoyo);
            vehicle.transform.DOLocalRotate(Vector3.back* rotationStrenght, bouncyDuration, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
        }
        _vehicleWheels = gameObject.GetComponent<VehicleWheels>();
    }

    // Update is called once per frame
    void Update()
    {
        if (destinations == null) return;

        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * speed);

        var value = Vector3.MoveTowards(transform.position, nextPosition, Time.deltaTime * speed);
        nextPosition.y = transform.position.y;
        var distance = Vector3.Distance(transform.position, nextPosition); //Mathf.Abs((transform.position - nextPosition).magnitude);
        if (distance < 0.1f)
        {
            //Debug.Log("Getting next destination, distance " + distance);
            nextPosition = GetNextDestination().position;
        }
        else
        {
            //Debug.Log("distance " + distance);
        }
        
        transform.position = value;
        if (wheelsRotation)
        {
            if (_vehicleWheels != null)
            {
                RotateWheels(_vehicleWheels.wheels);
            }
            else if (wheels != null)
            {
                RotateWheels(wheels);
            }
        }
    }
    private void RotateWheels(List<Transform> wheels)
    {
        foreach (var wheel in wheels)
        {
            //wheel.Rotate(Vector3.right * (wheelsRotationSpeed * 10 * Time.deltaTime));
            var rot = Vector3.right * speed * rotationSpeedFactorBasedOnSpeed * (wheel.transform.localRotation.eulerAngles.y == 0 ? -1 : 1);
            wheel.DOLocalRotate(rot, 0.1f, RotateMode.WorldAxisAdd).SetRelative().SetEase(Ease.Linear);
        }
    }

    private Transform GetNextDestination()
    {
        var currentDestination = destinations[currentDestIndex];
        if (destinations.Count > 1)
        {
            if (currentDestIndex < destinations.Count - 1)
            {
                ++currentDestIndex;
            }
            else
            {
                currentDestIndex = 0;
            }
            
        }
        var dir =  destinations[currentDestIndex].position - currentDestination.position;
        var r = Quaternion.LookRotation(dir).eulerAngles;
        //var r = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir),90).eulerAngles;
        transform.DORotate(r, rotationSpeed);
        return destinations[currentDestIndex];
    }
}
