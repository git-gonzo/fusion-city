using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleThumbnail : MonoBehaviour
{
    public RawImage raw;
    public bool autoInit;
    [ShowIf("autoInit")]
    [ValidateInput("CheckVehicleID", "$validationMSG")]
    [InfoBox("$validationMSG")]
    public int vehicleID;
    private Camera cam => GameManager.Instance.garaje3D_Canvas.vCam;
    private Garaje3D garage3D => GameManager.Instance.garaje3D_Canvas;


    GameConfigMerge _gameConfig;
    GameConfigMerge gameConfig => _gameConfig ??= GameObject.Find("GameConfig").GetComponent<GameConfigMerge>();

    private string validationMSG = "Test";

    private void Start()
    {
        if(autoInit)
        {
            GenerateThumbnail(vehicleID);
        }
    }


    public void GenerateThumbnail(int id)
    {
        //garage3D.AddVehicle(id, false, GameManager.Instance.playerData.GetVehicleMaterial(id));
        garage3D.AddVehicle(id, false, GameManager.Instance.playerData.GetVehicleMaterialIndex(id));
        //RenderTexture r = new RenderTexture(camTexture.width, camTexture.height, 24);
        RenderTexture r = new RenderTexture((int)raw.rectTransform.sizeDelta.x, (int)raw.rectTransform.sizeDelta.y, 24);
        GameManager.Instance.garaje3D_Canvas.vCam.targetTexture = r;
        cam.Render();
        //Graphics.CopyTexture(camTexture, raw.texture);
        raw.texture = r;

    }
    private bool CheckVehicleID()
    {
        if (vehicleID == 0)
        {
            validationMSG = "Insert Vehicle ID";
            return false;
        }
        else
        {
            var v = gameConfig.GetVehicleById(vehicleID);
            if (v == null)
            {
                validationMSG = "Vehicle not found";
                return false;
            }
            else
            {
                validationMSG = v.vehicleName;
            }
        }
        return true;
    }
}
