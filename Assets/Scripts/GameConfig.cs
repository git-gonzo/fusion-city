using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    [Serializable]
    public class VehicleIcon
    {
        public VehicleType transportType;
        public Sprite icon;
    }
    public bool objectsVisibility;
    public bool seaFoam;
    public int Level1XP = 5;
    public int Level2XP = 50;
    public int Level3XP = 150;
    public int Level4XP = 500;
    public int Level5XP = 2000;
    [Range(1,10)]
    public float LevelMultiplier = 1.5f;

    public List<SO_BuildingActivity> buildingsActivities;
    public List<SO_Vehicle> vehiclesDefinition;
    public List<VehicleIcon> vehicleIcons;
    public JobsConfig jobsConfig;
    public List<Sprite> allPortraits;

    private Dictionary<string, SO_BuildingActivity> activities;

    public int NextLevelXP(int currentLevel)
    {
        if (currentLevel == 1) return Level1XP;
        if (currentLevel == 2) return Level2XP;
        if (currentLevel == 3) return Level3XP;
        if (currentLevel == 4) return Level4XP;
        if (currentLevel == 5) return Level5XP;
        return (int)(Level5XP * Mathf.Pow(LevelMultiplier,currentLevel)*.5f);
    }

    private void FillDictionary()
    {
        activities = new Dictionary<string, SO_BuildingActivity>();
        for (var i = 0; i < buildingsActivities.Count; i++)
        {
            activities.Add(buildingsActivities[i].activityName, buildingsActivities[i]);
        }
    }

    public void Admin_ShowLevelsXP()
    {
        for(var i = 1; i< 25; i++)
        {
            Debug.Log("Level " + i + " - " + NextLevelXP(i));
        }
    }
}

[Serializable]
public class JobsConfig
{
    public List<SO_Job> jobsListCommon;
    public List<SO_Job> jobsListUncommon;
    public List<SO_Job> jobsListRare;
    public List<SO_Job> jobsListVeryRare;
    public List<SO_Job> jobsListSpecial;
    public List<SO_Job> jobsListEpic;
    public List<SO_Job> jobsListUnique;
}