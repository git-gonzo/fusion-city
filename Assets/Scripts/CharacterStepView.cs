using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CharacterStepView : MonoBehaviour
{
    public Image charImage;
    public TextMeshProUGUI txtSpeech;
    public Button buttonNext;
    public void Init(Sprite charSprite, CharacterStoryStep step)
    {
        charImage.sprite = charSprite;
        txtSpeech.text = step.textContent;
        //txtSpeech.GetComponent<LocalizeStringEvent>().StringReference = step.localizedKey;
        txtSpeech.DOFade(1, 0.3f);
    }
}
