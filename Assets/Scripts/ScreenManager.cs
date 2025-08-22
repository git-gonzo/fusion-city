using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager: MonoBehaviour
{
    public static ScreenManager Instance => instance;
    private static ScreenManager instance;

    public SideBarController sideBar;
    public Image canvasBackground;
    public Transform screenPositionIn;
    public Transform screenPositionOut;
    public Transform screenPositionCenter;

    public GameObject currentScreen;
    private MapManager mapManager;

    private void Awake()
    {
        instance = this;
    }

    public bool AnimateScreenIN(GameObject screen, Action OnScreenAnimEnd = null)
    {
        if (currentScreen != null && currentScreen != screen)
        {
            AnimateScreenOUT(currentScreen, false);
        }
        else if (currentScreen == screen)
        {
            return false;
        }
        if (sideBar != null)
        {
            sideBar.ShowSidebar(false, true);
        }
        canvasBackground.DOFade(1, 0.3f).OnComplete(() =>
        {
            if (mapManager != null)
            {
                mapManager.gameObject.SetActive(false);
            }
            OnScreenAnimEnd?.Invoke();
        });
        
        currentScreen = screen;
        screen.transform.DOKill();
        screen.transform.DOMove(screenPositionIn.position, 0);
        screen.SetActive(true);
        screen.transform.DOMove(screenPositionCenter.position, 0.5f).SetEase(Ease.OutExpo);
        return true;
    }

    public void AnimateScreenOUT(GameObject screen, bool showSideBar = true)
    {
        if (GameManager.Instance == null || !GameManager.Instance.MergeManager.IsMergeBoardActive)
        {
            canvasBackground.DOFade(0, 0.3f).OnStart(() => {
                if (mapManager != null)
                {
                    mapManager.gameObject.SetActive(true);
                }
            });
        }
        currentScreen = null;
        if (screen != null && screen.activeSelf)
        {
            screen.transform.DOMove(screenPositionOut.position, 0.5f).SetEase(Ease.OutExpo)
                .OnComplete(() => { screen.SetActive(false); });
        }
        if (GameManager.Instance == null)
        {
            return;
        }
        if (showSideBar
            && !GameManager.Instance.speedBoardManager.speedBoardController.gameObject.activeSelf
            && !GameManager.Instance.MergeManager.boardController.gameObject.activeSelf)
        {
            sideBar.ShowSidebar(true, true);
            GameManager.Instance.ShowMapLowerBar(GameManager.Instance.PlayerLevel > 1);
            GameManager.Instance.ShowMergeLowerBar(false);
            
        }
    }
}