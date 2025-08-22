using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GymMiniGame : MonoBehaviour
{
    public GameObject MinigameButtons;
    public GameObject MinigameProgressBarContainer;
    public Button buttonLeft;
    public Button buttonRight;
    public Image progressBar;
    public TextMeshProUGUI timeLeftText;
    [Range(0,10)]
    public float diffculty = 2;
    [Range(10, 200)]
    public int duration;

    bool miniGameEnabled;
    Animator _controller;
    Color barColor;
    float startTime;
    float timeLeft;

    float energy;
    // Start is called before the first frame update
    void Start()
    {
        EnableUI(false);
        buttonLeft.onClick.AddListener(OnLeftClick);
        buttonRight.onClick.AddListener(OnRightClick);
        MinigameButtons.GetComponent<CanvasGroup>().DOFade(0, 0);
        MinigameProgressBarContainer.GetComponent<CanvasGroup>().DOFade(0, 0);
        buttonLeft.transform.position += Vector3.left * 100;
        buttonRight.transform.position += Vector3.right * 100;
        MinigameProgressBarContainer.transform.position += Vector3.up * 100;
    }

    public void Init(Animator controller)
    {
        GameManager.Instance.ShowBars(false);
        EnableUI(true);
        AnimateUI(true);
        _controller = controller;
        energy = 0;
        timeLeft = 60;
        miniGameEnabled = true;
        barColor = new Color(1, 0, 0);
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!miniGameEnabled || _controller == null) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            energy += 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _controller.SetBool("KeepFlexiones", true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _controller.SetBool("KeepFlexiones", false);
        }
        timeLeft = duration - (Time.time - startTime);
        timeLeftText.text = UIUtils.FormatTime(timeLeft);
        if (timeLeft <= 0) {
            MinigameEnd();
        }
        energy -= (0.05f * Time.deltaTime) + (energy* diffculty*0.005f);
        energy = Mathf.Clamp(energy,0,10);
        _controller.SetInteger("Performance", Mathf.RoundToInt(energy));
        progressBar.fillAmount = energy / 10;
        barColor.r = energy<5? 1: 1 - (energy-5)*0.1f;
        barColor.g = 0 + energy*0.2f;
        progressBar.color = barColor;
    }

    private void OnLeftClick()
    {
        energy += 0.2f;
    }
    private void OnRightClick()
    {
        energy += 0.2f;
    }

    private void EnableUI(bool value = false)
    {
        MinigameButtons.SetActive(value);
        MinigameProgressBarContainer.SetActive(value);
        timeLeftText.gameObject.SetActive(value);
    }

    private Sequence AnimateUI(bool animateIn) { 
        var s = DOTween.Sequence();
        if (animateIn)
        {
            s.Insert(0.2f, MinigameButtons.GetComponent<CanvasGroup>().DOFade(1, 0.2f));
            s.Insert(0.1f, buttonLeft.transform.DOMove(buttonLeft.transform.position - Vector3.left * 100, 0.7f).SetEase(Ease.OutBack));
            s.Insert(0.2f, buttonRight.transform.DOMove(buttonRight.transform.position - Vector3.right * 100, 0.7f).SetEase(Ease.OutBack));
            s.Insert(0.3f, MinigameProgressBarContainer.GetComponent<CanvasGroup>().DOFade(1, 0.5f));
            s.Insert(0.3f, MinigameProgressBarContainer.transform.DOMove(MinigameProgressBarContainer.transform.position - Vector3.up * 100, 0.7f).SetEase(Ease.OutBack));
        }
        else
        {
            s.Insert(0.6f, MinigameButtons.GetComponent<CanvasGroup>().DOFade(0, 0.2f));
            s.Insert(0.1f, buttonLeft.transform.DOMove(buttonLeft.transform.position + Vector3.left * 100, 0.7f).SetEase(Ease.InBack));
            s.Insert(0.2f, buttonRight.transform.DOMove(buttonRight.transform.position + Vector3.right * 100, 0.7f).SetEase(Ease.InBack));
            s.Insert(0.7f, MinigameProgressBarContainer.GetComponent<CanvasGroup>().DOFade(0, 0.3f));
            s.Insert(0.3f, MinigameProgressBarContainer.transform.DOMove(MinigameProgressBarContainer.transform.position + Vector3.up * 100, 0.7f).SetEase(Ease.InBack));
        }
        return s;
    }

    private void MinigameEnd()
    {
        AnimateUI(false).OnComplete(()=> { EnableUI(false); });
        //TODO FIVE REWARDS
        GameManager.Instance.ShowBars(true);

    }
}
