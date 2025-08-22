using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerBar : MonoBehaviour
{
    public GameObject btnStorage;
    public GameObject btnGarage;
    public GameObject btnMissions;
    public GameObject btnShop;
    public GameObject btnLocation;
    

    public void ShowNormal()
    {
        btnStorage.SetActive(true);
        btnGarage.SetActive(true);
        btnMissions.SetActive(true);
        if (btnShop != null) btnShop.SetActive(true);
        if(btnLocation != null) btnLocation.SetActive(true);
    }
    public void ShowOnlyStorage()
    {
        btnStorage.SetActive(true);
        btnGarage.SetActive(false);
        btnMissions.SetActive(false);
        if(btnShop != null) btnShop.SetActive(false);
        if(btnLocation != null) btnLocation.SetActive(false);
    }
}
