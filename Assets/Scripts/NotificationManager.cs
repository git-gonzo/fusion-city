using NotificationSamples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using Unity.Notifications.Android;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using NotificationSamples.Android;
#elif UNITY_IOS
using NotificationSamples.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    public bool Initialized { get; private set; }
    public IGameNotificationsPlatform Platform { get; private set; }
    public List<PendingNotification> PendingNotifications { get; private set; }
    bool notificationChannelSet;
    private AndroidNotificationChannel androidNotificationChannel;

    // Flag set when we're in the foreground
    private bool inForeground = true;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (Initialized)
        {
            throw new InvalidOperationException("NotificationsManager already initialized.");
        }

#if UNITY_ANDROID
        AskForPermissions();
        Platform = new AndroidNotificationsPlatform();
        CreateChannel();
        Initialized = true;

#elif UNITY_IOS
        Platform = new iOSNotificationsPlatform();
#endif

        if (Platform == null)
        {
            return;
        }

        PendingNotifications = new List<PendingNotification>();
    }

    
    void CreateChannel()
    {
        androidNotificationChannel = new AndroidNotificationChannel()
        {
            Id = "mainChannelID",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };
        var channel = androidNotificationChannel;
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        notificationChannelSet = true;
    }
    
    public void AskForPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }

    public void SendNotification()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS") || !Globals.isSignedIn)
        {
            return;
        }

        var notification = new AndroidNotification();
        notification.LargeIcon = "icon_1";
        notification.Title = "Energy Full";
        notification.Text = "Your energy has been fully restored";
        notification.FireTime = System.DateTime.Now.AddSeconds(GameManager.Instance.MergeManager.boardController.energyRefillTotalSeconds);
        AndroidNotificationCenter.CancelAllNotifications();
        AndroidNotificationCenter.SendNotification(notification, "mainChannelID");
        Debug.Log("Notification Sent");
    }
    public static void SendNotification(int id)
    {
        var notification = new AndroidNotification();
        notification.Title = "Your Title";
        notification.Text = "Your Text";
        notification.FireTime = System.DateTime.Now.AddMinutes(1);
        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, "mainChannelID", id);
    }


    ///////// IOS
    /*
    IEnumerator RequestAuthorization()
    {
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log(res);
        }
    }

    public static void SendIOSNotification()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 1, 0),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = "_notification_01",
            Title = "Title",
            Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in 5 seconds",
            Subtitle = "This is a subtitle, something, something important...",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }*/


    /// <summary>
    /// Respond to application foreground/background events.
    /// </summary>
    protected void OnApplicationFocus(bool hasFocus)
    {
        if (Platform == null || !Initialized)
        {
            if (Platform == null) Debug.Log("Platform null");
            return;
        }

        inForeground = hasFocus;

        if (hasFocus)
        {
            //TODO: Cancel Notifications
            //OnForegrounding();
            return;
        }

        SendNotification();
    }
}
