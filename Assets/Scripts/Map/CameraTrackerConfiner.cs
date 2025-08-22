using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrackerConfiner : MonoBehaviour
{
    public Collider confiner;
    private bool isOutSide;
    private Vector3 lastPos;
    private Vector3 currentPos;

    // Start is called before the first frame update
    void Start()
    {
        isOutSide = false;
    }

    // Update is called once per frame
    void Update()
    {
        lastPos = currentPos;    
        currentPos = transform.position;    
    }
    private void LateUpdate()
    {
        if (isOutSide)
        {
            transform.position = lastPos;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other == confiner)
        {
            isOutSide = true;
            Debug.Log("Se salio");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == confiner)
        {
            isOutSide = false;
            Debug.Log("Entro");
        }
    }
}
