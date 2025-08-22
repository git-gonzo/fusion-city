using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopResourcePack : MonoBehaviour
{
    public TextMeshProUGUI txtAmount;
    public TextMeshProUGUI txtPrice;

    public RewardType resource;
    public int amount;
    public RewardType currency;
    public int price;

    public Button button;
    public AudioSource onBuySound;

    public void Start()
    {
        if (currency == RewardType.Coins)
        {
            button.onClick.AddListener(ConfirmBuy);
        }
    }

    public void updatePriceState()
    {
        if (currency == RewardType.Gems)
        {
            txtPrice.color = price > PlayerData.gems ? Color.red : Color.white;
        }

    }

    public void ConfirmBuy()
    {
        if(currency == RewardType.Gems)
        {
            if(price > PlayerData.gems)
            {
                //Not enough gems
                return;
            }
            onBuySound.Play();
            GameManager.Instance.AddRewardToPlayer(new RewardData(currency, -price));
            var rewards = new List<RewardData>();
            rewards.Add(new RewardData(resource, amount));
            GameManager.Instance.PopupsManager.ShowPopupPurchase(rewards);
            GameManager.Instance.AddRewardToPlayer(new RewardData(resource, amount));
        }
        /*var rewards = new List<RewardData>();
        rewards.Add(new RewardData(resource, amount));
        GameManager.Instance.PopupsManager.ShowPopupPurchase(rewards);
        GameManager.Instance.AddRewardToPlayer(new RewardData(resource, amount));
        MyAnalytics.LogPurchase(amount);*/
    }
}
