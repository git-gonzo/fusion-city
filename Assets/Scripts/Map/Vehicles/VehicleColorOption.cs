using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VehicleColorOption : MonoBehaviour
{
    public TextMeshProUGUI txtIndex;
    public GameObject check;
    public GameObject frameSelected;

    public void Init(int index)
    {
        txtIndex.text = index.ToString();
        Active(false);
    }

    public void Select(bool value)
    {
        frameSelected.SetActive(value);
    }

    public void Active(bool value)
    {
        check.SetActive(value);
    }
}
