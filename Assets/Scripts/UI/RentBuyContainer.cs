using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RentBuyContainer : MonoBehaviour
{
    public enum BuildingBuyType
    {
        Buy,
        Rent
    }

    public TextMeshProUGUI textAvailability;
    public TextMeshProUGUI textTitle;
    public TextMeshProUGUI textPrice;
    public TextMeshProUGUI textButton;
    public Button btnAccept;

    private int _price;
    private RewardType _currency;

    public void Init(BuildingBuyType buyType, bool isAvailable, int price, RewardType currency)
    {
        _price = price;
        _currency = currency;
        textTitle.text = buyType == BuildingBuyType.Buy ? "FOR BUYING":"FOR RENTING";
        textButton.text = buyType == BuildingBuyType.Buy ? "BUY":"RENT";
        textAvailability.text = isAvailable ? "<color=green>AVAILABLE" : "<color=grey>NOT AVAILABLE";
        textPrice.text = $"<color=yellow>{_price.ToString()}</color> {(buyType == BuildingBuyType.Buy ?"":"/hr")}";
    }
}
