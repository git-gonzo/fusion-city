using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum BuildingType
{
    None = 0,
    //Houses Location
    TutorialHouse = 38,

    //Interactive Buildings
    Academy = 5,
    Acting_School = 9,
    Airport = 23,
    AmericanFootball = 50,
    Bakery = 51,
    Bank_small = 42,
    BasketFieldSmall = 24,
    Bar_small = 33,
    Bar_medium = 34,
    Bar_big = 35,
    BicycleShop = 39,
    Casino = 10,
    CasinoHeist = 56,
    CarShop = 43,
    CarShopLuxury = 54,
    CarWorkshop = 40,
    Church = 37,
    Cinema = 41,
    Circuit = 44,
    Circus = 32,
    CityHall = 48,
    ConcertStageSmall = 21,
    ConcertStageBig = 22,
    Dance_School = 7,
    FootballSmall = 15,
    FootballMedium = 16,
    FootballBig = 17,
    GaleryArt = 49,
    Gym = 1, 
    Hospital = 30,
    Hotel = 29,
    House = 2,
    Hairdresser = 3,
    HelicpterShop = 45,
    Museum = 4,
    Music_School = 8,
    Noria = 25,
    Opera = 28,
    PoliceStationSmall = 46,
    RestaurantSmall = 18,
    RestaurantMedium = 19,
    RestaurantBig = 20,
    ShopMoto = 52,
    ShopSkate = 47,
    ShopSofa = 31,
    ShoppingCenter = 55,
    Skatepark = 36,
    Stadium = 11,
    Supermarket = 27,
    Surfing = 26,
    TenisSmall = 12,
    TenisMedium = 13,
    TenisBig = 14,
    University = 6,
    RestaurantSquare = 53,
    //Update Count with the last number when adding new type
    Count = 57
}

public class BuildingsManager : MonoBehaviour
{
    public static void SavePeopleInBuilding(BuildingType building, List<string> people)
    {
        var jsonNPCs = JsonConvert.SerializeObject(people);
        PlayerPrefs.SetString("peopleBuilding" + building, jsonNPCs);
    }

    public static void AdminRemoveAllPeopleFromAllBuildings()
    {
        for (var i = 0; i<(int)BuildingType.Count; i++)
        {
            PlayerPrefs.SetString("peopleBuilding" + (BuildingType)i, "");
        }
    }

    public static void SetBuildNameToText(SO_Building buildingData, TextMeshProUGUI textField, bool isName)
    {
        
        if (GameServerConfig.Instance.ConfigHasBuilding(buildingData.buildingType))
        {
            GameServerConfig.Instance.SetBuildingLocTitle(buildingData.buildingType, textField, isName?"_name":"_descrip");
        }
        else
        {
            textField.text = isName? buildingData.buildingName: buildingData.buildingDescrip;
        }
    }
}
