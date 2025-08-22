using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleRaceController : MonoBehaviour
{
    public int wheelsRotFactor=30;
    VehicleController _vehicle;
    float _currentSpeed = 0f;
    
    void Start()
    {
        _vehicle = GetComponent<VehicleController>();
    }

    
    void Update()
    {
        var rnd1 = Random.Range(5f, 10f) ;
        var addvalue = rnd1 * Time.deltaTime;
        if (_currentSpeed <=0.2f) addvalue += 1f;
        else if (_currentSpeed < 0.5f) addvalue -= 2f;
        else if (_currentSpeed < 1) addvalue += 5f;
        else if (_currentSpeed < 2) addvalue += 3f;
        //else if (_currentSpeed < 3) addvalue += 0.03f;
        _currentSpeed += addvalue * Time.deltaTime;
        _vehicle.speed = _currentSpeed;
        _vehicle.wheelsRotationSpeed = _currentSpeed * wheelsRotFactor;
    }
}
