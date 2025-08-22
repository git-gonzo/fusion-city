using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;

public class LevelUpManager : MonoBehaviour
{
    public int rewardLevel;
    public GameObject frameTitle;
    public GameObject frameLevel;
    public GameObject rewardCoins;
    public GameObject rewardSkill;
    public GameObject rewardGems;
    public GameObject buttonClaim;
    public GameObject rewardsBG;
    public Transform avatarsContainer;
    public TextMeshProUGUI textTitle;
    public TextMeshProUGUI textAvatars;
    public TextMeshProUGUI textCongratulations;
    public TextMeshProUGUI textLevel;
    public TextMeshProUGUI textCoins;
    public TextMeshProUGUI textGems;
    //public TextMeshProUGUI textSkillPoints;
    public AudioSource levelupSound;
    public PopupAvatarItem avatarPrefab;

    public List<LevelUpReward> RewardsForLevels;
    public Action OnClaimRewards;
    public Action OnLevelUpComplete;

    public void ShowRewardsForNextLevel(int curLevel)
    {
        levelupSound.Play();
        rewardLevel = Math.Min(curLevel, RewardsForLevels.Count-1);
        //Initialize state
        textTitle.enabled = false;
        textLevel.enabled = false ;
        textCongratulations.enabled = false;
        textAvatars.enabled = false;
        GetComponent<CanvasGroup>().DOFade(0, 0);
        gameObject.transform.DOScale(0.8f, 0);
        frameTitle.transform.DOScale(0.8f,0);
        frameTitle.GetComponent<Image>().DOFade(0, 0);
        textLevel.text = curLevel.ToString();
        textLevel.transform.DOScale(1.5f, 0);
        rewardCoins.transform.DOScale(0, 0);
        rewardGems.transform.DOScale(0, 0);
        rewardSkill.transform.DOScale(0, 0);
        buttonClaim.transform.DOScale(0, 0);
        buttonClaim.gameObject.SetActive(true);
        var FinalScaleBGRewards = rewardsBG.transform.localScale;
        var StartScaleBGRewards = new Vector3(0,1,1);
        rewardsBG.transform.DOScale(StartScaleBGRewards, 0);
        textCoins.text = RewardsForLevels[rewardLevel].coinsValue.ToString();
        textGems.text = RewardsForLevels[rewardLevel].gemsValue.ToString();
        //textSkillPoints.text = RewardsForLevels[rewardLevel].skillValue.ToString();
        GameManager.RemoveChildren(avatarsContainer.gameObject);
        
        var hasAvatars = GameConfigMerge.instance.Avatars.Any(c => c.unlockLevel == curLevel);
        avatarsContainer.gameObject.SetActive(hasAvatars);
        textAvatars.gameObject.SetActive(hasAvatars);

        //Animation
        var seq = DOTween.Sequence();
        seq.Insert(0f, GetComponent<CanvasGroup>().DOFade(1, 0.3f));
        seq.Insert(0f, gameObject.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack).OnComplete(() => {
            UIUtils.AnimateText(textTitle, this, null, 0.01f);
        }));
        seq.Insert(0f, frameLevel.transform.DOScale(1.5f, 0.8f).SetEase(Ease.OutSine).OnComplete(()=> {
            textLevel.enabled = true;
            textLevel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBounce);
            frameLevel.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBounce)
            .OnComplete(() => { UIUtils.AnimateText(textCongratulations, this, null, 0.01f);});  
        }));
        seq.Insert(0.3f, frameTitle.GetComponent<Image>().DOFade(1, 0.1f));
        seq.Insert(0.3f, frameTitle.transform.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        //Avatars
        var delay = 0f;
        if (hasAvatars)
        {
            seq.Insert(0.7f, textAvatars.DOFade(1, 0)
                .OnStart(() => { UIUtils.AnimateText(textAvatars, this, null, 0.01f); }));
            foreach (var caracter in GameConfigMerge.instance.Avatars.Where(c => c.unlockLevel == curLevel))
            {
                var cha = Instantiate(avatarPrefab, avatarsContainer);
                cha.Init(caracter, null, true);
                seq.Insert(1.2f + delay, cha.transform.DOScale(0, 0.4f).From().SetEase(Ease.OutBack));
                delay += 0.3f;
            }
        }
        seq.Insert(1.15f, rewardsBG.transform.DOScale(FinalScaleBGRewards, 0.6f).SetEase(Ease.InOutExpo));
        seq.Insert(1.5f, rewardCoins.transform.DOScale(hasAvatars ? 0.75f : 0.9f, 0.5f).SetEase(Ease.OutBack));
        seq.Insert(1.6f, rewardSkill.transform.DOScale(hasAvatars ? 0.75f : 0.9f, 0.5f).SetEase(Ease.OutBack));
        seq.Insert(1.7f, rewardGems.transform.DOScale(hasAvatars ? 0.75f : 0.9f, 0.5f).SetEase(Ease.OutBack));
        seq.Insert(2.2f, buttonClaim.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

        seq.Play();
    }

    public void Claim()
    {
        GameManager.Instance.soundNormalButton.Play();
        GameManager.Instance.soundPurchaseButton.Play();
        //if (GameManager.Instance.skillsManager)
        //    GameManager.Instance.skillsManager.availablePoints+= RewardsForLevels[rewardLevel].skillValue;
        var rCoins = new RewardData(RewardType.Coins, RewardsForLevels[rewardLevel].coinsValue);
        var rGems = new RewardData(RewardType.Gems, RewardsForLevels[rewardLevel].gemsValue);
        GameManager.Instance.AddRewardToPlayer(rCoins,true,true);
        GameManager.Instance.AddRewardToPlayer(rGems,true,true);
        GameManager.Instance.mergeModel.AddGift(Assets.Scripts.MergeBoard.PieceType.CommonChest, 0);
        UIUtils.FlyingParticles(rCoins.rewardType, buttonClaim.transform.position, 10, EndGivingRewards);
        UIUtils.FlyingParticles(rGems.rewardType, buttonClaim.transform.position, 10, null);
        buttonClaim.gameObject.SetActive(false);
    }
    void EndGivingRewards()
    {
        PlayerPrefs.SetInt("levelupClosed", 1);
        gameObject.SetActive(false);
        OnLevelUpComplete?.Invoke();
    }
}


[Serializable]
public class LevelUpReward
{
    public int coinsValue;
    public int gemsValue;
    public int skillValue;
    
}


