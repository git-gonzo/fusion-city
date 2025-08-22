using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.AddressableAssets.Addressables;

public class MergeMapMissionsController : MonoBehaviour
{
    public Transform missionsContainer;
    public MergeMapMissionElement missionPrefab;
    public MergeMapMissionElement limitedMissionPrefab;
    public MapMissionCharacter missionCharacterPrefab;
    public PopupTutorial popupTutorial;
    public MergeBoardModel mergeModel => GameManager.Instance.MergeManager.boardModel;
    private MergeMapMissionElement _limitedMissionPrefabInstance;

    public void Show()
    {
        LoadMissions();
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
    public void LoadMissions(BuildingType bType = BuildingType.None)
    {
        while (missionsContainer.childCount > 0)
        {
            DestroyImmediate(missionsContainer.GetChild(0).gameObject);
        }
        if (mergeModel.mapMissionsCharacters != null && mergeModel.mapMissionsCharacters.Count > 0) AddCharacterMissions();

        for (var i = 0; i < mergeModel.mapMissionsNew.Count; i++) {
            var missionConfig = mergeModel.mapMissionsNew[i];
            if (bType == BuildingType.None || bType == missionConfig.location)
            {
                var m = Instantiate(missionPrefab, missionsContainer);
                m.AddRequests(missionConfig, Close);
                if (m.CanBeCompleted) m.transform.SetAsFirstSibling();
            }
        }
        if(mergeModel.limitedMission != null) AddLimited();
    }

    public void LazyUpdate()
    {
        if (_limitedMissionPrefabInstance != null)
        {
            _limitedMissionPrefabInstance.LazyUpdate();
        }
    }

    public void AddCharacterMissions()
    {
        for (var i = 0; i < mergeModel.mapMissionsCharacters.Count; i++)
        {
            var missionConfig = mergeModel.mapMissionsCharacters[i];
            var m = Instantiate(missionCharacterPrefab, missionsContainer);
            m.AddRequests(missionConfig, Close);
            if (m.CanBeCompleted) m.transform.SetAsFirstSibling();
        }
    }
    public void AddLimited()
    {
        _limitedMissionPrefabInstance = Instantiate(limitedMissionPrefab, missionsContainer);
        _limitedMissionPrefabInstance.AddRequests(mergeModel.limitedMission, Close);
        _limitedMissionPrefabInstance.transform.SetAsFirstSibling();
    }

    public void ShowPopupTutoMissions()
    {
        GameManager.Instance.PopupsManager.ShowPopupTutorial(popupTutorial);
    }
}
