using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class RacingVehicle : MonoBehaviour
{
    public CanvasPlayerMap canvasPlayerMap;
    public SimplePlayer playerData;
    public CinemachineCamera virtualCameraPrefab;
    public CanvasPlayerMap canvas;
    private GameObject vehicleInstance;

    public void HidePlayersUI()
    {
        if (canvasPlayerMap == null) return;
        canvasPlayerMap.gameObject.SetActive(false);
    }

    public void ShowPlayersUI()
    {
        if (canvasPlayerMap == null) return;
        canvasPlayerMap.gameObject.SetActive(true);
    }


    internal GameObject Init(SimplePlayer player, bool addCamera = false)
    {
        GameManager.RemoveChildren(gameObject, true);
        if (vehicleInstance != null) DestroyImmediate(vehicleInstance);
        var v = GameConfigMerge.instance.vehiclesDefinition.Find(v => v.id == player.vehicleID);
        vehicleInstance = Instantiate(v.vehiclePrefab, transform);
        vehicleInstance.transform.DOLocalMoveZ(-0.5f, 3f).From().SetEase(Ease.OutExpo);

        canvas ??= Instantiate(GameConfigMerge.instance.mapPlayerName, transform);
        canvas.Init(player);
        playerData = player;
        canvasPlayerMap = canvas;
        //col.enabled = true;
        if (vehicleInstance.TryGetComponent<VehicleInstance>(out var vInstance))
        {
            if (v.materials.Count > 0)
            {
                vInstance.ApplyMaterial(v.materials[player.vehicleMaterial].material);
            }
            vInstance.RotateWheels();
        }
        if(addCamera)
        {
            var cam = Instantiate(virtualCameraPrefab, vehicleInstance.transform).GetComponent<CinemachineCamera>();
            cam.LookAt = vehicleInstance.transform;
        }
        return vehicleInstance;
    }

}