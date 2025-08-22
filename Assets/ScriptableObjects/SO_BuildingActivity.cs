using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new BuildingActivity", menuName = "Fame/BuildingActivity")]
public class SO_BuildingActivity : ScriptableObject
{
    public string activityName;
    public string activityDescrip;
    [Tooltip("In seconds")]
    public int duration;
    public bool videoAd;
    public List<RewardData> currencyRewards;
    public List<RewardAttribute> attributeRewards;
    public List<SO_SkillType> requiredSkills;
    public int enterPrice;
}