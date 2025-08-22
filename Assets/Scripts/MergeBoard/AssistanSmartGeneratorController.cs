using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssistanSmartGeneratorController : MonoBehaviour
{
    [SerializeField] private PopupAvatarItem _assistantAvatar;
    [SerializeField] private Transform _itemsContainer;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private TextMeshProUGUI _txtTimeLeft;

    MergeConfig _mergeConfig => GameManager.Instance.gameConfig.mergeConfig;
    MovingPiece _movingPiece;
    List<PieceDiscovery> _selectedPiecesToSpawn = new List<PieceDiscovery>();
    List<BoardTile> _tiles = new List<BoardTile>();
    DateTime expiration;

    public List<PieceDiscovery> SelectedPiecesToSpawn => _selectedPiecesToSpawn;

    public void Init(MovingPiece movingPiece)
    {
        if (_movingPiece == movingPiece) return;
        _selectedPiecesToSpawn.Clear();
        _tiles.Clear();
        _movingPiece = movingPiece;
        var assistantConfig = GameManager.Instance.gameConfig.GetAssistantConfig(AssistantType.SmartGenerators);
        _assistantAvatar.imgPortrait.sprite = assistantConfig.character.characterPortrait;
        expiration = GameManager.Instance.mergeModel.assistants.Expiration(AssistantType.SmartGenerators);
        _txtTimeLeft.text = UIUtils.FormatTime((expiration - DateTime.Now).TotalSeconds);
        //Load generator items
        AddItemsProduced();
        HighlightPreferred();
    }

    void AddItemsProduced(bool animated = false)
    {
        GameManager.RemoveChildren(_itemsContainer.gameObject);
        var items = _movingPiece.genConfig.PossiblePieces(_movingPiece.PieceLevel);
        for (var i = 0; i < Mathf.Min(items.Count, 5); i++)
        {
            var tile = Instantiate(_itemPrefab, _itemsContainer).GetComponent<BoardTile>();
            var itemPrefab = _mergeConfig.GetPiecePrefab(items[i]);
            var p = Instantiate(itemPrefab, tile.transform).GetComponent<MovingPiece>();
            p.transform.DOScale(0, 0);
            p.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack).SetDelay(i * 0.1f);
            p.SetConfig(items[i]);
            tile.InitForSmartGenerator(p, items[i], OnItemTap);
            _tiles.Add(tile);
        }
    }

    private void OnItemTap(BoardTile tile, PieceDiscovery discovery)
    {
        tile.SelectSmartGenerator(!tile.IsSelected);
        if (!tile.IsSelected)
        {
            var piece = _selectedPiecesToSpawn.Find(p => p.pType == discovery.pType && p.Lvl == discovery.Lvl);
            if (piece != null)
            {
                _selectedPiecesToSpawn.Remove(piece);
            }
            return;
        }
        _selectedPiecesToSpawn.Add(discovery);
    }

    void HighlightPreferred() 
    {
        var preferred = _movingPiece.genConfig.GetPreferredObjectsToSpawn(_movingPiece.PieceLevel);
        if(preferred != null && preferred.Count > 0) 
        {
            foreach( var tile in _tiles)
            {
                var preferredItem = preferred.Find(p => p.pieceType == tile.piece.PieceType && p.pieceLevel == tile.piece.PieceLevel);
                tile.SelectSmartGenerator(preferredItem != null);
                if (preferredItem != null)
                {
                    _selectedPiecesToSpawn.Add(new PieceDiscovery(tile.piece.PieceType, tile.piece.PieceLevel));
                }
            }
        }
    }

    private void Update()
    {
        _txtTimeLeft.text = UIUtils.FormatTime((expiration - DateTime.Now).TotalSeconds);
    }
}
