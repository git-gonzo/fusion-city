using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillsContainer : MonoBehaviour
{
    public SkillGroup skillGroup;
    public GameObject listContainer;
    public GameObject skillPrefab;
    public TextMeshProUGUI skillGroupPoints;

    //SkillsManager skillsManager { get => GameManager.Instance.skillsManager; }
    SkillsManager skillsManager => GameManager.Instance.skillsManager;
    List<SkillItemView> skillItems = new List<SkillItemView>();

    public void Init()
    {
        ClearChildren();
        AddSkills();
        Refresh();
    }

    public void Refresh()
    {
        //skillGroupPoints.text = GameManager.Instance.skillsManager.GetSkillGroupPoints(skillGroup).ToString();
        foreach(var item in skillItems)
        {
            item.UpdateSkill();
        }
    }

    public void ClearChildren()
    {
        for (var i = 0; i < listContainer.transform.childCount; i++)
        {
            Destroy(listContainer.transform.GetChild(i).gameObject);
        }
    }

    public void AddSkills()
    {
        var skills = skillsManager.GetAllSkillsOfGroup(skillGroup);
        skills.Sort((a,b)=> 
        {
            var aCanLearn = skillsManager.CanLearnSkill(a);
            var bCanLearn = skillsManager.CanLearnSkill(b);
            if (aCanLearn && !bCanLearn) return -1;
            if (!aCanLearn && bCanLearn) return 1;

            var pointsA = skillsManager.GetSkillLevel(a.SkillTypeName);
            var pointsB = skillsManager.GetSkillLevel(b.SkillTypeName);
            if (pointsA > pointsB) return -1;
            if (pointsB > pointsA) return 1;

            return 0;
        });
        for (var i = 0; i < skills.Count; i++)
        {
            var skill = Instantiate(skillPrefab, listContainer.transform).GetComponent<SkillItemView>();
            skill.InitSkill(skills[i], i, ()=>
            { //OnLearn
                Refresh();
            });
            skillItems.Add(skill);
        }
    }

}
