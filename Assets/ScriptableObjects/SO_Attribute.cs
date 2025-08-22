using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Attribute", menuName = "Fame/Attribute")]
public class SO_Attribute : ScriptableObject
{
    public AttributeType attributeType;
    public string attributeName;
    public float value;
    public AttributeGroup group;
    public Sprite icon;
    public string attShortName { get {
        if(attributeName.Length > 7)
        {
                return attributeName.Substring(0, 6) + "..";
        }
            return attributeName;
    } }

    public void ChangeAtt(float amount)
    {
        value += amount; 
        value = Mathf.Clamp(value,0,20);
        SetDirty();
    }
}

public enum AttributeGroup
{
    physic,
    mental,
    social
}
