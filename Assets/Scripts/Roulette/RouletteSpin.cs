using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class RouletteSpin : MonoBehaviour
{
    public Transform ruleta;
    public Transform marca;
    public Image rouletteBG;
    public Color colorRouletteCommon;
    public Color colorRouletteSpecial;
    public GameObject ticketInRouletteCommon;
    public TextMeshProUGUI ticketInRouletteCommontxt;
    public GameObject ticketInRouletteSpecial;
    public TextMeshProUGUI ticketInRouletteSpecialtxt;
    public Button btnBackRoulette;
    public Button btnSpin;
    public RewardItemUniversal premioPrefab;
    public int vueltas;
    public float duracionAnimacion = 5f;
    public AudioSource soundPrize;
    public AudioSource soundVehicle;
    public AudioSource soundSpin;
    public AudioSource soundClack;

    private List<WeightedRouletteItem> rouletteItemsConfig;
    private List<RewardItemUniversal> rouletteItems;
    private float anguloPorSector;
    public float itemsScale;
    public bool girando = false;
    private int premio;
    private bool _isSpecial;
    private int countCommon => GameManager.Instance.mergeModel.CountItemInStorage(pCommon, true);
    private int countSpecial => GameManager.Instance.mergeModel.CountItemInStorage(pSpecial, true);
    private bool canSpin => (_isSpecial ? countSpecial > 0 : countCommon > 0) && !girando;
    private PieceDiscovery pSpecial => new PieceDiscovery(PieceType.RouleteTicketSpecial, 3);
    private PieceDiscovery pCommon = new PieceDiscovery(PieceType.RouleteTicketCommon, 3);
    private CanvasGroup _canvasGroup;
    GameConfigMerge Config => GameManager.Instance.gameConfig;
    MapManager mapManager => GameManager.Instance.mapManager;

    //UIUtils.SaveTimeStamp("LastRouletteTicket", lastVideoTime);

    public void ShowRoulette(bool isSpecial = false)
    {
        _canvasGroup ??= GetComponent<CanvasGroup>();
        _canvasGroup.DOFade(0,0).OnComplete(()=>_canvasGroup.DOFade(1,0.2f));
        _isSpecial = isSpecial;
        rouletteBG.color = isSpecial ? colorRouletteSpecial : colorRouletteCommon;
        ticketInRouletteCommontxt.gameObject.SetActive(!isSpecial);
        ticketInRouletteCommon.SetActive(!isSpecial);
        ticketInRouletteSpecialtxt.gameObject.SetActive(isSpecial);
        ticketInRouletteSpecial.SetActive(isSpecial);
        btnBackRoulette.onClick.RemoveAllListeners();
        btnSpin.onClick.RemoveAllListeners();
        btnSpin.onClick.AddListener(GirarRuleta);
        rouletteItemsConfig = GetItemsConfig();
        ruleta.transform.DORotate(Vector3.zero, 0).OnComplete(SetItems);
        btnSpin.interactable = canSpin; 
    }

    public List<WeightedRouletteItem> GetItemsConfig()
    {
        var day = (int)DateTime.Now.DayOfWeek;
        if (day == 0) day = 7;
            var key = $"RouletteConfig{(_isSpecial ? "Special" : "Common")}{day}";
            return Config.RouletteConfigsDict.Find(r => r.key == key).refConfig.rouletteItems;
    }

    public void Hide()
    {
        _canvasGroup ??= GetComponent<CanvasGroup>();
        _canvasGroup.DOFade(0, 0.2f).OnComplete(()=>gameObject.SetActive(false));
    }

    void SetItems()
    {
        anguloPorSector = 360f / rouletteItemsConfig.Count;
        GameManager.RemoveChildren(rouletteBG.gameObject);
        rouletteItems = new List<RewardItemUniversal>();
        for (int i = 0; i < rouletteItemsConfig.Count; i++)
        {
            float angulo = i * anguloPorSector;
            Vector3 posicion = new Vector3(Mathf.Cos(angulo * Mathf.Deg2Rad), Mathf.Sin(angulo * Mathf.Deg2Rad), 0) * Vector3.Distance(ruleta.position, marca.position) * 0.71f;
            var premio = Instantiate(premioPrefab, rouletteBG.transform);
            premio.transform.position = ruleta.transform.position + posicion;
            premio.InitReward(rouletteItemsConfig[i].reward, GameManager.Instance.topBar);
            premio.transform.DORotate(Vector3.forward * (angulo - 90), 0);
            premio.transform.DOScale(0, 0);
            premio.transform.DOScale(itemsScale, 0.2f).SetEase(Ease.OutBack).SetDelay(0.1f + i * 0.04f);
            premio.SetStateNaked();
            rouletteItems.Add(premio);
        }
        GirarInit();
    }
    private void GirarInit()
    {
        girando = true;
        btnSpin.interactable = canSpin;
        ruleta.transform.DORotate(new Vector3(0f, 0, 90), 0).OnComplete(() =>
        {
            ruleta.transform.DORotate(Vector3.zero, 0.8f).OnComplete(() =>
            {
                girando = false;
                btnSpin.interactable = canSpin;
            });
        });
    }
    public void GirarRuleta()
    {
        if (!canSpin) return;
        if (!girando)
        {
            soundSpin.Play();
            //Consume Ticket
            GameManager.Instance.mergeModel.RemoveFromStorage(_isSpecial ? pSpecial : pCommon, true, true);
            TrackingManager.TrackRoulette(_isSpecial ? "Special" : "Common");
            ticketInRouletteCommontxt.text = "x" + countCommon;
            ticketInRouletteSpecialtxt.text = "x" + countSpecial;
            girando = true;
            btnSpin.interactable = canSpin;
            if (!_isSpecial)
            {
                var totalSpins = PlayerPrefs.GetInt("totalSpins");
                if (totalSpins == 0)
                {
                    premio = GetIndexOfCar();
                }
                else
                {
                    premio = rouletteItemsConfig.GetWeightedIndex();
                }
                totalSpins++;
                PlayerPrefs.SetInt("totalSpins",totalSpins);
            }
            else
            {
                premio = rouletteItemsConfig.GetWeightedIndex();
            }
            float anguloGiro = premio * anguloPorSector + 360 * vueltas - 90;
            ruleta.transform.DORotate(new Vector3(0f, 0, -anguloGiro), duracionAnimacion, RotateMode.FastBeyond360).SetEase(Ease.OutCubic)
                .OnUpdate(() =>
                {
                    var grados = ((ruleta.transform.rotation.eulerAngles.z) + anguloPorSector / 2) % 360;
                    var mod = grados % anguloPorSector;
                    if (mod > 0 && mod < 3)// && !DOTween.IsTweening(marca))
                    {
                        //marca.DOKill();
                        if(!soundClack.isPlaying || soundClack.time > 0.05f)
                            soundClack.Play();  
                        
                        marca.DORotate(Vector3.forward * 1, 0);
                        marca.DOPunchRotation(Vector3.forward * 40f, 0.2f, 1, 1f);//.SetEase(Ease.OutElastic);
                    }
                })
                .OnComplete(() =>
                {
                    GiveReward(premio);
                    girando = false;
                    btnSpin.interactable = canSpin;
                });
        }
    }

    private int GetIndexOfCar()
    {
        return rouletteItems.IndexOf(rouletteItems.First(item => item.RewardData.rewardType == RewardType.Vehicle));
    }

    private void GiveReward(int premio)
    {
        if(rouletteItems[premio].RewardData.rewardType == RewardType.Vehicle)
        {
            soundVehicle.Play();
            GameManager.Instance.playerData.AddVehicleByID(rouletteItems[premio].RewardData.vehicleID, true);
        }
        else
        {
            soundPrize.Play();
            List<RewardData> rewards = new List<RewardData> { rouletteItems[premio].RewardData };
            GameManager.Instance.GiveRewardsWithPopup(rewards, true);
        }
    }

    public void CreateItems(bool isSpecial = false)
    {
        //** 1- Car
        //** 2- Chests
        //** 3- Boosters
        //** 4- Pieces
        //Get Unlocked Buildings Generators
        var piecesCandidates = new List<PieceType>();
        var generators = mapManager.GetUnlockedGenerators(true);
        foreach (var generator  in generators)
        {
            for(var i = 0; i < generator.piecesChances.Count; i++)
            {
                piecesCandidates.Add(generator.piecesChances[i].pieceType);
                //Debug.Log(generator.piecesChances[i].pieceType + " rarity " + generator.GetItemRarity(i));
            }
        }

    }


   
}


///---------------------------------------------------------------------------------------------------------------------
[System.Serializable]
public class WeightedRouletteItem : IWeightedObject
{
    public RewardData reward;
    public float weight => Weight;
    public float Weight;
}

public static class WeightedRandomExtension
{
    public static List<T> GetWeightedRandomList<T>(this List<T> list, int count) where T : IWeightedObject
    {
        var result = new List<T>();
        var weights = new List<float>();

        float totalWeight = 0;
        foreach (var item in list)
        {
            totalWeight += item.weight;
            weights.Add(totalWeight);
        }

        for (int i = 0; i < count; i++)
        {
            float randomWeight = UnityEngine.Random.Range(0, weights[weights.Count - 1]);

            for (int j = 0; j < weights.Count; j++)
            {
                if (randomWeight <= weights[j])
                {
                    result.Add(list[j]);
                    break;
                }
            }
        }

        return result;
    }

    public static int GetWeightedIndex<T>(this List<T> list) where T : IWeightedObject
    {
        var result = new List<T>();
        var weights = new List<float>();

        float totalWeight = 0;
        foreach (var item in list)
        {
            totalWeight += item.weight;
            weights.Add(totalWeight);
        }

        for (int i = 0; i < list.Count; i++)
        {
            float randomWeight = UnityEngine.Random.Range(0, weights[weights.Count - 1]);

            for (int j = 0; j < weights.Count; j++)
            {
                if (randomWeight <= weights[j])
                {
                    return j;
                }
            }
        }

        return 0;
    }
}

public interface IWeightedObject
{
    float weight { get; }
}