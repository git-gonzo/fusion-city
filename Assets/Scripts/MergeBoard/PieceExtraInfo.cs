using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceExtraInfo : MonoBehaviour
{
    public GameObject MapReady;
    public GameObject MissionReady;
    public GameObject MaxLevel;
    public GameObject Locked;
    public GameObject LockedBack;
    public GameObject SpawnerBoosterFX;
    public GameObject Bubble;

    private bool _isLocked;
    private bool _isBubble;

    public void SetState(bool isMapReady = false,bool isMissionReady = false, bool isMaxLevel = false, bool isLocked = false)
    {
        if (_isLocked || _isBubble) return;
        HideAll();
        _isLocked = isLocked;
        LockedBack.SetActive(false);
        if (isLocked)
        {
            MapReady.SetActive(false);
            MissionReady.SetActive(false);
            MaxLevel.SetActive(false);
            Locked.SetActive(true);
            return;
        }
        Locked.SetActive(false);
        MapReady.SetActive(isMapReady);
        MissionReady.SetActive(isMissionReady);
        MaxLevel.SetActive(isMaxLevel);
    }

    public void SetLockedBack()
    {
        HideAll();
        LockedBack.SetActive(true);
    }

    public void Unlock()
    {
        Bubble.SetActive(false);
        _isLocked = false;
        Locked.SetActive(false);
        LockedBack.SetActive(false);
    }

    public void ActivateBoosterSpawner()
    {
        HideAll();
        SpawnerBoosterFX.SetActive(true);
    }

    public void SetBubble()
    {
        HideAll();
        _isBubble = true;
        Bubble.SetActive(true);
    }

    private void HideAll()
    {
        SpawnerBoosterFX.SetActive(false);
        MapReady.SetActive(false);
        MissionReady.SetActive(false);
        MaxLevel.SetActive(false);
        Locked.SetActive(false);
        Bubble.SetActive(false);
        LockedBack.SetActive(false);
    }
}
