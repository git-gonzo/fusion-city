using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillRequeriment
{
    public SO_SkillType skill;
    public int level;
}

[Serializable]
public class SkillGroupRequeriment
{
    public SkillGroup skill;
    public int level;
}
