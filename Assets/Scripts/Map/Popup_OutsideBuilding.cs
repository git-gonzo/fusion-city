using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Popup_OutsideBuilding : Popup_MapBase
{
    
    public TextMeshProUGUI txtVehiclesCounter;

    public VehicleThumbnail thumbnail;
    public GameObject iconWalking;
    public Button btnMoveHere;
    public Button btnGarage;



    protected override void FillData()
    {
        FillTexts();
        var vCount = FillVehicles();
        txtVehiclesCounter.text = vCount.ToString();
        vehiclesContainer.SetActive(vCount > 0);
        missionsContainer.SetActive(FillMissions() > 0);
        jobsContainer.SetActive(FillJobs()> 0);
        peopleContainer.SetActive(FillPeople() > 0);
        iconWalking.SetActive(PlayerData.vehicleSelected == 0);
        UpdateTravelling();
        btnMoveHere.onClick.RemoveAllListeners();
        btnMoveHere.onClick.AddListener(StartMoving);
        btnGarage.onClick.RemoveAllListeners();
        btnGarage.onClick.AddListener(OpenGarage);
        SetIconBestVehicle();
        //txtPeopleInside.text = BuildingsManager.GetPeopleInBuilding(buildingData.buildingType).Count.ToString();
    }

    public void UpdateTravelling()
    {
        thumbnail.gameObject.SetActive(PlayerData.vehicleSelected > 0);
        if (PlayerData.vehicleSelected > 0)
        {
            thumbnail.GenerateThumbnail(PlayerData.vehicleSelected);
        }
        txtTravelTime.text = UIUtils.FormatTime(GetTimeToGetBuilding(buildingData.buildingType));
    }

    private void OpenGarage()
    {
        if(GameManager.Instance.PlayerLevel < 2) { return; }
        GameManager.Instance.ShowCarShop(true);
    }
}
