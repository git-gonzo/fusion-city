using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    public TextMeshProUGUI txtCoins;
    public TextMeshProUGUI txtGems;
    public TextMeshProUGUI textPlayerLevel;
    public TextMeshProUGUI textPlayerName;
    public TextMeshProUGUI txtFame;
    public TextMeshProUGUI txtPosition;
    public TextMeshProUGUI txtPositionSidebar;
    public TextMeshProUGUI txtSeasonEnd;
    public GameObject txtSeasonEndContainer;
    public GameObject seasonEndClaim;
    public Image iconCharacter;

    public GameObject FameContainer;
    public LeaderboardManager leaderboardManager;

    private Vector3 FameContainerPos;

    private async void Start()
    {
        FameContainerPos = FameContainer.transform.position;
        HideFameContainer();
        await Globals.IsSignedIn();
        textPlayerName.text = PlayerPrefs.GetString("playerName");
        await Globals.IsPlayerDataLoaded();
        SetAvatar();
        UpdatePlayerEconomy();
    }

    public void HideFameContainer()
    {
        FameContainer.transform.DOKill();
        FameContainer.transform.DOMoveY(FameContainerPos.y + 400, 0);
    }
    public void ShowFameContainer()
    {
        FameContainer.SetActive(true);
        FameContainer.transform.DOMoveY(FameContainerPos.y, 0.4f).SetEase(Ease.OutBack).SetDelay(0.5f);
    }

    public void UpdatePlayerEconomy()
    {
        txtCoins.text = UIUtils.FormatNumber(PlayerData.coins);
        txtGems.text = PlayerData.gems.ToString();
    }

    public void UpdateFamePoints()
    {
        if(FameContainer.transform.position.y > FameContainerPos.y)
        {
            ShowFameContainer();
        }
        if (leaderboardManager.GetPlayerSeason != null)
        {
            txtPosition.text = leaderboardManager.GetPlayerSeason.position.ToString();
            if (txtPositionSidebar != null) txtPositionSidebar.text = leaderboardManager.GetPlayerSeason.position.ToString();
            PlayerData.famePoints = leaderboardManager.GetPlayerSeason.score;
            txtFame.text = PlayerData.famePoints.ToString();
        }
        else
        {
            txtPosition.text = "-";
            txtFame.text = "-";
        }
    }

    public void AnimateCoins(int amount, bool withDelay = false)
    {
        GameManager.AnimateFormatedNumber(txtCoins, PlayerData.coins, amount, withDelay);
        PlayerData.coins += amount;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.dailyTaskManager.OnWinCoins(amount);
        }
    }
    public void AnimateGems(int amount, bool withDelay = false)
    {
        GameManager.AnimateFormatedNumber(txtGems, PlayerData.gems, amount, withDelay);
        PlayerData.gems += amount;
    }
    public void AnimateFame(int amount, bool withDelay = false)
    {
        leaderboardManager.SendScore(amount);
        GameManager.AnimateFormatedNumber(txtFame, PlayerData.famePoints, amount, withDelay);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.dailyTaskManager.OnWinFame(amount);
        }
    }

    internal void ShowSeasonEndClaim(bool value = true)
    {
        if(value) HideFameContainer();
        seasonEndClaim.SetActive(value);
    }

    public void SetAvatar()
    {
        iconCharacter.sprite = GetPlayerSprite();
    }

    public Sprite GetPlayerSprite()
    {
        if (PlayerData.characterIndex == 0) return GameConfigMerge.instance.defaultAvatar;
        return GameConfigMerge.instance.GetCharacter(PlayerData.characterIndex).sprite;
    }

    public bool TryToSpendCoins(int amount)
    {
        if (PlayerData.coins < amount)
        {
            return false;
        }
        //PlayerData.coins -= amount;
        AnimateCoins(-amount);
        return true;
    }
    public bool TryToSpendGems(int amount)
    {
        if (PlayerData.gems < amount)
        {
            return false;
        }
        AnimateGems(-amount);
        return true;
    }
}
