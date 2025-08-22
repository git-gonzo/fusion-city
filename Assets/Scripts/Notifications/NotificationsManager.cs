using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationsManager
{
    public NotificationsController notificationController;

    public void AddAttributeChangeNotification(SO_Attribute attribute, float change)
    {
        notificationController.AddAttributeNotification(attribute, change);
    }
}
