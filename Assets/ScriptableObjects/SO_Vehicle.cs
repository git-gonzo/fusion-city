using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new VehicleData", menuName = "Fame/VehicleData")]
public class SO_Vehicle : ScriptableObject
{
    public VehicleType vehicleType;
    public int id;
    public string vehicleName;
    public string vehicleDescrip;
    [Range(1,1000)]
    public int reduceTimePercent;
    public int durability;
    public RewardData price;
    public GameObject vehiclePrefab;
    public List<VehicleMaterial> materials;

    public float speedMod { get => 1 + reduceTimePercent / 75f; }
    [JsonConstructor]
    public SO_Vehicle(VehicleType vehicleType, int id, string vehicleName, string vehicleDescrip, int reduceTimePercent, int durability, RewardData price)
    {
        this.vehicleType = vehicleType;
        this.id = id;
        this.vehicleName = vehicleName;
        this.vehicleDescrip = vehicleDescrip;
        this.reduceTimePercent = reduceTimePercent;
        this.durability = durability;
        this.price = price;
    }

    public void UpdateDataFromConfig(VehicleConfig newData)
    {
        this.vehicleDescrip = newData.vehicleDescrip;
        this.reduceTimePercent = newData.reduceTimePercent;
        this.durability = newData.durability;
        this.price = newData.price;
    }
}

public class VehicleConfig
{
    public VehicleType vehicleType;
    public int id;
    public string vehicleName;
    public string vehicleDescrip;
    public int reduceTimePercent;
    public int durability;
    public RewardData price;

    [JsonConstructor]
    public VehicleConfig(VehicleType vehicleType, int id, string vehicleName, string vehicleDescrip, int reduceTimePercent, int durability, RewardData price)
    {
        this.vehicleType = vehicleType;
        this.id = id;
        this.vehicleName = vehicleName;
        this.vehicleDescrip = vehicleDescrip;
        this.reduceTimePercent = reduceTimePercent;
        this.durability = durability;
        this.price = price;
    }
}

public enum VehicleType
{
    None,
    Bicycle,
    Motorcycle,
    Car,
    Helicopter,
    Plane,
    Boat,
    Skate,
    Scooter
}

[System.Serializable]
public class Vehicle
{
    public int id;
    public VehicleType vehicleType;
    public string vehicleName;
    public string vehicleDescrip;
    [Range(1, 99)]
    public int reduceTimePercent;
    public int durability;
    public int CurrentDurability;
    public RewardData price;
    public float speedMod { get => 1+reduceTimePercent / 75f;}
}

[System.Serializable]
public class VehicleInServer
{
    public int id;
    public int currentDurability;
    public int mat;

    [JsonConstructor]
    public VehicleInServer(int id, int durability, int material = 0)
    {
        this.id = id;
        currentDurability = durability;
        mat = material;
    }
    public VehicleInServer(SO_Vehicle v)
    {
        this.id = v.id;
        currentDurability = v.durability;
        mat = 0;
    }
}

[System.Serializable]
public class VehicleMaterial
{
    public string descrip;
    public Material material;
}