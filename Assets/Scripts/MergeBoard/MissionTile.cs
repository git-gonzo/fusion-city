using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine.Events;

public class MissionTile : MonoBehaviour
{
    public Sprite imgNormal;
    public Sprite imgReady;
    public Transform pieceContainer;
    public ParticleSystem particles;
    [SerializeField] private GameObject _trophy;
    [SerializeField] private TextMeshProUGUI _trophyAmount;
    private MovingPiece _movingPiece;

    public MovingPiece piece => _movingPiece;
        Image _img;
        Image image
        {
            get
            {
                if (_img == null) _img = GetComponent<Image>();
                return _img;
            }
        }

        private void Start()
        {
            transform.DOScale(0, 0.4f).From().SetEase(Ease.OutBack);
        }
        public void SetMissionNormal()
        {
            image.sprite = imgNormal;
        }
        public void SetMissionReady()
        {
            image.sprite = imgReady;
        }
        public void CompleteMission(MovingPiece mission, Action OnComplete)
        {
            //Debug.Log("particleplay");
            particles.gameObject.SetActive(true);
            //p.transform.SetParent(transform.parent); //To avoid transformation of mission tile
            //Destroy(p.gameObject, 1);
            transform.DOScale(0, 0.5f).SetDelay(0.5f).SetEase(Ease.InBack).OnComplete(()=> {
                Destroy(mission.gameObject); 
                Destroy(gameObject);
                OnComplete?.Invoke();
            });
        }
        public void CompleteSpeedMission(MovingPiece mission, Action OnComplete)
        {
            particles.gameObject.SetActive(true);
            
            transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(()=> {
                Destroy(mission.gameObject); 
                Destroy(gameObject);
                OnComplete?.Invoke();
            });
        }
        public void DiscardMission(MovingPiece mission, Action OnComplete)
        {
            transform.DOScale(0, 0.5f).SetDelay(0.5f).SetEase(Ease.InBack).OnComplete(()=> {
                Destroy(mission.gameObject); 
                Destroy(gameObject);
                OnComplete?.Invoke();
            });
        }

        public void ShowMission()
        {
            //transform.DOScale(1, 0.4f).SetDelay(0.1f).SetEase(Ease.OutBack);
        }
        public void ShowTrophy(bool value = true, int amount = 0)
        {
            _trophy.SetActive(value);
            _trophyAmount.text = amount.ToString();
        }
    public void SetMovingPiece(MovingPiece m)
    {
        _movingPiece = m;
    }
    public void AddButton(UnityAction<MovingPiece> onClick) 
    {
        gameObject.AddComponent<Button>().onClick.AddListener( ()=>
        {
            //gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
            onClick?.Invoke(_movingPiece);
        });
    }
    public void RemoveButton()
    {
        if (gameObject.TryGetComponent<Button>(out var btn))
        {
            btn.onClick.RemoveAllListeners();
        }
    }

}
