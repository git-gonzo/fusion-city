using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupAvatar : PopupBase
{
    [SerializeField] PopupAvatarItem avatarItemPrefab;
    [SerializeField] Transform avatarsContainer;
    [SerializeField] Button btnSelect;

    List<PopupAvatarItem> allAvatars;
    List<CharacterData> characters => GameManager.Instance.gameConfig.Avatars;
    int avatarSelected;
    Action _onAvatarChaged;
    private MergeBoardModel model => GameManager.Instance.mergeModel;
    PlayerData playerData => GameManager.Instance.playerData;
    public void Init(Action OnClose, Action OnAvatarChanged)
    {
        onCloseCallback = OnClose;
        _onAvatarChaged = OnAvatarChanged;
        btnSelect.interactable = false;
        btnSelect.onClick.RemoveAllListeners();
        btnSelect.onClick.AddListener(OnConfirmChange);
        GameManager.RemoveChildren(avatarsContainer.gameObject);
        allAvatars ??= new List<PopupAvatarItem>();
        //Free and unlocked first
        foreach (var cha in characters.Where(c=>(c.isFree && c.unlockLevel <= GameManager.Instance.PlayerLevel) || model.ownedCharacters.Contains(c.id)))
        {
            AddCharacter(cha);
        }
        //unlocked purchasable second
        foreach (var cha in characters.Where(c => (c.unlockLevel <= GameManager.Instance.PlayerLevel) && !model.ownedCharacters.Contains(c.id)))
        {
            if(allAvatars.Find(a=>a.AvatarIndex == cha.id) == null) AddCharacter(cha);
        }
        //locked last
        foreach (var cha in characters.Where(c => (c.unlockLevel > GameManager.Instance.PlayerLevel) && !model.ownedCharacters.Contains(c.id)))
        {
            if (allAvatars.Find(a => a.AvatarIndex == cha.id) == null) AddCharacter(cha);
        }
        Show();
    }

    private void AddCharacter(CharacterData cha)
    {
        var avatar = Instantiate(avatarItemPrefab, avatarsContainer).GetComponent<PopupAvatarItem>();
        allAvatars.Add(avatar);
        avatar.Init(cha, OnSelected);
    }

    private void OnSelected(int index)
    {
        btnSelect.interactable = true;
        foreach (var avatar in allAvatars)
        {
            avatar.Selected = avatar.AvatarIndex == index;
        }
        avatarSelected = index;
    }
    private void OnConfirmChange()
    {
        PlayerData.characterIndex = avatarSelected;
        GameManager.Instance.topBar.SetAvatar();
        GameManager.Instance.dailyTaskManager.OnChangeAvatar();
        _onAvatarChaged?.Invoke();
        TrackingManager.TrackAndSendChangeAvatar(avatarSelected);
        Close();
    }

    public void Close()
    {
        onCloseCallback?.Invoke();
    }
}
