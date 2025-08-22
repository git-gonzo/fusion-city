using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasPlayerMap : MonoBehaviour
{
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI playerPosition;
    public PopupAvatarItem avatar;
    public void Init(LeaderboardPlayer player)
    {
        playerName.text = player.playername;
        playerPosition.text = player.position.ToString();
        avatar.SetAvatarByIndex(player.charIndex);
        gameObject.SetActive(false);
    }
    public void Init(SimplePlayer player)
    {
        playerName.text = player.playername;
        playerPosition.text = player.position.ToString();
        avatar.SetAvatarByIndex(player.charIndex);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        //transform.LookAt(Camera.main.transform);
    }
}
