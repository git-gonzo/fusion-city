using Assets.Scripts.MergeBoard;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Building", menuName = "Fame/Building")]
public class SO_Building : ScriptableObject
{
    public BuildingType buildingType;
    public string buildingName;
    public string buildingDescrip;
    public bool isHouse;
    public int unlockLevel;
    public int boardUnlockLevel;
    public BoardConfig boardConfig;
    public List<SO_Vehicle> vehiclesInShop;
    public CharacterStory characterStory;

    public bool IsUnlocked => unlockLevel <= GameManager.Instance.PlayerLevel;
    public MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
    public CharacterStoryStep TryGetCharacterStep() 
    {
        if(characterStory == null)
        {
            return null;
        }
        for (int i = 0; i < characterStory.charactersSteps.Count; i++)
        {
            var step = characterStory.charactersSteps[i];
            if (step.IsAvailable())
            {
                return step;
            }
        }
        return null;
    }
}

[System.Serializable]
public class BuildingOwned
{
    public BuildingType buildingType;
    public int level;
    //Todo add last time claimed

    public BuildingOwned(BuildingType b)
    {
        buildingType = b;
        level = 1;
    }
    [JsonConstructor]
    public BuildingOwned(BuildingType b, int l = 1)
    {
        buildingType = b;
        level = l;
    }

    public TimeSpan LastBuildingClaim
    {
        get {
            return DateTime.Now - UIUtils.GetTimeStampByKey("LastClaimedReward" + (int)buildingType); 
        }
    }
    public void SetLastBuildingClaim()
    {
        UIUtils.SaveTimeStamp("LastClaimedReward" + (int)buildingType, DateTime.Now);
    }
}
