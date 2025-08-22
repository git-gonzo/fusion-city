using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class VehicleShopController : MonoBehaviour
{
    [Serializable]
    public class VehicleInMapShop
    {
        public SO_Vehicle vehicleData;
        public VehicleInMapItem vehicleObject;
    }

    public List<VehicleInMapShop> vehicles;
    private CinemachineBrain brain;
    private VehicleShopInMapManager vManager;
    private int index;
    private float defaultCameraBlendTime = 0f;

    public void Show()
    {
        index = -1;
        brain ??= Camera.main.GetComponent<CinemachineBrain>();
        vManager ??= GameManager.Instance.GetComponent<VehicleShopInMapManager>();
        if (defaultCameraBlendTime == 0) defaultCameraBlendTime = brain.DefaultBlend.Time;
            brain.DefaultBlend.Time = 1.3f;
        vManager.InitScreen();
        OnNext();
    }

    private void lowAllCamPriorities()
    {
        foreach(var v in vehicles)
        {
            v.vehicleObject.vehicleCam.Priority = 0;
        }
    }

    public void OnNext()
    {
        index++;
        if (index == vehicles.Count) index = 0;
        lowAllCamPriorities();
        vehicles[index].vehicleObject.vehicleCam.Priority = 50;
        //var v = new Vehicle(vehicles[index].vehicleData);
        vManager.ShowVehicleDetail(vehicles[index].vehicleData, OnNext, OnPrev, OnClose);
    }
    public void OnPrev()
    {
        index--;
        if (index == -1) index = vehicles.Count - 1;
        lowAllCamPriorities();
        vehicles[index].vehicleObject.vehicleCam.Priority = 50;
        //var v = new Vehicle(vehicles[index].vehicleData);
        vManager.ShowVehicleDetail(vehicles[index].vehicleData, OnNext, OnPrev, OnClose);
    }
    public void OnClose()
    {
        brain.DefaultBlend.Time = defaultCameraBlendTime;
        lowAllCamPriorities();
        GameManager.Instance.mapManager.OnInteractionCancel();
        GameManager.Instance.sideBar.ShowSidebar(true, true);
        vManager.ExitShop();
    }
}
