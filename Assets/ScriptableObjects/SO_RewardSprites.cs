using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new RewardSprites", menuName = "Fame/RewardSprites")]
public class SO_RewardSprites : ScriptableObject
{
    public Sprite goldImage;
    public Sprite gemImage;
    public Sprite fameImage;
    public Sprite xpImage;

    public Sprite GetSpriteByType(RewardType type)
    {
        switch (type)
        {
            case RewardType.Coins:
                return goldImage;
            case RewardType.Gems:
                return gemImage;
            case RewardType.XP:
                return xpImage;
            case RewardType.FamePoints:
                return fameImage;
        }
        return null;
    }
}
