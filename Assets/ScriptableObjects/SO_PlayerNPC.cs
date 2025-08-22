using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newNPC", menuName = "NPC")]
public class SO_PlayerNPC : ScriptableObject
{

    public string npcName;
    public Gender gender;
    public int age;
    public int level;
    public int xp;
    public int famePoints;
    public List<NPC_Skill> skills;
    public List<NPC_Attribute> attributes;
    public Sprite avatar;
}

public enum Gender
{
    Male,
    Femmale
}

[System.Serializable]
public class NPC_Skill
{
    public SO_SkillType skillType;
    public int skillValue;
}

[System.Serializable]
public class NPC_Attribute
{
    public AttributeType attType;
    public float value;
}
