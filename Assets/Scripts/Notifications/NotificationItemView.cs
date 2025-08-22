using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static NotificationsController;

public class NotificationItemView : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RewardItemUniversal attIcon;
    public TextMeshProUGUI title;

    public void Init(PendingAtt attChange) 
    {
        //attIcon.InitAttribute(attChange);
        title.text = attChange.att.attributeName;
    }
}
