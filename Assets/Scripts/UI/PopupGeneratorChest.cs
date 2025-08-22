using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts.MergeBoard;
using System;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using DG.Tweening;

public class PopupGeneratorChest : PopupBase
{
    public Transform selectedItemContainer;
    public Transform guaranteedContainer;
    public Transform itemsContainer;
    [SerializeField] Transform _levelContainer;
    public GameObject boardTilePrefab;
    public TextMeshProUGUI txtItemTitle;
    public TextMeshProUGUI txtUpgradePiecesOwnedAmount;
    public TextMeshProUGUI txtUpgradePiecesCost;
    public TextMeshProUGUI txtGeneratorLevel;
    public Button btnUpgrade;
    
    public Action OnLevelUp;

    int _upgradePiecesOwned = 0;
    int _upgradeCost => _movingPiece!=null? (int)MathF.Pow(2, _movingPiece.PieceLevel) : 99;
    GeneratorConfig _genConfig;
    MovingPiece _movingPiece;
    public void Init(PieceDiscovery piece)
    {
        AddSelectedItem(piece);
        Init2();
    }
    public void Init(MovingPiece piece)
    {
        _movingPiece = piece;
        AddSelectedItem(piece.PieceDiscovery);
        UpdateData();
        Init2();
    }

    public void UpdateData(bool animated = false)
    {
        _upgradePiecesOwned = GameManager.Instance.mergeModel.CountItemInStorage(PieceType.UpgradeGenerator, 0, true);
        txtUpgradePiecesOwnedAmount.text = _upgradePiecesOwned.ToString();
        txtGeneratorLevel.text = (_movingPiece.PieceLevel + 1).ToString();
        txtUpgradePiecesCost.text = _upgradeCost.ToString();
        btnUpgrade.interactable = _upgradeCost <= _upgradePiecesOwned;
        if(animated)
        {
            _levelContainer.DOPunchScale(Vector3.one * 1.05f, 0.3f, 0, 0);
            txtGeneratorLevel.transform.DOPunchScale(Vector3.one * 1.1f,0.4f,0,0);
        }
    }

    private void Init2()
    {
        if (guaranteedContainer != null && _genConfig.guaranteedPieces.Count > 0)
        {
            AddItemsProduced(guaranteedContainer, boardTilePrefab);
        }
        AddItemsProduced(itemsContainer, boardTilePrefab);
        Show();
    }

    public void AddSelectedItem(PieceDiscovery piece)
    {
        var prefab = _mergeConfig.GetPiecePrefab(piece);
        GameManager.RemoveChildren(selectedItemContainer.gameObject);
        var item = Instantiate(prefab, selectedItemContainer).GetComponent<MovingPiece>();
        item.SetSimpleState();
        txtItemTitle.text = piece.pType.ToString();
        _genConfig = prefab.GetComponent<MovingPiece>().genConfig;
    }

    void AddItemsProduced(Transform container, GameObject prefab, bool animated = false)
    {
        GameManager.RemoveChildren(container.gameObject);
        var items = container== guaranteedContainer? _genConfig.guaranteedPieces:_genConfig.PossiblePieces(_movingPiece != null ? _movingPiece.PieceLevel : 0);
        for (var i = 0; i < Mathf.Min(items.Count,10); i++)
        {
            var tile = Instantiate(prefab, container).transform.GetComponent<BoardTile>();
            tile.inputDisabled = true;
            var itemPrefab = _mergeConfig.GetPiecePrefab(items[i]);
            var p = Instantiate(itemPrefab, tile.transform);
                p.transform.DOScale(0, 0);
                p.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack).SetDelay(i*0.1f);
            
        }
    }

    public void TryLevelUP()
    {
        if (_upgradePiecesOwned >= _upgradeCost)
        {
            _movingPiece.LevelUP();
            for (int i = 0; i < _upgradeCost-1; i++)
            {
                GameManager.Instance.mergeModel.RemoveFromStorage(new PieceDiscovery(PieceType.UpgradeGenerator,0), false, true);
            }
            //Last one to save data
            GameManager.Instance.mergeModel.RemoveFromStorage(new PieceDiscovery(PieceType.UpgradeGenerator,0), true, true);
            UpdateData(true);
            AddItemsProduced(itemsContainer, boardTilePrefab);
            //OnLevelUp?.Invoke();
        }
    }
}
