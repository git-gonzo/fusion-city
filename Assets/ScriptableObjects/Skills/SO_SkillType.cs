using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newSkillDef", menuName = "SkillDef")]
public class SO_SkillType : ScriptableObject
{
    public List<SkillRequeriment> requirement;
    public SkillGroup skillGroup;
    public string SkillTypeName;
    public string DisplayName;
    public string skillDescription;
}

public enum SkillGroup
{
    Art,
    Mind,
    Body
}