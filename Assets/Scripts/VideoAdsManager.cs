using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

public class VideoAdsManager : MonoBehaviour, IUnityAdsShowListener, IUnityAdsInitializationListener, IUnityAdsLoadListener
{
    public string gameID_google = "4668409";
    public string gameID_ios = "4668408";
    public bool testMode = false;
    private bool _initializing = false;
    private bool _initialized = false;
    private bool _initializedFailed = false;
    private bool _videoLoading = false;
    private bool _videoLoadingFailed = false;
    private bool _videoLoaded = false;
    public bool videoLoaded => _videoLoaded;
    public bool initialized => _initialized;
    string BannerID => Application.platform == RuntimePlatform.Android ? "Banner_Android" : "Banner_iOS";
    string VideoID => Application.platform == RuntimePlatform.Android ? "Rewarded_Android" : "Rewarded_iOS";

    public enum VideoSource
    {
        Bank,
        Shop,
        RefillEnergy
    }

    public static VideoAdsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new VideoAdsManager();
            }
            return instance;
        }
    }

    private static VideoAdsManager instance;

    public UnityEvent OnVideoEndSuccess;
    void Awake()
    {
        instance ??= this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(gameObject);
        InitializeAds();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InitializeAds()
    {
        _initializing = true;
        Advertisement.Initialize(Application.platform == RuntimePlatform.Android ? gameID_google : gameID_ios, testMode, this);
    }

    public void DisplayIntestitial()
    {
        //Advertisement.Show();
    }

    public void PlayVideo()
    {
        CheckReady();
        StartCoroutine(waitAndshowVideo());
    }
    public void ShowBanner()
    {
        TrackingManager.AddTracking(TrackingManager.Track.ShowBanner, "Banner", true);
        StartCoroutine(waitAndshowBanner());
    }
    public void HideBanner()
    {
        Advertisement.Banner.Hide(true);
    }
    IEnumerator waitAndshowVideo()
    {
        while (!Advertisement.isInitialized || !_videoLoaded)
        {
            if (!Advertisement.isInitialized && !_initializing) InitializeAds();
            else if (!_videoLoaded && !_videoLoading) CheckReady();
            else Debug.Log("CANNOT SHOW VIDEO, INITIALIZED " + Advertisement.isInitialized + ", VideoLoaded " + _videoLoaded);
            yield return null;
        }
        Advertisement.Show(VideoID, this);
        //Advertisement.Load(VideoID,this);
        Debug.Log("SHOW VIDEO!");
    }
    IEnumerator waitAndshowBanner()
    {
        while (!Advertisement.isInitialized)
        {
            Debug.Log("ADS NOT INITIALIZED");
            if (!_initializing) InitializeAds();
            yield return null;
        }
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(BannerID);
        Debug.Log("SHOW AD BANNER");
    }

    public void OnUnityAdsReady(string placementId)
    {
        Debug.Log("OnUnityAdsReady");
        _initializing = false;
    }

    public void OnUnityAdsDidError(string message)
    {
        Debug.Log("OnUnityAdsDidError");
        _initializing = false;
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        Debug.Log("Video ad Started");
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        _videoLoaded = false;
        Debug.Log("OnUnityAdsDidFinish " + placementId);
        if (showResult == ShowResult.Finished)
        {
            OnVideoEndSuccess?.Invoke();
            GameManager.Instance.dailyTaskManager.OnWatchVideo();
            Debug.Log("Video ad completed");
        }
        else if (showResult == ShowResult.Skipped)
        {
            Debug.Log("Video ad Skipped");
        }
        else if (showResult == ShowResult.Failed)
        {
            Debug.Log("Video ad Failed");
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("OnUnityAdsShowFailure");
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("OnUnityAdsShowStart");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("OnUnityAdsShowClick");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        _videoLoaded = false;
        if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            OnVideoEndSuccess?.Invoke();
            GameManager.Instance.dailyTaskManager.OnWatchVideo();
        }
    }

    public void OnInitializationComplete()
    {
        //Debug.Log("UnityAds Initialization Complete");
        _initializing = false;
        _initialized = true;
        if(GameManager.Instance && GameManager.Instance.PlayerLevel > 2)
            CheckReady();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        //Debug.Log("UnityAds Initialization FAILED " + error + "-" + message);
        _initializedFailed = true;
        _initialized = false;
        _initializing = false;
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("UnityAds VideoLoad Complete");
        _videoLoaded = true;
        _videoLoading = false;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        _videoLoading = false;
        _videoLoadingFailed = true;
        Debug.Log("UnityAds VideoLoad FAILED");
    }

    internal void CheckReady()
    {
        if (!_videoLoaded && !_videoLoading)
        {
            _videoLoading = true;
            Advertisement.Load(VideoID, this);
            Debug.Log("Load Video called");
        }
    }

    public bool IsInitialized() {
        return _initialized || _initializedFailed;
    }

    public bool IsVideoReady()
    {
        return _videoLoaded || _videoLoadingFailed;
    }

    
}

