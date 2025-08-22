using DG.Tweening;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance =>_instance;
    static PlayerData _instance;

    public Action OnLevelUp;
    public List<Sprite> charIcons;
    public TextMeshProUGUI textPlayerLevel;
    public TextMeshProUGUI textPlayerName;
    public Image iconCharacter;
    public Image progressBar;
    public List<string> friends = new List<string>(); 
    public List<VehicleInServer> vehiclesOwned;
    public List<BuildingOwned> buildingsOwned;
    public Action OnPlayerLocationChange;
    public int seasonFame = 0;

    private static int _famepoints;
    private static int _coins;
    private static int _gems;
    private static int _xp;
    private static int _location;
    private static int _vehicleSelected;

    private int _playerID;
    private int _nextLevelXP => GameConfigMerge.instance.NextLevelXP(_level);
    private int _level;
    private int _playerLocation;
    private bool vehiclesLoaded = false;
    private bool vehiclesChanged = false;

    public static BuildingType playerLocation
    {
        get => (BuildingType)_location; 
        
        set {
            _location = (int)value;
            GameManager.Instance.playerData.OnPlayerLocationChange?.Invoke();
            var data = new Dictionary<string, object> { { "location", (int)value } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
    }
    public static string playerName {
        get => PlayerPrefs.GetString("playerName"); 
        set {
            AuthenticationService.Instance.UpdatePlayerNameAsync(value);
            PlayerPrefs.SetString("playerName", value);
        } 
    }
    public static int characterIndex {
        get => PlayerPrefs.GetInt("playerCharacterIndex");
        set { PlayerPrefs.SetInt("playerCharacterIndex", value);
            if(GameManager.Instance != null) GameManager.Instance.LeaderboardManager.SendScore(0);
        }
        
    }
    public static string playerID {
        get => PlayerPrefs.GetString("playerID"); 
        set { PlayerPrefs.SetString("playerID", value); }
    }
    public static int MissionsCount {
        get => PlayerPrefs.GetInt("missionsCount"); 
        set => PlayerPrefs.SetInt("missionsCount", value);
    }
    public static int MapMissionsCount {
        get => PlayerPrefs.GetInt("mapmissionsCount"); 
        set => PlayerPrefs.SetInt("mapmissionsCount", value);
    }
    public static int mergeCount {
        get => PlayerPrefs.GetInt("mergeCount"); 
        set => PlayerPrefs.SetInt("mergeCount", value);
    }
    public static int vehicleSelected {
        get => _vehicleSelected;
        set
        {
            _vehicleSelected = value;
            var data = new Dictionary<string, object> { { "vehicle", value } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
    }
    
    public static int travelCount {
        get => PlayerPrefs.GetInt("travelCount"); 
        set => PlayerPrefs.SetInt("travelCount", value);
    }
    public static int giftsSent {
        get => PlayerPrefs.GetInt("giftsSent"); 
        set => PlayerPrefs.SetInt("giftsSent", value);
    }
    public static int itemsStolen {
        get => PlayerPrefs.GetInt("itemsStolen"); 
        set => PlayerPrefs.SetInt("itemsStolen", value);
    }
    public static int storageSlots {
        get => PlayerPrefs.GetInt("storageSlots"); 
        set => PlayerPrefs.SetInt("storageSlots", value);
    }
    public static int discoveries {
        get => PlayerPrefs.GetInt("discoveries"); 
        set => PlayerPrefs.SetInt("discoveries", value);
    }
    public int level { get { return _level; }
        set {
            _level = value;
            textPlayerLevel.text = _level.ToString();
            if (_level > 1)
            {
                OnLevelUp?.Invoke();
            }
            else
            {
                progressBar.transform.DOScaleX(0, 0);
            }
        }
    }
    public int nextLevelXP => _nextLevelXP; 
    public bool IsTravelling{
        get { return PlayerPrefs.GetInt("playerIsTravelling") == 1 && travellingTimeLeft.TotalSeconds > 0;}
        set { PlayerPrefs.SetInt("playerIsTravelling", value?1:0); }
    }
    public BuildingType TravellingDestination{
        get { return (BuildingType)PlayerPrefs.GetInt("TravellingDestination"); }
        set { PlayerPrefs.SetInt("TravellingDestination", (int)value); }
    }
    public static int TravellingVehicle{
        get { return PlayerPrefs.GetInt("TravellingVehicle"); }
        set { PlayerPrefs.SetInt("TravellingVehicle", value); }
    }
    public TimeSpan travellingTimeLeft{
        get{
            if (PlayerPrefs.HasKey("TravellingEndTime"))
            {
                var timeleft = UIUtils.GetTimeStampByKey("TravellingEndTime") - DateTime.Now;

                return timeleft;
            } else
                return new TimeSpan();
        }
    }

    public static int gems { get { return _gems; }
        set {
            _gems = value;
            var data = new Dictionary<string, object> { { "gems", value } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        } }
    public static int coins { get { return _coins; }
        set {
            _coins = value;
            var data = new Dictionary<string, object> { { "coins", value } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
        } }
    public static int xp { get { return _xp; }
        set {
            _xp = value;
            var data = new Dictionary<string, object> { { "xp", value } };
            CloudSaveService.Instance.Data.Player.SaveAsync(data);
            if(Instance != null) Instance.CheckPlayerLevel();
        } }


public static int famePoints
    {
        get { return _famepoints; }
        set
        {
            if (String.IsNullOrEmpty(playerName) || playerName == "SinNombre" || playerName == " "
                && !GameManager.Instance.tutorialManager.IsTutorialRunning)
            {
                GameManager.Instance.tutorialManager.StartTutorialChangeName();
            }
            _famepoints = value;
        }
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("playerLevel"))
        {
            level = PlayerPrefs.GetInt("playerLevel");
        }
        else
        {
            level = 1;
        }
        _instance = this;
    }
    public void CheckPlayerLevel()
    {
        if(_xp >= _nextLevelXP && _nextLevelXP >= GameConfigMerge.instance.Level1XP)
        {
            _xp -= _nextLevelXP;
            level++;
            SaveData();
        }
        progressBar.transform.DOScaleX((float)_xp / _nextLevelXP,0.5f);
    }

    public void ADMIN_LevelUP()
    {
        xp += _nextLevelXP;
    }

    public void StartTravelling(BuildingType destination, int durationSecs)
    {
        var currentBuilding = GameManager.Instance.mapManager.GetBuildingInteractiveFromType(playerLocation);
        currentBuilding.RemovePlayerVehicle();
        TravellingDestination = destination;
        TravellingVehicle = HasAvailableVehicle?GetBestVehicle().id:0;
        UIUtils.SaveTimeStamp("TravellingEndTime", DateTime.Now.AddSeconds(durationSecs));
        IsTravelling = true;
    }
    

    public void SaveData()
    {
        PlayerPrefs.SetInt("famepoints", famePoints);
        //PlayerPrefs.SetInt("nextLevelXP", _nextLevelXP);
        PlayerPrefs.SetInt("playerLevel", level);
    }

    public async void SaveVehicles()
    {
        GameManager.Log("Save Vehicles Sent");
        if ((vehiclesOwned == null || vehiclesOwned.Count == 0) && !vehiclesChanged) return;
        
        var data = new Dictionary<string, object> { 
            { "vehicles", vehiclesOwned },
            { "location", playerLocation},
            { "vehicle", vehicleSelected }
        };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        vehiclesLoaded = false;
    }

    private IEnumerator LoadVehicles1()
    {
        while (!Globals.isSignedIn) yield return null;
        LoadVehicles2();
    }
    private async void LoadVehicles2()
    {
        var keys = new HashSet<string>();
        keys.Add("vehicles");
        Dictionary<string, Item> savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
        if (savedData.ContainsKey("vehicles"))
        {
            vehiclesOwned = JsonConvert.DeserializeObject<List<VehicleInServer>>(savedData["vehicles"].Value.GetAsString());
            vehiclesLoaded = true;
        }
        else
        {
            vehiclesLoaded = false;
        }

        vehiclesOwned ??= new List<VehicleInServer>();
        GameManager.Instance.mapManager.AddPlayerVehicle();
    }


    public void SaveBuildings()
    {
        PlayerPrefs.SetString("buildingsOwned", JsonConvert.SerializeObject(buildingsOwned));
    }

    public async void LoadData()
    {


        //_nextLevelXP = PlayerPrefs.GetInt("nextLevelXP");
        buildingsOwned = new List<BuildingOwned>();
        if (PlayerPrefs.GetString("friends") != String.Empty)
        {
            friends = JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString("friends"));
        }
        if (PlayerPrefs.GetString("buildingsOwned") != String.Empty)
        {
            buildingsOwned = JsonConvert.DeserializeObject<List<BuildingOwned>>(PlayerPrefs.GetString("buildingsOwned"));
            if (buildingsOwned == null)
                buildingsOwned = new List<BuildingOwned>();
            else {
                //fix level for new feature upgrade
                foreach (var b in buildingsOwned)
                {
                    if(b.level == 0) b.level = 1;
                }
            }
        }
        //Check if it is first time
        if (level == 0) level = 1;

        StartCoroutine(LoadVehicles1());
        await Globals.IsSignedIn();
        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { "coins", "gems", "xp", "location", "vehicle" }
        );

        //Check Name is the same as Unity Auth
        if (playerName == "" && AuthenticationService.Instance.PlayerName == null) 
        {
            Debug.Log("No player names found");
        }
        else if (playerName != UIUtils.GetCleanName(AuthenticationService.Instance.PlayerName))
        {
            Debug.Log("Changing remote Player name.. local: " + playerName + ", remote: " + UIUtils.GetCleanName(AuthenticationService.Instance.PlayerName));
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        }
        else
        {
            Debug.Log("Player name matches remote");
        }
        textPlayerName.text = playerName;

        foreach (var result in results)
        {
            if (result.Key == "coins") _coins = result.Value.Value.GetAs<int>();
            else if (result.Key == "gems") _gems = result.Value.Value.GetAs<int>();
            else if (result.Key == "vehicle")
            {
                _vehicleSelected = result.Value.Value.GetAs<int>();
                GameManager.Instance.mapManager.AddPlayerVehicle();
            }
            else if (result.Key == "location")
            {
                _location = result.Value.Value.GetAs<int>();
                GameManager.Instance.playerData.OnPlayerLocationChange?.Invoke();
                GameManager.Instance.mapManager.FocusOnPlayerLocation();

            }
            else if (result.Key == "xp")
            {
                _xp = result.Value.Value.GetAs<int>();
                if (Instance != null) Instance.CheckPlayerLevel();
            }
        }
        Globals.Instance.gameLoaded = true;
        Globals.playerDataLoaded = true;
    }

    public void AddReward(RewardData reward)
    {
        if (reward.rewardType == RewardType.FamePoints)
        {
            famePoints += reward.amount;
            seasonFame += reward.amount;
        }
        else if (reward.rewardType == RewardType.XP)
        {
            xp += reward.amount;
        }
        else if (reward.rewardType == RewardType.Energy)
        {
            GameManager.Instance.MergeManager.boardModel.energy += reward.amount;
        }
        MyAnalytics.LogEventCurrency(reward.rewardType, reward.amount);
        SaveData();
    }

    public bool CheckJustArrived()
    {
        //If Arrived update location
        if (PlayerPrefs.GetInt("playerIsTravelling") == 1
            && travellingTimeLeft.TotalSeconds < 0
            && playerLocation != TravellingDestination)
        {
            if (TravellingVehicle > 0)
            {
                var v = vehiclesOwned.First(a => a.id == TravellingVehicle);
                if (v != null)
                {
                    v.currentDurability--;
                    Debug.Log("Durability--");
                    if (v.currentDurability == 0)
                    {
                        GameManager.Instance.playerData.RemoveVehicle(v);
                        GameManager.Instance.PopupsManager.ShowPopupYesNo("Vehicle Bronken",
                            "Your vehicle <color=yellow>" + GameConfigMerge.instance.GetVehicleById(v.id).vehicleName + "</color> has broken", PopupManager.PopupType.ok,
                            () => {
                                GameManager.Instance.TryToCreateCarOffer();
                            });
                    }
                }
                else
                {
                    vehicleSelected = 0;
                }
                TravellingVehicle = 0;
            }
            
            IsTravelling = false;
            playerLocation = TravellingDestination;
            //TrackingManager.AddTracking(TrackingManager.Track.TravelComplete, "Destination", playerLocation.ToString());
            TravellingDestination = BuildingType.None;
            GameManager.Instance.dailyTaskManager.OnTravel();
            GameManager.Instance.server.CheckPendingGifts();
            GameManager.Instance.server.CheckPendingSteal();
            if (!vehiclesLoaded && !vehiclesChanged && level > 2) return true;
            SaveVehicles();
            return true;
        }
        return false;
    }

    public void CancelTravelling()
    {
        IsTravelling = false;
        TravellingDestination = BuildingType.None;
        TravellingVehicle = 0;
        FinishNowTravelling();
    }
    public void FinishNowTravelling(bool savevehicles = false)
    {
        UIUtils.SaveTimeStamp("TravellingEndTime", DateTime.Now);
        if(savevehicles) SaveVehicles();
    }

    public SO_Vehicle GetBestVehicle()
    {
        if (vehicleSelected == 0) return null;
        return GameConfigMerge.instance.vehiclesDefinition.Find(veh => veh.id == vehicleSelected);

        //OLD SYSTEM
        /*int mod = 0;
        SO_Vehicle v = null;
        for (var i = 0; i < vehiclesOwned.Count; i++)
        {
            var ve = gameConfig.vehiclesDefinition.Find(veh => veh.id == vehiclesOwned[i].id);
            if (ve.reduceTimePercent > mod && vehiclesOwned[i].currentDurability > 0)
            {
                mod = ve.reduceTimePercent;
                v = ve;
            }
        }
        return v;*/
    }
    public string GetBestVehicleID()
    {
        int mod = 0;
        SO_Vehicle v = null;
        for (var i = 0; i < vehiclesOwned.Count; i++)
        {
            var ve = GameConfigMerge.instance.vehiclesDefinition.Find(veh => veh.id == vehiclesOwned[i].id);
            if (ve.reduceTimePercent > mod && vehiclesOwned[i].currentDurability > 0)
            {
                mod = ve.reduceTimePercent;
                v = ve;
            }
        }
        if (v == null) return "0";
        return v.id.ToString();
    }

    public bool HasAvailableVehicle => GetBestVehicle() != null;

    [Button]
    public void AddVehicleByID(int id, bool showPopup = true) // From Roullete or RewardData
    {
        var vDef = GameConfigMerge.instance.GetVehicleById(id);
        var isAdded = AddVehicle2(vDef);
        if(showPopup) GameManager.Instance.PopupsManager.ShowPopupPurchaseVehicle(vDef, isAdded);
    }
    public void AddVehicle(SO_Vehicle vDef)
    {
        var isAdded = AddVehicle2(vDef);
        GameManager.Instance.PopupsManager.ShowPopupPurchaseVehicle(vDef,isAdded);
    }

    private bool AddVehicle2(SO_Vehicle vDef)
    {
        if (HasVehicle(vDef.id))
        {
            vehiclesOwned.Find(v => v.id == vDef.id).currentDurability = vDef.durability;
            return false;
        }
        var v = new VehicleInServer(vDef);
        if (vehiclesOwned == null || vehiclesOwned.Count == 0 || vehicleSelected == 0) vehicleSelected = v.id;
        vehiclesOwned.Add(v);
        vehiclesChanged = true;
        SaveVehicles();
        return true;
    }
    public void ChangeVechicleMaterial(int vID, int material)
    {
        var v = vehiclesOwned.Find(v => v.id == vID);
        if (v!=null)
        {
            v.mat = material;
            vehiclesChanged = true;
            SaveVehicles();
        }
    }
    public void RemoveVehicle(VehicleInServer v)
    {
        if (vehicleSelected == v.id) vehicleSelected = 0;
        vehiclesOwned.Remove(v);
        vehiclesChanged = true;
        //SaveVehicles();
    }

    private bool HasVehicle(int id)
    {
        if(vehiclesOwned.Find(v=> v.id == id) != null)
        {
            return true;
        }
        return false;
    }

    public Material GetVehicleMaterial(int id)
    {
        var vDef = GameConfigMerge.instance.vehiclesDefinition.Find(veh => veh.id == id);
        Material mat = null;
        var vOwned = GameManager.Instance.playerData.vehiclesOwned.Find(v => v.id == vDef.id);
        var currentMatIndex = vOwned != null ? vOwned.mat : 0;
        //Get material
        if (vDef.materials != null && vDef.materials.Count > 0)
            mat = vDef.materials[currentMatIndex].material;

        return mat;
    }
    public int GetVehicleMaterialIndex(int id)
    {
        var vDef = GameConfigMerge.instance.vehiclesDefinition.Find(veh => veh.id == id);
        Material mat = null;
        var vOwned = GameManager.Instance.playerData.vehiclesOwned.Find(v => v.id == vDef.id);
        return  vOwned != null ? vOwned.mat : 0;
    }


    public bool HasBuilding(BuildingType b)
    {
        return buildingsOwned.Find(x => x.buildingType == b)!=null;
    }
    public BuildingOwned GetBuildingOwned(BuildingType b)
    {
        return buildingsOwned.Find(x => x.buildingType == b);
    }

    public void AddBuilding(BuildingType b)
    {
        var building = new BuildingOwned(b);
        building.SetLastBuildingClaim();
        TrackingManager.AddTracking(TrackingManager.Track.BuyBuilding, b.ToString(), true);
        buildingsOwned.Add(building);
        SaveBuildings();
        var txt = GameServerConfig.Instance.GetBuildingConfig(building.buildingType).BuildingNameTranslated;
        GameManager.Instance.PopupsManager.ShowPopupPurchaseVehicleBuilding(txt);
    }
    public void RemoveBuilding(BuildingType b)
    {
        var building = new BuildingOwned(b);
        buildingsOwned.Remove(building);
        SaveBuildings();
    }
    public void UpgradeBuilding(BuildingType b)
    {
        var building = buildingsOwned.Find(bu => bu.buildingType == b);
        if (building == null) return;
        building.level++;
        SaveBuildings();
    }

    public void LazyUpdate()
    {

    }

}
