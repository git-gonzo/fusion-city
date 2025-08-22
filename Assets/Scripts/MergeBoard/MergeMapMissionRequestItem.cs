using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MergeMapMissionRequestItem : MonoBehaviour
{
    public GameObject bgNormal;
    public GameObject bgOrange;
    public GameObject bgReady;
    public GameObject tickReady;
    public GameObject tickPending;
    public Transform imgContainer;
    public Transform tooltip;
    public Button btnChainInfo;
    public Button btnBuildingSource;
    public bool isReady;
    public PopupMergeChain popupChain;

    private bool _isShowingSource = false;
    private PieceType _pieceType;
    private PieceState _pieceState;
    Transform btnBuildingSourceT => btnBuildingSource.GetComponent<Transform>();


    public MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;
    public MergeBoardModel mergeModel => GameManager.Instance.MergeManager.boardModel;
    MergeBoardManager mergeManager => GameManager.Instance.MergeManager;

    public void AddItem(PieceDiscovery piece)
    {
        _pieceState = new PieceState(piece.pType, piece.Lvl);
        tooltip.DOScale(0, 0);
        //_onTapItem = onTapItem;
        //pieceState = pState;
        _pieceType = piece.pType;
        var prefab = mergeConfig.GetPiecePrefab(piece);
        var p = Instantiate(prefab, imgContainer).GetComponent<MovingPiece>();
        var rect = p.GetComponent<RectTransform>();
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        p.transform.localPosition = Vector3.zero;
        p.transform.localScale = Vector3.one * 1.15f;
        //GetComponent<Button>().onClick.AddListener(OnTap);

        //Check if it is ready (in the storage)
        isReady = mergeModel.isItemInStorage(piece);
        bgNormal.SetActive(!isReady);
        bgReady.SetActive(isReady);
        tickPending.SetActive(!isReady);
        tickReady.SetActive(isReady);

        if (!isReady)
        {
            bgNormal.GetComponent<Button>().onClick.AddListener(ShowBuildingSource);
            btnBuildingSource.onClick.AddListener(GotoBuildingSource);
            btnChainInfo.onClick.AddListener(ShowChainInfo);
        }
    }

    public void SetDefaultBGOrange(bool value)
    {
        bgOrange.SetActive(value);
    }

    private void ShowChainInfo()
    {
        var _popupChainInstance = GameManager.Instance.PopupsManager.ShowPopup(popupChain).GetComponent<PopupMergeChain>();
        _popupChainInstance.Init(_pieceState);
    }

    private void GotoBuildingSource()
    {
        if (mergeManager.IsBoardActive)
        {
            GameManager.Instance.ShowMergeBoard(false);
        }
        GameManager.Instance.ShowMergeMapMissions(false);
        var buildingSource = GameManager.Instance.mapManager.GetBuildingSourceOfMergeItem(_pieceType);
        if(buildingSource == BuildingType.None)
        {
            Debug.Log("ATTENTION: Piece " + _pieceType + " has no building with a board containing its generator config");
            return;
        }
        GameManager.Instance.FocusOnBuilding(buildingSource);
    }

    private void ShowBuildingSource()
    {
        //Debug.Log("Show Building Source");
        if (!_isShowingSource)
        {
            _isShowingSource = true;
            tooltip.DOScale(1, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            HideBuildingSource();
        }
    }

    public void HideBuildingSource()
    {
        if (_isShowingSource) {
            _isShowingSource = false;
            tooltip.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HideBuildingSource();
        }
    }
}
