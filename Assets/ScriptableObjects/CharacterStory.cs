using Assets.Scripts.MergeBoard;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "CharacterStory", menuName = "Fame/CharacterStory")]
public class CharacterStory : ScriptableObject
{
    public List <CharacterStoryStep> charactersSteps;
    public CharacterStoryModel model => GameManager.Instance.mergeModel.characterStoryModel;
    public CharacterStoryStep GetStep()
    {
        // Try To find steps with valid requirements
        foreach (CharacterStoryStep step in charactersSteps)
        {
            if(step.hasRequirement)
            {
                if (step.IsAvailable())
                {
                    return step;
                }
            }
        }
        // Return first not completed step with no requirements
        foreach (CharacterStoryStep step in charactersSteps)
        {
            if (step.hasRequirement) continue;

            if (step.IsAvailable())
            {
                return step;
            }
        }
        //Else return fist step
        return charactersSteps[0];
    }
}

[System.Serializable]
public class CharacterStoryStep
{
    public string id;
    public SO_Character character;
    public string textContent;
    public List<LocalizedString> localizedKeys;
    public bool isUnique;
    public bool hasRequirement;
    [ShowIf("@hasRequirement == true")] public int requiredLevel;
    [ShowIf("@hasRequirement == true")] public string requiredId;
    public bool hasMission;
    [ShowIf("@hasMission == true")] public MapMissionCloudCharacter mission;
    [ShowIf("@hasMission == true")] public List<LocalizedString> waitingForMissionlocalizedKeys;
    public bool hasRewards;
    [ShowIf("@hasRewards == true")] public List<RewardData> rewards;

    public Sprite CharacterImage => character.characterImage;
    public Sprite CharacterPortrait => character.characterPortrait;
    public int CharacterId => character.characterId;
    public string CharacterName => character.characterName;


    public CharacterStoryModel model => GameManager.Instance.mergeModel.characterStoryModel;
    public MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
    public bool IsAvailable()
    {
        if(hasRequirement)
        {
            if (requiredLevel > PlayerData.Instance.level)
            {
                return false;
            }
            //Allow only one Character mission at a time
            if(mergeModel.mapMissionsCharacters.Count > 0)
            {
                if (mergeModel.mapMissionsCharacters[0].characterId == CharacterId)
                {
                    return true;
                }
                return false;
            }

            if(requiredId != string.Empty)
            {
                return !model.StepNotSeen(requiredId) && model.StepNotSeen(id);
            }
            else
            {
                return model.StepNotSeen(id);
            }
        }
        return model.StepNotSeen(id);
    }
}
