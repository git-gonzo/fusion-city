using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Coffee.UIEffects;

public class Popup_BuildingLocked : MonoBehaviour
{
    public TextMeshProUGUI txtLevel;
    public TextMeshProUGUI txtBuildingName;
    public GameObject lockedContent;
    public GameObject unlockedContent;
    public UIShiny uIShiny;

    Sequence s;
    float _posY;
    // Start is called before the first frame update
    public void Init()
    {
        _posY = transform.position.y;
        transform.DOMoveY(_posY-350, 0);
        gameObject.SetActive(false);
    }

    public void ShowPopupLocked(int level)
    {
        gameObject.SetActive(true);
        lockedContent.SetActive(true);
        unlockedContent.SetActive(false);
        txtLevel.text = level.ToString();
        Show(1.5f);
    }
    public void ShowPopupUnLocked(SO_Building building)
    {
        uIShiny.Play();
        gameObject.SetActive(true);
        BuildingsManager.SetBuildNameToText(building, txtBuildingName, true);
        lockedContent.SetActive(false);
        unlockedContent.SetActive(true);
        Show(3f);
    }

    private void Show(float hideTime)
    {
        if (s != null) s.Kill();
        s = DOTween.Sequence();
        s.Append(transform.DOMoveY(_posY, 0.5f).SetEase(Ease.OutBack));
        s.Insert(hideTime, transform.DOMoveY(_posY - 350, 0.5f).SetEase(Ease.InBack).OnComplete(() => { gameObject.SetActive(false); }));
    }
}
