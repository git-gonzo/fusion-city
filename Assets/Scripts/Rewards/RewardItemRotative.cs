using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class RewardItemRotative : MonoBehaviour
{
    public GameObject glow;
    public GameObject specialBG;
    public GameObject extraInfo;
    public GameObject rewardIcon;
    public TextMeshProUGUI amount;
    public SO_RewardSprites rewardsImages;
    public GameObject fxGold;
    public GameObject fxPurple;

    private List<RewardData> _rewards;
    private Sequence _seq;
    private Image rewardImage;

    private int currentRewardIndex;

    public void Init(List<RewardData> rewards)
    {
        fxGold.SetActive(false);
        fxPurple.SetActive(false);
        _seq = DOTween.Sequence();
        _rewards = rewards;
        rewardImage = rewardIcon.GetComponent<Image>();
        if (rewards != null && rewards.Count > 0)
        {
            rewardImage.sprite = GetSpriteByType(_rewards[0].rewardType);
            amount.text = _rewards[0].amount.ToString();
            fxGold.SetActive(_rewards[0].rewardType == RewardType.Coins);
            fxPurple.SetActive(_rewards[0].rewardType == RewardType.Gems);
        }
    }

    public void StartAnim()
    {
        /*for (var i = 0; i < rewards.Count; i++)
        {
            _seq.Append(image.)

        }*/
    }

    private Sprite GetSpriteByType(RewardType type)
    {
        switch (type)
        {
            case RewardType.Coins:
                return rewardsImages.goldImage;
            case RewardType.Gems:
                return rewardsImages.gemImage;
            case RewardType.XP:
                return rewardsImages.xpImage;
            case RewardType.FamePoints:
                return rewardsImages.fameImage;
        }
        return null;
    }
}
