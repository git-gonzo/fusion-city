using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garaje3D : MonoBehaviour
{
    [SerializeField] Vector3 biciPosition;
    [SerializeField] Vector3 motoPosition;
    [SerializeField] Vector3 carsPosition;
    [SerializeField] Vector3 helicoptersPosition;
    public GameObject background;
    public GameObject CarContainer;
    public Camera vCam;

    public void AddPlayerVehicle()
    {
        GameManager.RemoveChildren(CarContainer);
        var vehicle = GameManager.Instance.playerData.GetBestVehicle();
        vCam.transform.localPosition = 
            vehicle.vehicleType == VehicleType.Helicopter ? helicoptersPosition :
            vehicle.vehicleType == VehicleType.Bicycle ? biciPosition :
            vehicle.vehicleType == VehicleType.Motorcycle ? motoPosition : 
            carsPosition;
        
        //var prefab = GameManager.Instance.gameConfig.vehiclesPrefabs.Find(v => v.vehicleConfig.vehicleName == vehicle.vehicleName).vehiclePrefab;
        //if (prefab == null) return;
        if (vehicle.vehiclePrefab == null) return;
        Material mat = null;

        var vOwned = GameManager.Instance.playerData.vehiclesOwned.Find(v => v.id == vehicle.id);
        var currentMatIndex = vOwned != null ? vOwned.mat : 0;
        //Get material
        if(vehicle.materials != null && vehicle.materials.Count > 0)
            mat = vehicle.materials[currentMatIndex].material;

        InstantiateVehicle(vehicle.vehiclePrefab, true, mat);

    }
    public void AddVehicle(int id, bool withBG = true, int mat = 0)
    {
        GameManager.RemoveChildren(CarContainer,true);
        var vehicle = GameConfigMerge.instance.vehiclesDefinition.Find(v => v.id == id);
        if (vehicle != null)
        {
            vCam.transform.localPosition = 
                vehicle.vehicleType == VehicleType.Helicopter ? helicoptersPosition :
                vehicle.vehicleType == VehicleType.Bicycle ? biciPosition :
                vehicle.vehicleType == VehicleType.Motorcycle ? motoPosition :
                carsPosition;
            var prefab = vehicle.vehiclePrefab;
            if (prefab == null) return;
            Material material = null;
            if (vehicle.materials != null && vehicle.materials.Count > 0)
                material = vehicle.materials[mat].material;
            InstantiateVehicle(prefab, withBG, material); 
        }
    }

    private void InstantiateVehicle(GameObject prefab, bool withBG, Material mat = null)
    {
        background.SetActive(withBG);
        var instance = Instantiate(prefab, CarContainer.transform);
        instance.layer = LayerMask.NameToLayer("3dCanvas");
        foreach (Transform t in instance.GetComponentsInChildren<Transform>())
        {
            t.gameObject.layer = LayerMask.NameToLayer("3dCanvas");
        }

        //Try to set material
        if (instance.TryGetComponent<VehicleInstance>(out var vInstance) && mat !=null)
        {
            vInstance.ApplyMaterial(mat);
        }

    }
}
