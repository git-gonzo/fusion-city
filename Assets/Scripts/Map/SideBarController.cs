using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;

public class SideBarController : MonoBehaviour
{
    public SideBarButton btnShop;
    public SideBarButton btnLeaderboard;
    public SideBarButton btnDailyTask;
    public SideBarButton btnFreeRewards;
    public GameObject btnDailyTaskCompleted;
    public GameObject welcomePack;
    public SideBarButton genericOffer;
    public SideBarButton btnDailyBonus;
    public AssistantButton btnAssistant;
    public TextMeshProUGUI textWPTimeLeft;
    public TextMeshProUGUI textGenericOfferTimeLeft;

    private DateTime _WPEndTime;
    private DateTime _genericOfferEndTime;
    private DailyTaskManager dailyTaskManager => GameManager.Instance.dailyTaskManager;

#if UNITY_EDITOR
    public bool WelcomePackEnable = false;
    public void ShowWelcomePackClick()
    {
        PlayerPrefs.SetInt("WP_Seen", welcomePack?0:2);
    }
#endif
    Vector3 sideBarInitPos;
    public void ShowSidebar(bool value, bool animated)
    {
        if (value) gameObject.SetActive(true);
        if (sideBarInitPos == Vector3.zero) sideBarInitPos = transform.position;
        transform.DOMove(
            value ? sideBarInitPos : sideBarInitPos + Vector3.right * 400,
            animated ? 0.5f : 0f).OnComplete(() => gameObject.SetActive(value));
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (GameManager.Instance.PlayerLevel < 2 || GameManager.Instance.tutorialManager.IsTutorialRunning)
            {
                btnShop.gameObject.SetActive(false);
                btnFreeRewards.gameObject.SetActive(false);
                btnDailyBonus.gameObject.SetActive(false);
                btnDailyTask.gameObject.SetActive(false);
                return;
            }
            if (dailyTaskManager.canBeShown)
            {
                var pendingClaimCount = 0;
                for (int i = 0; i < dailyTaskManager.model.dailyTasks.Count; i++)
                {
                    if (dailyTaskManager.model.dailyTasks[i].completed && !dailyTaskManager.model.dailyTasks[i].claimed) pendingClaimCount++;
                }
                btnDailyTaskCompleted.SetActive(pendingClaimCount > 0);
                btnDailyTask.bubleContainer.SetActive(pendingClaimCount > 0);
                btnDailyTask.textBubleCounter.text = pendingClaimCount.ToString();
            }
        }
    }

    public void LazyUpdate()
    {
        
        if (gameObject.activeSelf) 
        {
            if (GameManager.Instance.PlayerLevel >= GameConfigMerge.instance.welcomePackShowOnLevel)
            {
                //GameManager.Log("try to show welcome pack");
                TryToShowWelcomePack();
            }
            TryToShowOffer();
            
            return;
        }
        welcomePack.SetActive(false);
    }

    public void SuperLazyUpdate()
    {
        if (gameObject.activeSelf && GameManager.Instance.PlayerLevel > 1 && !GameManager.Instance.tutorialManager.IsTutorialRunning) 
        {
            btnShop.gameObject.SetActive(true);
            btnDailyBonus.Shine(GameManager.Instance.DailyBonusManager.CanBeShown);
            btnDailyBonus.gameObject.SetActive(true);
            btnDailyTask.gameObject.SetActive(dailyTaskManager.canBeShown && !dailyTaskManager.allDailyClaimed);
            var _lastVideoWatched = UIUtils.GetTimeStampByKey("LastVideoWatched") - System.DateTime.Now;
            btnFreeRewards.gameObject.SetActive(_lastVideoWatched.TotalSeconds <= 0);
        }
        TryToShowAssistant();
    }

    public void TryToShowOffer()
    {
        if(PlayerPrefs.GetInt("genericOffer") == 1)
        {
            genericOffer.gameObject.SetActive(true);
            var secondsLeft = GetGenericOfferTimeLeft();
            if (secondsLeft <= 0)
            {
                PlayerPrefs.SetInt("genericOffer", 0);
                genericOffer.gameObject.SetActive(false);
                return;
            }
            textGenericOfferTimeLeft.text = UIUtils.FormatTime(secondsLeft);
        }
        else
        {
            genericOffer.gameObject.SetActive(false);
        }
    }

    private void TryToShowWelcomePack()
    {
        var wpState = PlayerPrefs.GetInt("WP_Seen");
        if (wpState == 0)
        {
            //GameManager.Log("empieza");
            UIUtils.SaveTimeStamp("WP_end", DateTime.Now.AddHours(GameConfigMerge.instance.welcomePackHoursDuration));
            PlayerPrefs.SetInt("WP_Seen", 1);
        }
        else if (wpState == 1)
        {
            //GameManager.Log("visto?");
            var secondsLeft = GetWPTimeLeft();
            if (secondsLeft <= 0)
            {
                PlayerPrefs.SetInt("WP_Seen", 2);
            }
            textWPTimeLeft.text = UIUtils.FormatTime(secondsLeft);
        }
        else
        {
            //GameManager.Log("fuera");
            welcomePack.SetActive(false);
            return;
        }
        welcomePack.SetActive(true);
        return;
    }

    public void TryToShowAssistant()
    {
        //Check Assistant Button
        if (GameManager.Instance.AssistantsManager.TryToShowAssistantOffer(out var assistantConfig))
        {
            btnAssistant.gameObject.SetActive(true);
            btnAssistant.Init(assistantConfig.assistantType);
        } else
        {
            btnAssistant.gameObject.SetActive(false);
        };
    }

    public double GetWPTimeLeft()
    {
        if(_WPEndTime == null || _WPEndTime < DateTime.Now.AddMonths(-2))
        {
            _WPEndTime = UIUtils.GetTimeStampByKey("WP_end");
        }
        return (_WPEndTime - DateTime.Now).TotalSeconds;
    }
    public double GetGenericOfferTimeLeft()
    {
        if(_genericOfferEndTime == null || _genericOfferEndTime < DateTime.Now.AddMonths(-2))
        {
            _genericOfferEndTime = UIUtils.GetTimeStampByKey("genericOffer_end");
        }
        return (_genericOfferEndTime - DateTime.Now).TotalSeconds;
    }
}
