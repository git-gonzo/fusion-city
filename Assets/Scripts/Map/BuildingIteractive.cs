using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Assets.Scripts.MergeBoard;

public class BuildingIteractive : MonoBehaviour
{
    public Action OnCancel;

    public bool isActive;
    public SO_Building buildingData;
    public Transform vehicleCarPlayerContainer;
    public OtherPlayerVehicle vehicleCarOtherContainer;
    public OtherPlayerVehicle vehicleCarOtherContainer2;
    public OtherPlayerVehicle vehicleCarOtherContainer3;
    public Transform vehicleHelicopterPlayerContainer;
    private CanvasPlayerMap otherPlayerCanvas;
    private CanvasPlayerMap otherPlayerCanvas2;
    private CanvasPlayerMap otherPlayerCanvas3;
    [Header("Arrow on top of building")]
    public GameObject ArrowPointer;
    private float arrowPosY;
    private MapPointer _mapPointer;
    private List<RewardItemUniversal> _listRewards;

    public VehicleShopController vehicleShopInMap;
    private GameObject _playerVehicle;
    private GameObject _otherPlayerVehicle;
    private GameObject _otherPlayerVehicle2;
    private GameObject _otherPlayerVehicle3;
    public List<LeaderboardPlayer> _playersInBuilding = new List<LeaderboardPlayer>();

    public MapPointer mapPointer { get {
            if(_mapPointer == null)
            {
                _mapPointer = GetComponentInChildren<MapPointer>();
            }
            return _mapPointer;
        } }
    

    // Start is called before the first frame update
    void Start()
    {
        if (mapPointer != null) mapPointer.Init(buildingData.buildingType,buildingData.unlockLevel);
        if (ArrowPointer != null)
        {
            arrowPosY = ArrowPointer.transform.position.y;
            ArrowPointer.SetActive(false);
        }
        vehicleShopInMap = gameObject.GetComponent<VehicleShopController>();
        _playersInBuilding = new List<LeaderboardPlayer>();
    }

    public void Cancel()
    {
        OnCancel?.Invoke();
    }

    public void ShowArrowPointer()
    {
        DOTween.Kill(ArrowPointer);
        mapPointer.gameObject.SetActive(false);
        var width = 120f;
        var height = 10f;
        var duration = 0.5f;
        
        if(arrowPosY == 0)
        {
            Debug.Log("<color=yellow>ShowArrowPointer</color> - Arrow was not set start position Y " + arrowPosY + " - " + ArrowPointer.transform.position.y);
            arrowPosY = ArrowPointer.transform.position.y;
        }

        //Reset State
        ArrowPointer.transform.DOScaleY(width, 0);
        ArrowPointer.transform.DOScaleY(width, 0);
        ArrowPointer.transform.DOMoveY(arrowPosY, 0);
        

        var seq = DOTween.Sequence();
        ArrowPointer.SetActive(true);
        seq.Insert(0,ArrowPointer.transform.DOMoveY(arrowPosY + 0.2f, duration).SetEase(Ease.OutQuad));
        seq.Insert(0,ArrowPointer.transform.DOScaleY(100f, duration).SetEase(Ease.OutExpo));
        seq.Insert(0,ArrowPointer.transform.DOScaleX(100f, duration).SetEase(Ease.OutExpo));
        seq.Insert(duration, ArrowPointer.transform.DOMoveY(arrowPosY, duration).SetEase(Ease.InQuad));
        seq.Insert(duration, ArrowPointer.transform.DOScaleY(width, duration).SetEase(Ease.InExpo));
        seq.Insert(duration, ArrowPointer.transform.DOScaleX(height, duration).SetEase(Ease.InExpo));
        seq.SetLoops(-1);
    }
    public void HideArrowPointer()
    {
        DOTween.Kill(ArrowPointer);
        ArrowPointer.SetActive(false);
        mapPointer.gameObject.SetActive(true);
    }

    public void LoadPlayerVehicle(SO_Vehicle vehicle)
    {
        if (vehicle.vehicleType == VehicleType.Helicopter)
        {
            if (vehicleHelicopterPlayerContainer == null) return;
            RemovePlayerVehicle();
            _playerVehicle = Instantiate(vehicle.vehiclePrefab, vehicleHelicopterPlayerContainer);
            _playerVehicle.transform.DOLocalMoveY(0.6f, 3.5f).From().SetEase(Ease.OutQuad);
            _playerVehicle.transform.DOLocalMoveZ(-0.3f, 3f).From().SetEase(Ease.OutSine);
            _playerVehicle.transform.DOLocalRotate(new Vector3(30,0,0), 4f).From().SetEase(Ease.OutBack);
            var helicopterController = _playerVehicle.GetComponentInChildren<HelicopterController>();
            helicopterController.FlyAndStop();
        }
        else
        {
            RemovePlayerVehicle();
            _playerVehicle = Instantiate(vehicle.vehiclePrefab, vehicleCarPlayerContainer);
            _playerVehicle.transform.DOLocalMoveZ(-0.5f, 3f).From().SetEase(Ease.OutExpo);
            //Try to set material
            if(_playerVehicle.TryGetComponent<VehicleInstance>(out var vInstance)) 
            {
                var vOwned = GameManager.Instance.playerData.vehiclesOwned.Find(v => v.id == vehicle.id);
                var currentMatIndex = vOwned!=null?vOwned.mat:0;
                //Get material
                var mat = vehicle.materials[currentMatIndex];
                vInstance.ApplyMaterial(mat.material);
                vInstance.RotateWheels();
            }
        }
    }

    public void LoadOtherPlayerVehicle(LeaderboardPlayer player)
    {
        /*if (vehicle.vehicleConfig.vehicleType == VehicleType.Helicopter)
        {
            if (vehicleHelicopterPlayerContainer == null) return;
            _playerVehicle = Instantiate(vehicle.vehiclePrefab, vehicleHelicopterPlayerContainer);
            _playerVehicle.transform.DOLocalMoveY(0.6f, 3.5f).From().SetEase(Ease.OutQuad);
            _playerVehicle.transform.DOLocalMoveZ(-0.3f, 3f).From().SetEase(Ease.OutSine);
            _playerVehicle.transform.DOLocalRotate(new Vector3(30, 0, 0), 4f).From().SetEase(Ease.OutBack);
            var helicopterController = _playerVehicle.GetComponentInChildren<HelicopterController>();
            helicopterController.FlyAndStop();
        }
        else*/

        //if (_playersInBuilding.Contains(player))
        if (_playersInBuilding.Find(p => p.playername == player.playername) == null) _playersInBuilding.Add(player);

        if (IsPlayerVehicleInBuilding(player))
        {
            return;
        }
        else if (vehicleCarOtherContainer == null)
        {
            GameManager.Log($"<color=yellow>Attention</color> No other player container in {buildingData.buildingName}");
            return;
        }
        if (_otherPlayerVehicle == null) //Add most famous here
        {
            AddOtherPlayerVehicle(out _otherPlayerVehicle, vehicleCarOtherContainer, otherPlayerCanvas, player);
        }
        else if (_otherPlayerVehicle2 == null && vehicleCarOtherContainer2 != null) //Add 2nd most famous here
        {
            AddOtherPlayerVehicle(out _otherPlayerVehicle2, vehicleCarOtherContainer2, otherPlayerCanvas2, player);
        }
        else if (_otherPlayerVehicle3 == null && vehicleCarOtherContainer3 != null) //Add 3rd most famous here
        {
            AddOtherPlayerVehicle(out _otherPlayerVehicle3, vehicleCarOtherContainer3, otherPlayerCanvas3, player);
        }
    }

    private void AddOtherPlayerVehicle(out GameObject otherPlayerVehicle, OtherPlayerVehicle Container, CanvasPlayerMap canvas, LeaderboardPlayer player)
    {
        otherPlayerVehicle = Container.Init(player);
    }

    public void CleanOthersVehiclesHolders(List<LeaderboardPlayer> players)
    {
        List<LeaderboardPlayer> playersInBuilding = new();
        if (vehicleCarOtherContainer == null && vehicleCarOtherContainer2 == null) return;

        CleanOtherPlayerContainer(vehicleCarOtherContainer, _otherPlayerVehicle);
        CleanOtherPlayerContainer(vehicleCarOtherContainer2, _otherPlayerVehicle2);
        CleanOtherPlayerContainer(vehicleCarOtherContainer3, _otherPlayerVehicle3);
        _playersInBuilding = new List<LeaderboardPlayer>();

        void CleanOtherPlayerContainer(OtherPlayerVehicle otherPlayerVehicle, GameObject container)
        {
            if (otherPlayerVehicle != null && otherPlayerVehicle.playerData != null)
            {
                var otherPlayer = players.Find(p => p.playername == otherPlayerVehicle.playerData.playername && p.location == (int)buildingData.buildingType);
                if (otherPlayer == null || playersInBuilding.Contains(otherPlayer))
                {
                    RemoveOtherVehicle(container, otherPlayerVehicle);
                }
                else
                {
                    playersInBuilding.Add(otherPlayer);
                }
            }
        }
    }

    private bool IsPlayerVehicleInBuilding(LeaderboardPlayer player)
    {
        if (IsPlayerInSlot1(player) || IsPlayerInSlot2(player)) return true;
        return false;
    }

    private bool IsPlayerInSlot1(LeaderboardPlayer player)
    {
        if (vehicleCarOtherContainer != null
            && vehicleCarOtherContainer.playerData != null
            && vehicleCarOtherContainer.playerData.playername == player.playername)
            return true;
        return false;
    }
    private bool IsPlayerInSlot2(LeaderboardPlayer player)
    {
        if (vehicleCarOtherContainer2 != null
            && vehicleCarOtherContainer2.playerData != null
            && vehicleCarOtherContainer2.playerData.playername == player.playername)
            return true;
        return false;
    }

    private void RemoveOtherVehicle(GameObject otherPlayerVehicle, OtherPlayerVehicle container)
    {
        GameManager.Log("Removed player in " + buildingData.buildingName);
        if (otherPlayerVehicle != null) DestroyImmediate(otherPlayerVehicle);
        if (container != null)
        {
            _playersInBuilding.Remove(container.playerData);
            container.Reset();
        }
    }

    public void RemovePlayerVehicle()
    {
        if(_playerVehicle != null) DestroyImmediate(_playerVehicle);
    }

    public void ShowPlayersUI(bool value)
    {
        if (otherPlayerCanvas != null) otherPlayerCanvas.gameObject.SetActive(value);
        if (otherPlayerCanvas2 != null) otherPlayerCanvas2.gameObject.SetActive(value);
    }

    public VehicleInstance GetPlayerVehicleInstance()
    {
        return _playerVehicle.GetComponent<VehicleInstance>();
    }
}