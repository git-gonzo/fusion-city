using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillsManager : MonoBehaviour
{
    public Action OnCareerPointUpdate;
    public TextMeshProUGUI TextMaturePoints;
    public TextMeshProUGUI TextPointsAvailable;
    public GameObject careerPrefab;
    public GameObject careerChildPrefab;
    public GameObject careersContainer;
    public List<SO_SkillType> allSkills;
    public Button MainSkill1;
    public Button MainSkill2;
    public Button MainSkill3;
    public SkillsContainer skills1;
    public SkillsContainer skills2;
    public SkillsContainer skills3;
    public GameObject skills1Children;
    public GameObject skills2Children;
    public GameObject skills3Children;
    public Sprite spriteButtonOn;
    public Sprite spriteButtonOff;

    // Skills by group
    //public const 

    public int availablePoints { get { return _availablePoints; }
        set
        {
            _availablePoints = value;
            TextPointsAvailable.text = "<color=yellow>" + value.ToString() + "</color> Skillpoints available";
            OnCareerPointUpdate?.Invoke();
            SavePoints();
        }
    }
    public int maturePoints
    {
        get { return _maturePoints; }
        set
        {
            _maturePoints = value;
            TextMaturePoints.text = value.ToString();
            SavePoints();
        }
    }

    private bool isFirstTime = true;
    private int _totalPoints;
    private int _availablePoints;
    private int _maturePoints;

    public void OnShow()
    {
        if (isFirstTime)
        {
            MainSkill1.onClick.AddListener(OnMainSkill1);
            MainSkill2.onClick.AddListener(OnMainSkill2);
            MainSkill3.onClick.AddListener(OnMainSkill3);
            LoadPoints();
            isFirstTime = false;
            skills1.Init();
            skills2.Init();
            skills3.Init();
            var p1 = GetSkillGroupPoints(SkillGroup.Art);
            var p2 = GetSkillGroupPoints(SkillGroup.Mind);
            var p3 = GetSkillGroupPoints(SkillGroup.Body);
            
            if (p2 > p1 && p2 > p3)
            {
                OnMainSkill2();
            }
            else if (p3 > p1 && p3 > p2)
            {
                OnMainSkill3();
            }
            else
            {
                OnMainSkill1();
            }
        }
    }

    private void OnMainSkill1() { ButtonOn(MainSkill1); skills1.gameObject.SetActive(true); }
    private void OnMainSkill2() { ButtonOn(MainSkill2); skills2.gameObject.SetActive(true); }
    private void OnMainSkill3() { ButtonOn(MainSkill3); skills3.gameObject.SetActive(true); }

    private void AllButtonsOff()
    {
        MainSkill1.GetComponent<Image>().sprite = spriteButtonOff;
        MainSkill2.GetComponent<Image>().sprite = spriteButtonOff;
        MainSkill3.GetComponent<Image>().sprite = spriteButtonOff;
        skills1.gameObject.SetActive(false);
        skills2.gameObject.SetActive(false);
        skills3.gameObject.SetActive(false);
    }

    private void ButtonOn(Button btn)
    {
        AllButtonsOff();
        btn.GetComponent<Image>().sprite = spriteButtonOn;
        DOTween.Kill(btn.transform);
        btn.transform.localScale = new Vector3(1, 1, 1);
        btn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);
    }

    public void SavePoints()
    {
        PlayerPrefs.SetInt("AvailableCareerPoints", _availablePoints);
        PlayerPrefs.SetInt("MaturePoints", _maturePoints);
        /*GameManager.Instance.CareersNotification.SetActive(_availablePoints > 0);
        if(_availablePoints > 0)
        {
            GameManager.Instance.CareersNotification.GetComponent<ButtonCounterNotification>().SetCounter(_availablePoints);
        }*/
    }
    public void LoadPoints()
    {
        availablePoints = PlayerPrefs.GetInt("AvailableCareerPoints");
        if(_availablePoints == 0)
        {
            //availablePoints = 1;
        }
        //maturePoints = PlayerPrefs.GetInt("MaturePoints");
    }

    public int GetSkillLevel(string skillTypeName)
    {
        return PlayerPrefs.GetInt(skillTypeName + "Points");
    }

    public int GetSkillGroupPoints(SkillGroup group)
    {
        var points = 0;
        foreach(var skill in allSkills)
        {
            if(skill.skillGroup == group)
            {
                points+= GetSkillLevel(skill.SkillTypeName);
            }
        }
        return points;
    }

    public void UpgradeSkill(string skillTypeName)
    {
        var current = PlayerPrefs.GetInt(skillTypeName + "Points");
        PlayerPrefs.SetInt(skillTypeName + "Points", ++current);
    }

    public List<SO_SkillType> GetAllSkillsOfGroup(SkillGroup group)
    {
        var skills = new List<SO_SkillType>();
        foreach(var s in allSkills)
        {
            if(s.skillGroup == group)
            {
                skills.Add(s);
            }
        }
        return skills;
    }
    public List<string> GetAllSkillsOfGroupStrings(SkillGroup group)
    {
        var skills = new List<string>();
        foreach(var s in allSkills)
        {
            if(s.skillGroup == group)
            {
                skills.Add(s.SkillTypeName);
            }
        }
        return skills;
    }

    public bool CanLearnSkill(SO_SkillType skill)
    {
        if (skill.requirement.Count > 0)
        {
            for (var i = 0; i < skill.requirement.Count; i++)
            {
                if (GetSkillLevel(skill.requirement[i].skill.SkillTypeName) < skill.requirement[i].level)
                {
                    return false;
                }
            }
        }
        return true;
    }   
    
    public bool HasEnoughSkillLevel(SO_SkillType skill, int level)
    {
        return GetSkillLevel(skill.SkillTypeName) >= level;
    }

    public bool HasEnoughSkillGroupLevel(SkillGroup group, int level)
    {
        return GetSkillGroupPoints(group) >= level;
    }


    public void OnClose()
    {
        //GameManager.Instance.ShowSkills(false);
        PlayerPrefs.SetInt("SkillsScreenClosed", 1);
    }
}


