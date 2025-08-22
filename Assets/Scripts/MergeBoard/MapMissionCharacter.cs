using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MapMissionCharacter : MergeMapMissionElement
{
    public Image CharacterAvatarImage;
    public TextMeshProUGUI txtCharacterName;
    public Color CharacterNameColor;
    public LocalizedString specialDeliveryLocalizedString;
    private MapMissionCloudCharacter _characterMissionConfig;
    public void AddRequests(MapMissionCloudCharacter request, Action onClose)
    {
        RemoveObjects();
        _missionLocation = request.location;
        _missionConfig = request;
        _characterMissionConfig = request;
        _onClose = onClose;
        var SOChar = GameManager.Instance.gameConfig.GetCharacterById(request.characterId);
        txtCharacterName.text =  $"<color=yellow> " + SOChar.characterName + "</color>";//,  + specialDeliveryLocalizedString.GetLocalizedString();
        CharacterAvatarImage.sprite = SOChar.characterPortrait;
        requestItems = new List<MergeMapMissionRequestItem>();
        for (var j = 0; j < request.piecesRequest.Count; j++)
        {
            var r = Instantiate(requestItem, missionsContainter);
            r.AddItem(request.piecesRequest[j]);
            r.SetDefaultBGOrange(true);
            requestItems.Add(r);
        }
        rewardsContainer.FillRewards(request.rewards, GameManager.Instance.topBar, true);
        CheckCanbeCompleted();
        CheckLocation();
        btnCompleteMission.onClick.AddListener(CompleteCharacterMission);
    }
    protected void CompleteCharacterMission()
    {
        CompleteCommon();
        GameManager.Instance.CompleteCharacterMission(_characterMissionConfig);
    }
    
}