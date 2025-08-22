using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Scripts.MergeBoard;
using DG.Tweening;

public class PopupAvatarItem : MonoBehaviour, IPointerClickHandler
{
    public Image imgPortrait;
    public GameObject locked;
    public Image normalFrame;
    public GameObject selectedFrame;
    public ButtonBuy btnBuy;
    [SerializeField] Material greyMaterial;
    private int _avatarIndex;
    private bool _enabled;
    private bool _purchasable;
    private CharacterData _character;
    public int AvatarIndex => _avatarIndex;

    Action<int> _onSelected;

    private MergeBoardModel model => GameManager.Instance.mergeModel;

    bool _selected;
    public bool Selected 
    {
        get => _selected;
        set
        {
            _selected = value;
            selectedFrame.SetActive(value);
        }
    }

    public void Init(CharacterData character, Action<int> onSelected, bool dummy = false)
    {
        _character = character;
        _enabled = true;
        _purchasable = false;
        locked.SetActive(false);
        btnBuy.gameObject.SetActive(false);
        _avatarIndex = character.id;
        imgPortrait.sprite = character.sprite;
        _onSelected = onSelected;
        Selected = false;
        if(character.unlockLevel > GameManager.Instance.PlayerLevel)
        {
            imgPortrait.material = greyMaterial;
            normalFrame.material = greyMaterial;
            locked.SetActive(true);
            _enabled = false;
        } 
        else if (!character.isFree && !model.CharacterOwned(character.id) && !dummy)
        {
            btnBuy.gameObject.SetActive(true);
            btnBuy.Init(character.price, true);
            btnBuy.OnBuyAction = AskPurchase;
            imgPortrait.material = greyMaterial;
            normalFrame.material = greyMaterial;
            _purchasable = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_enabled) return;
        if (_purchasable)
        {
            AskPurchase();
            return;
        }

        if (_selected) return;
        _onSelected?.Invoke(_avatarIndex);
    }

    public void SetAvatarByIndex(int index)
    {
        imgPortrait.sprite = GameConfigMerge.instance.GetCharacter(index).sprite;
    }

    private void AskPurchase()
    {
        //if (!GameManager.Instance.HasEnoughCurrency(_character.price, true)) return;
        GameManager.Instance.PopupsManager.ShowPopupYesNo(
            "Buy Avatar",
            $"Are you sure you want to buy this avatar?<br>You will pay <color=yellow>{_character.price.amount}</color> {_character.price.currencyName}.",
            PopupManager.PopupType.yesno,
            () =>
            {
                if (!GameManager.TryToSpend(_character.price)) return;
                GiveCharacter();
            });
    }
    
    private void GiveCharacter()
    {
        btnBuy.gameObject.SetActive(false);
        btnBuy.OnBuyAction = null;
        _enabled = true;
        _purchasable = false;
        imgPortrait.material = null;
        normalFrame.material = null;
        model.OnBuyCharacter(_avatarIndex);
        transform.DOPunchScale(Vector3.one * 0.7f, 0.4f, 2, 0.3f);
    }
}
