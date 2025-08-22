using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System;

public class PopupOtherPlayer : Popup_MapBase
{
    public PopupAvatarItem avatar;
    public TextMeshProUGUI txtPosition;
    public TextMeshProUGUI txtPoints;
    public TextMeshProUGUI txtLevel;
    public TextMeshProUGUI txtLocation;
    public GameObject containerNotInPosition;
    public GameObject containerInPosition;
    public Button btnMakeGift;
    public Button btnSteal;
    public Button btnProfile;
    public Button btnGo;

    private BuildingType _location;
    private LeaderboardPlayer _playerData;

    public void Show(LeaderboardPlayer playerData)
    {
        _playerData = playerData;
        _location = (BuildingType)playerData.location;
        content ??= GetComponent<CanvasGroup>();
        Title.text = playerData.playername;
        containerNotInPosition.SetActive(playerData.location != (int)PlayerData.playerLocation);
        containerInPosition.SetActive(playerData.location == (int)PlayerData.playerLocation);
        btnMakeGift.gameObject.SetActive(playerData.location == (int)PlayerData.playerLocation);
        btnSteal.gameObject.SetActive(playerData.location == (int)PlayerData.playerLocation);
        btnClose.onClick.RemoveAllListeners();
        btnClose.onClick.AddListener(Cancel);
        txtPoints.text = UIUtils.FormatNumber(playerData.score);
        avatar.SetAvatarByIndex(playerData.charIndex);

        if (playerData.location == (int)PlayerData.playerLocation)
        {
            //GameManager.Instance.GiftsStealController.GenerateItemsToGive(out var availableGifts, out var itemsInMissions, playerData);
            txtPosition.text = playerData.position.ToString();
            txtLevel.text = playerData.level.ToString();
            btnProfile.onClick.RemoveAllListeners();
            btnMakeGift.onClick.RemoveAllListeners();
            btnSteal.onClick.RemoveAllListeners();
            btnMakeGift.onClick.AddListener(OnMakeGift);
            btnSteal.onClick.AddListener(OnSteal);
            btnProfile.onClick.AddListener(()=> GameManager.Instance.PopupsManager.ShowOtherPlayerStats(playerData));
        }
        else
        {
            txtTravelTime.text = UIUtils.FormatTime(GetTimeToGetBuilding(_location));
            SetIconBestVehicle();
            btnGo.onClick.RemoveAllListeners();
            btnGo.onClick.AddListener(StartMoving);
            if (GameServerConfig.Instance.ConfigHasBuilding(_location))
            {
                GameServerConfig.Instance.SetBuildingLocTitle(_location, txtLocation, "_name");
            }
            else
            {
                var building = GameManager.Instance.mapManager.GetBuildingInteractiveFromType(_location);
                txtLocation.text = building.buildingData.buildingName;
            }
        }
        gameObject.SetActive(true);
        content.DOFade(1, 0.3f);
    }

    private void OnSteal()
    {
        if(GameManager.Instance.PlayerLevel < 4)
            GameManager.Instance.PopupsManager.ShowPopupYesNo("Feature locked", "Unlock this feature at level 5!",PopupManager.PopupType.ok);
        else
            GameManager.Instance.ShowStealScreen(true, _playerData);
        //GameManager.Instance.PopupsManager.ShowPopupYesNo("Coming Soon", "This feature will come in a next release\nStay tunned!",PopupManager.PopupType.ok);
    }

    private void OnMakeGift()
    {
        GameManager.Instance.ShowSendGifts(true, _playerData);
    }

    public override void StartMovingEnd()
    {
        Cancel();
        GameManager.Instance.StartTravelling(_location);
    }
}
