using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Mueble", menuName = "House")]
public class SO_Mueble : ScriptableObject
{
    public MuebleType type;
    public string muebleName;
    public int price;
    public RewardType currency;
    //public Sprite spriteImg;
    //public Material material;
    public GameObject prefab;
    public bool owned;
    public bool unlocked;

    //TODO: Unlock requirements
}
