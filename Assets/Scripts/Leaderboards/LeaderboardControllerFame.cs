using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardControllerFame : MonoBehaviour
{
    public Transform Container;
    public GameObject listItemPrefab;
    public GameObject listItemPrefabTop;
    public GameObject listItemTimeLeft;
    
    public Button btnAllTime;
    public Button btnMonth;
    public Button btnShowRewards;
    public List<LeaderBoardItenFameView> items;
    public List<LeaderboardPlayer> listMonth;
    public List<LeaderboardPlayer> listAlltime;
    public Sprite spriteButtonOn;
    public Sprite spriteButtonOff;
    GameObject seasonTimeLeftObj;

    public string txtSeasonTimeLeft
    {
        get
        {
            var sec = Globals.Instance.seasonSecondsLeft % 60;
            var min = (Globals.Instance.seasonSecondsLeft / 60) % 60;
            var hor = (Globals.Instance.seasonSecondsLeft / 3600) % 24;
            var day = (Globals.Instance.seasonSecondsLeft / 86400);
            if(day > 0)
            {
                return $"Season ends in <color=yellow>{day}d {hor}h</color>";
            }
            return $"Season ends in <color=yellow>{hor}h {min}m</color>";
        }
    }

    public void Init()
    {
        items = new List<LeaderBoardItenFameView>();
        GameManager.RemoveChildren(Container.gameObject);
        var itemTimeLeft = Instantiate(listItemTimeLeft, Container).GetComponent<LeaderBoardItemSeasonTimeLeft>();
        itemTimeLeft.txtTimeLeft.text = txtSeasonTimeLeft;
        seasonTimeLeftObj = itemTimeLeft.gameObject;
        var top = 0;
        var list = btnAllTime.GetComponent<Image>().sprite == spriteButtonOn? listAlltime:listMonth;
        foreach (var def in list)
        {
            var item = Instantiate(top < 10? listItemPrefabTop : listItemPrefab, Container).GetComponent<LeaderBoardItenFameView>();
            item.Init(def);
            items.Add(item);
            top++;
        }
        btnAllTime.onClick.AddListener(ShowAllTime);
        btnMonth.onClick.AddListener(ShowMonth);
    }

    public void ShowAllTime()
    {
        ButtonOn(btnAllTime);
        if (items == null || items.Count == 0 || items.Count != listAlltime.Count)
        {
            Init();
        }
        else
        {
            seasonTimeLeftObj.SetActive(false);
            var seq = DOTween.Sequence();
            for (var i = 0; i<items.Count; i++)
            {
                var cg = items[i].GetComponent<CanvasGroup>();
                cg.DOFade(0, 0);
                seq.Insert(i * 0.05f, cg.DOFade(1, 0.4f));
                items[i].Init(listAlltime[i]);
                items[i].gameObject.SetActive(i < listAlltime.Count);
            }
        }
    }
    
    public void ShowMonth()
    {
        ButtonOn(btnMonth);
        if (items == null || items.Count == 0 || items.Count != listMonth.Count)
        {
            Init();
        }
        else
        {
            seasonTimeLeftObj.SetActive(true);
            var seq = DOTween.Sequence();
            for(var i = 0; i<items.Count; i++)
            {
                var cg = items[i].GetComponent<CanvasGroup>();
                cg.DOFade(0, 0);
                seq.Insert(i * 0.05f, cg.DOFade(1, 0.4f));
                items[i].gameObject.SetActive(i < listMonth.Count);
                if (i < listMonth.Count)
                {
                    items[i].Init(listMonth[i]);
                }
            }
        }
    }

    internal void Sort()
    {
        listMonth.Sort((a, b) => { return b.score.CompareTo(a.score); });
        for (var i = 0; i < listMonth.Count; i++)
        {
            listMonth[i].position = i + 1;
        }

        listAlltime.Sort((a, b) => { return b.score.CompareTo(a.score); });
        for (var i = 0; i < listAlltime.Count; i++)
        {
            listAlltime[i].position = i + 1;
        }
    }

    private void AllButtonsOff()
    {
        btnAllTime.GetComponent<Image>().sprite = spriteButtonOff;
        btnMonth.GetComponent<Image>().sprite = spriteButtonOff;
    }

    private void ButtonOn(Button btn)
    {
        AllButtonsOff();
        btn.GetComponent<Image>().sprite = spriteButtonOn;
        DOTween.Kill(btn.transform);
        btn.transform.localScale = new Vector3(1, 1, 1);
        btn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);
    }

    public void CloseLeaderboard()
    {
        ScreenManager.Instance.AnimateScreenOUT(gameObject, true);
    }
}
