using Assets.Scripts.MergeBoard;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public enum TutorialKeys
{
    PlayerSelection,
    HouseWelcome,
    HouseAttributtes,
    MapButton,
    MapStartFromHome1,
    MapStartFromHome2,
    MapGoToBuilding1,
    MapAskForJobFirstBuilding,
    MapDoFirstJob,
    MapFirsJobCompleted,
    MapGoToBuilding2,
    MapAskForJobSecondBuilding,
    MapDoSecondJob,
    MapAfterLevelUp,
    MapGoBackToAppartment,
    MapEnterAppartment,
    HouseAfterMap,
    MergeBoardTapGenerator,
    MergeBoardDoFirstMerge,
    MergeBoardDoSecondMerge,
    MergeBoardOpenMission,
    MergeBoardCompleteMission,
    MergeBoardSelectXP,
    MergeBoardCollectXP,
    MergeDoMoreMissions,
    MergeRefillGenerator,
    ChangeName,
    ChangeAvatar,
    MergeSelectBox,
    MergeBuyBox,
    MergeNewMission,
    MergeNewGenerator,
    MergeDrinks,
    MergeCompleteMission2,
    MergeSelectBox2,
    MergeBuyBox2,
    MergeSpawnFromChest,
    MergeCompleteOrders,
    MergeStorageButton,
    Deliveries,
    GiveVehicle,
    MergeBoardTapGenerator2,
    MergeBoardDoMergePatatas,
    MergeBoardDoMergePatatas2,
    MergeBoardCompleteMission2,
    MergeBoardSelectXP2,
    MergeBoardCollectXP2,
    Storage,
    StorageRewards
}

public class TutorialManager : MonoBehaviour
{
    public static int OrdersCompletedForFirstMapMission = 6;
    public TutorialSequence TutorialsContainer;
    public List<TutorialBase> tutorials;
    [SerializeField] GameObject _overlay;
    [SerializeField] TutorialBase _tutorialDeliveries;
    [SerializeField] TutorialBase _tutorialRefill;
    [SerializeField] TutorialBase _tutorialStorage;
    [SerializeField] TutorialBase _tutorialGiveVehicle;
    [SerializeField] TutorialBase _tutorialChangeName;
    [SerializeField] TutorialBase _tutorialChangeAvatar;

    // Start is called before the first frame update
    void Start()
    {
        TutorialsContainer.gameObject.SetActive(false);
    }


    public async Task CheckTutorials()
    {
        for(var i = 0; i< tutorials.Count; i++)
        {
            if(tutorials[i].CanStartTutorial())
            {
                tutorials[i].StartTutorial(TutorialsContainer);
                if(i == 0)
                {
                    //Set ResetVersion as current version, meaning no need to reset
                    await Globals.IsConfigLoaded();
                    if (RemoteConfigService.Instance.appConfig.HasKey("AppVersionBelowReset"))
                    {
                        var configVersion = RemoteConfigService.Instance.appConfig.GetString("AppVersionBelowReset");
                        var data = new Dictionary<string, object> { { "ResetVersion", configVersion } };
                        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                        Debug.Log("ResetVersion Cloud Updated");
                    }
                }
                return;
            }
            if (i == 0)
            {
                //Check if a Player Reset is needed
                await Globals.IsConfigLoaded();
                if (RemoteConfigService.Instance.appConfig.HasKey("AppVersionBelowReset") && !Globals.minVersionResetChecked)
                {
                    var needsReset = await NeedsResetVersion();
                    if(needsReset)
                    {
                        Debug.Log("FORCE RESET HERE");
                        GameManager.Instance.ShowResetProgressPopup();
                    }
                }
            }
        }
        if(!IsTutorialRunning)TutorialsContainer.gameObject.SetActive(false);
    }

    private async Task<bool> NeedsResetVersion()
    {
        try
        {
            Globals.minVersionResetChecked = true;
            var configVersion = RemoteConfigService.Instance.appConfig.GetString("AppVersionBelowReset");


            var resetVersion = "0.1.1";
            var results = await CloudSaveService.Instance.Data.Player.LoadAsync(
                    new HashSet<string> { "ResetVersion" }
                );
            foreach (var result in results)
            {
                if (result.Key == "ResetVersion")
                {
                    resetVersion = result.Value.Value.GetAs<string>(); 
                    //Debug.Log($"ResetVersion in Cloud {resetVersion}");
                } 
            }
            if (Globals.IsVersionLower(resetVersion, configVersion))
            {
                //Debug.Log($"ResetVersion in Cloud {resetVersion} is lower than config {configVersion} NEEDS RESET");
                return true;
            }
            //Debug.Log($"ResetVersion in Cloud {resetVersion} is like config {configVersion} NO NEED To Reset");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
            return false;
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
            return false;
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
            return false;
        }
        return false;
    }

    public void ResetTutorials()
    {
        for (var i = 0; i < tutorials.Count; i++)
        {
            PlayerPrefs.DeleteKey("Tutorial"+tutorials[i].TutorialKey);
        }
        PlayerPrefs.SetInt("SkillsScreenClosed", 0);
        PlayerPrefs.SetInt("PopupMissionClosed", 0);
        PlayerPrefs.SetInt("PopupMissionOpen", 0);
    }

    public void MarkTutorialNotDone(TutorialKeys key)
    {
        tutorials.Find(t => t.TutorialKey == key).MarkTutorialNotDone();
    }
    public void ResetTutorialPlayerSelection()
    {
        PlayerPrefs.DeleteKey("Tutorial"+ TutorialKeys.PlayerSelection);
        PlayerPrefs.SetInt("playerNameNumChanges", 0);
    }

    public bool IsTutorialRunning=> ActiveTutorial()!=null;

    public TutorialBase ActiveTutorial()
    {
        for (var i = 0; i < tutorials.Count; i++)
        {
            if (tutorials[i].isRunning)
            {
                return tutorials[i];
            }
        }
        if (_tutorialDeliveries.isRunning) return _tutorialDeliveries;
        if (_tutorialStorage.isRunning) return _tutorialStorage;
        if (_tutorialRefill.isRunning) return _tutorialRefill;
        if (_tutorialGiveVehicle.isRunning) return _tutorialGiveVehicle;
        if (_tutorialChangeName.isRunning) return _tutorialChangeName;
        if (_tutorialChangeAvatar.isRunning) return _tutorialChangeAvatar;
        return null;
    }

    public static bool IsTutoCompleted(TutorialKeys tutoKey)
    {
        if (PlayerPrefs.GetInt("Tutorial" + tutoKey) == 1)
        {
            return true;
        }
        return false;
    }

    public void Admin_RemoveTutorial_HouseAfterMap()
    {
        PlayerPrefs.DeleteKey("Tutorial"+TutorialKeys.HouseAfterMap);
    }

    public void StartTutorialDeliveries()
    {
        if (_tutorialDeliveries.IsThisTutoCompleted()) return; 
        _tutorialDeliveries.StartTutorial(TutorialsContainer);
    }
    public void StartTutorialStorage()
    {
        if (_tutorialStorage.IsThisTutoCompleted()) return;
        _tutorialStorage.StartTutorial(TutorialsContainer);
    }
    public void StartTutorialRefill()
    {
        if (_tutorialRefill.IsThisTutoCompleted()) return;
        _tutorialRefill.StartTutorial(TutorialsContainer);
    }
    public void StartTutorialChangeName()
    {
        _tutorialChangeName.StartTutorial(TutorialsContainer);
    }
    public void StartTutorialChangeAvatar()
    {
        _tutorialChangeAvatar.StartTutorial(TutorialsContainer);
    }
    public void StartTutorialGiveVehicle()
    {
        if(!_tutorialGiveVehicle.IsThisTutoCompleted())
            _tutorialGiveVehicle.StartTutorial(TutorialsContainer);
    }

    public void Update()
    {
        if (!IsTutorialRunning) return;
        ActiveTutorial()?.CheckTutoEnd();
    }

    internal void ShowOverlay()
    {
        _overlay.SetActive(true);
        _overlay.GetComponent<Image>().DOFade(0, 0);
        _overlay.GetComponent<Image>().DOFade(0.8f, 0.3f);
    }

    internal void HideOverlay()
    {
        _overlay.GetComponent<Image>().DOFade(0, 0.3f).OnComplete(()=> _overlay.SetActive(false));
    }
}

[System.Serializable]
public class TutorialStep
{
    public bool hideTextBubble;
    public bool hideContinueButton;
    [HideIf("hideTextBubble")]
    public LocalizedString localizedKey;
    public GameObject targetToTap;
    [ShowIf("targetToTap")]public bool useHandPointer = false;
    [ShowIf("@this.targetToTap && this.useHandPointer")]public Vector3 handPosition;
    [ShowIf("@this.targetToTap && !this.useHandPointer")]public bool pointerBounceOnTop = true;
    [HideInInspector] public string translation;
}

[System.Serializable]
public class Tutorial
{
    public string TutorialKey;
    public int minLevel = 0;
    public TutorialSequence tutorialSequence;
    public List<TutorialStep> steps;
}

[System.Serializable]
public class TutorialMergeStep
{
    public bool hideTextBubble;
    public bool hideContinueButton;
    [HideIf("hideTextBubble")]
    public LocalizedString localizedKey;
    public TutorialMergeType stepType;
    public TutorialMergeCompleteType completeType;
    public PieceState targetPiece;
    public PieceAmount result;

    [HideInInspector] public string translation;
}

[System.Serializable]
public class PieceAmount
{
    public PieceState pieceType;
    public int amount;
    public int Level => pieceType.pieceLevel;
    public PieceType Type => pieceType.pieceType;
}

public enum TutorialMergeType
{
    Generator,
    Merge,
    Mission,
    Selection
}
public enum TutorialMergeCompleteType
{
    PieceOnBoard,
    PieceSelected
}
