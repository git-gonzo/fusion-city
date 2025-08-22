using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.MergeBoard;
using System.Collections.Generic;
using System;

public class AssistantButton : MonoBehaviour
{
    [SerializeField] Button btnOpenAssistantPopup;
    [SerializeField] Image assistantPortrait;
    public TextMeshProUGUI assistantName;

    List<AssistantConfig> assistantsConfig => GameManager.Instance.gameConfig.assistantsConfig;
    AssistantConfig _currentAssistantConfig;

    public void Init(AssistantType assistantType)
    {
        assistantPortrait.sprite = GetAssistantPortrait(assistantType);
        btnOpenAssistantPopup.onClick.RemoveAllListeners();
        btnOpenAssistantPopup.onClick.AddListener(OpenPopup);
    }

    private void OpenPopup()
    {
        GameManager.Instance.PopupsManager.ShowPopupAssistant(_currentAssistantConfig.id);
    }

    private Sprite GetAssistantPortrait(AssistantType assistantType)
    {
        foreach(var assistant in assistantsConfig)
        {
            if(assistant.assistantType == assistantType)
            {
                _currentAssistantConfig = assistant;
                return assistant.character.characterPortrait;
            }
        }
        return null;
    }
}
