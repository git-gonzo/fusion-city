using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class ShopSpecialOffer : MonoBehaviour
{
    public List<RewardData> rewards;
    public TextMeshProUGUI txtTimeLeft;
    public RewardsContainer rewardsContainer;
    [SerializeField] bool _hasGenerator;

    private void Update()
    {
        txtTimeLeft.text = UIUtils.FormatTime(GameManager.Instance.sideBar.GetGenericOfferTimeLeft());
    }

    public void AnimIn()
    {
        //Init rewards
        rewardsContainer.FillRewards(rewards, GameManager.Instance.topBar);
        Sequence seq = DOTween.Sequence();
        for (var i = 0; i < rewardsContainer.rewardItems.Count; i++)
        {
            seq.Insert(0.1f * i + 0.1f, rewardsContainer.rewardItems[i].transform.DOScale(0, 0.4f).From().SetEase(Ease.OutBack));
        }
    }

    public void OnBuySuccess()
    {
        //rewardsContainer.ClaimRewardsBySteps(OnClose);
    }
}
