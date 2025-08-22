using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(Button))]
public class ButtonWithPrice : MonoBehaviour
{
    public TextMeshProUGUI textSufix;
    public TextMeshProUGUI textPrice;
    public GameObject iconCurrency;
    public Color freeTextColor = Color.yellow;
    public Button button => GetComponent<Button>();
    public RewardData cost;
    public void SetPrice(int amount, RewardType currency)
    {
        SetAmount(amount, currency);
    }
    public void SetPrice(int amount, string sufix, RewardType currency)
    {
        textSufix.text = sufix;
        SetAmount(amount,currency);
    }

    private void SetAmount(int amount, RewardType currency)
    {
        cost = new RewardData(currency, amount);
        if (amount > 0)
        {
            textPrice.text = amount.ToString();
            textPrice.color = (amount > (currency == RewardType.Gems?
                PlayerData.gems: PlayerData.coins)) ? Color.red : Color.white;
        }
        else
        {
            textPrice.text = "Free";
            textPrice.color = freeTextColor;
        }
        iconCurrency.SetActive(amount > 0);
        
    }
}
