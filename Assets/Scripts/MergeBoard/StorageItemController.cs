using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageItemController : MonoBehaviour
{
    public GameObject bgNormal;
    public GameObject bgReady;
    public GameObject iconPlus;
    public GameObject selected;
    public PieceExtraInfo pieceExtraInfo;
    public ButtonWithPrice btnBuy;
    public PieceState pieceState;
    public Action OnBuySlot;
    private Action<PieceState,Transform> _onTapItem;
    private Action<PieceState> _onTapItem2;

    private void HideAll()
    {
        bgReady.SetActive(false);
        bgNormal.SetActive(false);
        iconPlus.SetActive(false);
        btnBuy.gameObject.SetActive(false);
    }
    public void SetNormalState()
    {
        HideAll();
        bgNormal.SetActive(true);
    }
    public void SetReadyState()
    {
        HideAll();
        bgReady.SetActive(true);
    }
    public void SetBuyState(int buyPrice)
    {
        HideAll();
        iconPlus.SetActive(true);
        btnBuy.gameObject.SetActive(true);
        btnBuy.textPrice.text = buyPrice.ToString();
        btnBuy.GetComponent<Button>().onClick.AddListener(OnTapBuy);
    }

    public void AddItem(GameObject prefab, PieceState pState, Action<PieceState,Transform> onTapItem,bool isReady = false, bool isMaxed = false)
    {
        _onTapItem = onTapItem;
        pieceState = pState;
        var p = Instantiate(prefab, transform).GetComponent<MovingPiece>();
        var rect = p.GetComponent<RectTransform>();
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        p.transform.localPosition = Vector3.zero;
        p.transform.localScale = Vector3.one * 1.15f;
        GetComponent<Button>().onClick.AddListener(OnTap);
        if (isReady)
        {
            SetReadyState();
            p.SetExtraInfo(true, false, isMaxed, false, pieceExtraInfo);
        }
    }

    private void OnTapBuy()
    {
        OnBuySlot?.Invoke();
    }
    private void OnTap()
    {
        _onTapItem?.Invoke(pieceState,transform);
    }
    private void OnTap2()
    {
        _onTapItem2?.Invoke(pieceState);
    }

    public void Select(bool value)
    {
        selected.SetActive(value);
    }
}
