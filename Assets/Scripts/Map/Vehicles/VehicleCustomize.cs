using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
public class VehicleCustomize : MonoBehaviour
{
    public Transform colorIconsContainer;
    public VehicleColorOption colorIconPrebab;
    public Button btnNext;
    public Button btnPrev;
    public ButtonBuy btnBuy;
    public Button btnSelect;
    public Button btnClose;
    public CinemachineCamera camCustomize;

    private int _activeMatIndex;
    private int _currentMatIndex;
    private int _totalMaterials;
    private VehicleInstance _vInstance;
    private SO_Vehicle _v;
    private List<VehicleColorOption> options;
    private CanvasGroup _canvasGroup;
    private RewardData _cost;

    public void Show()
    {
        _canvasGroup ??= GetComponent<CanvasGroup>();
        if (_canvasGroup != null)
        {
            _canvasGroup.DOFade(1, 0.3f).SetDelay(0.2f);
        }
        gameObject.SetActive(true);
        camCustomize.Priority = 50;
        GameManager.Instance.sideBar.ShowSidebar(false, true);
    }

    public void InitVehicle(SO_Vehicle v, VehicleInstance vInstance)
    {
        _v = v;
        _vInstance = vInstance;
        _totalMaterials = v.materials.Count;
        GameManager.RemoveChildren(colorIconsContainer.gameObject);
        options = new List<VehicleColorOption>();
        for (int i = 0; i < v.materials.Count; i++)
        {
            var option = Instantiate(colorIconPrebab, colorIconsContainer);
            option.Init(i+1);
            options.Add(option);
        }
        var vOwned = GameManager.Instance.playerData.vehiclesOwned.Find(ve => ve.id == v.id);
        _currentMatIndex = vOwned != null ? vOwned.mat : 0;
        _activeMatIndex = _currentMatIndex;
        _cost = new RewardData(_v.price.rewardType, (int)(_v.price.amount * 0.1f));
        options[_currentMatIndex].Select(true);
        options[_currentMatIndex].Active(true);


        btnNext.onClick.RemoveAllListeners();
        btnPrev.onClick.RemoveAllListeners();
        btnSelect.onClick.RemoveAllListeners();
        btnClose.onClick.RemoveAllListeners();
        btnBuy.button.onClick.RemoveAllListeners();

        btnNext.onClick.AddListener(showNextColor);
        btnPrev.onClick.AddListener(showPrevColor);
        btnSelect.onClick.AddListener(ApplyColor);
        btnBuy.button.onClick.AddListener(ApplyColor);
        btnClose.onClick.AddListener(OnClose);
        btnBuy.Init(_cost);
        btnBuy.gameObject.SetActive(false);
    }

    private void ApplyColor()
    {
        //TODO: buy color
        if (GameManager.TryToSpend(_cost))
        {
            GameManager.Instance.playerData.ChangeVechicleMaterial(_v.id, _currentMatIndex);
            AllOptionsActiveOff();
            options[_currentMatIndex].Active(true);
            _activeMatIndex = _currentMatIndex;
            RefreshOptions();
        }
    }

    private void showPrevColor()
    {
        _currentMatIndex--;
        RefreshOptions();
    }

    private void showNextColor()
    {
        _currentMatIndex++;
        RefreshOptions();
    }

    private void RefreshOptions()
    {
        if (_currentMatIndex < 0) _currentMatIndex = _totalMaterials - 1;
        else if (_currentMatIndex >= _totalMaterials) _currentMatIndex = 0;
        PreviewColor();
        AllOptionsSelectedOff();
        options[_currentMatIndex].Select(true);
        btnBuy.gameObject.SetActive(_currentMatIndex != _activeMatIndex);
    }

    private void PreviewColor()
    {
        _vInstance.ApplyMaterial(_v.materials[_currentMatIndex].material);
    }

    private void AllOptionsSelectedOff()
    {
        foreach (var opt in options)
        {
            opt.Select(false);
        }
    }
    private void AllOptionsActiveOff()
    {
        foreach (var opt in options)
        {
            opt.Active(false);
        }
    }

    private void OnClose()
    {
        if (_currentMatIndex != _activeMatIndex)
        {
            _vInstance.ApplyMaterial(_v.materials[_activeMatIndex].material);
        }
        camCustomize.Priority = 0;
        if (_canvasGroup != null)
        {
            _canvasGroup.DOFade(0, 0.3f).OnComplete(()=>gameObject.SetActive(false));
            GameManager.Instance.sideBar.ShowSidebar(true, true);
        }
    }
}
