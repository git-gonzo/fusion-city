using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class SwitchButton : MonoBehaviour
{
    public enum SettingType
    {
        Music,
        Graphics,
        Sound
    }

    public SettingType settingType;
    public bool isActive = true;
    public Image barOn;
    public Image bSliderBack;
    public Image bSliderFront;
    public Color colorBackOn;
    public Color colorfontOn;
    public Color colorBackOff;
    public Color colorfontOff;
    public Transform ball;
    [SerializeField] float _ballOffsetLeft=0;
    [SerializeField] float _ballOffsetRight=0;
    [SerializeField] Image _iconOn;
    [SerializeField] Image _iconOff;
    

    float barPosX => barOn.transform.position.x;
    float barWidth => barOn.GetComponent<RectTransform>().rect.size.x;
    float ballPosOn => barPosX + barWidth - _ballOffsetRight;
    float ballPosOff => barPosX + _ballOffsetLeft;

    public int GraphicsQuality
    {
        get { return PlayerPrefs.GetInt("GraphicsQuality"); }
        set { PlayerPrefs.SetInt("GraphicsQuality", value); }
    }
    public int MusicSetting
    {
        get { return PlayerPrefs.GetInt("MusicSetting"); }
        set { PlayerPrefs.SetInt("MusicSetting", value); }
    }
    public int SoundSetting
    {
        get { return PlayerPrefs.GetInt("SoundSetting"); }
        set { PlayerPrefs.SetInt("SoundSetting", value); }
    }

    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if(settingType == SettingType.Graphics)
        {
            isActive = GraphicsQuality == 1;
        }
        else if(settingType == SettingType.Music)
        {
            isActive = MusicSetting == 0;
            SetIconsVisible();
        }
        else if(settingType == SettingType.Sound)
        {
            isActive = SoundSetting == 0;
            SetIconsVisible();
        }
        ToggleBall(true);
    }

    public void SetMusic()
    {
        ToggleBall();
        MusicSetting = isActive ? 0 : 1;
        if (isActive) { GameManager.Instance.Music.Play(); } 
        else { GameManager.Instance.Music.Stop(); }
        SetIconsVisible();
    } 
    public void SetSound()
    {
        ToggleBall();
        SoundSetting = isActive ? 0 : 1;
        AudioListener.volume = isActive ? 1 : 0;
        SetIconsVisible();
    } 
    
    public void SetObjectsVisibility()
    {
        ToggleBall();
        
        GameManager.Instance.gameConfig.objectsVisibility = isActive;
    }

    public void SetSetQuality()
    {
        ToggleBall();
        GraphicsQuality = isActive?1:0;
        //Camera.main.GetComponent<PostProcessLayer>().enabled = isActive;
        //

        //UniversalAdditionalCameraData uac = GameManager.Instance.mapManager.cameraMap.GetComponent<UniversalAdditionalCameraData>();
        UniversalAdditionalCameraData uac = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        uac.renderPostProcessing = isActive ? true : false;
        
        
        
        // = GameManager.Instance.mapManager.cameraMap.GetComponent<PostProcessVolume>();
        
    }

    private void SetIconsVisible()
    {
        _iconOn.gameObject.SetActive(isActive);
        _iconOff.gameObject.SetActive(!isActive);
    }

    void ToggleBall(bool isInit = false)
    {
        if(!isInit) isActive = !isActive;
        barOn.DOColor(isActive ? Color.white : Color.red, 0.2f);
        ball.DOMoveX(isActive ? ballPosOn : ballPosOff, 0.23f);
        bSliderBack.DOColor(isActive ? colorBackOn : colorBackOff, 0.2f);
        bSliderFront.DOColor(isActive ? colorfontOn : colorfontOff, 0.2f);
    }
}


