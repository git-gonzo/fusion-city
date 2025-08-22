using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Popup_InsideBuilding : Popup_MapBase
{
    public Button btnJobs;
    public Button btnEnterShop;
    public Button btnCustomize;
    public Button btnRoulette;
    public Button btnEnterHouse;
    public Button btnBuilding;
    public Button btnMissions;
    public Button btnPeople;
    public Button btnConcert;
    public ButtonWithTooltip btnCharacter;
    public TextMeshProUGUI txtBuildingPrice;
    public TextMeshProUGUI txtOwnedProfit;
    public GameObject ownedContainer;
    public GameObject buyIconGems;
    public GameObject buyIconCoins;
    public GameObject lockedJobs;
    public GameObject lockedBuilding;
    public GameObject lockedShop;
    public VehicleCustomize vehicleCustomizePanel;
    public PeopleContainer peopleScreen;
    public CharacterStepView characterStepView;
    public Transform charViewParent;
    public Dialog dialog;

    //public SO_SkillType RealStateSkill;
    BuildingConfigRaw buildingConfig;
    RewardData _buildingPrice;
    bool refreshProfit = false;
    bool buildingLocked = true;

    public override void Show(SO_Building building)
    {
        base.Show(building);

        /*if (building.isHouse)
        {
            //btnEnterHouse.gameObject.SetActive(true);
        }*/
        ownedContainer.SetActive(false);
        buildingConfig = GameServerConfig.Instance.GetBuildingConfig(building.buildingType);
        buildingLocked = building.boardUnlockLevel > GameManager.Instance.PlayerLevel;
        UpdateButtons();

        if (playerData.HasBuilding(building.buildingType) && buildingConfig != null) //Building Owned
        {
            SetStateOwned();
        }
        else 
        {
            refreshProfit = false;
            _buildingPrice = GameServerConfig.Instance.GetBuildingPrice(building.buildingType);
            Debug.Log("Building price = " + _buildingPrice.amount);
            if (_buildingPrice.amount > 0)
            {
                //txtBuildingPrice.text = "<color=yellow>" + UIUtils.FormatNumber(_buildingPrice.amount);
            }
        }
    }

    public void UpdateButtons()
    {
        var totalButtons = 0;
        ResetButtons();
        if (FillMissions() > 0)
        {
            btnMissions.gameObject.SetActive(true);
            btnMissions.onClick.AddListener(ShowMissions);
            totalButtons++;
        }
        var characterStep = buildingData.TryGetCharacterStep();
        if (characterStep != null)
        {
            btnCharacter.gameObject.SetActive(true);
            btnCharacter.button.onClick.AddListener(ShowCharacter);
            btnCharacter.textButton.text = characterStep.CharacterName;
            btnCharacter.iconImage.sprite = characterStep.CharacterPortrait;
            totalButtons++;
        }
        if (buildingData.boardConfig != null)
        {
            btnJobs.gameObject.SetActive(true);
            btnJobs.onClick.AddListener(ShowJobsScreen);
            totalButtons++;
        }
        if (vehicles != null && vehicles.Count > 0)
        {
            btnEnterShop.gameObject.SetActive(true);
            btnEnterShop.onClick.AddListener(OpenShop);
            totalButtons++;
        }
        if(buildingData.buildingType == BuildingType.CarWorkshop && totalButtons < 4)
        {
            btnCustomize.gameObject.SetActive(true);
            btnCustomize.onClick.AddListener(OpenCustomize);
            totalButtons++;
        }
        if(buildingData.buildingType == BuildingType.Casino && totalButtons < 4)
        {
            btnRoulette.gameObject.SetActive(true);
            btnRoulette.onClick.AddListener(OpenRoulette);
            totalButtons++;
        }
        if(buildingData.buildingType == BuildingType.ConcertStageBig && totalButtons < 4)
        {
            btnConcert.gameObject.SetActive(true);
            btnConcert.onClick.AddListener(OpenConcert);
            totalButtons++;
        }
        if (buildingConfig != null && buildingConfig.forSale && totalButtons < 4)
        {
            btnBuilding.gameObject.SetActive(true);
            totalButtons++;
        }
        if (FillPeople() > 0 && totalButtons < 4)
        {
            btnPeople.gameObject.SetActive(true);
            btnPeople.onClick.AddListener(OpenPeopleInside);
        }

        lockedJobs.SetActive(buildingLocked);
        lockedBuilding.SetActive(buildingLocked);
        lockedShop.SetActive(buildingLocked);
    }

    private void SetStateOwned()
    {
        ownedContainer.SetActive(true);
        refreshProfit = true;
        RefreshProfitTxt(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ownedContainer.GetComponent<RectTransform>());
    }

    private void ResetButtons()
    {
        btnRoulette.onClick.RemoveAllListeners();
        btnCustomize.onClick.RemoveAllListeners();
        btnConcert.onClick.RemoveAllListeners();
        btnJobs.onClick.RemoveAllListeners();
        btnEnterShop.onClick.RemoveAllListeners();
        btnPeople.onClick.RemoveAllListeners();
        btnMissions.onClick.RemoveAllListeners();
        btnCharacter.button.onClick.RemoveAllListeners();

        btnEnterHouse.gameObject.SetActive(false);
        btnJobs.gameObject.SetActive(false);
        btnMissions.gameObject.SetActive(false);
        btnBuilding.gameObject.SetActive(false);
        btnEnterShop.gameObject.SetActive(false);
        btnCustomize.gameObject.SetActive(false);
        btnRoulette.gameObject.SetActive(false);
        btnPeople.gameObject.SetActive(false);
        btnCharacter.gameObject.SetActive(false);
        btnConcert.gameObject.SetActive(false);
    }

    private void ShowJobsScreen()
    {
        if (buildingLocked)
        {
            btnJobs.GetComponent<ButtonWithTooltip>().ShowToolTip("Unlock at level " + buildingConfig.UnlockLevel);
            return;
        }
        if (buildingData.boardConfig != null)
        {
            Hide();
            if(buildingData.boardConfig.isSpeedBoard)
                GameManager.Instance.ShowSpeedMergeBoard(true, buildingData.boardConfig, OnCancel);
            else
                GameManager.Instance.ShowMergeBoard(true, buildingData.boardConfig, OnCancel);
        }
    }

    private void ShowCharacter()
    {
        var charView = Instantiate(dialog, charViewParent);

        charView.StartDialog(buildingData.characterStory.GetStep(), UpdateButtons);
    }

    private void OpenShop()
    {
        var buildingInteractive = GameManager.Instance.mapManager.GetBuildingInteractiveFromType(buildingData.buildingType);
        if (buildingInteractive.vehicleShopInMap != null)
        {
            Hide();
            GameManager.Instance.sideBar.ShowSidebar(false, true);
            buildingInteractive.vehicleShopInMap.Show();
        }
        else
        {
            GameManager.Instance.ShowCarShop(true, true, vehicles);
        }
    }

    private void OpenCustomize()
    {
        vehicleCustomizePanel.Show();
        var vInstance = GameManager.Instance.mapManager.GetBuildingInteractiveFromPlayerLocation().GetPlayerVehicleInstance();
        vehicleCustomizePanel.InitVehicle(playerData.GetBestVehicle(), vInstance ? vInstance : new VehicleInstance());
    }
    private void OpenRoulette()
    {
        //Hide();
        GameManager.Instance.ShowRouletteScreen(true);
    }
    
    private void OpenConcert()
    {
        if(MyScenesManager.Instance == null)
        {
            SceneManager.LoadScene(2);
            return;
        }
        MyScenesManager.Instance.LoadScene2();
    }

    private void OpenPeopleInside()
    {
        peopleScreen.gameObject.SetActive(true);
        peopleScreen.FillPeopleInBuilding(buildingData.buildingType);
    }

    public void ShowBuildingInfo()//Called From Unity button
    {
        if (buildingLocked)
        {
            btnBuilding.GetComponent<ButtonWithTooltip>().ShowToolTip("Unlock at level " + buildingConfig.UnlockLevel);
            return;
        }
        GameManager.Instance.PopupsManager.ShowBuildingPopup(buildingData);
    }

    public void LazyUpdate()
    {
        if (refreshProfit)
        {
            RefreshProfitTxt();
        }
    }

    private void RefreshProfitTxt(bool fromZero = false) {
        var profit = buildingConfig.GetCurrentProfit();
        if (fromZero)
        {
            GameManager.AnimateFormatedNumber(txtOwnedProfit, 0, profit,false,0.9f);
            return;
        }
        txtOwnedProfit.text = profit.ToString();
    }

    private void ShowMissions()
    {
        GameManager.Instance.ShowMergeMapMissions(true, buildingData.buildingType);
    }
}
