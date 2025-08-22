using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaceUIController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float accelerationUp;
    public float accelerationDown;
    public Image imageFill;
    public Image imageBtnON;
    public Transform aguja;
    public float progress => _progress;
    bool _isPressing;
    bool _isBroken;
    float _progress;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Mouse Down");
        _isPressing = true;
        imageBtnON.gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Mouse Up");
        _isPressing = false;
        imageBtnON.gameObject.SetActive(false);
    }

    private void Start()
    {
        imageBtnON.gameObject.SetActive(false);
    }

    private void Update()
    {
        _progress += (_isPressing&&!_isBroken?
            (_progress>0.25f?
                    (_progress>0.7f?accelerationUp*4:accelerationUp*2) 
                :accelerationUp):
            - accelerationDown)*Time.deltaTime;
        _progress = Mathf.Clamp( _progress,0,1);
        imageFill.fillAmount = _progress;
        aguja.eulerAngles = Vector3.back * 180 * _progress;
        if(_progress > 0) {
            //aguja.DORotate(Vector3.right * 180 * _progress, 0.1f);
            if (_progress > 0.9f)
            {
                _isBroken = true;
            }
        }
        if (_isBroken && _progress < 0.4f)
        {
            _isBroken=false;
        } ;
    } 
}
