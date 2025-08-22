using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.MergeBoard;
using Sirenix.OdinInspector;
using Unity.Services.RemoteConfig;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class GameConfigMerge : MonoBehaviour
{
    [Serializable]
    public class VehicleIcon
    {
        public VehicleType transportType;
        public Sprite icon;
    }
    [Serializable]
    public class CarOffersConfig
    {
        public List<CarOffer> carOffers;
    }
    [Serializable]
    public class CarOffer
    {
        [ValidateInput("CheckVehicleID", "$validationMSG")]
        [InfoBox("$validationMSG")]
        public int vehicleId;
        public List<RewardData> extraRewards;
        public int priceTier;
        private string validationMSG = "Test";
        GameConfigMerge _gameConfig;
        GameConfigMerge gameConfig => _gameConfig ??= GameObject.Find("GameConfig").GetComponent<GameConfigMerge>();
        private bool CheckVehicleID()
        {
            if (vehicleId == 0)
            {
                validationMSG = "Insert Vehicle ID";
                return false;
            }
            else
            {
                var v = gameConfig.GetVehicleById(vehicleId);
                if (v == null)
                {
                    validationMSG = "Vehicle not found";
                    return false;
                }
                else
                {
                    validationMSG = v.vehicleName;
                }
            }
            return true;
        }
    }
    public bool objectsVisibility;

    public int Level1XP = 5;
    public int Level2XP = 50;
    public int Level3XP = 150;
    public int Level4XP = 500;
    public int Level5XP = 2000;
    [Range(1,10)]
    public float LevelMultiplier = 1.5f;
    public Sprite defaultAvatar;
    public List<CharacterData> Avatars;
    public List<SO_Vehicle> vehiclesDefinition;
    public List<VehicleIcon> vehicleIcons;
    public List<GeneratorConfig> generatorsConfig;
    public BuildingsConfig buildingsConfig; 
    public List<SO_Character> charactersStory;
    public List<AssistantConfig> assistantsConfig;
    public SeasonRewards seasonRewardsConfig;
    public MergeConfig mergeConfig;
    public RewardData energyRefillCost;
    public int energyRefillAmountWithGems = 50;
    public int energyRefillAmountWithVideo = 20;
    public int welcomePackHoursDuration = 72;
    public int welcomePackShowOnLevel = 4;
    public int specialOfferDuration = 24;
    public List<CarOffer> carOffers;
    public CanvasPlayerMap mapPlayerName;

    public static GameConfigMerge instance;

    [SerializeField] public List<RouletteConfigRef> RouletteConfigsDict;

    public struct userAttributes
    {
        // Optionally declare variables for any custom user attributes:
        //public bool expansionFlag;
    }

    public struct appAttributes
    {
        // Optionally declare variables for any custom app attributes:
        //public int level;
        //public int score;
        //public string appVersion;
    }

    public int NextLevelXP(int currentLevel)
    {
        if (currentLevel == 1) return Level1XP;
        if (currentLevel == 2) return Level2XP;
        if (currentLevel == 3) return Level3XP;
        if (currentLevel == 4) return Level4XP;
        if (currentLevel == 5) return Level5XP;
        return (int)(Level5XP * Mathf.Pow(LevelMultiplier,currentLevel)*.5f);
    }

    public void Admin_ShowLevelsXP()
    {
        for(var i = 1; i< 25; i++)
        {
            Debug.Log("Level " + i + " - " + NextLevelXP(i));
        }
    }
    private async void Awake()
    {
        DontDestroyOnLoad(this);
        instance = this;
        await Globals.IsSignedIn();
        foreach (var item in buildingsConfig.data)
        {
            //Debug.Log($"GameConfig building {item.buildingID}: {item.stringKey}, price: {item.price}");
            var stringOperation1 = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Buildings", item.stringKey + "_name");
            var stringOperation2 = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Buildings", item.stringKey + "_descrip");
        }
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        ChangeEnvironment();
    }

    async Task InitializeRemoteConfigAsync()
    {
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync();

        // options can be passed in the initializer, e.g if you want to set AnalyticsUserId or an EnvironmentName use the lines from below:
        // var options = new InitializationOptions()
        // .SetEnvironmentName("testing")
        // .SetAnalyticsUserId("test-user-id-12345");
        // await UnityServices.InitializeAsync(options);

        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public AssistantConfig GetAssistantConfig(string id)
    {
        foreach (var assistant in assistantsConfig)
        {
            if(assistant.id == id)
            {
                return assistant;
            }
        }
        throw new Exception($"Assistant config id {id} not found");
    }
    public AssistantConfig GetAssistantConfig(AssistantType assistantType)
    {
        foreach (var assistant in assistantsConfig)
        {
            if(assistant.assistantType == assistantType)
            {
                return assistant;
            }
        }
        throw new Exception($"Assistant config type {assistantType.ToString()} not found");
    }

    public CharacterData GetCharacter(int id)
    {
        var ch = Avatars.Find(c => c.id == id);
        if (ch == null)
        {
            var c = new CharacterData();
            c.sprite = defaultAvatar;
            return c;
        }
        return ch;
    }

    public SO_Vehicle GetVehicleById(int id)
    {
        return vehiclesDefinition.Find(v => v.id == id);
    }
    public SO_Character GetCharacterById(int id)
    {
        return charactersStory.Find(c => c.characterId == id);
    }

    public void ChangeEnvironment(bool isProd = true)
    {
#if UNITY_EDITOR
        if (!isProd)
        {
            RemoteConfigService.Instance.SetEnvironmentID("cc0a50ab-797a-494c-bdea-e745b17296d0");
        }
        else
        {
            RemoteConfigService.Instance.SetEnvironmentID("58a6b02f-72ad-482e-b925-f624f58d4247");
        }
#endif
        RemoteConfigService.Instance.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

    void ApplyRemoteConfig(ConfigResponse configResponse)
    {
        
        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                GameManager.Log("No configs loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                GameManager.Log("No configs loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("AppVersion:" + Application.version);
                //Debug.Log("New settings loaded this session; updating values.");
                foreach (var v in vehiclesDefinition)
                {
                    if (RemoteConfigService.Instance.appConfig.HasKey(v.name))
                    {
                        var vehicleConfig = RemoteConfigService.Instance.appConfig.GetJson(v.name);
                        v.UpdateDataFromConfig(JsonConvert.DeserializeObject<VehicleConfig>(vehicleConfig));
                        //Debug.Log(v.name + "Config Updated");
                    }
                }
                if (RemoteConfigService.Instance.appConfig.HasKey("MapMissionsConfig"))
                { 
                    var newMissionsConfig = RemoteConfigService.Instance.appConfig.GetJson("MapMissionsConfig");
                    mergeConfig.mapMissionsConfig = JsonConvert.DeserializeObject<MergeMapMissionsConfigs>(newMissionsConfig);
                    //Debug.Log("Map Missions Updated");
                }
                if (RemoteConfigService.Instance.appConfig.HasKey("SeasonRewardsConfig"))
                {
                    var newSeasonRewardsConfig = RemoteConfigService.Instance.appConfig.GetJson("SeasonRewardsConfig");
                    seasonRewardsConfig = JsonConvert.DeserializeObject<SeasonRewards>(newSeasonRewardsConfig);
                    //Debug.Log("Map Missions Updated");
                }
                foreach (var roulette in RouletteConfigsDict) 
                {
                    if (RemoteConfigService.Instance.appConfig.HasKey(roulette.key))
                    {                        
                        var r = RemoteConfigService.Instance.appConfig.GetJson(roulette.key);
                        roulette.refConfig = JsonConvert.DeserializeObject<RouletteConfig>(r);
                    }
                }
                if (RemoteConfigService.Instance.appConfig.HasKey("BuildingsConfig"))
                {
                    var buildings = RemoteConfigService.Instance.appConfig.GetJson("BuildingsConfig");
                    buildingsConfig = JsonConvert.DeserializeObject<BuildingsConfig>(buildings);
                }
                if (RemoteConfigService.Instance.appConfig.HasKey("CarOffersConfig"))
                {
                    var carOffersremote = RemoteConfigService.Instance.appConfig.GetJson("CarOffersConfig");
                    carOffers = JsonConvert.DeserializeObject<List<CarOffer>>(carOffersremote);
                }
                /*foreach(var generator in generatorsConfig)
                {
                    if (ConfigManager.appConfig.HasKey(generator.name))
                    {
                        var generatorConfig = ConfigManager.appConfig.GetJson(generator.name);
                        generator.UpdateDataFromConfig(JsonConvert.DeserializeObject<GeneratorConfig>(generatorConfig));
                        //Debug.Log(generator.name + "Config Updated");
                    }
                }*/
                Globals.configLoaded = true;
                break;
        }
    }

}

[Serializable]
public class VehicleData
{
    public SO_Vehicle vehicleConfig;
    public GameObject vehiclePrefab;
}

[Serializable]
public class SeasonRewards
{
    public List<RewardData> rewards1;
    public List<RewardData> rewards2;
    public List<RewardData> rewards3;
    public List<RewardData> rewards4;
    public List<RewardData> rewards5;
    public List<RewardData> rewards6;
    public List<RewardData> rewards7_8;
    public List<RewardData> rewards9_10;
}

[Serializable]
public class CharacterData
{
    public int id;
    public Sprite sprite;
    public int unlockLevel;
    public bool isFree;
    [HideIf("isFree")]
    public RewardData price;
}

[Serializable]
public class RouletteConfigRef
{
    public string key;
    public RouletteConfig refConfig;
}

[Serializable]
public class AssistantConfig
{
    public string id;
    public AssistantType assistantType;
    public SO_Character character;
    public LocalizedString assistantDescrip;
    public DayOfWeek dayOfWeekToShow;
    public List<AssistantDurations> tiers;
}

[Serializable]
public class AssistantDurations
{
    public int durationDays;
    public AssistantTier tier;
}