using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class PopupAssistant : PopupBase
{
    [SerializeField] private Button btnTryFree;
    [SerializeField] private TextMeshProUGUI txtTitle;
    [SerializeField] private TextMeshProUGUI txtDescrip;
    [SerializeField] private TextMeshProUGUI txtDuration;
    [SerializeField] private LocalizedString locTitleHire;
    [SerializeField] private LocalizedString locDays;
    [SerializeField] private Image characterImage;
    [SerializeField] List<DurationTierController> tiers;

    AssistantConfig assistantConfig;

    public void Init(string assistantId)
    {
        assistantConfig = GameManager.Instance.gameConfig.GetAssistantConfig(assistantId);
        characterImage.sprite = assistantConfig.character.characterImage;
        txtTitle.text = locTitleHire.GetLocalizedString() + " " + assistantConfig.character.characterName;
        txtDescrip.text = assistantConfig.assistantDescrip.GetLocalizedString();
        CreateTiers();
        btnClose.onClick.AddListener(OnClose);
    }

    private void CreateTiers()
    {
        DisableTiers();
        var count = 0;
        foreach(var tier in assistantConfig.tiers)
        {
            var t = GetTierController(tier.tier);
            if (tier.tier == AssistantTier.Free && !GameManager.Instance.mergeModel.assistants.IsAssistantFirstTime(assistantConfig.assistantType))
            {
                continue;  
            }
            //btnTryFree.gameObject.SetActive(GameManager.Instance.mergeModel.assistants.IsAssistantFirstTime(assistantConfig.assistantType));
            t.gameObject.SetActive(true);
            t.Init(assistantConfig.assistantType, tier.durationDays, locDays, OnClose);
            count++;
            if (count > 3) break; //Show first 3 elements
        }
        
    }

    private void DisableTiers()
    {
        foreach (var tier in tiers)
        {
            tier.gameObject.SetActive(false);
        }
    }

    private DurationTierController GetTierController(AssistantTier assistantTier)
    {
        foreach (var tier in tiers)
        {
            if(tier.assistantTier == assistantTier) return tier;
        }
        return null;
    }
}

public enum AssistantTier
{
    Free,
    Tier1,
    Tier2,
    Tier3
}