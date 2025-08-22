using Assets.Scripts.MergeBoard;
using DG.Tweening;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Events;
using System.Threading.Tasks;
using NotificationSamples;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => instance;
    private static GameManager instance;

    public Action<bool> OnSelectShop;

    public GameObject player;
    public TopBar topBar;
    public LowerBar lowerBar;
    public ShopManager shopManager;
    public MergeLowerBar mergeLowerBar;
    public GameObject lowerBarShop;
    public SideBarController sideBar;
    public TextMeshProUGUI txtFPS;
    public Image canvasBackground;
    public Image speedBoardBackground;
    public GameObject ShopScreen;
    public GameObject SkillsScreen;
    public GameObject CarShopScreen;
    public GameObject mergeMapMissionsScreen;
    public GameObject dailyTasksScreen;
    public PopupCasinoRoulette rouletteScreen;
    public GameObject mergeStorageScreen;
    public GameObject giftsStealScreen;
    public GameObject levelUPPopup;
    public PopupLeagueReward leagueRewardPopup;
    public GameObject AdminPanel;
    public GameObject TravellingContainer;
    public ButtonMergeStore btnStorage;
    public PlayerSelectionManager charactersList;
    public MapManager mapManager;
    public TutorialManager tutorialManager;
    public GameNotificationsManager gameNotificationsManager;
    public AudioSource soundNormalButton;
    public AudioSource soundSpeedUplButton;
    public AudioSource soundPurchaseButton;
    public AudioSource soundCoins;
    public AudioSource soundGems;
    public AudioSource soundStorageBounce;
    public AudioSource sfxClaimEnergy;
    public AudioSource sfxClaimXP;
    public AudioSource Music;
    public AudioSource MusicBooster;
    public Server server;
    public GameConfigMerge gameConfig;
    public Garaje3D garaje3D;
    public Garaje3D garaje3D_Canvas;
    public GameServerConfig gameServerConfigPrefab;
    public Transform mainCanvasTransform;
    public SeasonEndController seasonEndController;
    public SpeedBoardManager speedBoardManager;
    public CanvasGroup loadingScreen;

    public int energy { set { _mergeManager.boardModel.energy = value; 
            topBar.UpdatePlayerEconomy(); } get { return _mergeManager.boardModel.energy; } }
    public MergeBoardModel mergeModel => _mergeManager.boardModel;
    public DailyTaskManager dailyTaskManager => _dailyTaskManager;
    [HideInInspector]
    public PlayerData playerData => _playerData??= GetComponent<PlayerData>();
    PlayerData _playerData;
    
    VehiclesManager _vehiclesManager;
    MergeStorageController _storageController;
    GiftsStealBase _giftStealController;
    MergeMapMissionsController _mergeMissionsController;
    SkillsManager _careersManager;
    PopupManager _popupManager;
    DailyBonusManager _dailyBonusManager;
    MergeBoardManager _mergeManager;
    DailyTaskManager _dailyTaskManager = new DailyTaskManager();
    DailyTaskScreen _dailyTaskScreen;
    VideoAdsManager _videoAdsManager;
    AssistantsManager _assistantsManager;
    float _bgMusicVolume;

    const string HouseScene = "HouseScene2";
    public LeaderboardManager LeaderboardManager{ get; internal set; }

    public VehiclesManager VehiclesManager =>_vehiclesManager ??= CarShopScreen.GetComponent<VehiclesManager>();
    public SkillsManager skillsManager =>_careersManager ??= SkillsScreen.GetComponent<SkillsManager>();
    public MergeStorageController StorageController => _storageController ??= mergeStorageScreen.GetComponent<MergeStorageController>();
    public GiftsStealBase GiftsStealController => _giftStealController ??= giftsStealScreen.GetComponent<GiftsStealBase>();
    public MergeMapMissionsController mergeMissionsController => _mergeMissionsController ??= mergeMapMissionsScreen.GetComponent<MergeMapMissionsController>();
    public PopupManager PopupsManager => _popupManager ??= GetComponent<PopupManager>();
    public AssistantsManager AssistantsManager => _assistantsManager ??= GetComponent<AssistantsManager>();
    public DailyBonusManager DailyBonusManager => _dailyBonusManager ??= GetComponent<DailyBonusManager>();
    public MergeBoardManager MergeManager => _mergeManager ??= GetComponent<MergeBoardManager>();
    public DailyTaskScreen dailyTaskScreen => _dailyTaskScreen ??= dailyTasksScreen.GetComponent<DailyTaskScreen>();
    public VideoAdsManager videoAdsManager => _videoAdsManager ??= GetComponent<VideoAdsManager>();
    public bool IsShowingShop => ScreenManager.Instance.currentScreen == ShopScreen;
    public ButtonMergeStore BtnStorage
    {
        get
        {
            if (mergeLowerBar.gameObject.activeSelf)
            {
                return mergeLowerBar.btnsStorage[0];
            }
            return btnStorage;
        }
    }

    int tutoStorage
    {
        get => PlayerPrefs.HasKey("tutoStorageTimes")? PlayerPrefs.GetInt("tutoStorageTimes"): 0;
        set { PlayerPrefs.SetInt("tutoStorageTimes", value); }
    }
    Vector3 topBarInitPos;
    Vector3 lowerBarInitPos;
    
    public static DateTime epochStart;
    private bool _allTutorialsCompleted;
    private bool _appIsFocused = false;
    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
        DOTween.Init();
        Physics.simulationMode = SimulationMode.Script;
        //Graphis Setup
        //Camera.main.GetComponent<PostProcessLayer>().enabled = PlayerPrefs.GetInt("GraphicsQuality")==1;
        if (PlayerPrefs.GetInt("MusicSetting") == 0) Music.Play();
        if (PlayerPrefs.GetInt("SoundSetting") == 1) AudioListener.volume = 0;
        _bgMusicVolume = Music.volume;
        Initialize();
        Application.targetFrameRate = 60;
    }
    
    private async void Initialize()
    {
        Globals.Instance.OnInitialize();
        await Globals.CheckUnityService();
        if (GameServerConfig.Instance == null || GameServerConfig.Instance.gameObject == null)
        {
            Globals.isSigning = true;
            //Debug.Log("Creating GameServerConfig instance");
            var serverConfig = Instantiate(gameServerConfigPrefab, transform);
            StartCoroutine(SendInitialRequests());
            return;
        }
        if (!Globals.isSigning && !AuthenticationService.Instance.IsSignedIn || AuthenticationService.Instance.IsExpired)
        {
            Debug.Log("SignInAnonymouslyAsync in GameManager");
            Globals.isSigning = true;
            await AuthenticationService.Instance.SignInAnonymouslyAsync().ContinueWith((t) => { 
                Globals.Instance.gameLoaded = true;
                Globals.isSigning = false;
                StartCoroutine(SendInitialRequests());
            }); ;
        }
    }

    public void Admin_UpdateName()
    {
        AuthenticationService.Instance.UpdatePlayerNameAsync(PlayerData.playerName);
    }

    public void ADMIN_ForceTokenLost()
    {
        //AuthenticationService.Instance.ClearSessionToken();
        AdminCheckToken();
    }

    private async void AdminCheckToken()
    {

            Globals.Instance.gameLoaded = false;
            Globals.isSigning = true;
            loadingScreen.DOFade(0, 0);
            loadingScreen.DOFade(1, 0.1f).OnStart(()=>loadingScreen.gameObject.SetActive(true));
            
            //ReloadGame();
            await Task.Delay(2000).ContinueWith((t) => {
                Globals.Instance.gameLoaded = true;
                Globals.isSigning = false;
                loadingScreen.DOKill();
                loadingScreen.DOFade(0, 0.2f).OnComplete(() => loadingScreen.gameObject.SetActive(false));
            });    
        
    }
    private async void CheckToken()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            TrackingManager.TrackTokenLost(UnityServices.State.ToString());
            return;
        }

        if (AuthenticationService.Instance.IsExpired)
        {
            TrackingManager.TrackTokenLost("TokenExpired");
            Globals.Instance.gameLoaded = false;
            Globals.isSigning = true;
            loadingScreen.DOFade(0, 0);
            loadingScreen.DOFade(1, 0.1f).OnStart(() => loadingScreen.gameObject.SetActive(true));

            //ReloadGame();
            await AuthenticationService.Instance.SignInAnonymouslyAsync().ContinueWith((t) =>
            {
                Globals.Instance.gameLoaded = true;
                Globals.isSigning = false;
                loadingScreen.DOKill();
                loadingScreen.DOFade(0, 0.2f).OnComplete(() => loadingScreen.gameObject.SetActive(false));
            });
        }
        else
        {
            loadingScreen.DOKill();
            loadingScreen.DOFade(0, 0.2f).OnComplete(() => loadingScreen.gameObject.SetActive(false));
        }
    }

    IEnumerator SendInitialRequests()
    {
        while (!Globals.Instance.gameLoaded ||
            tutorialManager.IsTutorialRunning)// && tutorialManager.ActiveTutorial().TutorialKey == TutorialKeys.HouseWelcome)
            yield return null;

        Log("Send Initial Request Player Login, player level is " + PlayerLevel);
        GameServerConfig.Instance.SendPlayerLogin();
    }

    void Start()
    {
        epochStart = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        playerData.LoadData();
        playerData.OnLevelUp += ShowLevelUP;
        StartCoroutine(server.AfterLogin());
        LoadPlayerPrefab();

        if (topBar)             topBarInitPos = topBar.transform.position;
        if (mergeLowerBar)      ShowMergeLowerBar(false);
        if (SkillsScreen)       ShowSkills(false);


        levelUPPopup.SetActive(false);
        if (MergeManager) 
        {
            StartCoroutine(MergeManager.Init());
            MergeManager.ShowMergeBoard(false, null); 
        }
        /*if (mapManager == null || !playerData.IsTravelling)
        {
            if(TravellingContainer != null)
                TravellingContainer.GetComponent<TravellingController>().Hide();
        }*/
        if (mapManager != null)
        {
            mapManager.Init();
        }
        if (TravellingContainer != null)
        {
            //TravellingContainer.SetActive(true);
            TravellingContainer.GetComponent<TravellingController>().Init();
            if (playerData.IsTravelling)
            {
                ShowTravellingUI();
            }
        }
        _allTutorialsCompleted = PlayerPrefs.GetInt("Tutorial" + TutorialKeys.MapAfterLevelUp) == 1;
        if (MyScenesManager.Instance)UIUtils.DelayedCall(0.1f,MyScenesManager.Instance.HideScreen,this);
        speedBoardBackground.DOFade(0, 0f);
    }

    public void PlayerSignedIn()
    {
        UpdateMapMissions();
    }


    public TimeSpan activityTimeLeft { get {
            if (PlayerPrefs.HasKey("ActivityEndTime")) 
                return UIUtils.GetTimeStampByKey("ActivityEndTime") - DateTime.Now;
            else
                return new TimeSpan();
        } }

    float fps = 30f;
    int currentSeconds = 0;
    bool superLazy = false;
    // Update is called once per frame
    async void Update()
    {
        if (canvasBackground != null)
        {
            canvasBackground.gameObject.SetActive(canvasBackground.color.a > 0);
        }
        if (speedBoardBackground != null)
        {
            speedBoardBackground.gameObject.SetActive(speedBoardBackground.color.a > 0);
        }
        /*
        float newFPS = Mathf.Clamp(1.0f / Time.smoothDeltaTime,0,120);
        Debug.Log(newFPS+ "/"+ fps);
        fps = Mathf.Lerp(fps, newFPS, 0.005f);
        txtFPS.text = "FPS: " + (int)fps;
        */
        //Profiler.BeginSample("TestGM_UpdateCheckTutorials");
        if (!_allTutorialsCompleted && tutorialManager != null && Globals.Instance.boardsLoaded && Globals.configLoaded)
        {
            tutorialManager.CheckTutorials();
            if (sideBar.gameObject.activeSelf) sideBar.SuperLazyUpdate();
        }
        //Profiler.EndSample();

        if (currentSeconds % 5 == 0 && !superLazy)
        {
            superLazy = true;
            SuperLazyUpdate();
            currentSeconds = 0;
        }
        else if (currentSeconds != DateTime.Now.Second)
        {
            currentSeconds = DateTime.Now.Second;
            LazyUpdate();
        }
        else if (currentSeconds % 5 == 1) superLazy = false;
    }

    private void LazyUpdate()
    {
        //
        playerData.LazyUpdate();
        //tutorialManager.LazyUpdate();
        if (MergeManager!=null && _mergeManager.IsMergeBoardActive) MergeManager.LazyUpdate();
        if (PlayerLevel < 2) return;
        shopManager.LazyUpdate();
        
        if (mapManager!=null) mapManager.LazyUpdate();
        if (sideBar.gameObject.activeSelf) sideBar.LazyUpdate();
        if (mergeMapMissionsScreen.activeSelf) mergeMissionsController.LazyUpdate();
        if (dailyTasksScreen.activeSelf) dailyTaskScreen.LazyUpdate();
        //Check Limited mission expired
        if (mergeModel.limitedMission != null && mergeModel.limitedMission.endTime < DateTime.Now)
        {
            mergeModel.limitedMission = null;
            UpdateMapMissions();
        }
        if (!_dailyTaskManager.dataLoaded && !_dailyTaskManager.dataLoading
            && Globals.isSignedIn && PlayerLevel > 2
            && Globals.Instance.boardsLoaded)
            _dailyTaskManager.LoadData();
        if (_dailyTaskManager.dataLoaded && !_dailyTaskManager.initialized) _dailyTaskManager.Init();
        CheckSeasonEnd();

    }

    private void SuperLazyUpdate()
    {
        if (sideBar.gameObject.activeSelf) sideBar.SuperLazyUpdate();
        if(_dailyTaskManager.isExpired && Globals.isSignedIn && PlayerLevel > 2 && Globals.Instance.boardsLoaded)
            _dailyTaskManager.LoadData();
        server.CheckPendingGifts();
        server.CheckPendingSteal();
    }



    public void SetMusicBooster(bool value)
    {
        Music.volume = value ? 0 : _bgMusicVolume;
        if (value)
        {
            MusicBooster.Play();
        }
        else
        {
            MusicBooster.Stop();
        }
    }

    public bool IsTravelling => playerData.IsTravelling;
    public BuildingType TravellDestination => playerData.TravellingDestination;
    
    public void LoadPlayerPrefab()
    {
        if(player !=null)
        {
            if(player.transform.GetChild(0) != null)
            {
                Destroy(player.transform.GetChild(0).gameObject);
            }
            if (SceneManager.GetSceneByName(HouseScene).isLoaded)
            {
                var character = Instantiate(charactersList.characters[PlayerData.characterIndex].character, player.transform);
                character.SetActive(true);
            }
        }
    }

    public void ShowBars(bool value)
    {
        ShowTopBar(value);
        ShowMapLowerBar(value);
    }
    public void ShowMapLowerBar(bool value, bool animated = true)
    {
        if (lowerBar) {
            if (value) lowerBar.gameObject.SetActive(true);
            if (lowerBarInitPos == Vector3.zero) lowerBarInitPos = lowerBar.transform.position;
            lowerBar.transform.DOMove(
                value ? lowerBarInitPos : lowerBarInitPos + Vector3.down * 400, 
                animated ? 0.5f: 0f).OnComplete(() => lowerBar.gameObject.SetActive(value));
        }
        if (sideBar)
        {
            sideBar.ShowSidebar(value,animated);
        }
    }
    public void ShowMapBtnStorageOnly(bool value)
    {
        //Maybe is interesting
        if (value) lowerBar.ShowOnlyStorage();
        if (lowerBarInitPos == Vector3.zero) lowerBarInitPos = lowerBar.transform.position;
        lowerBar.transform.DOMove(
            value ? lowerBarInitPos : lowerBarInitPos + Vector3.down * 400,
            0.5f ).OnComplete(() =>
            {
                lowerBar.gameObject.SetActive(value);
                if(!value) lowerBar.ShowNormal();
            });
    }
    public void ShowMergeLowerBar(bool value, bool animated = true)
    {
        if (mergeLowerBar) 
        {
            if (PlayerLevel < 2) { 
                mergeLowerBar.gameObject.SetActive(false);
                return;
            }
            if (value) mergeLowerBar.gameObject.SetActive(true);
            if (lowerBarInitPos == Vector3.zero) lowerBarInitPos = lowerBar.transform.position;
            mergeLowerBar.transform.DOMove(
                value ? lowerBarInitPos : lowerBarInitPos + Vector3.down * 400, 
                animated ? 0.5f: 0f).OnComplete(()=>mergeLowerBar.gameObject.SetActive(value));
        }
    }
    public void ShowTopBar(bool value)
    {
        if (topBar)
        {
            if (topBarInitPos == Vector3.zero) topBarInitPos = topBar.transform.position;
            topBar.transform.DOMove(value ? topBarInitPos : topBarInitPos + Vector3.up * 300, 0.5f); ;
        }
    }

    public void ShowMergeBoard(bool value, BoardConfig boardConfig = null, System.Action OnClose = null, Action OnBoardReady = null)
    {
        ShowMapLowerBar(!value);
        ShowMergeLowerBar(value);
        mergeLowerBar.btnMap.gameObject.SetActive(true);
        //Todo: Set Bubble when mission is ready
        //mergeLowerBar.btnMap.SetBubbleCounter(_mergeManager.boardModel.mapMissions.Count, true);
        MergeManager.ShowMergeBoard(value, boardConfig, OnClose);
        if (value)
        {
            canvasBackground.DOFade(1, 0.3f).OnComplete(() => { mapManager.gameObject.SetActive(false); OnBoardReady?.Invoke(); });
        }
        else
        {
            canvasBackground.DOFade(0, 0.3f).OnStart(() => { mapManager.gameObject.SetActive(true); }).SetDelay(0.2f);
        }
    }

    public void ShowSpeedMergeBoard(bool value, BoardConfig boardConfig = null, System.Action OnClose = null, Action OnBoardReady = null)
    {
        ShowMapLowerBar(!value);
        ShowMergeLowerBar(false);
        mergeLowerBar.btnMap.gameObject.SetActive(true);

        speedBoardManager.ShowSpeedBoard(value, OnClose);
        if (value)
        {
            speedBoardBackground.DOFade(1, 0.3f).OnComplete(() => { mapManager.gameObject.SetActive(false); OnBoardReady?.Invoke(); });
        }
        else
        {
            speedBoardBackground.DOFade(0, 0.3f).OnStart(() => { mapManager.gameObject.SetActive(true); }).SetDelay(0.2f);
        }
    }

    /*public bool IsCareersScreenActive()
    {
        return currentScreen == SkillsScreen;
    }*/
    public void ShowSkills(bool value){
        if (value){
            if (ScreenManager.Instance.AnimateScreenIN(SkillsScreen)) skillsManager.OnShow();
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(SkillsScreen);
    }

    public void ShowShop(bool value){
        if (value){
            if (ScreenManager.Instance.AnimateScreenIN(ShopScreen, videoAdsManager.CheckReady))
            {
                shopManager.OnShow();
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(ShopScreen);
        shopManager.HideBG();
    }

    public void ShowActivity(bool value){
        //Focus on activity building
        mapManager.FocusOnBuilding(PlayerData.playerLocation);
    }

    // --- Load Scenes -----
    public void ShowMap()
    {
        if (PlayerPrefs.GetInt("Tutorial" + TutorialKeys.MapButton) == 0)
        {
            PlayerPrefs.SetInt("Tutorial" + TutorialKeys.MapButton, 1);
        }
        StartCoroutine(LoadScene("MapScene"));
    }
    public void ShowHouseScene()
    {
        if (PlayerPrefs.GetInt("Tutorial" + TutorialKeys.MapEnterAppartment) == 0)
        {
            PlayerPrefs.SetInt("Tutorial" + TutorialKeys.MapEnterAppartment, 1);
        }
        StartCoroutine(LoadScene(HouseScene));
    }
    IEnumerator LoadScene(string sceneName) {
        if (MyScenesManager.Instance != null)
        {
            MyScenesManager.Instance.ShowScreen();
            yield return new WaitForSeconds(0.3f);
        }
        SceneManager.LoadScene(sceneName);
    }


    public void ShowCarShop(bool value)
    {
        ShowCarShop(value, false, null, VehicleType.None);
    }
    public void ShowCarShop(bool value, bool isShop, List<SO_Vehicle> vehicles)
    {
        ShowCarShop(value, isShop, vehicles, vehicles[0].vehicleType);
    }
    public void ShowCarShop(bool value, bool isShop, List<SO_Vehicle> vehicles, VehicleType shopType)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != CarShopScreen)
            {
                ScreenManager.Instance.AnimateScreenIN(CarShopScreen);
                VehiclesManager.OnShow(isShop, vehicles, shopType, mapManager.RefeshVehicle);
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(CarShopScreen);
    }
    
    public void ShowMergeStorage(bool value)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != mergeStorageScreen)
            {
                ScreenManager.Instance.AnimateScreenIN(mergeStorageScreen);
                StorageController.LoadItems(gameConfig.mergeConfig);
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(mergeStorageScreen);
    }
    public void ShowSendGifts(bool value, LeaderboardPlayer playerData)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != giftsStealScreen)
            {
                ScreenManager.Instance.AnimateScreenIN(giftsStealScreen);
                GiftsStealController.LoadItemsFromStorage(gameConfig.mergeConfig,playerData);
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(giftsStealScreen);
    }
    public void ShowStealScreen(bool value, LeaderboardPlayer playerData)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != giftsStealScreen)
            {
                ScreenManager.Instance.AnimateScreenIN(giftsStealScreen);
                GiftsStealController.LoadItemsToSteal(gameConfig.mergeConfig,playerData);
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(giftsStealScreen);
    }
    public void ShowDailyTaskScreen(bool value)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != dailyTasksScreen)
            {
                ScreenManager.Instance.AnimateScreenIN(dailyTasksScreen);
                dailyTaskScreen.Show();
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(dailyTasksScreen);
    }
    public void ShowRouletteScreen(bool value)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != rouletteScreen.gameObject)
            {
                ScreenManager.Instance.AnimateScreenIN(rouletteScreen.gameObject);
                rouletteScreen.Show();
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(rouletteScreen.gameObject);
    }
    public void ShowMergeMapMissions(bool value)
    {
        ShowMergeMapMissions(value, BuildingType.None);
    }
    public void ShowMergeMapMissions(bool value, BuildingType bType)
    {
        if (value)
        {
            if (ScreenManager.Instance.currentScreen != mergeMapMissionsScreen)
            {
                ScreenManager.Instance.AnimateScreenIN(mergeMapMissionsScreen, () => 
                {
                    //First Time Show Popup tuto missions
                    if (tutoStorage < 1 && PlayerLevel < 3)
                    {
                        PopupsManager.ShowPopupTutorial(_mergeMissionsController.popupTutorial);
                        tutoStorage++;
                    }
                });
                mergeMissionsController.LoadMissions(bType);
                ShowMapLowerBar(!value);
                ShowMergeLowerBar(value);
                if (!MergeManager.IsBoardActive)
                {
                    mergeLowerBar.btnMap.gameObject.SetActive(false);
                    //mergeLowerBar.btnMissions.gameObject.SetActive(false);
                }
            }
            return;
        }
        ScreenManager.Instance.AnimateScreenOUT(mergeMapMissionsScreen,true);
    }

    public void AddRewardWithParticles(RewardData reward,Transform fromTransform, Action Callback = null, bool save = false)
    {
        AddRewardToPlayer(reward, true, save);
        var amount = reward.amount;
        if (reward.rewardType == RewardType.Coins) amount = reward.amount / 10;
        else if (reward.rewardType == RewardType.FamePoints && !topBar.FameContainer.activeSelf) topBar.FameContainer.SetActive(true);
        
        amount = Math.Min(amount, 10);
        UIUtils.FlyingParticles(reward.rewardType, fromTransform.position, amount, Callback);
    }
    public void AddRewardToPlayer(RewardData reward,bool withDelay = false, bool saveData = true)
    {
        var amount = reward.amount;
        /*else if (reward.rewardType == RewardType.XP)
        {
            var current = int.Parse(txtXP.text);
            seq.Insert(withDelay ? 1 : 0, DOTween.To(() => current, x => current = x, current + amount, 1).OnUpdate(() => txtXP.text = current.ToString()));
        }*/
        if (reward.rewardType == RewardType.Energy && _mergeManager.IsMergeBoardActive)
        {
            sfxClaimEnergy.Play();
            AnimateText(_mergeManager.boardController.energyController.txtEnergy, saveData?energy:energy-amount, amount, withDelay);
        }
        else if (reward.rewardType == RewardType.MergeItem)
        {
            mergeModel.AddGift(reward.mergePiece, reward.amount);
        }
        if (saveData)
        {
            if (reward.rewardType == RewardType.Coins)
            {
                topBar.AnimateCoins(reward.amount, withDelay);
            }
            else if (reward.rewardType == RewardType.Gems)
            {
                topBar.AnimateGems(reward.amount, withDelay);
            }
            else if (reward.rewardType == RewardType.FamePoints)
            {
                topBar.AnimateFame(reward.amount, withDelay);
            }
            else
            {
                playerData.AddReward(reward);
            }
        }
    }

    public void AnimateText(TextMeshProUGUI textField, int from, int amount, bool withDelay, bool addPlusFirst = false)
    {
        var seq = DOTween.Sequence();
        seq.Insert(withDelay ? 1 : 0, DOTween.To(() => from, x => from = x, from + amount, 1).OnUpdate(() => textField.text = (addPlusFirst?"+":"") + from));
    }
    public static void AnimateFormatedNumber(TextMeshProUGUI textField, int from, int amount, bool withDelay, float duration = 1)
    {
        var seq = DOTween.Sequence();
        seq.Insert(withDelay ? 1 : 0, DOTween.To(() => from, x => from = x, from + amount, duration).OnUpdate(() => textField.text = UIUtils.FormatNumber(from)));
    }

    public int PlayerLevel { get { return playerData.level; }}

    /*public bool PlayerHasSkill(string skill, int level)
    {
        //Debug.Log("Skill level for " + skill + " = " + skillsManager.GetSkillLevel(skill) + ", required " + level);
        return skillsManager.GetSkillLevel(skill) >= level;
    }
    public bool PlayerHasSkill(SkillGroup skillGroup, int level)
    {
        //Debug.Log("Skill level for " + skillGroup + " = " + skillsManager.GetSkillGroupPoints(skillGroup) + ", required " + level);
        return skillsManager.GetSkillGroupPoints(skillGroup) >= level;
    }*/


    public void DelayedCall(float time, Action action)
    {
        StartCoroutine(DelayCall(time, action));
    }

    IEnumerator DelayCall(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }

    public void ShowLevelUP()
    {
        topBar.gameObject.SetActive(true);
        levelUPPopup.SetActive(true);
        var levelUpManager = levelUPPopup.GetComponent<LevelUpManager>();
        levelUpManager.OnLevelUpComplete = CheckBuildingsUnlocked;
        levelUpManager.ShowRewardsForNextLevel(playerData.level);
        var rewardLevel = Math.Min(playerData.level, levelUpManager.RewardsForLevels.Count - 1);
        playerData.AddReward(new RewardData(RewardType.Coins, levelUpManager.RewardsForLevels[rewardLevel].coinsValue));
        playerData.AddReward(new RewardData(RewardType.Gems, levelUpManager.RewardsForLevels[rewardLevel].gemsValue));
        TrackingManager.TrackAndSendLevelUp();
        if(_mergeManager.boardController != null && _mergeManager.IsBoardActive)
        {
            _mergeManager.boardController.SaveState(true);
        }
    }
    public void ShowLeagueRewardPopup()
    {
        topBar.gameObject.SetActive(true);
        levelUPPopup.SetActive(true);
        var levelUpManager = levelUPPopup.GetComponent<LevelUpManager>();
        levelUpManager.OnLevelUpComplete = CheckBuildingsUnlocked;
        levelUpManager.ShowRewardsForNextLevel(playerData.level);
        var rewardLevel = Math.Min(playerData.level, levelUpManager.RewardsForLevels.Count - 1);
        playerData.AddReward(new RewardData(RewardType.Coins, levelUpManager.RewardsForLevels[rewardLevel].coinsValue));
        playerData.AddReward(new RewardData(RewardType.Gems, levelUpManager.RewardsForLevels[rewardLevel].gemsValue));
        //if(skillsManager)skillsManager.availablePoints += levelUpManager.RewardsForLevels[rewardLevel].skillValue;
    }

    public void CheckBuildingsUnlocked()
    {
        var unlockedBuildings = mapManager.GetUnlockedBuildingsAtLevel(playerData.level);
        if (unlockedBuildings.Count == 0) return;
        mapManager.gameObject.SetActive(true);
        ShowMergeBoard(false);
        ShowMergeMapMissions(false);
        Debug.Log("Showing Unlocked Buildings " + unlockedBuildings.Count);
        StartCoroutine(ShowUnlockBuildings(unlockedBuildings));
    }

    IEnumerator ShowUnlockBuildings(List<SO_Building> buildings)
    {
        yield return mapManager.isActiveAndEnabled;
        mapManager.playerInputEnable = false;
        foreach (var b in buildings)
        {
            mapManager.UnlockBuilding(b);
            yield return new WaitForSeconds(4);
        }
        mapManager.playerInputEnable = true;
        mapManager.OnInteractionCancel();
        mapManager.FocusOnPlayerLocation();
        PlayerPrefs.SetInt("buildingUnlocked", 1);
    }

    private Action OnCloseShopCallBack;
    

    public void ShowLowerBarShop(Action OnCloseCallback)
    {
        if (lowerBarShop == null) return;
        lowerBarShop.SetActive(true);
        OnCloseShopCallBack = OnCloseCallback;
    }
    public void HideLowerBarShop()
    {
        lowerBarShop.SetActive(false);
        OnCloseShopCallBack?.Invoke();
    }
    public GameObject GetLowerBarShop()
    {
        return lowerBarShop;
    }

    public static string currentTime
    {
        get
        {
            var currentEpochTime = DateTime.UtcNow.Subtract(epochStart);
            return currentEpochTime.TotalMilliseconds.ToString();
        }
    }
    public static double currentTimenum
    {
        get
        {
            var currentEpochTime = DateTime.UtcNow - epochStart;
            return currentEpochTime.TotalMilliseconds;
        }
    }

    public static int GetTimePrice(double time)
    {
        //return Math.Max(1,(int)Math.Sqrt(time/10));
        return (int)Math.Sqrt(time/10);
    }

    public bool HasEnoughCurrency(int amount, RewardType currency,bool showShop = false)
    {
        bool hasEnough = false;
        if (currency == RewardType.Coins)
        {
            hasEnough = PlayerData.coins >= amount;
        }
        if (currency == RewardType.Gems)
        {
            hasEnough = PlayerData.gems >= amount;
        }
        if (!hasEnough && showShop) ShowShop(true);
        return hasEnough;   
    }
    public bool HasEnoughCurrency(RewardData cost, bool showShop = false)
    {
        return HasEnoughCurrency(cost.amount, cost.rewardType, showShop);
    }

    public static bool TryToSpend(RewardData cost)
    {
        return instance.TryToSpend(cost.amount, cost.rewardType);
    }
    public bool TryToSpend(int amount,RewardType currency)
    {
        if (currency == RewardType.Coins)
        {
            return TryToSpendCoins(amount);
        }
        else if (currency == RewardType.Gems)
        {
            return TryToSpendGems(amount);
        }
        return false;
    }
    private bool TryToSpendCoins(int amount)
    {
        if (PlayerData.coins < amount)
        {
            if (TryToCreateOffer())
                return false;
            ShowShop(true);
            return false;
        }
        topBar.AnimateCoins(-amount);
        //PlayerData.coins -= amount;
        MyAnalytics.LogEventCurrency(RewardType.Coins, -amount);
        return true;
    }
    private bool TryToSpendGems(int amount)
    {
        if (PlayerData.gems < amount)
        {
            if (TryToCreateOffer())
                return false;
            ShowShop(true);
            return false;
        }
        topBar.AnimateGems(-amount);
        //PlayerData.gems -= amount;
        MyAnalytics.LogEventCurrency(RewardType.Gems, -amount);
        return true;
    }
    public bool HasEnoughGems(int amount, bool withWarningPopup = false)
    {
        return PlayerData.gems >= amount;
    }
    public bool HasEnoughCoins(int amount)
    {
        return PlayerData.coins >= amount;
    }
    public bool HasEnoughCurrency(RewardData price)
    {
        if(price.rewardType == RewardType.Gems) return PlayerData.gems >= price.amount;
        if(price.rewardType == RewardType.Coins) return PlayerData.coins >= price.amount;
        return false;
    }

    #region OffersManager
    //Offers Manager
    public bool TryToCreateOffer(float delayToShow = 0.05f)
    {
        if (!PlayerPrefs.HasKey("genericOffer") || 
            (PlayerPrefs.GetInt("genericOffer") == 0 && PlayerPrefs.GetInt("WP_Seen") != 1))
        {
            var lastOfferdays = (DateTime.Now - UIUtils.GetTimeStampByKey("genericOffer_end")).TotalDays;
            if (PlayerPrefs.HasKey("genericOffer") && lastOfferdays < 7) 
                return false;
            Log("Creating OFFER");
            PlayerPrefs.SetInt("genericOffer", 1);
            UIUtils.SaveTimeStamp("genericOffer_end", DateTime.Now.AddHours(gameConfig.specialOfferDuration));
            DOVirtual.DelayedCall(delayToShow, ()=> { PopupsManager.ShowGenericOfferPopup(); });
            return true;
        }
        return false;
    }
    public bool TryToCreateCarOffer(float delayToShow = 0.05f)
    {
        var carOfferIndex = -1;
        if (PlayerPrefs.HasKey("carOffersShown"))
        {
            carOfferIndex = PlayerPrefs.GetInt("carOffersShown");
        }

        if (carOfferIndex != (int)DateTime.Now.DayOfWeek)
        {
            carOfferIndex = (int)DateTime.Now.DayOfWeek;
            PlayerPrefs.SetInt("carOffersShown", carOfferIndex);
            //DOVirtual.DelayedCall(delayToShow, () => { PopupsManager.ShowCarOffer(gameConfig.carOffers[carOfferIndex]); });
            //TEST
            DOVirtual.DelayedCall(delayToShow, () => { PopupsManager.ShowCarOffer(gameConfig.carOffers[UnityEngine.Random.Range(0, gameConfig.carOffers.Count)]); });
            return true;
        }
        //Log("COULD NOT CREATE A CAR OFFER");
        return false;
    }
    #endregion

    public void StartTravelling(BuildingType destination)
    {
        playerData.StartTravelling(destination, PlayerLevel < 2?3:mapManager.GetTimeToReachBuilding(destination));
        ShowTravellingUI();
        TrackingManager.AddTracking(TrackingManager.Track.StartTravelling, "Destination", destination.ToString());
    }
    public void CancelTravelling()
    {
        playerData.CancelTravelling();
        TravellingContainer.GetComponent<TravellingController>().Hide();
        mapManager.ResetTravelling();
    }

    public void ShowTravellingUI()
    {
        TravellingContainer.SetActive(true);
        TravellingContainer.GetComponent<TravellingController>().Show();
    }

    public void FocusOnBuilding(BuildingType buildingType)
    {
        mapManager.FocusOnBuilding(buildingType);
    }
    public void UpdateMapMissions()
    {
        if (!MergeManager) return;
        if (mapManager)
        {
            mapManager.UpdateMapMissions();
        }
        UpdateMapMissionsMergeLowerBar();
    }
    private void UpdateMapMissionsMergeLowerBar()
    {
        bool missionReady = mergeModel.HasAnyMissionReady();
        UpdateStorageButtons();
        mergeLowerBar.UpdateMissionsBtn(mergeModel.MissionsCount, missionReady);
        if (MergeManager.IsBoardActive)
        {
            mergeLowerBar.SetMissionsVisible(mergeModel.mapMissionsNew.Count > 0);
            if (missionReady && PlayerPrefs.GetInt("tooltip_missionready") == 0)
            {
                mergeLowerBar.btnMergeMissions.ShowToolTip("Mission Ready to Complete");
                PlayerPrefs.SetInt("tooltip_missionready", 1);
            }
        }
    }
    public void UpdateStorageButtons()
    {
        mergeLowerBar.UpdateStorageBtn(mergeModel.storage.Count, mergeModel.storageSlots, mergeModel.gifts.Count);
    }
    internal void CompleteMapMission(MapMissionCloud missionConfig)
    {
        mergeModel.CompleteMapMission(missionConfig);
        MapMissionCompleted(missionConfig.location);
    }
    internal void CompleteCharacterMission(MapMissionCloudCharacter missionConfig)
    {
        mergeModel.CompleteCharacterMission(missionConfig);
        MapMissionCompleted(missionConfig.location);
    }
    internal void CompleteLimitedMission(LimitedMissionMapConfig missionConfig)
    {
        mergeModel.CompleteLimitedMission(missionConfig);
        MapMissionCompleted(missionConfig.location);
    }
    private void MapMissionCompleted(BuildingType location)
    {
        PlayerData.MapMissionsCount++;
        dailyTaskManager.OnCompleteMission();
        TrackingManager.TrackMapMissionComplete();
        mapManager.GetBuildingMapPointerFromType(location).CheckPointer();
        UpdateMapMissions();
    }

    public void SendGiftSuccess(bool result, PieceDiscovery piece,string toPlayer)
    {
        dailyTaskManager.OnGift();
        if (result)
        {
            PopupsManager.ShowPopupGiftSent(RewardsGranter.GetRewardsOfGift(piece));
            TrackingManager.TrackGiftSent(piece,toPlayer);
            mergeModel.RemoveFromStorage(piece,true);
            UpdateStorageButtons();
        }
        else
        {
            PopupsManager.ShowPopupYesNo("Failed to send", "Failed to send item", PopupManager.PopupType.ok);
        }
        if (giftsStealScreen.activeSelf)
        {
            ShowSendGifts(false, null);
        }
    }

    public void ProcessGiftSteal(JSONArray result)
    {
        StartCoroutine(ShowAllGiftsSteals(result));
    }

    IEnumerator ShowAllGiftsSteals(JSONArray result)
    {
        var itemsGift = new List<RewardData>();
        var itemsStolen = new List<RewardData>();
        var playersGift = new List<String>();
        var playersSteal = new List<String>();

        for (int i = 0; i < result.Count; i++)
        {
            var pTypeGift = (int)result[i].AsObject["gift"];
            var pTypeStolen = (int)result[i].AsObject["stolen"];
            var level = (int)result[i].AsObject["lvl"];
            var pieceGift = new PieceDiscovery((PieceType)pTypeGift, level);
            var pieceStolen = new PieceDiscovery((PieceType)pTypeStolen, level);
            if(pieceGift.pType != PieceType.None)
            {
                var r = new RewardData(pieceGift);
                itemsGift.Add(r);
                if(!playersGift.Contains(result[i].AsObject["playername"]))
                    playersGift.Add(result[i].AsObject["playername"]);
            }
            else
            {
                //Remove from Storage
                if (mergeModel.storage.Find(p => p.pType == pieceStolen.pType) != null) 
                {
                    mergeModel.storage.RemoveAt(mergeModel.storage.FindIndex(p => p.pType == pieceStolen.pType));
                    UpdateStorageButtons();
                }
                var r = new RewardData(pieceStolen);
                itemsStolen.Add(r);
                
                if (!playersSteal.Contains(result[i].AsObject["playername"]))
                    playersSteal.Add(result[i].AsObject["playername"]);
            }
            
        }
        
        if (itemsStolen.Count > 0)
        {
            PopupsManager.ShowGiftStealReceivedPopup(itemsStolen, playersSteal, false);
        }
        if (itemsGift.Count > 0)
        {
            PopupsManager.ShowGiftStealReceivedPopup(itemsGift, playersGift, true);
        }
        yield return null;
    }

    
    public void ShowMyPlayerStats()
    {
        PopupsManager.ShowMyPlayerStats();
    }

    private async void CheckSeasonEnd()
    {
        //TODO: CHECK ONLY IF IS SUNDAY
        var previousWeekId = await Leaderboards.GetYesterdayId(LeaderboardID.mainWeekly);
        if(LeaderboardManager.lastLeaderboardId != previousWeekId && !Globals.Instance.seasonRewardsShowing)
        {
            if (topBar.seasonEndClaim.activeSelf) return;
            await LeaderboardManager.LoadWinners();
            var playerPosition = LeaderboardManager.GetPlayerWinnerPosition();
            Debug.Log("Season ENDED, position = " + playerPosition);
            if(playerPosition > -1)
            {
                topBar.ShowSeasonEndClaim();
                return;
            }
            else
            {
                LeaderboardManager.lastLeaderboardId = previousWeekId;
            }
        }
        topBar.ShowSeasonEndClaim(false);
    }

    public void ShowSeasonEnd() //Called from Unity button
    {
        //Debug.Log("Season End Click!");
        Globals.Instance.seasonRewardsShowing = true;
        topBar.seasonEndClaim.SetActive(false);
        HideUI();
        seasonEndController.gameObject.SetActive(true);
        seasonEndController.ShowResults(LeaderboardManager.winners, FinishSeasonEnd);
    }

    private async void FinishSeasonEnd()
    {
        var previousWeekId = await Leaderboards.GetYesterdayId(LeaderboardID.mainWeekly);
        LeaderboardManager.lastLeaderboardId = previousWeekId;
        sideBar.ShowSidebar(true, true);
        ShowMapLowerBar(true);
        topBar.ShowFameContainer();
        Globals.Instance.seasonRewardsShowing = false;
        seasonEndController.gameObject.SetActive(false);
    }

    public void HideUI(bool sidebar = true,bool lowerbars = true)
    {
        if (lowerbars)
        {
            ShowMapLowerBar(false);
            ShowMergeLowerBar(false);
        }
        if (sidebar)
        {
            sideBar.ShowSidebar(false, true);
        }
        if (ScreenManager.Instance.currentScreen != null)
        {
            ScreenManager.Instance.AnimateScreenOUT(ScreenManager.Instance.currentScreen, false);
        }
    }

    #region ADMIN
    public int PlayerCareerPoints(string careerTitle)
    {
        return PlayerPrefs.GetInt(careerTitle + "Points");
    }


    public void AdminCheckPendingGifts()
    {
        server.CheckPendingGifts();
    }

    public void ADMIN_GiveGems()
    {
        AdminPanel.SetActive(false);
        AddRewardToPlayer(new RewardData(RewardType.Gems, 50));
    }
    public void ADMIN_GiveCoins(int value)
    {
        AdminPanel.SetActive(false);
        AddRewardToPlayer(new RewardData(RewardType.Coins, value));
    }
    public void ADMIN_LevelUp()
    {
        AdminPanel.SetActive(false);
        playerData.ADMIN_LevelUP();
    }
    public void ADMIN_AddSkillPoint()
    {
        skillsManager.availablePoints++;
    }
    public void ADMIN_ShowMergeSpeed()
    {
        AdminPanel.SetActive(false);
        ShowSpeedMergeBoard(true);
    }
    public void ShowHideAdminPanel()
    {
        //AdminPanel.SetActive(!AdminPanel.activeSelf);
        //if (AdminPanel.activeSelf)AdminPanel.GetComponent<AdminPanelController>().Refresh();
    }

    public void AdminCreateCarOffer()
    {
        PlayerPrefs.SetInt("carOffersShown", 8);
        TryToCreateCarOffer();
        AdminPanel.SetActive(false);
    }
    public void ShowResetProgressPopup()
    {
        var gemsGift = playerData.level * 50;
        PopupsManager.ShowPopupReset(gemsGift, ResetGame);
    }
    public async void ResetGame()
    {
        if(MyScenesManager.Instance != null)
        {
            MyScenesManager.Instance.ShowScreen();
        }
        var gemsGift = playerData.level * 50;
        var playerName = PlayerData.playerName;
        //Reset Cloud Save
        await CloudSaveService.Instance.Data.Player.DeleteAllAsync();
        PlayerPrefs.DeleteAll();
        AuthenticationService.Instance.SignOut();
        await Globals.CheckUnityService();

        var data = new Dictionary<string, object> { { "gems", gemsGift } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        if (playerName != string.Empty)
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        }
        SceneManager.LoadScene(0);
    }

    public void CreateDailyTasks()
    {
        dailyTaskManager.model.dailyTasks = null;
        dailyTaskManager.model.allClaimed = false;
        dailyTaskManager.Init();
    }

    public void TutoGiveVehicle()
    {
        playerData.AddVehicleByID(201);
    }

#if UNITY_EDITOR
    public void AdminTuto()
    {
        PlayerPrefs.DeleteKey("Tutorial" + TutorialKeys.Deliveries);
    }
    public void AdminTuto2()
    {
        PlayerPrefs.DeleteKey("Tutorial" + TutorialKeys.GiveVehicle);
        tutorialManager.StartTutorialGiveVehicle();
    }

    public void Admin_ShowLevelProgresion()
    {
        for (var i = 0; i<20; i++)
        {
            var xp = gameConfig.NextLevelXP(i);
            Debug.Log($"Level {i} = {xp}");
        }
    }
    public void Admin_ShowLeagueReward()
    {
        AdminPanel.SetActive(false);
        PopupsManager.ShowPopupLeagueReward(3,null);
    }

    public void Admin_AddRandomGift()
    {
        var r = UnityEngine.Random.Range(0, 5);
        switch (r)
        {
            case 0: mergeModel.AddGift(new PieceDiscovery(PieceType.BoosterAutoMerge, 0)); break;
            case 1: mergeModel.AddGift(new PieceDiscovery(PieceType.BoosterEnergy, 0)); break;
            case 2: mergeModel.AddGift(new PieceDiscovery(PieceType.BoosterGenerators, 0)); break;
            case 3: mergeModel.AddGift(new PieceDiscovery(PieceType.PoliceWeapon, 6)); break;
            case 4: mergeModel.AddGift(new PieceDiscovery(PieceType.PoliceWeapon, 7)); break;
        }
    }

    public void AdminForceSeasonEnd()
    {
        AdminPanel.SetActive(false);
        PlayerPrefs.SetInt("LastWeekChecked",Globals.Instance.currentWeek-1);
        Globals.Instance.seasonSecondsLeft = -1;
    }

    public void AdminClearAvatars()
    {
        mergeModel.ClearAvatars();
    }

    public void AdminShowLevelUP()
    {
        AdminPanel.SetActive(false);
        topBar.gameObject.SetActive(true);
        levelUPPopup.SetActive(true);
        var levelUpManager = levelUPPopup.GetComponent<LevelUpManager>();
        levelUpManager.ShowRewardsForNextLevel(playerData.level);
    }

    public void AdminChangeCarColor()
    {
        var vInstance = mapManager.GetBuildingInteractiveFromPlayerLocation().GetPlayerVehicleInstance();
        var view = GameObject.Find("VehicleCustomize").GetComponent<VehicleCustomize>();

        view.InitVehicle(playerData.GetBestVehicle(), vInstance?vInstance: new VehicleInstance());
    }

    public void ADMIN_AddVehicle()
    {
        /*if(playerData.vehiclesOwned.Count < gameConfig.vehiclesDefinition.Count)
        {
            //int v = UnityEngine.Random.Range(0, gameConfig.vehiclesDefinition.Count);
            int v = gameConfig.vehiclesDefinition.Count-1;
            while (playerData.vehiclesOwned.Find(ve => ve.id == gameConfig.vehiclesDefinition[v].id) != null)
            {
                v = UnityEngine.Random.Range(0, gameConfig.vehiclesDefinition.Count);
            }
            playerData.AddVehicle(v);
        }*/
        playerData.AddVehicleByID(114);
        AdminPanel.SetActive(false);
    }

    public void ADMIN_ShowItemsRarity()
    {
        var generators = mapManager.GetUnlockedGenerators(true);
        foreach (var generator in generators)
        {
            for (var i = 0; i < generator.piecesChances.Count; i++)
            {
                Debug.Log(generator.piecesChances[i].pieceType + " rarity " + generator.GetChainRarity(i));
            }
        }
    }

#endif
    #endregion

    private DateTime _idleTime;

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !_appIsFocused)
        {
            /*if(_idleTime != null)
            {
                var totaltime = (DateTime.Now - _idleTime).TotalSeconds;
                //Debug.Log("Time out " + );
                if (totaltime > 3600) ReloadGame();
                return;
            }*/
            //Check Access Token
            CheckToken();
            _appIsFocused = true;
        }
        else if (!hasFocus)
        {
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.DOFade(1, 0);
            _appIsFocused = false;
            _idleTime = DateTime.Now;
        }
    }

    public void ReloadGame()
    {
        Globals.Instance.leaderboardReady = false;
        SceneManager.LoadScene(0);
    }

    // IAP Listener
    public void OnPurchase(Product product)
    {
        Log("Purchased " + product.definition.id);
        if (product.definition.payout !=null && product.definition.payout.subtype == "gems")
        {
            var rewards = new List<RewardData>();
            var reward = new RewardData(RewardType.Gems, (int)product.definition.payout.quantity);
            rewards.Add(reward);
            AddRewardToPlayer(reward);
            PopupsManager.ShowPopupPurchase(rewards);
            MyAnalytics.LogPurchase((int)product.definition.payout.quantity);
            TrackingManager.TrackPurchase(product.definition.id, "OnPurchasePopup");
        }
    }

    public void GiveRewardsWithPopup(List<RewardData> rewards, bool applyReward)
    {
        PopupsManager.ShowPopupPurchase(rewards, applyReward);
    }

    public void PlayVideoAd(UnityAction OnVideoEnd)
    {
        TrackingManager.TrackVideoAdStart();
        Log("Watch Video");
        videoAdsManager.OnVideoEndSuccess.RemoveAllListeners();
        videoAdsManager.OnVideoEndSuccess.AddListener(OnVideoEnd);
        videoAdsManager.PlayVideo();
    }

    LightmapData[] _originalLightmaps;
    Texture2D blackTexture;
    public void LightMaps()
    {
        _originalLightmaps = LightmapSettings.lightmaps;
        LightmapData[] turnedOffLightmaps = new LightmapData[_originalLightmaps.Length];
        for (int i = 0; i < turnedOffLightmaps.Length; i++)
        {
            var thisOriginalLightmap = _originalLightmaps[i];
            var thisTurnedOffLightmap = new LightmapData();

            thisTurnedOffLightmap.lightmapDir = thisOriginalLightmap.lightmapDir;
            thisTurnedOffLightmap.shadowMask = thisOriginalLightmap.shadowMask;
            thisTurnedOffLightmap.lightmapColor = blackTexture;

            turnedOffLightmaps[i] = thisTurnedOffLightmap;
        }
        LightmapSettings.lightmaps = turnedOffLightmaps;
    }

    #region UTILS STATIC

    public static void RemoveChildren(GameObject g, bool immediate = false)
    {
        if (immediate)
        {
            while (g.transform.childCount > 0)
            {
                DestroyImmediate(g.transform.GetChild(0).gameObject);
            }
        }
        else
        {
            for (var i = 0; i < g.transform.childCount; i++)
            {
                Destroy(g.transform.GetChild(i).gameObject);
            }
        }
    }

    public static void SetLocString(string table, string key, TMP_Text textField)
    {
        var stringOperation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, key);
        LoadString(stringOperation, textField);
    }

    private static void LoadString(AsyncOperationHandle<string> stringOperation, TMP_Text textField)
    {
        if (stringOperation.IsDone)
        {
            SetString(stringOperation, textField);
        }
        else
        {
            object[] parms = new object[2] { stringOperation, textField };
            instance.StartCoroutine("LoadStringWithCoroutine", parms);
        }
    }

    IEnumerator LoadStringWithCoroutine(AsyncOperationHandle<string> stringOperation, TMP_Text textField)
    {
        yield return stringOperation;
        SetString(stringOperation, textField);
    }

    static void SetString(object[] parms)
    {
        AsyncOperationHandle<string> stringOperation = (AsyncOperationHandle<string>)parms[0];
        TMP_Text textField = (TMP_Text)parms[1];
        SetString(stringOperation, textField);
    }
    static void SetString(AsyncOperationHandle<string> stringOperation, TMP_Text textField)
    {
        textField.text = stringOperation.Result;
    }
    
    public static void Log(string msg)
    {
#if UNITY_EDITOR
        Debug.Log(msg);
#endif
    }
    #endregion
}

public class WeightedList<T>
{
    public List<WItem<T>> wItems;

    public WeightedList()
    {
        wItems = new List<WItem<T>>();
    }

    public int Count => wItems.Count;

    public void Add(T item,int w)
    {
        wItems.Add(new WItem<T>(item, w ));
    }
    public void Add(T item,float w)
    {
        wItems.Add(new WItem<T>(item, w ));
    }

    public WItem<T> GetWieghted()
    {
        var totalW = GetTotalWeight();
        var candidate = UnityEngine.Random.Range(0, totalW);
        wItems.Sort((a, b) => { return a.weight.CompareTo(b.weight); });
        var currentWeight = 0f;
        for(var i = 0; i < wItems.Count; i++)
        {
            currentWeight += wItems[i].weight;
            if (candidate < currentWeight)
            {
                return wItems[i];
            }
        }
        return wItems[wItems.Count-1];
    }
    
    public void Remove(WItem<T> m)
    {
        wItems.Remove(m);
    }

    private float GetTotalWeight()
    {
        var total = 0f;
        foreach(var i in wItems)
        {
            total += i.weight;
        }
        return total;
    }
}
public class WItem<T>
{
    public T item;
    public float weight;

    public WItem(T item, int w)
    {
        this.item = item;
        weight = w;
    }
    public WItem(T item, float w)
    {
        this.item = item;
        weight = w;
    }
}

public enum RarityType
{
    Common,
    Uncommon,
    Rare,
    VeryRare,
    Especial,
    Epic,
    Unique
}
/*
public enum Screen
{
    Jobs,
    Garage,
    Player,
    Career
}*/

public enum MuebleType
{
    Closet,
    Cuadro,
    Sofa,
    Carpet,
    Plant,
    CenterTable,
    Lamp,
    Count
}
