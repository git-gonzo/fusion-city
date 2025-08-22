using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Attributes", menuName = "Fame/Attributes")]
public class SO_Attributes : ScriptableObject
{
    public List<SO_Attribute> Physical;
    public List<SO_Attribute> Mental;
    public List<SO_Attribute> Social;

    //public Dictionary<AttributeGroup, SO_Attribute> Phy;
    /*
    //Physical
    public int Strength;
    public int Endurance;
    public int Agility;
    public int Balance;
    public int Health;
    public int Taste;
    public int Sight;
    public int Speed;
    public int Desterity;

    //Mental
    public int Intelligence;
    public int Imagination;
    public int Charisma;
    public int Intuition;
    public int Memory;
    public int Wisdom;
    public int Determination;
    public int Creativity;
    public int Perception;

    //Social
    public int Humor;
    public int Charm;
    public int Sentuality;
    public int Empathy;
    public int Loyalty;
    public int Curiosity;
    public int Patience;
    public int Humility;
    public int Sympathy;
    public int Kindness;
    public int Candor; //Sinceridad, franqueza*/
}
