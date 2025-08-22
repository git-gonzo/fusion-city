using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;

public class GameServerConfig : MonoBehaviour, IStoreListener
{
    public static GameServerConfig Instance => _instance;
    
    public Action OnSignedIn { get; set; }

    private static GameServerConfig _instance;


    //public BuildingsConfigOLD buildingsConfig;
    public BuildingsConfig buildingsConfig => GameConfigMerge.instance.buildingsConfig;
    public bool isConfigReady = false;
    public Action<string> OnLeaderboardResult;
    public string initialLeaderboardData;
    public JSONArray resultLogin;
    
    public bool isLogedin;

    internal void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    float startTime;
    private float startTime2;

    internal void GetLeaderboard()
    {
        startTime = Time.time;
        //StartCoroutine(StartGetLeaderboard());
    }

    public bool ConfigHasBuilding(BuildingType buildingType)
    {
        //if (buildingsConfig == null || buildingsConfig.data == null) return false;
        return GameConfigMerge.instance.buildingsConfig.data.Find((b) => b.buildingType == buildingType) != null;
    }
    public RewardData GetBuildingPrice(BuildingType buildingType)
    {
        if (!ConfigHasBuilding(buildingType)) return new RewardData(0,0);
        return buildingsConfig.data.Find((b) => b.buildingType == buildingType).BuildingPrice;
    }
    public BuildingConfigRaw GetBuildingConfig(BuildingType buildingType)
    {
        return buildingsConfig?.data?.Find((b) => b.buildingType == buildingType);
    }

    public void SetBuildingLocTitle(BuildingType buildingType, TMP_Text textField,string sufix)
    {
        if (ConfigHasBuilding(buildingType))
        {
            var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Buildings", buildingsConfig.GetBuildingLocKey(buildingType) + sufix);
            LoadString(stringOperation, textField);
        }
        else
        {
            textField.text = buildingType.ToString();
        }
    }

    private void LoadString(AsyncOperationHandle<string> stringOperation, TMP_Text textField) { 
        if (stringOperation.IsDone)
        {
            SetString(stringOperation, textField);
           
        } else { 
        //if (useCoroutine)
        //{
            object[] parms = new object[2] { stringOperation, textField };
            StartCoroutine("LoadStringWithCoroutine", parms);
        }
        /*
        else
            stringOperation.Completed += SetString;*/
    }

    IEnumerator LoadStringWithCoroutine(AsyncOperationHandle<string> stringOperation, TMP_Text textField)
    {
        yield return stringOperation;
        SetString(stringOperation, textField);
    }

    void SetString(object[] parms)
    {
        AsyncOperationHandle<string> stringOperation = (AsyncOperationHandle<string>)parms[0];
        TMP_Text textField = (TMP_Text)parms[1];
        SetString(stringOperation, textField);
    }
    void SetString(AsyncOperationHandle<string> stringOperation, TMP_Text textField)
    {
        // Its possible that something may have gone wrong during loading. We can handle this locally
        // or ignore all errors as they will still be captured and reported by the Localization system.
        if (stringOperation.Status == AsyncOperationStatus.Failed)
            Debug.LogError("Failed to load string");
        else
            GameManager.Log("Loaded String: " + stringOperation.Result);
        textField.text = stringOperation.Result;
    }


    public void SendPlayerLogin()
    {
        //int level, gold, fame, gems, activities, jobs;
        if (isLogedin) return;
        string url = "https://www.mylittlelifesim.com/game/playerloginmerge2.php?playername="
                + (String.IsNullOrEmpty(PlayerData.playerName) ? "SinNombre" : PlayerData.playerName)
                + "&playerid=" + (String.IsNullOrEmpty(PlayerData.playerID) ? "-1" : PlayerData.playerID)
                + "&charindex=" + PlayerData.characterIndex;
#if UNITY_EDITOR
        Debug.Log("Sending Player Login " + url);
#endif
        startTime2 = Time.time;
        //GameManager.Instance.server.PlayerLogin(url);
        StartCoroutine(SendRequestLogin(url));
    }

    IEnumerator SendRequestLogin(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        resultLogin = JSON.Parse(request.downloadHandler.text) as JSONArray;
        GameManager.Log("AfterLogin " + (Time.time - startTime2) + " url:" + url + " Result: " + request.downloadHandler.text);
        isLogedin = true;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        TrackingManager.TrackPurchase(product.definition.id, "ProcessPurchase");
        StartCoroutine(GiveReward(product));
        return PurchaseProcessingResult.Complete;
    }

    IEnumerator GiveReward(Product product)
    {
        while (!Globals.Instance.gameLoaded || GameManager.Instance == null)
        {
            yield return null;
        }
        GameManager.Instance.OnPurchase(product);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        if (!Globals.Instance.gameLoaded || GameManager.Instance == null) return;
        GameManager.Instance.PopupsManager.ShowPopupYesNo("Something went wrong","Your purchase failed\nPlease contact support", PopupManager.PopupType.ok);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        throw new NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new NotImplementedException();
    }
}

[System.Serializable]
public class BuildingsConfigOLD
{
    public List<BuildingConfig> data;
    

    public BuildingsConfigOLD(JSONArray json)
    {
        data = new List<BuildingConfig>();

        for (var i = 0; i < json.Count; i++)
        {
            BuildingConfig item = new BuildingConfig();
            item.buildingID = json[i].AsObject["buildingID"];
            item.stringKey = json[i].AsObject["stringKey"];
            item.currency = json[i].AsObject["currency"];
            item.price = json[i].AsObject["price"];
            item.profit = json[i].AsObject["profit"];
            item.ownerID = json[i].AsObject["owner"];
            data.Add(item);
        }
    }

    public string GetBuildingLocKey(BuildingType buildingType)
    {
        return data.Find((b) => b.buildingID == (int)buildingType)?.stringKey;
    }
    
}



public class BuildingConfig
{
    public int buildingID; // is buildingType
    public string stringKey;
    public int currency;
    public int price;
    public int profit;
    public int ownerID;
    public LocalizedString localizedStringTitle;
    public LocalizedString localizedStringDescrip;
    public int maxProfitFactor = 10;
    public int Cap => profit * maxProfitFactor;
    public RewardData BuildingPrice => new RewardData((RewardType)currency, price);
    private BuildingType buildingType => (BuildingType)buildingID;
    string _buildingNameTranslated;
    public string BuildingNameTranslated { get => _buildingNameTranslated; set => _buildingNameTranslated = value; }
    public int UnlockLevel => GameManager.Instance.mapManager.GetBuildingDataFromType(buildingType).unlockLevel;
    public int GetCurrentProfit()
    {
        var building = GameManager.Instance.playerData.GetBuildingOwned(buildingType);
        var time = building.LastBuildingClaim;
        var currentProfit = (building.level == 1? profit:profit* building.level*0.75f) * time.TotalSeconds / 3600;
        var cap = building.level == 1 ? Cap : Cap * building.level * 0.75f;
        return Mathf.Min((int)currentProfit, (int)cap);
    }
}
