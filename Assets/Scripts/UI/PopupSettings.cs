using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PopupSettings : PopupBase
{
    public TextMeshProUGUI txtAppVersion;
    public TextMeshProUGUI txtPlayerID;
    public void Init(Action onClose)
    {
        onCloseCallback = onClose;
        txtAppVersion.text = "Version: " + Application.version;
        txtPlayerID.text = "Player ID: " + PlayerData.playerID;
        Show();
    }
    public void OpenMail()
    {
        Application.OpenURL("mailto:mylittlemerge@gmail.com");
    }
    
    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/tUXRthtN");
    }

    public void OpenFaceboof()
    {
        Application.OpenURL("https://www.facebook.com/profile.php?id=100085108237624");
    }

}
