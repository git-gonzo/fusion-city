using System.Collections.Generic;
using System;
using UnityEngine;

public class AssistantsManager : MonoBehaviour
{
    private static AssistantsManager instance;
    public static AssistantsManager Instance => instance; 
    List<AssistantConfig> assistantsConfig => GameManager.Instance.gameConfig.assistantsConfig;
    AssistantsModel assistantsModel => GameManager.Instance.mergeModel.assistants;

    public bool TryToShowAssistantOffer(out AssistantConfig assistantConfig)
    {
        assistantConfig = null;
        foreach (var config in assistantsConfig)
        {
            if (!assistantsModel.HasAssistant(config.assistantType) &&
                config.dayOfWeekToShow == DateTime.UtcNow.DayOfWeek)
            {
                //SHOW ASSISTANT IN SIDE BAR
                assistantConfig = config;
                return true;
            }
        }
        return false;
    }
}

public enum AssistantType
{
    SmartGenerators,
    BuildingMoneyCollector
}
public enum AssistantExpiration
{
    Day,
    Week,
    Month
}

[Serializable]
public class AssistantsModel
{
    public List<AssistantModel> assistants = new();
    public List<AssistantType> assistantsUsed = new();

    public void AddAssistantDays(AssistantType assistantType, int expirationDays)
    {
        if(!assistantsUsed.Contains(assistantType))
        {
            assistantsUsed.Add(assistantType);
        }
        assistants ??= new List<AssistantModel>();
        CheckExpired();
        assistants.Add(new AssistantModel(assistantType, DateTime.Now.AddDays(expirationDays)));
    }

    public void CheckExpired()
    {
        /*foreach (var assistant in assistants)
        {
            if (assistant.expiration > DateTime.Now)
            {
                
            }
        }*/
        assistants.RemoveAll(a => a.expiration < DateTime.Now);
    }

    public bool HasAssistant(AssistantType assistantType)
    {
        foreach (var assistant in assistants)
        {
            if (assistant.assistantType == assistantType && assistant.expiration > DateTime.Now)
            {
                return true;
            }
        }
        return false;
    }

    public DateTime Expiration(AssistantType assistantType) 
    {
        foreach (var assistant in assistants)
        {
            if (assistant.assistantType == assistantType)
            {
                return assistant.expiration;
            }
        }
        return DateTime.Now;
    }

    public bool IsAssistantFirstTime(AssistantType assistantType)
    {
        return !assistantsUsed.Contains(assistantType);
    }

#if UNITY_EDITOR
    public void AdminClearAssistants()
    {
        assistants.Clear();
    }
#endif
}


[Serializable]
public class AssistantModel
{
    public DateTime expiration;
    public AssistantType assistantType;

    public AssistantModel(AssistantType assistantType, DateTime expiration)
    {
        this.assistantType = assistantType;
        this.expiration = expiration;
    }
}
