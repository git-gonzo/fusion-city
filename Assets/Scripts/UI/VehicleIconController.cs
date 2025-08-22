using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameConfigMerge;

public class VehicleIconController : MonoBehaviour
{
    public Image imageContainer;

    public void SetIcon(VehicleType vehicleType)
    {
        foreach(VehicleIcon t in GameManager.Instance.gameConfig.vehicleIcons)
        {
            if(t.transportType == vehicleType)
            {
                imageContainer.sprite = t.icon;
            }
        }
    }
}
