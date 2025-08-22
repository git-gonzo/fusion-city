using Sirenix.OdinInspector;
using UnityEngine;

public class SO_CharacterStoryStep : ScriptableObject
{
    public string id;
    public string textContent;
    public bool hasRequirement;
    [ShowIf("@hasRequirement == true")] public string requiredId;
} 
