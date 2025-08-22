using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assets.Scripts.MergeBoard;
public class PopupBase : MonoBehaviour
{
    public Button btnClose;
    protected float finalScale = 1f;
    public Action onCloseCallback;

    protected MergeConfig _mergeConfig => GameManager.Instance.gameConfig.mergeConfig;
    protected MergeBoardModel _mergeModel => GameManager.Instance.mergeModel;
    
    public virtual void Show()
    {
        var s = DOTween.Sequence();
        s.Append(transform.DOScale(0, 0));
        s.Append(transform.DOScale(finalScale, 0.3f).SetEase(Ease.OutBack));
        btnClose.onClick.AddListener(OnClose);
    }

    protected virtual void OnClose()
    {
        onCloseCallback?.Invoke();
    }

    protected void AddChainItems(Transform container, GameObject prefab, GameObject frameTilePrefab, PieceDiscovery piece)
    {
        GameManager.RemoveChildren(container.gameObject);
        var chain = _mergeConfig.GetChain(piece.pType);
        for (var i = 0; i < chain.levels.Count; i++)
        {
            var tile = Instantiate(prefab, container).transform.GetComponent<BoardTile>();
            tile.inputDisabled = true;
            if (i == piece.Lvl)
            {
                Instantiate(frameTilePrefab, tile.transform);
            }
            else
            {
                if (!_mergeModel.IsDiscovered(piece.pType, i))
                {
                    tile.ShowQuestionMark();
                    continue;
                }
            }
            Instantiate(chain.levels[i], tile.transform);
        }
    }
}
