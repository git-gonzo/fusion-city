using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Job", menuName = "Fame/Jobs")]
public class SO_Job : ScriptableObject
{
    public string Title;
    public string Description;
    public int timeToComplete;
    public RarityType rarityType;
    public List<RewardData> rewards;
    public List<SkillRequeriment> requiredSkills;
    public List<SkillGroupRequeriment> requiredSkillGroup ;
    public int requiredPlayerLevel;

    public bool IsValid()
    {
        if (GameManager.Instance.PlayerLevel < requiredPlayerLevel) return false;

        //Is in cooldown

        if (PlayerPrefs.HasKey("Cooldown" + Title))
        {
            Debug.Log("SOJob " + Title + " key found");
            if (UIUtils.GetTimeStampByKey("Cooldown" + Title) > DateTime.Now)
            {
                Debug.Log("SOJob " + Title + " is in cooldown");
                return false;
            }

        }

        return true;
    }
}
