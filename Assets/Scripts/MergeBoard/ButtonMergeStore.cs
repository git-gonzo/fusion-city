using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.MergeBoard
{

    public class ButtonMergeStore : ButtonWithTooltip, IPointerEnterHandler, IPointerExitHandler
    {
        //public Transform bg;
        //public Transform icon;
        public TextMeshProUGUI txtItemsCount;
        public Action<bool> OverButton;
        public Action OnButtonStoreageUP;


        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("Pointer Enter");
            OverButton?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("Pointer Exit");
            OverButton?.Invoke(false);
        }

        public void OnAddItem(int current, int max)
        {
            Bounce();
            txtItemsCount.text = $"{current}/{max}";
        }

        public void Bounce()
        {
            bg.DOKill();
            bg.DOScale(1, 0);
            icon.DOKill();
            icon.DOScale(1, 0);
            bg.DOPunchScale(Vector3.one * 0.3f, 0.5f, 1, 0.2f);
            icon.DOPunchScale(Vector3.one * 0.7f, 0.4f, 2, 0.3f);
        }

        public void SetBubbleCounter(int number)
        {
            bubleContainer.SetActive(number > 0);
            textBubleCounter.text = number.ToString();
        }

        public Vector3 GetCenteredPosition()
        {
            Vector3 pos = transform.position;
            pos.x += GetComponent<RectTransform>().rect.width * 0.5f + 30;
            pos.y += GetComponent<RectTransform>().rect.height * 0.5f;
            return pos;
        }
    }
}
