using Assets.Scripts.MergeBoard;
using Coffee.UIEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GiftStealItemController : MonoBehaviour
{
    [SerializeField] Material GreyMaterial;
    public GameObject bgUnavailable;
    public GameObject bgNormal;
    public GameObject bgSelected;
    public PieceExtraInfo pieceExtraInfo;
    public ButtonWithPrice btnBuy;
    public PieceState pieceState;
    public Action OnBuySlot;
    private Action<PieceState, GiftStealItemController> _onTapItem;
    private MovingPiece _movingPiece;

    private void HideAll()
    {
        bgSelected.SetActive(false);
        bgNormal.SetActive(false);
        bgUnavailable.SetActive(false);
    }
    public void SetNormalState()
    {
        HideAll();
        bgNormal.SetActive(true);
        if(_movingPiece!=null) _movingPiece.SetGreyState(false, null);
    }
    public void SetReadyState()
    {
        HideAll();
        bgSelected.SetActive(true);
    }
    public void SetStateUnavailable()
    {
        HideAll();
        bgUnavailable.SetActive(true);
        if (_movingPiece != null) _movingPiece.SetGreyState(true, GreyMaterial);
        foreach (Transform t in _movingPiece.transform)
        {
            if(t.TryGetComponent<UIShiny>(out var sh))
            {
                sh.enabled = false;
            }

        }
    }
    public void SetStateWeapon()
    {
        HideAll();
        bgUnavailable.SetActive(true);
    }

    public void AddItem(GameObject prefab, PieceState pState, Action<PieceState, GiftStealItemController> onTapItem, bool isMaxed, bool available)
    {
        _movingPiece = Instantiate(prefab, transform).GetComponent<MovingPiece>();
        if (available)
        {
            _onTapItem = onTapItem;
            GetComponent<Button>().onClick.AddListener(OnTap);
        }
        else
        {
            SetStateUnavailable();
        }
        pieceState = pState;
        var rect = _movingPiece.GetComponent<RectTransform>();
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        _movingPiece.transform.localPosition = Vector3.zero;
        _movingPiece.transform.localScale = Vector3.one * 1.15f;
        _movingPiece.SetExtraInfo(false, false, isMaxed, false, pieceExtraInfo);
    }

    private void OnTap()
    {
        _onTapItem?.Invoke(pieceState,this);
        Select(true);
    }

    public void Select(bool value)
    {
        bgSelected.SetActive(value);
    }
}
