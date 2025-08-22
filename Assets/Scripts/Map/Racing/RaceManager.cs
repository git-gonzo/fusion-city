using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public RacingVehicle racingVehiclePlayer;
    public RacingVehicle racingVehicleOpponent;
    public Transform destinationPlayer;
    public Transform destinationOpponent;
    public RaceUIController raceUI;

    private GameObject playerCar;
    private GameObject opponentCar;
    private VehicleController playerVehicleController;
    private VehicleController opponentVehicleController;

    bool isRacing;

    [Button]
    public void PrepareRace()
    {
        GameManager.Instance.ShowMapLowerBar(false);
        raceUI.gameObject.SetActive(true);
        InstantiateCars();
    }

    void InstantiateCars()
    {
        var temp1 = new SimplePlayer();
        temp1.vehicleMaterial = 0;
        temp1.vehicleID = 116;
        temp1.playername = "PRUEBA TRON";
        temp1.charIndex = 6;
        temp1.position = 6;
        var temp2 = new SimplePlayer();
        temp2.vehicleMaterial = 0;
        temp2.vehicleID = 115;
        temp2.playername = "OPPONENT";
        temp2.charIndex = 8;
        temp2.position = 2;

        playerCar = racingVehiclePlayer.Init(temp1,true);
        opponentCar = racingVehicleOpponent.Init(temp2);

        if(playerCar.GetComponent<VehicleController>() == null)
        {
            playerCar.AddComponent<VehicleController>();
        }
        if(opponentCar.GetComponent<VehicleController>() == null)
        {
            opponentCar.AddComponent<VehicleController>();
        }

        playerVehicleController = playerCar.GetComponent<VehicleController>();
        playerVehicleController.destinations ??= new List<Transform>();
        playerVehicleController.destinations.Add(destinationPlayer);
        playerVehicleController.wheelsRotation = true;
        playerVehicleController.wheelsRotationSpeed = 35;


        opponentVehicleController = opponentCar.GetComponent<VehicleController>();
        opponentVehicleController.destinations ??= new List<Transform>();
        opponentVehicleController.destinations.Add(destinationOpponent);
        opponentVehicleController.wheelsRotation = true;
        playerVehicleController.wheelsRotationSpeed = 35;
    }

    [Button]
    public void StartRace()
    {
        playerVehicleController.speed = 0.1f;
        opponentVehicleController.speed = 0.5f;
        isRacing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRacing)
        {
            playerVehicleController.speed += raceUI.progress * 0.02f;
            opponentVehicleController.speed += (Random.Range(0, 10) - 4) * 0.01f;
        }
    }
}
