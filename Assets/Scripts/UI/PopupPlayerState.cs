using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PopupPlayerState : PopupBase
{
    public Image iconCharacter;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtLevel;
    public TextMeshProUGUI txtFame;
    public TextMeshProUGUI txtMapMissions;
    public TextMeshProUGUI txtJobsCount;
    public TextMeshProUGUI txtMergeCount;
    public TextMeshProUGUI txtTravelCount;
    public TextMeshProUGUI txtVehicleCount;
    public TextMeshProUGUI txtBuildingsCount;
    public TextMeshProUGUI txtGiftsCount;
    public TextMeshProUGUI txtStolenCount;
    public TextMeshProUGUI txtXP;
    public TextMeshProUGUI txtVehicleViewFail;
    public Slider levelProgress;
    public Button btnChangeName;
    public GameObject btnChangeAvatar;
    public GameObject carView;

    
    Action _onChangeNameCallback;

    // Start is called before the first frame update
    public void InitMyPlayer(Action onCloseCallback, Action onChangeNameCallback, float finalScale = 1f)
    {
        InitElements(true);
        Show();
        this.onCloseCallback = onCloseCallback;
        _onChangeNameCallback = onChangeNameCallback;
        txtName.text = PlayerData.playerName;
        txtLevel.text = GameManager.Instance.playerData.level.ToString();
        txtXP.text = $"{PlayerData.xp}/{GameManager.Instance.playerData.nextLevelXP}";
        levelProgress.value = (float)PlayerData.xp / GameManager.Instance.playerData.nextLevelXP;
        txtFame.text = PlayerData.famePoints.ToString();
        txtMapMissions.text = PlayerData.MapMissionsCount.ToString();
        txtJobsCount.text = PlayerData.MissionsCount.ToString();
        txtMergeCount.text = PlayerData.mergeCount.ToString();
        txtTravelCount.text = PlayerData.travelCount.ToString();
        txtVehicleCount.text = GameManager.Instance.playerData.vehiclesOwned.Count.ToString();
        txtBuildingsCount.text = GameManager.Instance.playerData.buildingsOwned.Count.ToString();
        txtGiftsCount.text = PlayerData.giftsSent.ToString();
        txtStolenCount.text = PlayerData.itemsStolen.ToString();

        btnChangeName.onClick.AddListener(OnChangeName);
        UpdateAvatar();
        //Load Vehicle
        if(GameManager.Instance.playerData.vehiclesOwned.Count == 0)
        {
            txtVehicleViewFail.text = "don't own any vehicle yet";
            GameManager.Instance.garaje3D.gameObject.SetActive(false);
            carView.SetActive(false);
            return;
        }
        GameManager.Instance.garaje3D.gameObject.SetActive(true);
        var bestcar = GameManager.Instance.playerData.GetBestVehicle();
        if (bestcar == null)
        {
            txtVehicleViewFail.text = "Vehicle view not available";
            return;
        }
        carView.SetActive(true);
        GameManager.Instance.garaje3D.AddPlayerVehicle();
    }

    public void InitOtherPlayer(Action onCloseCallback, LeaderboardPlayer data)
    {
        this.onCloseCallback = onCloseCallback;
        InitElements(false);
        txtName.text = data.playername;
        txtLevel.text = data.level.ToString();
        levelProgress.value = (float)data.xp / GameManager.Instance.gameConfig.NextLevelXP(data.level);
        txtXP.text = $"{data.xp}/{GameManager.Instance.gameConfig.NextLevelXP(data.level)}";
        txtFame.text = data.score.ToString();
        txtMergeCount.text = data.merges.ToString();
        txtMapMissions.text = data.missions.ToString();
        txtJobsCount.text = data.orders.ToString();
        txtTravelCount.text = data.travels.ToString();
        txtVehicleCount.text = data.vehicles.ToString();
        txtBuildingsCount.text = data.buildings.ToString();
        txtGiftsCount.text = data.gifts.ToString();
        txtStolenCount.text = data.stolen.ToString();
        iconCharacter.sprite = GameManager.Instance.gameConfig.GetCharacter(data.charIndex).sprite;
        //Load Vehicle
        carView.SetActive(data.vehicle > 0);
        if (data.vehicle > 0)
        {
            GameManager.Instance.garaje3D.gameObject.SetActive(true);
            GameManager.Instance.garaje3D.AddVehicle(data.vehicle,true,data.material);
        }
        Show();
    }

    private void InitElements(bool isPlayer)
    {
        btnChangeName.gameObject.SetActive(isPlayer);
        btnChangeAvatar.gameObject.SetActive(isPlayer);
    }

    protected override void OnClose()
    {
        base.OnClose();
        GameManager.Instance.garaje3D.gameObject.SetActive(false);
    }

    private void OnChangeName()
    {
        _onChangeNameCallback?.Invoke();
    }

    public void OpenSelectAvatarPopup()
    {
        GameManager.Instance.PopupsManager.ShowChangeAvatar(UpdateAvatar);
    }

    void UpdateAvatar()
    {
        iconCharacter.sprite = GameManager.Instance.topBar.GetPlayerSprite();
    }
}
