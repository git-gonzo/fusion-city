using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NoVehicleView : MonoBehaviour
{
    public Action OnSelect;

    public void LookForBicycle()
    {
        GameManager.Instance.mapManager.FocusOnBuilding(BuildingType.BicycleShop);
        OnSelect?.Invoke();
    }
    public void LookForMoto()
    {
        GameManager.Instance.mapManager.FocusOnBuilding(BuildingType.ShopMoto);
        OnSelect?.Invoke();
    }
    public void LookForCar()
    {
        GameManager.Instance.mapManager.FocusOnBuilding(BuildingType.CarShop);
        OnSelect?.Invoke();
    }
    public void LookForHelicopter()
    {
        GameManager.Instance.mapManager.FocusOnBuilding(BuildingType.HelicpterShop);
        OnSelect?.Invoke();
    }
}
