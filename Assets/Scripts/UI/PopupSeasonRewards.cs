using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupSeasonRewards : PopupBase
{
    public List<SeasonRewardItem> seasonRewardItems;
    public GameConfigMerge config => GameManager.Instance.gameConfig;

    internal void Init()
    {
        seasonRewardItems[0].Init(config.seasonRewardsConfig.rewards1);
        seasonRewardItems[1].Init(config.seasonRewardsConfig.rewards2);
        seasonRewardItems[2].Init(config.seasonRewardsConfig.rewards3);
        seasonRewardItems[3].Init(config.seasonRewardsConfig.rewards4);
        seasonRewardItems[4].Init(config.seasonRewardsConfig.rewards5);
        seasonRewardItems[5].Init(config.seasonRewardsConfig.rewards6);
        seasonRewardItems[6].Init(config.seasonRewardsConfig.rewards7_8);
        seasonRewardItems[7].Init(config.seasonRewardsConfig.rewards9_10);
    }
}
