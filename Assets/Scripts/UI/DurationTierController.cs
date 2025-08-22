using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Purchasing;
using System;
using Unity.Services.Analytics;

public class DurationTierController : MonoBehaviour
{
    public AssistantTier assistantTier;
    [SerializeField] private TextMeshProUGUI txtDuration;
    [SerializeField] private CodelessIAPButton buyButton;
    AssistantType _assistantType;
    int _durationDays;
    Action _onClose;

    public void Init(AssistantType assistantType, int durationDays, LocalizedString locDays, Action onClose)
    {
        _assistantType = assistantType;
        _durationDays = durationDays;
        txtDuration.text = $"{durationDays} {locDays.GetLocalizedString()}";
        _onClose = onClose;
    }

    public void OnBuySuccess(int tier)
    {
       
        if(tier == (int)assistantTier)
        {
            TrackingManager.TrackPurchase($"{_assistantType}_{_durationDays}", "AssistantShop");
            GameManager.Instance.mergeModel.AddAssistant(_assistantType, _durationDays);
            //TODO: Some rewarding animation
            _onClose?.Invoke();
        }
    }
}
