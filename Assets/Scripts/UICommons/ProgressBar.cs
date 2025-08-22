using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class ProgressBar : MonoBehaviour
{
    public Image progressBarLine;
    public TextMeshProUGUI textTimeLeft;
    public Button FinishNowBtn;
    public int finishNowPrice;

    private ButtonWithPrice btnFinish;
    
    public void UpdateProgressBar(int duration, double timeLeft, bool animate)
    {
        if (btnFinish == null)
        {
            btnFinish = FinishNowBtn.GetComponent<ButtonWithPrice>();
        }
        if (textTimeLeft != null) UpdateTimeLeft(timeLeft);
        var currentProgress = 1 - timeLeft / duration;
        currentProgress = Mathf.Clamp((float)currentProgress, 0, 1);
        if ((float)currentProgress != progressBarLine.transform.localScale.x)
        {
            //gameObject.SetActive(currentProgress < 1);
            progressBarLine.transform.DOScaleX((float)currentProgress, animate ? 0.1f : 0).SetEase(Ease.Linear);
        }
        finishNowPrice = GameManager.GetTimePrice(timeLeft);
        if (btnFinish != null)
        {
            //Debug.Log("Progress bar, duration = " + duration + " timeLeft=" + timeLeft + "  current progress=" + currentProgress.ToString() + ", finishPrice = " + finishNowPrice);
            btnFinish.SetPrice(finishNowPrice,RewardType.Gems);
        }
    }

    private void UpdateTimeLeft(double time)
    {
        var n = Mathf.Ceil((float)time);
        
        if (n <= 0)
        {
            textTimeLeft.text = "Complete";
            textTimeLeft.color = Color.green;
        }
        else
        {
            var timeText = UIUtils.FormatTime(n);
            textTimeLeft.color = Color.yellow;
            textTimeLeft.text = timeText;
        }
    }

    public void Completed()
    {
        textTimeLeft.text = "Complete";
        textTimeLeft.color = Color.green;
        progressBarLine.transform.DOScaleX(1, 0);
    }

    public void UpdateSimple(int current, int max, bool withColor = true)
    {
        var currentProgress = (float)current / max;
        progressBarLine.transform.DOScaleX((float)currentProgress, 0);
        if (withColor)
        {
            progressBarLine.color = GetRedGreenColor(currentProgress);
        }
    }
    public void UpdateAnimated(int current, int max, bool withColor = true)
    {
        var currentProgress = (float)current / max;
        progressBarLine.transform.DOScaleX((float)currentProgress, 0.3f).OnUpdate(() =>
        {
            if (withColor)
            {
                progressBarLine.color = GetRedGreenColor(progressBarLine.transform.localScale.x);
            }
        });
    }

    private Color GetRedGreenColor(float val)
    {
        if (val > 0.9f)
        {
            return new Color(0, 1, 0);
        }
        else if (val > 0.6f)
        {
            return new Color(1, 1, 0);
        }
        else if (val > 0.3f)
        {
            return new Color(0.8f, 0.2f, 0);
        }
        else 
        {
            return new Color(1, 0, 0);
        }
    }

    
}
