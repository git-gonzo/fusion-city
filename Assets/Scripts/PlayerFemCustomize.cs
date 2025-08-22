using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFemCustomize : MonoBehaviour
{
    public List<GameObject> hairList;

    private int _current = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            HideAllHair();
            hairList[_current++].SetActive(true);
            if (_current == hairList.Count) _current = 0;
        }
    }

    private void HideAllHair()
    {
        foreach(var g in hairList)
        {
            g.SetActive(false);
        }
    }
}
