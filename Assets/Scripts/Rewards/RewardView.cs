using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardView : MonoBehaviour
{

    public RewardType rewardType;
    public GameObject iconXP;
    public GameObject iconCoins;
    public GameObject iconGems;
    public GameObject iconFamePoints;
    public GameObject iconTime;
    public TextMeshProUGUI textAmount;
    public Transform iconContainer;

    int amount = 0;
    public int Amount { set
        {
            amount = value;
            textAmount.text = amount.ToString();
        } }

    // Start is called before the first frame update
    void Start()
    {
        //ActivateAllIcons();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FillReward(RewardData data)
    {
        rewardType = data.rewardType;
        Amount = data.amount;
        ActivateAllIcons();
    }

    private void ActivateAllIcons()
    {
        switch (rewardType)
        {
            case RewardType.XP:
                Instantiate(iconXP, iconContainer); break;
            case RewardType.Coins:
                Instantiate(iconCoins, iconContainer); break;
            case RewardType.Gems:
                Instantiate(iconGems, iconContainer); break;
            case RewardType.FamePoints:
                Instantiate(iconFamePoints, iconContainer); break;
        }
    }
}
