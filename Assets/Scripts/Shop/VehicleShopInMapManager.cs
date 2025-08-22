using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VehicleShopInMapManager : MonoBehaviour
{
    public VehicleView vehicleInMapView;
    public Button btnNext;
    public Button btnPrev;
    public Button btnClose;

    PlayerData playerData { get => GameManager.Instance.playerData; }

    // Start is called before the first frame update
    void Start()
    {
        vehicleInMapView.gameObject.SetActive(false);
    }

    public void InitScreen()
    {
        vehicleInMapView.ShowScreenInMap();
    }
    
    public void ShowVehicleDetail(SO_Vehicle vehicle, UnityAction OnNext, UnityAction OnPrev, UnityAction OnClose)
    {
        var owned = playerData.vehiclesOwned.Any(v => v.id == vehicle.id);
        vehicleInMapView.InitVehicleInShop(vehicle, owned, true);
        btnNext.onClick.RemoveAllListeners();
        btnPrev.onClick.RemoveAllListeners();
        btnClose.onClick.RemoveAllListeners();
        btnNext.onClick.AddListener(OnNext);
        btnPrev.onClick.AddListener(OnPrev);
        btnClose.onClick.AddListener(OnClose);
    }

    public void ExitShop()
    {
        vehicleInMapView.gameObject.SetActive(false);
    }

}
