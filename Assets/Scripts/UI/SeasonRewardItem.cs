using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonRewardItem : MonoBehaviour
{
    public RewardsContainer rewardsContainer;

    public void Init(List<RewardData> rewards)
    {
        rewardsContainer.ClearRewards();
        rewardsContainer.FillRewards(rewards, GameManager.Instance.topBar);
    }
}
