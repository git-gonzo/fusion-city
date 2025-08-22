using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingPoints : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _pointsAmount;

    public void ShowPlusPoints(Vector3 position, int amount)
    {
        transform.position = position;
        _pointsAmount.text = "+"+amount;
        transform.DOLocalMoveY(transform.localPosition.y+40,1.25f).SetEase(Ease.OutExpo);
        _pointsAmount.DOFade(0,0.5f).SetDelay(0.75f).OnComplete(()=>Destroy(gameObject));
    }
}
