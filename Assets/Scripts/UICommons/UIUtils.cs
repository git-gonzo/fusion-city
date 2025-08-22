using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Text;

public class UIUtils
{
    private static float textAnimSpeed = 0.02f;
    public static void DelayedCall(float time, Action action, MonoBehaviour who)
    {
        who.StartCoroutine(StartDelay(time, action));
    }
    public static void DelayedCallWithTween(float time, Action action)
    {
        var aux = 0;
        DOTween.To(() => aux, x => aux = x, aux + 1, time).OnComplete(() => { action.Invoke(); });
    }

    private static IEnumerator StartDelay(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action.Invoke();
    }

    public static void AnimateText(TextMeshProUGUI text, MonoBehaviour who, Action onFinishAnim = null, float speed = 0.02f, float delayTime = 0, bool SpeedUpOnClick = true)
    {
        textAnimSpeed = speed;
        if (delayTime > 0)
        {
            DelayedCall(delayTime, () => { AnimateText2(text, who, SpeedUpOnClick, onFinishAnim); text.enabled = true; }, who);
        }
        else
        {
            DelayedCall(0.1f, () => { AnimateText2(text, who, SpeedUpOnClick, onFinishAnim); text.enabled = true; }, who);
            //AnimateText2(text,who,SpeedUpOnClick, onFinishAnim);
        }
    }

    private static void AnimateText2(TextMeshProUGUI text, MonoBehaviour who, bool SpeedUpOnClick = true, Action onFinishAnim = null)
    {
        text.enabled = true;
        who.StartCoroutine(DoTextAnimation(text, SpeedUpOnClick, onFinishAnim));
    }

    private static IEnumerator DoTextAnimation(TextMeshProUGUI text, bool SpeedUpOnClick, Action onFinishAnim = null)
    {
        text.maxVisibleCharacters = 0;
        yield return new WaitForSecondsRealtime(textAnimSpeed);
        var a = 0;
        var end = text.textInfo.characterCount;
        DOTween.To(() => a, x => a = x, end, end * 0.02f).SetEase(Ease.Linear)
            .OnUpdate(() => {
                text.maxVisibleCharacters = a;
            })
            .OnComplete(() => onFinishAnim?.Invoke());
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        /*if (results.Count > 0)
            Debug.Log("Checking UI click true, hay " + results.Count);
        else
            Debug.Log("Checking UI click false");*/
        return results.Count > 0;
    }

    public static string FormatNumber(long number)
    {
        if(number > 100000 && number < 1000000)
        {
            var n = number / 100;
            if (n % 10 == 0)
            {
                return $"{(n / 10)}K";
            }
            else
            {
                return $"{(n / 10f)}K";
            }
        }
        else if (number > 1000000)
        {
            var n = number / 100000;
            if (n % 10 == 0)
            {
                return $"{(n / 10)}M";
            }
            else
            {
                return $"{(n / 10f)}M";
            }
        }
        return number.ToString();
    }

    public static string FormatTime(float time)
    {
        return FormatTimePriv((int)time);
    }
    public static string FormatTime(double time)
    {
        return FormatTimePriv((int)time);
    }

    private static string FormatTimePriv(int time)
    {
        //TODO: add Hours
        var seg = (int)(time % 60);
        var min = (int)(time / 60);
        var hour = (int)(time / 3600);
        var day = (int)(time / 86400);
        if (day > 0)
        {
            hour = (int)(time / 3600) % 24;
            return $"{day}d {hour}h";
        }
        if (hour > 0)
        {
            min = (time - hour * 3600) / 60;
            if (min > 0)
            {
                return $"{hour}h {min}m";

            }
            return $"{hour}h";
        }
        return "" + (min > 0 ? min + "m " : "") + (seg > 0 ? seg + "s" : "");
    }

    public static void FlyingParticles(RewardType reward, Vector3 from, int amountOfParticles, Action CallBack)
    {
        AudioSource sound = null;
        float scale = 1f;
        GameObject prefab = null;
        if (reward == RewardType.Coins)
        {
            prefab = GameObject.Find("TopBarCoins");
            sound = GameManager.Instance.soundCoins;
        }
        else if (reward == RewardType.XP)
        {
            prefab = GameObject.Find("XPParticle");
        }
        else if (reward == RewardType.Gems)
        {
            prefab = GameObject.Find("TopBarGems");
            sound = GameManager.Instance.soundGems;
        }
        else if (reward == RewardType.FamePoints)
        {
            prefab = GameObject.Find("FameParticle");
            //scale = 0.06f;
        }
        else if (reward == RewardType.Energy)
        {
            prefab ??= GameObject.Find("EnergyIcon");
            prefab ??= GameObject.Find("EnergyParticle");
            scale = 1.4f;
        }
        if (prefab == null)
        {
            Debug.LogError("Particle " + reward + " not implemented");
            return;
        }
        Vector3 to = prefab.transform.position;
        var parent = GameObject.Find("Canvas").transform;
        var seq = DOTween.Sequence();
        for (var i = 0; i < amountOfParticles; i++)
        {
            var p = UnityEngine.Object.Instantiate(prefab, from, Quaternion.identity, parent);
            p.transform.DOScale(scale, 0);
            var randomV = from + new Vector3(UnityEngine.Random.Range(-70, 70), UnityEngine.Random.Range(-70, 70), 0);
            seq.Insert(0 + i * 0.06f, p.transform.DOMove(randomV, 0.2f));
            //Play sound only evey 2
            if (i % 2 == 0)
            {
                seq.Insert(0.2f + i * 0.07f, p.transform.DOMove(to, 0.8f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    if (sound != null) sound.Play();
                    UnityEngine.Object.Destroy(p);
                }));
            }
            else
            {
                seq.Insert(0.2f + i * 0.07f, p.transform.DOMove(to, 0.8f).SetEase(Ease.InCubic).OnComplete(() =>
                {
                    UnityEngine.Object.Destroy(p);
                }));
            }
        }
        seq.OnComplete(()=> { CallBack?.Invoke(); });
    }

    public static void SaveTimeStamp(string key, DateTime time)
    {

        PlayerPrefs.SetString(key, time.ToBinary().ToString());
    }
    public static DateTime GetTimeStampByKey(string key)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            return DateTime.Now;
        }
        long temp = Convert.ToInt64(PlayerPrefs.GetString(key));
        return DateTime.FromBinary(temp);
    }

    public static string GetCleanName(string value)
    {
        var indexofAlmohadilla = value.IndexOf("#");
        if (indexofAlmohadilla <= 0)
        {
            indexofAlmohadilla = value.Length;
        }
        return value.Substring(0, indexofAlmohadilla);
    }

    private static string key = "Musguete"; // Debe ser la misma para encriptar y desencriptar
    public static string EncryptString(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        var textBytes = Encoding.UTF8.GetBytes(text);
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var result = new byte[textBytes.Length];

        for (int i = 0; i < textBytes.Length; i++)
        {
            result[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Convert.ToBase64String(result);
    }

    public static string DecryptString(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return "";

        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var result = new byte[encryptedBytes.Length];

        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            result[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }

        return Encoding.UTF8.GetString(result);
    }
}
