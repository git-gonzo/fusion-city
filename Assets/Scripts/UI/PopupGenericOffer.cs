using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
public class PopupGenericOffer : PopupBase
{
    public List<RewardData> rewards;
    public TextMeshProUGUI txtTimeLeft;
    public RewardsContainer rewardsContainer;
    [SerializeField] bool _hasGenerator;

    private void Update()
    {
        txtTimeLeft.text = UIUtils.FormatTime(GameManager.Instance.sideBar.GetGenericOfferTimeLeft());
    }

    public override void Show()
    {
        base.Show();
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
        if(_hasGenerator)
            PlayerPrefs.SetInt("SpecialOfferGenerator", 2);
        else
            PlayerPrefs.SetInt("genericOffer", 0);
        GameManager.Instance.ShowMapLowerBar(true);
        rewardsContainer.ClaimRewardsBySteps(OnClose);
    }
}
