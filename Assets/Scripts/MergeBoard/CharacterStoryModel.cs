using System;
using System.Collections.Generic;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Assets.Scripts.MergeBoard
{
    [Serializable]
    public class CharacterStoryModel
    {
        public List<CharacterStoryStepState> CharacterStepsStates = new List<CharacterStoryStepState>();


        public void StepState(string id, bool completed, bool hasMission = false, bool save = true)
        {
            CharacterStepsStates ??= new List<CharacterStoryStepState>();
            
            if (CharacterStepsStates.Exists(s => s.stepID== id)) 
            {
                CharacterStepsStates.Find(s => s.stepID == id).completed = completed;
            }
            else
            {
                var stepState = new CharacterStoryStepState();
                stepState.stepID = id;
                stepState.completed = completed;
                stepState.hasMission = hasMission;
                CharacterStepsStates.Add(stepState);
            }
            if(save) CloudSaveCharacterStory();
        }
        
        public void CompleteStepMission(string id)
        {
            CharacterStepsStates.Find(s => s.stepID == id).hasMission = true;
        }

        public void CloudSaveCharacterStory()
        {
            Debug.Log("Save characterStory Sent");
            var data = new Dictionary<string, object> { { "CharacterStepsStates", CharacterStepsStates } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }

        public bool StepNotSeen(string id)
        {
            var step = CharacterStepsStates.Find(s => s.stepID == id);
            if(step != null)
            {
                return !step.completed;
            }
            return true;
        }
        public bool StepExists(string id)
        {
            var step = CharacterStepsStates.Find(s => s.stepID == id);
            return step != null;
        }

        public bool IsCharacterMissionCompleted(string id)
        {
            var step = CharacterStepsStates.Find(s => s.stepID == id);
            if(step != null)
            {
                return step.hasMission;
            }
            return false;
        }

        public bool StepMissionCompleted(string stepId)
        {
            if (CharacterStepsStates.Exists(s => s.stepID == stepId))
            {
                return CharacterStepsStates.Find(s => s.stepID == stepId).hasMission;
            }
            return false;
        }
    }
}

[Serializable]
public class CharacterStoryStepState
{
    public string stepID;
    public bool completed;
    public bool hasMission;
}