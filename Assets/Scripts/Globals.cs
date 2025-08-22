using System;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Globals
{
    public static Globals Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Globals();
            }
            return _instance;
        }
    }

    public int Sessions { 
        get => PlayerPrefs.GetInt("sessions");
        set => PlayerPrefs.SetInt("sessions", value); }

    private static Globals _instance;

    public static bool isSigning = false;
    public static bool isSignedIn = false;
    public static bool configLoaded = false;
    public static bool playerDataLoaded = false;
    public static bool minVersionResetChecked = false;
    public bool gameLoaded = false;
    public bool boardsLoaded = false;
    public bool errorOnSigning = false;
    public bool leaderboardReady = false;
    public bool seasonRewardsChecking = false;
    public bool seasonRewardsReady = false;
    public bool seasonRewardsShowing = false;
    public bool seasonFirstCheck = false;
    public int currentWeek;
    
    public int seasonSecondsLeft { 
        get { return (int)(_seasonEndTime - DateTime.Now).TotalSeconds; } 
        set { _seasonEndTime = DateTime.Now.AddSeconds(value); } }

    private DateTime _seasonEndTime;
    private DateTime startDate;

    public void OnInitialize()
    {
        if (!PlayerPrefs.HasKey("StartDate"))
        {
            //StartDate is the current Date and Time.
            startDate = DateTime.Now;
            //Store the current date and time in PlayerPrefs under the key "StartDate"
            PlayerPrefs.SetString("StartDate", DateTime.Now.ToString());
            Sessions = 1;
        }
        else //We have already set the StartDate.
        {
            // The start date and time pulled from PlayerPrefs.
            startDate = DateTime.Parse(PlayerPrefs.GetString("StartDate"));
            Sessions++;
        }
    }

    public static async Task CheckUnityService()
    {
        //IF SERVICE NOT INTIALIZED
        if (UnityServices.State == ServicesInitializationState.Uninitialized && UnityServices.State != ServicesInitializationState.Initializing)
        {
            Debug.Log("UnityServices InitializeAsync");
            await UnityServices.InitializeAsync();
        }
        if (!isSigning && (!AuthenticationService.Instance.IsSignedIn || AuthenticationService.Instance.IsExpired))
        {
            Debug.Log("SignInAnonymouslyAsync from Globals");
            isSigning = true;
            await SignInAnonymously();
        }
    }

    private static async Task SignInAnonymously()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            isSigning = false;
            PlayerData.playerID = AuthenticationService.Instance.PlayerId;

            Debug.Log("Signed in as: " + PlayerData.playerID);
            isSignedIn = true;
            //SendPlayerLogin();
            //OnSignedIn?.Invoke();
        };
        AuthenticationService.Instance.SignInFailed += s =>
        {
            // Take some action here...
            Debug.Log(s);
            isSigning = false;
            Globals.Instance.errorOnSigning = true;
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        AnalyticsService.Instance.StartDataCollection();
    }

    internal static async Task IsSignedIn()
    {
        while (!isSignedIn)
        {
            await Task.Delay(10);
        }
        await Task.CompletedTask;
    }
    internal static async Task IsConfigLoaded()
    {
        await IsSignedIn();
        while (!configLoaded)
        {
            await Task.Delay(10);
        }
        await Task.CompletedTask;
    }
    internal static async Task IsPlayerDataLoaded()
    {
        await IsSignedIn();
        while (!playerDataLoaded)
        {
            await Task.Delay(10);
        }
        await Task.CompletedTask;
    }

    public static bool IsVersionBelow(string version)
    {
        var gameVersion = new Version(Application.version);
        var otherVersion = new Version(version);

        var result = gameVersion.CompareTo(otherVersion);
        if (result > 0)
        {
            Debug.Log("gameVersion " + gameVersion + " is greater than Other " + otherVersion);
            return false;
        }
        else if (result < 0)
            Debug.Log("otherVersion " + version + " is greater than " + gameVersion);
        else
            Debug.Log("versions are equal");
        return true;
    }
    
    public static bool IsVersionLower(string currentVersion, string targetVersion)
    {
        var version1 = new Version(currentVersion);
        var otherVersion = new Version(targetVersion);

        var result = version1.CompareTo(otherVersion);
        if (result > 0)
        {
            //Debug.Log("gameVersion " + version1 + " is greater than Other " + otherVersion);
            return false;
        }
        else if (result < 0)
        {
            //Debug.Log("otherVersion " + otherVersion + " is greater than " + version1);
            return true;
        }
        //Debug.Log("versions are equal");
        return false;
    }
}