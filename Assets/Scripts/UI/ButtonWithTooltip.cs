using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonWithTooltip: MonoBehaviour
{
    public Transform bg;
    public Transform icon;
    public GameObject bubleContainer;
    public Transform Tooltip;
    public Transform TooltipAnchor;
    public TextMeshProUGUI textButton;
    public TextMeshProUGUI textBubleCounter;
    public TextMeshProUGUI tooltipText;
    public Vector3 tooltipOffset;
    public Vector3 tooltipAnchorOffset;
    public Button button => GetComponent<Button>();
    public Image iconImage => icon.GetComponent<Image>();

    protected bool _isShowingTooltip = false;
    public void ShowToolTip(string txt = "New mission Added")
    {
        if (Tooltip == null) return;
        //Debug.Log("ShowToolTip");
        if (!_isShowingTooltip)
        {
            
            Tooltip.DOScale(1, 0.3f).SetDelay(0.1f).SetEase(Ease.OutBack).OnStart(()=> 
            {
                _isShowingTooltip = true;
                Tooltip.gameObject.SetActive(true);
                TooltipAnchor.localPosition = tooltipAnchorOffset;
                Tooltip.position = transform.position + tooltipOffset;
            });
            tooltipText.text = txt;
            Tooltip.DOMoveZ(0, 0).SetDelay(3).OnComplete(HideTooltip);
        }
        else
        {
            HideTooltip();
        }
    }
    public void HideTooltip()
    {
        if (Tooltip != null && _isShowingTooltip)
        {
            Tooltip.DOKill();
            _isShowingTooltip = false;
            Tooltip.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }

    protected virtual void Update()
    {
        if (Tooltip != null && Input.GetMouseButtonDown(0))
        {
            HideTooltip();
        }
    }

    protected void Punch()
    {
        if (bg == null) return;
        bg.DOPunchScale(Vector3.one * 0.3f, 0.5f, 1, 0.2f);
        icon.DOPunchScale(Vector3.one * 0.6f, 0.4f, 2, 0.3f);
        bubleContainer.transform.DOPunchScale(Vector3.one * 0.7f, 0.3f, 2, 0.3f);
    }

}