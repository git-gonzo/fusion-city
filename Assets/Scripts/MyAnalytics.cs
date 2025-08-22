using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MyAnalytics : MonoBehaviour
{
    protected bool firebaseInitialized = false;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    public enum Currency
    {
        xp,
        coins,
        gems,
        fame
    }

    public virtual void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        Debug.Log("Enabling data collection.");
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        Debug.Log("Set user properties, playerID = " + PlayerData.playerID);
        // Set the user's sign up method.
        FirebaseAnalytics.SetUserProperty(
          FirebaseAnalytics.UserPropertySignUpMethod,
          "Google");
        // Set the user ID.
        FirebaseAnalytics.SetUserId(PlayerData.playerID);
        // Set default session duration values.
        FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
        firebaseInitialized = true;
    }

    public static void LogEventCurrency(RewardType currency, int value)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventEarnVirtualCurrency, currency.ToString(), value);
    }

    public static void LogLevelUp()
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelUp,new Parameter(FirebaseAnalytics.ParameterLevel,GameManager.Instance.PlayerLevel));
    }
    public static void LogPurchase(int amount)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase,new Parameter(FirebaseAnalytics.ParameterCurrency,amount));
    }
}
