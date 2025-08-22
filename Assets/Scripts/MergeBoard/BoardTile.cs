using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Profiling;

namespace Assets.Scripts.MergeBoard
{
    public class BoardTile : MonoBehaviour, IPointerDownHandler
    {
        /*[System.NonSerialized]*/public Color colorSelected;
        public MovingPiece piece;
        public GameObject _question;
        public GameObject missionBg;
        public GameObject missionMapBg;
        public Image smartGeneratorSelector;
        public Image selectedView;
        public bool inputDisabled = false;
        public Color colorReadyMision;
        public Color colorReadyMapMision;
        public GameObject SmokeFX;
        Color colorMissionNormal;
        private Image _img;
        private bool _selected;
        //private bool _wasSelected;

        Action<MovingPiece> _onClick;
        Image _selectedImg;

        Action<BoardTile,PieceDiscovery> _onSelect;
        PieceDiscovery _pieceDiscovery;

        public Image image => _img ??= GetComponent<Image>();
                
        public void SetContent(MovingPiece piece, Action<MovingPiece> onClick, bool animate = false, bool isSpawn = false, GameObject trail = null, bool isSpeed = false)
        {
            smartGeneratorSelector.gameObject.SetActive(false);
            this.piece = piece;
            _onClick = onClick;
            if (piece == null)
            {
                return;
            }
            piece.SetBoardTileParent(this, animate, isSpawn,trail,isSpeed);
            missionBg.SetActive(false);
        }
        

        public void SetColorBoardNormal(Color color)
        {
            colorMissionNormal = color;
            image.DOColor(colorMissionNormal,0.1f).SetLink(image.gameObject);
        }
        public void Select(bool value)
        {
            /*if (value && value == _selected && image.color == colorSelected) return;
            if (!value && value == _selected && image.color == colorMissionNormal) return;
            image.DOColor(value?colorSelected: colorMissionNormal,0.15f);*/
            _selected = value;
            SetStateNormal();
        }
        public void Unselect()
        {
            //_wasSelected = false;
            Select(false);
            selectedView.DOFade(0, 0.2f).OnComplete(() => selectedView.gameObject.SetActive(false));
        }
        public void SelectSmartGenerator(bool value)
        {
            smartGeneratorSelector.DOKill();
            if (value)
            {
                piece.transform.DOScale(Vector3.one, 0f).SetLink(piece.gameObject);
                piece.transform.DOPunchScale(Vector3.one * 0.4f, 0.3f, 0, 0).SetLink(piece.gameObject);
                smartGeneratorSelector.gameObject.SetActive(true);
                smartGeneratorSelector.DOFade(1, 0.2f).SetLink(smartGeneratorSelector.gameObject);
            }
            else
            {
                smartGeneratorSelector.DOFade(0, 0.2f).OnComplete(()=> smartGeneratorSelector.gameObject.SetActive(false));
            }
            _selected = value;
        }

        public void ShowQuestionMark()
        {
            _question.SetActive(true);
        }
        public bool IsSelected => _selected;

        public void OnPointerDown(PointerEventData eventData)
        {
            if(inputDisabled) return;
            if(piece == null)
            {
                GameManager.Log("Click on Empty Board Tile");
                return;
            }
            //GameManager.Log("Click on Board Tile with piece " + piece.PieceType);
            _onClick?.Invoke(piece);
            _onSelect?.Invoke(this, _pieceDiscovery);
        }

        public void InitForSmartGenerator(MovingPiece p, PieceDiscovery pieceDiscovery, Action<BoardTile, PieceDiscovery> callback)
        {
            piece = p;
            _pieceDiscovery = pieceDiscovery;
            _onSelect = callback;
        }

        public void SetStateMissionReady()
        {
            image.DOColor(IsSelected ? colorSelected : colorReadyMision, 0.15f).SetLink(image.gameObject);
        }
        public void SetStateMapMissionReady()
        {
            image.DOColor(IsSelected ? colorSelected : colorReadyMapMision, 0.15f).SetLink(image.gameObject);
        }
        public void SetStateNormal()
        {
            //image.DOColor(IsSelected ? colorSelected : colorMissionNormal, 0.15f);
            if (IsSelected)
            {
                //_wasSelected = true;
                if (selectedView.gameObject.activeSelf) return;
                selectedView.DOFade(0, 0).SetLink(selectedView.gameObject);
                selectedView.gameObject.SetActive(true);
                selectedView.DOFade(0.7f, 0.3f).SetLink(selectedView.gameObject);
                selectedView.transform.DOScale(1, 0).SetLink(selectedView.gameObject);
                selectedView.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(selectedView.gameObject);
            }
            else
            {
                if (!selectedView.gameObject.activeSelf) return;
                selectedView.DOFade(0, 0).SetLink(selectedView.gameObject);
                selectedView.gameObject.SetActive(false);
            }
        }
        public void AddSmoke()
        {
            //Debug.Log("Adding Smoke");
            var smoke = Instantiate(SmokeFX, transform);
            Destroy(smoke, 1);
        }
    }
}