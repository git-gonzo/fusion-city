using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;
using System.Security.Cryptography;

public class LeaderBoardItenFameView : MonoBehaviour
{
    public TextMeshProUGUI txtPosition;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtPoints;
    public TextMeshProUGUI txtLevel;
    public Image progress;
    public Image bg;
    public Sprite bgIsPlayer;
    public Sprite bgNormal;
    public PopupAvatarItem avatar;

    LeaderboardPlayer _data;
    Button _btn;

    public void Init(LeaderboardPlayer def)
    {
        _btn ??= GetComponent<Button>();
        _data = def;
        txtName.text = def.playername;
        txtPosition.text = def.position.ToString();
        txtPoints.text = UIUtils.FormatNumber(def.score);
        txtLevel.text = def.level.ToString();
        progress.transform.DOScaleX((float)def.xp / GameConfigMerge.instance.NextLevelXP(def.level), 0.5f);
        bg.sprite= def.isPlayer?bgIsPlayer: bgNormal;
        SetAvatar(def.charIndex);
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(ShowPlayer);
    }
    public void Init(BasicLeaderboardItemData def, bool isPlayer = false)
    {
        _btn ??= GetComponent<Button>();
        txtName.text = def.playerName;
        txtPosition.text = def.rank.ToString();
        txtPoints.text = UIUtils.FormatNumber((int)def.score);
        if(txtLevel != null) txtLevel.text = def.level.ToString();
        //bg.sprite= def.isPlayer?bgIsPlayer: bgNormal;
        SetAvatar(def.avatar);
        bg.sprite = isPlayer ? bgIsPlayer : bgNormal;
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(ShowPlayer);
    }

    public void SetAvatar(int index)
    {
        if (avatar == null) return;
        avatar.SetAvatarByIndex(index);
    }

    private void ShowPlayer()
    {
        GameManager.Instance.PopupsManager.ShowOtherPlayerStats(_data);
    }

    public void SetPeopleCallback(UnityAction callback) 
    {
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(callback);
    }
}
