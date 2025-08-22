using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsContainer : MonoBehaviour
{
    public List<RewardItemUniversal> rewardItems;
    public GameObject rewardPrefab;
    private TopBar _topbar;
    // Start is called before the first frame update
    Action _onCompleteRewards;

    public void ClearRewards()
    {
        GameManager.RemoveChildren(gameObject);  
        rewardItems.Clear();
    }

    public virtual void FillRewards(List<RewardData> rewards, TopBar topbar, bool clearFirst = true)
    {
        _topbar = topbar;
        if (clearFirst)
        {
            ClearRewards();
        }
        for (var i = 0; i < rewards.Count; i++)
        {
            var r = Instantiate(rewardPrefab, transform);
            rewardItems.Add(r.GetComponent<RewardItemUniversal>());
            r.GetComponent<RewardItemUniversal>().InitReward(rewards[i],topbar);
        }
    }
    public void InitScaleZero()
    {
        for (var i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].transform.DOScale(0,0);
        }
    }
    public void AnimateScaleDelay(float delay = 0.2f)
    {
        for (var i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].transform.DOScale(1,0.3f).SetEase(Ease.OutBack).SetDelay(delay * (i+1));
        }
    }


    public void ClaimRewards()
    {
        for (var i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].ApplyReward();
        }
    }

    public void ClaimRewardsBySteps(Action OnComplete)
    {
        _onCompleteRewards = OnComplete;
        StartCoroutine(ClaimSteps());
    }

    IEnumerator ClaimSteps()
    {
        for (var i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].ApplyReward();
            rewardItems[i].transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(()=>{ DestroyImmediate(rewardItems[i]); });
            yield return new WaitForSeconds(0.2f);
        }
        _onCompleteRewards?.Invoke();
    }

    public void BounceAll()
    {
        for (var i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].Bounce();
        }
    }
}
