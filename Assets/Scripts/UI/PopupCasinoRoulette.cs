using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.MergeBoard;
using System;

public class PopupCasinoRoulette : PopupBase
{
    public RouletteSpin roulette;
    
    public Button btnBack;
    
    public Button btnBuyTickets;
    public Button btnBuyTicketsCommonx1;
    public Button btnBuyTicketsCommonx5;
    public Button btnBuyTicketsSpecialx1;
    public Button btnBuyTicketsSpecialx5;
    public Button btnRouletteComon;
    public Button btnRouletteSpecial;
    public VehicleThumbnail vehicleComon;
    public VehicleThumbnail vehicleSpecial;
    public TextMeshProUGUI txtTicket1;
    public TextMeshProUGUI txtTicket2;
    public List<TextMeshProUGUI> txtTicketsCommon;
    public List<TextMeshProUGUI> txtTicketsSpecial;
    public Transform ticketCommonBuyTarget;
    public Transform ticketSpecialBuyTarget;
    public GameObject buttonsBuy;
    public GameObject rouletteView;
    public AudioSource btnClick;
    public AudioSource onBuyTickets;
    public AudioSource soundOpenRoulette;
    public AudioSource soundAddTicket;


    private int previousScreen = 0; //0 - Selection, 1 - Roulette 
    private int countCommon= 0;
    private int countSpecial = 0;
    private CanvasGroup canvasBtnRCommon;
    private CanvasGroup canvasBtnRSpecial;
    private CanvasGroup canvasBuyButtons;

    private Vector3 buttonsBuyInitialPos;
    private bool _isSpecial;

    GameConfigMerge Config => GameManager.Instance.gameConfig;

    private void Awake()
    {
        buttonsBuyInitialPos = buttonsBuy.transform.position;
    }
    public override void Show()
    {
        
        //base.Show();
        HideRouletteView();
        HideBuyButtons();
        ShowSelectionView();
        btnClose.onClick.AddListener(OnClose);
    }


    private void ShowSelectionView()
    {
        previousScreen = 0;
        canvasBtnRCommon ??= btnRouletteComon.GetComponent<CanvasGroup>();
        canvasBtnRSpecial ??= btnRouletteSpecial.GetComponent<CanvasGroup>();
        canvasBtnRCommon.DOFade(0, 0);
        canvasBtnRSpecial.DOFade(0, 0);
        txtTicket1.DOFade(0, 0);
        txtTicket2.DOFade(0, 0);
        vehicleComon.GenerateThumbnail(GetVehicleID(false));
        vehicleSpecial.GenerateThumbnail(GetVehicleID(true));
        btnRouletteComon.transform.DOKill();
        btnRouletteComon.transform.DOScale(1,0);
        btnRouletteSpecial.transform.DOKill();
        btnRouletteSpecial.transform.DOScale(1,0);
        btnRouletteComon.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 1, 0).SetDelay(0.3f);
        btnRouletteSpecial.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 1).SetDelay(0.5f);
        canvasBtnRCommon.DOFade(1, 0.2f).SetDelay(0.3f);
        canvasBtnRSpecial.DOFade(1, 0.3f).SetDelay(0.5f); 
        
        txtTicket1.DOFade(1, 0.3f);
        txtTicket2.DOFade(1, 0.3f).SetDelay(0.3f);

        btnRouletteComon.onClick.RemoveAllListeners();
        btnRouletteSpecial.onClick.RemoveAllListeners();
        btnBuyTickets.onClick.RemoveAllListeners();
        btnBuyTickets.onClick.AddListener(ShowBuyButtons);
        btnRouletteComon.onClick.AddListener(OpenRuletteCommon);
        btnRouletteSpecial.onClick.AddListener(OpenRuletteSpecial);

        var pCommon = new PieceDiscovery(PieceType.RouleteTicketCommon, 3);
        var pSpecial = new PieceDiscovery(PieceType.RouleteTicketSpecial, 3);
        countCommon = GameManager.Instance.mergeModel.CountItemInStorage(pCommon, true);
        countSpecial = GameManager.Instance.mergeModel.CountItemInStorage(pSpecial, true);
        foreach ( var t in txtTicketsCommon) t.text = $"x{countCommon}";
        foreach (var t in txtTicketsSpecial) t.text = $"x{countSpecial}";
        btnBuyTickets.gameObject.SetActive(true);
        btnClose.gameObject.SetActive(true);
    }

    private void OpenRuletteCommon()
    {
        soundOpenRoulette.Play();
        HideSelectionView();
        ShowRouletteView();
    }
    private void OpenRuletteSpecial()
    {
        soundOpenRoulette.Play();
        HideSelectionView();
        ShowRouletteView(true);
    }

    private void HideSelectionView()
    {
        btnRouletteComon.onClick.RemoveAllListeners();
        btnRouletteSpecial.onClick.RemoveAllListeners();
        btnBuyTickets.onClick.RemoveAllListeners();
        canvasBtnRCommon.DOFade(0, 0.2f).SetDelay(0.1f);
        canvasBtnRSpecial.DOFade(0, 0.2f);
        txtTicket1.DOFade(0, 0.3f);
        txtTicket2.DOFade(0, 0.3f).SetDelay(0.1f); 
    }

    private void ShowRouletteView(bool isSpecial = false)
    {
        _isSpecial = isSpecial;
        roulette.ShowRoulette(isSpecial);
        roulette.btnBackRoulette.onClick.RemoveAllListeners();
        btnBuyTickets.onClick.RemoveAllListeners();
        btnBuyTickets.onClick.AddListener(ShowBuyButtons);
        roulette.btnBackRoulette.onClick.AddListener(()=>HideRouletteView());
        btnBuyTickets.gameObject.SetActive(true);
        previousScreen = 1;
        roulette.gameObject.SetActive(true);
        btnClose.gameObject.SetActive(true);

    }
    private void HideRouletteView(bool comesFromBuy = false)
    {
        roulette.Hide();
        roulette.btnSpin.onClick.RemoveAllListeners();
        if(!comesFromBuy) ShowSelectionView();
    }

    public void ShowBuyButtons()
    {
        if (roulette.girando) return;
        btnClick.Play();
        if (rouletteView.activeSelf)
            HideRouletteView(true);
        else
            HideSelectionView();
        btnBuyTickets.gameObject.SetActive(false);
        canvasBuyButtons ??= buttonsBuy.GetComponent<CanvasGroup>();
        canvasBuyButtons.DOFade(0, 0);
        btnClose.gameObject.SetActive(false);
        buttonsBuy.gameObject.SetActive(true);
        btnBack.gameObject.SetActive(true);
        btnBuyTicketsCommonx1.onClick.RemoveAllListeners();
        btnBuyTicketsCommonx5.onClick.RemoveAllListeners();
        btnBuyTicketsSpecialx1.onClick.RemoveAllListeners();
        btnBuyTicketsSpecialx5.onClick.RemoveAllListeners();
        btnBack.onClick.RemoveAllListeners();
        canvasBuyButtons.DOFade(1, 0.2f).SetDelay(0.2f).OnStart(() =>
        {
            buttonsBuy.transform.DOKill();
            buttonsBuy.transform.DOMoveX(buttonsBuyInitialPos.x + 50, 0);
            buttonsBuy.transform.DOMoveX(buttonsBuyInitialPos.x, 0.2f);
        }).OnComplete(() => 
        {
            btnBack.onClick.AddListener(HideBuyButtons);
            btnBuyTicketsCommonx1.onClick.AddListener(() => BuyTickets(0));
            btnBuyTicketsCommonx5.onClick.AddListener(() => BuyTickets(1));
            btnBuyTicketsSpecialx1.onClick.AddListener(() => BuyTickets(2));
            btnBuyTicketsSpecialx5.onClick.AddListener(() => BuyTickets(3));
        });
    }

    public void HideBuyButtons()
    {
        btnBack.onClick.RemoveAllListeners();
        btnBack.gameObject.SetActive(false);    
          
        canvasBuyButtons.DOFade(0, 0.2f).OnComplete(()=> { 
            buttonsBuy.gameObject.SetActive(false); 
            if (previousScreen == 1) ShowRouletteView(_isSpecial);
        });
        if (previousScreen == 0) ShowSelectionView();
        buttonsBuy.transform.DOMoveX(buttonsBuyInitialPos.x - 50,0.2f);
    }

    private void BuyTickets(int ticketType)
    {
        int cost = 0;
        Transform from = null;
        Transform to = null;
        RewardData reward = null;
        if (ticketType == 0)
        {
            cost = 10;
            reward = new RewardData(new PieceDiscovery(PieceType.RouleteTicketCommon, 3),1);
            from = btnBuyTicketsCommonx1.transform;
            to = ticketCommonBuyTarget;
        }
        else if (ticketType == 1)
        {
            cost = 40;
            reward = new RewardData(new PieceDiscovery(PieceType.RouleteTicketCommon, 3), 5);
            from = btnBuyTicketsCommonx5.transform;
            to = ticketCommonBuyTarget;
        }
        else if (ticketType == 2)
        {
            cost = 50;
            reward = new RewardData(new PieceDiscovery(PieceType.RouleteTicketSpecial, 3), 1);
            from = btnBuyTicketsSpecialx1.transform;
            to = ticketSpecialBuyTarget;
        }
        else if (ticketType == 3)
        {
            cost = 200; 
            reward = new RewardData(new PieceDiscovery(PieceType.RouleteTicketSpecial, 3), 5);
            from = btnBuyTicketsSpecialx5.transform;
            to = ticketSpecialBuyTarget;
        }

        if (GameManager.Instance.TryToSpend(cost,RewardType.Gems))
        {
            onBuyTickets.Play();
            var prefab = Config.mergeConfig.GetPiecePrefab(reward.mergePiece);
            GameManager.Instance.AddRewardToPlayer(reward);
            //Spawn Tickets
            for (int i = 0; i < reward.amount; i++)
            {
                var p = Instantiate(prefab,transform);
                p.transform.position = from.position;
                var pos = from.position;
                pos.y += UnityEngine.Random.Range(175, 350);
                pos.x += UnityEngine.Random.Range(-150, 150);
                p.transform.DOMove(pos, 0.2f).SetEase(Ease.OutQuint).SetDelay(i*0.1f);
                p.transform.DOScale(1.3f, 0.2f).SetEase(Ease.OutExpo).SetDelay(i*0.1f);
                p.transform.DOMove(to.position, 0.4f).SetEase(Ease.InQuint).SetDelay(0.3f + i * 0.1f);
                p.transform.DOScale(1, 0.4f).SetEase(Ease.OutExpo).SetDelay(0.3f + i * 0.1f)
                    .OnComplete(() => 
                    {
                        if(ticketType <=1)
                        {
                            countCommon++;
                            foreach (var t in txtTicketsCommon)
                            {
                                t.text = $"x{countCommon}";
                                TicketPunch(t.transform, to);
                                soundAddTicket.Play();
                            }
                        }
                        else
                        {
                            countSpecial++;
                            foreach (var t in txtTicketsSpecial)
                            {
                                t.text = $"x{countSpecial}";
                                TicketPunch(t.transform, to);
                                soundAddTicket.Play();
                            }
                        }
                        Destroy(p); 
                    }) ;
            }
        }
    }
    private void TicketPunch(Transform text, Transform ticket)
    {
        text.DOKill();
        text.DOScale(1, 0);
        text.DOPunchScale(Vector3.one, 0.3f, 1);
        ticket.DOKill();
        ticket.DOScale(1, 0);
        ticket.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1);
    }

    protected override void OnClose()
    {
        base.OnClose();
        GameManager.Instance.ShowRouletteScreen(false);
    }

    public int GetVehicleID(bool special)
    {
        var day = (int)DateTime.Now.DayOfWeek;
        if (day == 0) day = 7;
        var key = $"RouletteConfig{(special?"Special":"Common")}{day}";
        var rouletteConfig = Config.RouletteConfigsDict.Find(r => r.key == key).refConfig;

        return rouletteConfig.rouletteItems.Find(i => i.reward.rewardType == RewardType.Vehicle).reward.vehicleID;
    }
}