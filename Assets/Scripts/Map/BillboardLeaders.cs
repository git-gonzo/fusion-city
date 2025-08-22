using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BillboardLeaders : MonoBehaviour
{
    public TextMeshPro textPlayer1;
    public TextMeshPro textPlayer2;
    public TextMeshPro textPlayer3;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FillNames());
    }

    IEnumerator FillNames()
    {
        while (!Globals.Instance.leaderboardReady)
        {
            yield return null;
        }
        if (GameManager.Instance.LeaderboardManager.leaderboardMonth.Count > 0)
            textPlayer1.text = GameManager.Instance.LeaderboardManager.leaderboardMonth[0].playername;
        if (GameManager.Instance.LeaderboardManager.leaderboardMonth.Count > 1)
            textPlayer2.text = GameManager.Instance.LeaderboardManager.leaderboardMonth[1].playername;
        if (GameManager.Instance.LeaderboardManager.leaderboardMonth.Count > 2)
            textPlayer3.text = GameManager.Instance.LeaderboardManager.leaderboardMonth[2].playername;
    }
}
