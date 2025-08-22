using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class PopupLeagueReward : PopupBase
{
    public AudioSource levelupSound;
    public GameObject frameTitle;
    public GameObject frameLevel;
    public GameObject buttonClaim;
    public GameObject rewardsBG;
    public TextMeshProUGUI textTitle;
    public TextMeshProUGUI textCongratulations;
    public TextMeshProUGUI textPosition;
    public TextMeshProUGUI textGetYourRewards;
    public RewardsContainer rewardsContainer;
    public GameConfigMerge config => GameConfigMerge.instance;

    int positionForView => _position + 1;

    private int _position;
    public void ShowRewardsForNextLevel(int position, Action OnClose)
    {
        levelupSound.Play();
        onCloseCallback = OnClose;
        _position = position;
        //Give Reward
        switch (position)
        {
            case 1: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards1, GameManager.Instance.topBar); break;
            case 2: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards2, GameManager.Instance.topBar); break;
            case 3: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards3, GameManager.Instance.topBar); break;
            case 4: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards4, GameManager.Instance.topBar); break;
            case 5: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards5, GameManager.Instance.topBar); break;
            case 6: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards6, GameManager.Instance.topBar); break;
            case 7: 
            case 8: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards7_8, GameManager.Instance.topBar); break;
            case 9: 
            case 10: rewardsContainer.FillRewards(config.seasonRewardsConfig.rewards9_10, GameManager.Instance.topBar); break;
            default:
                break;
        }
        
        //Initialize state
        textTitle.enabled = false;
        textPosition.enabled = false;
        textGetYourRewards.enabled = false;
        textCongratulations.enabled = false;
        GetComponent<CanvasGroup>().DOFade(0, 0);
        gameObject.transform.DOScale(0.8f, 0);
        frameTitle.transform.DOScale(0.8f, 0);
        frameTitle.GetComponent<Image>().DOFade(0, 0);
        textPosition.text = _position.ToString();
        textPosition.transform.DOScale(1.5f, 0);

        rewardsContainer.InitScaleZero();
        textGetYourRewards.transform.DOScale(0, 0);
        buttonClaim.transform.DOScale(0, 0);
        buttonClaim.gameObject.SetActive(true);
        var FinalScaleBGRewards = rewardsBG.transform.localScale;
        var StartScaleBGRewards = new Vector3(0, 1, 1);
        rewardsBG.transform.DOScale(StartScaleBGRewards, 0);
        

        var seq = DOTween.Sequence();
        seq.Insert(0f, GetComponent<CanvasGroup>().DOFade(1, 0.3f));
        seq.Insert(0f, gameObject.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack).OnComplete(() => {
            UIUtils.AnimateText(textTitle, this, null, 0.01f);
        }));
        seq.Insert(0f, frameLevel.transform.DOScale(1.5f, 0.8f).SetEase(Ease.OutSine).OnComplete(() => {
            textPosition.enabled = true;
            textPosition.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce);
            frameLevel.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBounce).OnComplete(() => {
                UIUtils.AnimateText(textCongratulations, this, null, 0.01f);
            });
        }));
        seq.Insert(0.3f, frameTitle.GetComponent<Image>().DOFade(1, 0.1f));
        seq.Insert(0.3f, frameTitle.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        seq.Insert(1.15f, rewardsBG.transform.DOScale(FinalScaleBGRewards, 0.6f).SetEase(Ease.InOutExpo).OnStart(()=>rewardsContainer.AnimateScaleDelay()));
        //seq.Insert(1.5f, rewardsContainer.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack));
        seq.Insert(2f, textGetYourRewards.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack).OnComplete(() => {
            UIUtils.AnimateText(textGetYourRewards, this, null, 0.01f);
        })); 
        seq.Insert(2.2f, buttonClaim.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));/* */

        seq.Play();
    }

    public void Claim()
    {
        rewardsContainer.ClaimRewards();
        GameManager.Instance.soundNormalButton.Play();
        GameManager.Instance.soundPurchaseButton.Play();

        //GameManager.Instance.AddRewardToPlayer(rewards[Mathf.Min(rewards.Count - 1, _position)], true, false);
        //UIUtils.FlyingParticles(rewards[Mathf.Min(rewards.Count - 1, _position)].rewardType, buttonClaim.transform.position, 10,()=> { onCloseCallback?.Invoke(); });
        buttonClaim.gameObject.SetActive(false);
        onCloseCallback?.Invoke();
    }
}
