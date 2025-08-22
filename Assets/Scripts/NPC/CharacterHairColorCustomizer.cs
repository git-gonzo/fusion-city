//using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHairColorCustomizer : MonoBehaviour
{
    [System.Serializable]
    public class Outfit
    {
        public GameObject outfit;
        public List<Material> materialsOutFit;
    }

    public GameObject currentOutfit;
    public GameObject currentHair;

    public List<Outfit> outfits;
    public List<Material> materialsBody;
    public List<Material> materialsMakeup;
    public List<Material> materialsHair;

    private void ChangeOutfitMaterial(int materialIndex )
    {
        currentOutfit.GetComponent<SkinnedMeshRenderer>().materials[0].CopyPropertiesFromMaterial(outfits[0].materialsOutFit[materialIndex]);
    }
    private void ChangeBodyMaterial(int materialIndex )
    {
        currentOutfit.GetComponent<SkinnedMeshRenderer>().materials[1].CopyPropertiesFromMaterial(materialsBody[materialIndex]);
    }
    private void ChangeMakeUpMaterial(int materialIndex )
    {
        currentOutfit.GetComponent<SkinnedMeshRenderer>().materials[2].CopyPropertiesFromMaterial(materialsMakeup[materialIndex]);
    }
    private void ChangeHairMaterial(int materialIndex )
    {
        var i = Random.Range(0, materialsHair.Count);
        currentHair.GetComponent<SkinnedMeshRenderer>().materials[0].CopyPropertiesFromMaterial(materialsHair[i]);
    }

    public void SetBodyMaterial1(){ ChangeBodyMaterial(0);}

    public void SetMakeUpMaterial2(){ ChangeMakeUpMaterial(0);}
    public void SetMakeUpMaterial3(){ ChangeMakeUpMaterial(1);}
    public void SetMakeUpMaterial4(){ ChangeMakeUpMaterial(2);}

    public void SetRandomHairMaterial(){ ChangeHairMaterial(0);}
    public void SetRandomOutfitMaterial(){
        var i = Random.Range(0, outfits[0].materialsOutFit.Count);
        ChangeOutfitMaterial(i);
    }


    public void SetOutfitMat1(){ChangeOutfitMaterial(0);}
    public void SetOutfitMat2(){ChangeOutfitMaterial(1);}
    public void SetOutfitMat3(){ChangeOutfitMaterial(2);}
    public void SetOutfitMat4(){ChangeOutfitMaterial(3);}
    public void SetOutfitMat5(){ChangeOutfitMaterial(4);}
    public void SetOutfitMat6(){ChangeOutfitMaterial(5);}

}
