using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;
using System;

public class VehiclesManager : MonoBehaviour
{
    
    PlayerData playerData{ get => GameManager.Instance.playerData; }

    public GameObject vehiclesContainer;
    public GameObject vehiclePrefab;
    public NoVehicleView noVehiclePrefab;
    public GameObject separatorPrefab;
    public TextMeshProUGUI TextTitle;
    public float animDelay = 0.08f;
    public float animDuration = 0.3f;
    public float animForce = 160;

    private List<SO_Vehicle> _shopVehicles;
    private VehicleType _shopType;
    private List<VehicleView> vehiclesViewList;
    private GameConfigMerge gameConfig => GameManager.Instance.gameConfig;
    private bool thumbNailsGenerated = false;
    private Action _onVehicleChange;

    public void OnShow(bool isShop, List<SO_Vehicle> vehicles = null, VehicleType shopType = VehicleType.None, Action OnVehicleChange = null)
    {
        _shopVehicles = vehicles;
        _shopType = shopType;
        _onVehicleChange = OnVehicleChange;
        vehiclesViewList = new List<VehicleView>();
        GameManager.RemoveChildren(vehiclesContainer);
        if (isShop)
        {
            LoadVehicles();
        }
        else
        {
            if (LoadOwnedVehicles())
            {
                StartCoroutine(AnimVehiclesIn());
            }
            else
            {
                InstantiateNoVehicle();
            }
        }        
    }

    private void LoadVehicles()
    {
        GameManager.RemoveChildren(vehiclesContainer);
        vehiclesViewList.Clear();
        if (LoadShopVehicles(_shopVehicles) < 0)
        {

        }
        if (CountOwnedVehiclesOfType(_shopType) > 0)
        {
            Instantiate(separatorPrefab, vehiclesContainer.transform);
            LoadOwnedVehiclesOfType(_shopType);
        }
        TextTitle.text = _shopType.ToString() + " Shop";
        StartCoroutine(AnimVehiclesIn());
    }

    private IEnumerator AnimVehiclesIn()
    {
        yield return new WaitForEndOfFrame();
        
        var seq = DOTween.Sequence();
        //for (var i = 0; i < vehiclesContainer.transform.childCount; i++)
        for (var i = 0; i < vehiclesViewList.Count; i++)
        {
            var vehicle = vehiclesViewList[i];
            var canv = vehicle.canvasGroup.alpha = 0;
            //canv.alpha = 0;
            seq.Insert(animDelay * i, vehicle.transform.DOPunchPosition(Vector3.down * animForce, animDuration, 1)
                //.OnStart(vehicle.GenerateThumbnail)
                );
            seq.Insert(0.08f * i + 0.1f, vehicle.canvasGroup.DOFade(1, 0.1f));
        }
        seq.Play();
    }

    public bool LoadOwnedVehicles()
    {
        bool hasVehicles = false;
        playerData.vehiclesOwned.Sort((a, b) => {
            if (a.id == PlayerData.vehicleSelected) return -1;
            var speedA = GameManager.Instance.gameConfig.vehiclesDefinition.Find(v => v.id == a.id).reduceTimePercent;
            var speedB = GameManager.Instance.gameConfig.vehiclesDefinition.Find(v => v.id == b.id).reduceTimePercent;
            return speedA > speedB ? -1 : 1;
        });
        for (var i = 0; i < playerData.vehiclesOwned.Count; i++)
        {
            hasVehicles = true;

            var v = gameConfig.GetVehicleById(playerData.vehiclesOwned[i].id);
            InstantiateVehicle(v,playerData.vehiclesOwned[i].currentDurability);
        }
        return hasVehicles;
    }

    public int CountOwnedVehiclesOfType(VehicleType vType)
    {
        int count = 0;
        for (var i = 0; i < playerData.vehiclesOwned.Count; i++)
        {
            if (gameConfig.GetVehicleById(playerData.vehiclesOwned[i].id).vehicleType == vType)
            {
                count++;
            }
        }
        return count;
    }
    public void LoadOwnedVehiclesOfType(VehicleType vType)
    {
        for (var i = 0; i < playerData.vehiclesOwned.Count; i++)
        {
            var v = gameConfig.GetVehicleById(playerData.vehiclesOwned[i].id);
            if (gameConfig.GetVehicleById(playerData.vehiclesOwned[i].id).vehicleType == vType)
            {
                InstantiateVehicle(v, playerData.vehiclesOwned[i].currentDurability, true,true);
            }
        }
    }
    private int LoadShopVehicles(List<SO_Vehicle> vehicles)
    {
        var count = 0;
        if (vehicles == null) return 0;
        for (var i = 0; i < vehicles.Count; i++)
        {
            if (!playerOwnVehicle(vehicles[i].id))
            {
                InstantiateVehicle(vehicles[i], 0, false);
                count++;
            }
        }
        return count;
    }

    private void InstantiateVehicle(SO_Vehicle v, int curDurability = 0, bool owned = true, bool inShop = false)
    {
        var j = Instantiate(vehiclePrefab, vehiclesContainer.transform).GetComponent<VehicleView>();
        vehiclesViewList.Add(j);
        j.InitVehicle(v, owned, inShop, curDurability, RefresStates);
    }

    private void RefresStates()
    {
        _onVehicleChange?.Invoke();
        foreach (var item in vehiclesViewList)
        {
            item.SetState();
        }
    }

    private void InstantiateNoVehicle()
    {
        Instantiate(noVehiclePrefab, vehiclesContainer.transform).OnSelect = Close;
    }
    private void Close()
    {
        GameManager.Instance.ShowCarShop(false);
    }

    private bool playerOwnVehicle(int id)
    {
        return playerData.vehiclesOwned.Any(v => v.id == id);
    }
}
