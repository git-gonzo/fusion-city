using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerVehicle : MonoBehaviour
{
    public CanvasPlayerMap canvasPlayerMap;
    public LeaderboardPlayer playerData;
    public BoxCollider col;

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

    public void DisableInput()
    {
        col.enabled = false;
    }

    internal GameObject Init(LeaderboardPlayer player)
    {
        if(vehicleInstance != null) DestroyImmediate(vehicleInstance);
        var v = GameConfigMerge.instance.vehiclesDefinition.Find(v => v.id == player.vehicle);
        vehicleInstance = Instantiate(v.vehiclePrefab, transform);
        vehicleInstance.transform.DOLocalMoveZ(-0.5f, 3f).From().SetEase(Ease.OutExpo);

        canvas ??= Instantiate(GameConfigMerge.instance.mapPlayerName, transform);
        canvas.Init(player);
        playerData = player;
        canvasPlayerMap = canvas;
        col.enabled = true;
        if (vehicleInstance.TryGetComponent<VehicleInstance>(out var vInstance))
        {
            if (v.materials.Count > 0)
            {
                vInstance.ApplyMaterial(v.materials[player.material].material);
            }
            vInstance.RotateWheels();
        }
        return vehicleInstance;
    }

    public void Reset()
    {
        col.enabled = false;
    }

}
