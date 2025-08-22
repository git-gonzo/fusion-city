using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CronoPieceController : MonoBehaviour
{
    public Image progress;
    public TextMeshProUGUI txtTimeLeft;

    private int _totalTime;
    private bool _isPlaying;
    private DateTime _endTime;
    private int _lastSecondBounce = 0;

    UnityAction _onTimeEnd;

    public void Init(int totalTime, UnityAction onTimeEnd) 
    {
        _totalTime = totalTime;
        _onTimeEnd = onTimeEnd;
    }

    public void Run()
    {
        _endTime = DateTime.Now.AddSeconds(_totalTime);
        _isPlaying = true;
    }

    void Update() 
    {
        if (_isPlaying)
        {
            var timeleft = (int)(_endTime - DateTime.Now).TotalSeconds;
            
            txtTimeLeft.text = $"{timeleft}";
            progress.fillAmount = ((float)timeleft / _totalTime);
            if(timeleft <= 0)
            {
                EndRound();
            }
            else if(timeleft < 10 )
            {
                if (timeleft != _lastSecondBounce)
                {
                    _lastSecondBounce = timeleft;
                    transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 2).OnComplete(()=> txtTimeLeft.color = Color.red);
                    txtTimeLeft.color = Color.white;
                }
            }
        }
    }

    void EndRound()
    {
        _isPlaying = false;
        _onTimeEnd.Invoke();
    }
}
