using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillItemView : MonoBehaviour
{
    public Action OnLearn;
    public Image BGImage;
    public Image ProgressImg;
    public TextMeshProUGUI TextTitle;
    public TextMeshProUGUI TextDescrip;
    public TextMeshProUGUI TextPoints;
    public GameObject ChildrenContainer;
    public GameObject ProgressContainer;
    public GameObject ButtonLearn;
    public GameObject Requirement;
    public TextMeshProUGUI RequirementText;

    SO_SkillType skillData;
    //SkillsManager skillsManager { get => GameManager.Instance.skillsManager; }
    SkillsManager skillsManager => new SkillsManager(); 
    Color colorDescripLocked = new Color(0.6f, 0, 0, 0.6f);

    public float skillPoints { get => skillsManager.GetSkillLevel(skillData.SkillTypeName);}
    float _currentPoints;


    public void Refresh()
    {
        CheckCanLearn();
    }

    public void InitSkill(SO_SkillType data, int index, Action OnLearnCallback) {
        skillsManager.GetSkillLevel(data.SkillTypeName);
        skillsManager.OnCareerPointUpdate += Refresh;
        skillData = data;
        TextTitle.text = data.DisplayName;
        TextDescrip.text = data.skillDescription;
        TextPoints.text = skillPoints.ToString();
        ProgressImg.fillAmount = skillPoints / 5;
        CheckCanLearn();
        OnLearn = OnLearnCallback;
    }

    public void UpdateSkill()
    {
        CheckCanLearn();
    }

    public void LearnSkill()
    {
        if (skillsManager.availablePoints > 0)
        {
            TextPoints.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutExpo).OnComplete(()=> 
            {
                skillsManager.maturePoints++;
                TextPoints.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
                ProgressImg.DOFillAmount(skillPoints / 5, 0.3f);
            });
            skillsManager.availablePoints--;
            skillsManager.UpgradeSkill(skillData.SkillTypeName);
            TextPoints.text = skillPoints.ToString();
            OnLearn?.Invoke();
        }
        CheckCanLearn();
    }

    private void CheckCanLearn()
    {
        Color black = Color.black;
        black.a = 0.3f;
        Requirement.SetActive(false);
        ProgressContainer.SetActive(true);
        if (skillData.requirement.Count > 0)
        {
            for(var i = 0; i < skillData.requirement.Count; i++)
            {
                if(skillsManager.GetSkillLevel(skillData.requirement[i].skill.SkillTypeName) < skillData.requirement[i].level)
                {
                    ButtonLearn.SetActive(false);
                    RequirementText.text = skillData.requirement[i].skill.DisplayName+"-"+ skillData.requirement[i].level;
                    Requirement.SetActive(true);
                    ProgressContainer.SetActive(false);
                    black.a = 0.6f;
                    BGImage.color = black;
                    return;
                }
            }
        }
        BGImage.color = black;
        ButtonLearn.SetActive(skillsManager.availablePoints > 0);
    }

    public void ClearChildren()
    {
        for (var i = 0; i < ChildrenContainer.transform.childCount; i++)
        {
            Destroy(ChildrenContainer.transform.GetChild(i).gameObject);
        }
        /*if(careerData.careerPoints > 0)
        {
            AddCareerChildren();
        }*/
    }
    /*
    private void AddCareerChildren()
    {
        GetComponent<ContentResize>().SetHeight(careerData.hijos_SO.Count * 100 + 120, () => { careersManager.AddCareerChild(careerData.hijos_SO, ChildrenContainer); });
        DOTween.To(x => ChildrenContainer.GetComponent<VerticalLayoutGroup>().padding.bottom = (int)x, -15, 11, 0.5f);
    }*/
}
