using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System;

public class MyScenesManager : MonoBehaviour
{
    public static MyScenesManager Instance => _instance;
    static MyScenesManager _instance;

    WaitForSecondsRealtime waitForSecondsRealtime;
    public float crossFadeTime = 0.5f;
    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += SelectedLocaleChanged;

        if (waitForSecondsRealtime == null)
            waitForSecondsRealtime = new WaitForSecondsRealtime(crossFadeTime);

        if (!LocalizationSettings.InitializationOperation.IsDone)
            StartCoroutine(Preload(null));
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= SelectedLocaleChanged;
    }

    void SelectedLocaleChanged(Locale locale)
    {
        StartCoroutine(Preload(locale));
    }

    IEnumerator Preload(Locale locale)
    {

        var operation = LocalizationSettings.InitializationOperation;

        do
        {
            // When we first initialize the Selected Locale will not be available however
            // it is the first thing to be initialized and will be available before the InitializationOperation is finished.
            if (locale == null)
                locale = LocalizationSettings.SelectedLocaleAsync.Result;

            yield return null;
        }
        while (!operation.IsDone);

        if (operation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
        {
            //progressText.text = operation.OperationException.ToString();
            //progressText.color = Color.red;
        }
        else
        {
            //background.CrossFadeAlpha(0, crossFadeTime, true);
            //progressText.CrossFadeAlpha(0, crossFadeTime, true);

            waitForSecondsRealtime.Reset();
            yield return waitForSecondsRealtime;
        }
    }

    private void Awake()
    {
        if(_instance!= null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;
    }

    void Start()
    {
        DontDestroyOnLoad(this);
        LoadMap2();
    }

    private async void LoadMap2()
    {
        await Globals.CheckUnityService();
        SceneManager.LoadScene(1);
    }

    public void HideScreen()
    {
        if (gameObject == null || GetComponent<CanvasGroup>() == null) return;
        GetComponent<CanvasGroup>()?.DOFade(0f, 0.3f).OnComplete(()=> { gameObject.SetActive(false); });
    }

    public void ShowScreen()
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);
            GetComponent<CanvasGroup>().DOFade(1f, 0.3f);
        }
    }

    internal void LoadScene2()
    {
        ShowScreen();
        DOVirtual.DelayedCall(0.3f, ()=>SceneManager.LoadScene(2));
    }
}
