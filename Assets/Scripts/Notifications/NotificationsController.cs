using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NotificationsController : MonoBehaviour
{
    public class PendingAtt
    {
        public SO_Attribute att;
        public float change;
        public PendingAtt(SO_Attribute att, float change)
        {
            this.att = att;
            this.change = change;
        }
    }

    public Transform notificationsContainer;
    public NotificationItemView notificationItemView;
    public List<NotificationItemView> notifications;
    public float notificationsHeightOffset = 20f;

    private bool _isAnimatingIn;
    private float _notificationSize;
    private List<PendingAtt> attsPending;

    public void AddAttributeNotification(SO_Attribute attribute, float change)
    {
        if (attsPending == null)
        {
            attsPending = new List<PendingAtt>();
        }
        attsPending.Add(new PendingAtt(attribute, change));
        //Debug.Log("Adding att notif, pendig:" + attsPending.Count);
        StartCoroutine(TryToShowNotification());
    }

    IEnumerator TryToShowNotification()
    {
        while (attsPending.Count > 0)
        {
            while (_isAnimatingIn)
            {
                yield return _isAnimatingIn;
            }
            if (attsPending.Count == 0) yield break;
            //Debug.Log("Creating notification, count " + attsPending.Count);
            var item = Instantiate(notificationItemView, notificationsContainer);
            if (_notificationSize == 0) _notificationSize = item.GetComponent<RectTransform>().sizeDelta.x;
            item.transform.localPosition = new Vector3(_notificationSize, 0, 0);
            //item.Init(attsPending[0]);
            attsPending.Remove(attsPending[0]);
            _isAnimatingIn = true;
            notifications.Add(item);

            item.transform.DOLocalMoveX(0, 0.5f).OnComplete(() => _isAnimatingIn = false);

            RepositionNotifications();
            StartCoroutine(AnimNotificationOut(item));
            yield break;
        }
    }


    IEnumerator AnimNotificationOut(NotificationItemView item)
    {
        yield return new WaitForSecondsRealtime(2);
        item.canvasGroup.DOFade(0, 1)
            .OnComplete(() => 
            {
                notifications.Remove(item);
                Destroy(item.gameObject);
            });
    }

    private void RepositionNotifications()
    {
        for(var i= notifications.Count-1; i>=0; i--)
        {
            //notifications[i].transform.DOKill();
            notifications[i].transform.DOLocalMoveY((notifications.Count - i -1) * -notificationsHeightOffset, 0.5f);
        }
    }
}
