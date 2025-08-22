using UnityEngine;
using DG.Tweening;

public class BoxController : MonoBehaviour
{
    public PriceWithIcon price;
    public Transform box;
    [SerializeField] Color ColorNotEnough = new Color(1, 0.8f, 0.8f);
    private RewardData _cost;
    private bool hasCurrency => GameManager.Instance.HasEnoughCurrency(_cost);

    public void SetPrice(RewardData cost)
    {
        price.Init(cost);
        _cost = cost;
        CheckShake();
    }

    public void ShakeBox()
    {
        //box.DOShakeRotation(1.2f,Vector3.forward * 30,5,60).SetDelay(Random.Range(3,10)).OnComplete(CheckShake);
        box.DORotate(Vector3.forward * 15, 0.1f).SetDelay(Random.Range(10, 25))
            .OnComplete(() =>
            {
                box.DORotate(Vector3.forward * -15, 0.12f).SetLoops(3,LoopType.Yoyo).SetEase(Ease.Linear)
                .OnComplete(() => { box.DORotate(Vector3.forward * 0, 0.2f).SetEase(Ease.OutBack).OnComplete(CheckShake); });
            });
    }

    public void CheckShake() 
    {
        price.txtPrice.DOColor(hasCurrency ? Color.white : ColorNotEnough, 0.1f);
        if (hasCurrency) ShakeBox();
    }

    private void OnDestroy()
    {
        box.DOKill();
    }
}
