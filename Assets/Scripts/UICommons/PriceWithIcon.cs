using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PriceWithIcon : MonoBehaviour
{
    public TextMeshProUGUI txtPrice;
    public GameObject iconCoins;
    public GameObject iconGems;
    public GameObject iconXP;
    public GameObject iconFame;
    public GameObject iconEnergy;
    public RectTransform layoutGroup;
    public void Init(RewardData cost)
    {
        txtPrice.text = cost.amount == 0 ? "FREE" : UIUtils.FormatNumber(cost.amount);
        SetIcon(cost.rewardType, cost.amount == 0);
    }

    private void SetIcon(RewardType currency, bool hideIcon)
    {
        iconCoins.SetActive(currency == RewardType.Coins && !hideIcon);
        iconGems.SetActive(currency == RewardType.Gems && !hideIcon);
        if (iconXP != null) iconXP.SetActive(currency == RewardType.XP && !hideIcon);
        if (iconFame != null) iconFame.SetActive(currency == RewardType.FamePoints && !hideIcon);
        if (iconEnergy != null) iconEnergy.SetActive(currency == RewardType.Energy && !hideIcon);
        try
        {//Crashed once, just to be safe
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
        }
        catch (Exception e)
        {
            GameManager.Log(e.Message);
        }
    }
}
