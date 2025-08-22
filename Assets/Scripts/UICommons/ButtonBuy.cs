using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(Button))]
public class ButtonBuy : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public GameObject iconCoins;
    public GameObject iconGems;
    public GameObject iconXP;
    public GameObject iconFame;
    public GameObject iconEnergy;
    public RectTransform layoutGroup;
    [SerializeField] Color _notEnoughColor = new Color(1, 0.37f, 0.37f);

    public Action OnBuyAction;

    Button _button;
    public Button button => _button ??= GetComponent<Button>();

    public void Init(int amount, RewardType currency)
    { 
        priceText.text = amount==0?"FREE":amount.ToString();
        SetIcon(currency,amount==0);
        GetComponent<Button>()?.onClick.AddListener(OnBuy);
    }

    public void Init(RewardData price, bool setRedIfNotEnough = false)
    {
        priceText.text = price.amount == 0 ? "FREE" : price.amount.ToString(); 
        SetIcon(price.rewardType, price.amount == 0);
        GetComponent<Button>()?.onClick.AddListener(OnBuy);
        if (setRedIfNotEnough && !GameManager.Instance.HasEnoughCurrency(price))
        {
            priceText.color = _notEnoughColor;
        }
        else
        {
            priceText.color = Color.white;
        }
    }

    private void SetIcon(RewardType currency, bool hideIcon)
    {
        iconCoins.SetActive(currency == RewardType.Coins && !hideIcon);
        iconGems.SetActive(currency == RewardType.Gems && !hideIcon);
        if(iconXP != null) iconXP.SetActive(currency == RewardType.XP && !hideIcon);
        if (iconFame != null) iconFame.SetActive(currency == RewardType.FamePoints && !hideIcon);
        if (iconEnergy != null) iconEnergy.SetActive(currency == RewardType.Energy && !hideIcon);
        try {//Crashed once, just to be safe
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
        }
        catch (Exception e){
            GameManager.Log(e.Message);
        }
    }

    private void OnBuy()
    {
        OnBuyAction?.Invoke();
    }
}
