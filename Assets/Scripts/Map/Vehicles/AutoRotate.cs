using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public GameObject target;
    public float rotationSpeed = 3;
    public bool rotationEnabled;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotationEnabled)
        {
            target.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}
