using Assets.Scripts.MergeBoard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TutorialMapStartFromHome : TutorialBase
{
    public BuildingIteractive focusOnBuilding;

    public override void StartTutorial(TutorialSequence TutorialsContainer)
    {
        if (TutorialKey == TutorialKeys.MapStartFromHome1)
        {
            PlayerData.playerLocation = BuildingType.Hospital;
        }
        base.StartTutorial(TutorialsContainer);
        if (TutorialKey == TutorialKeys.MapGoToBuilding1 || TutorialKey == TutorialKeys.MapGoToBuilding2)
        {
            GameManager.Instance.mapManager.FocusOnBuilding(focusOnBuilding);
        }
        else if (TutorialKey == TutorialKeys.MapAfterLevelUp)
        {
            GameManager.Instance.ShowMapLowerBar(false,false);
            GameManager.Instance.ShowMapLowerBar(true);
        }
        if (buildingToTap != null)
        {
            buildingToTap.ShowArrowPointer();
            GameManager.Instance.mapManager.OnBuildingTap += CheckBuildingTapTutorial;
        }
        
    }

    public override bool CanStartTutorial()
    {
        var canStartNormal = base.CanStartTutorial();
        if (!canStartNormal) return false;

        if (TutorialKey == TutorialKeys.MapAskForJobFirstBuilding)
        {
            if (PlayerData.playerLocation != BuildingType.RestaurantSquare)
            {
                return false;
            }
        }
        else if (TutorialKey == TutorialKeys.MapAskForJobSecondBuilding)
        {
            if (PlayerData.playerLocation != BuildingType.BicycleShop)
            {
                return false;
            }
        }
        else if (TutorialKey == TutorialKeys.MapEnterAppartment)
        {
            if (PlayerData.playerLocation != BuildingType.TutorialHouse)
            {
                return false;
            }
        }
        else if (TutorialKey == TutorialKeys.MapGoBackToAppartment)
        {
            if (PlayerPrefs.GetInt("SkillsScreenClosed") == 0)
            {
                return false;
            }
        }
        else if (TutorialKey == TutorialKeys.MapFirsJobCompleted && canStartNormal)
        {
            
            if (steps != null && steps.Count > 0 && steps[0].targetToTap != null)
            {
                steps[0].targetToTap.SetActive(true);
            }
        }
        else if (TutorialKey == TutorialKeys.MapGoToBuilding2)
        {
            if (PlayerPrefs.GetInt("buildingUnlocked") == 0 || !GameManager.Instance.mapManager.playerInputEnable)
            {
                return false;
            }
        }
        else if (TutorialKey == TutorialKeys.MergeBuyBox && !GameManager.Instance.MergeManager.IsBoardActive)
        {
            Debug.Log("Time for Tutorial:" + TutorialKey + ", but no merge board active");
            ResetTutorial();
            return false;
        }
        return canStartNormal;
    }

    private void CheckBuildingTapTutorial(BuildingType buildingType)
    {
        if(buildingType == buildingToTap.buildingData.buildingType)
        {
            GameManager.Instance.mapManager.OnBuildingTap -= CheckBuildingTapTutorial;
            _tutorialsContainer.NextStep();
            buildingToTap.HideArrowPointer();
        }
    }

    public override void CompleteTutorial()
    {
        base.CompleteTutorial();
        if (TutorialKey == TutorialKeys.MapAfterLevelUp)
        {

        }
        /*if (TutorialKey == TutorialKeys.MergeBuyBox)
        {
            MergeMissionConfig m = new MergeMissionConfig();
            m.GenerateMission(new PieceState(PieceType.Drink1, 1), 2, 1);
            GameManager.Instance.MergeManager.boardController.AddForcedMission(m);
        }*/
    }
}
