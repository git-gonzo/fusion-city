using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Cinemachine;

public class MapPointer : MonoBehaviour
{
    public CinemachineCamera camera;
    public List<PointerTypes> pointerTypes;
    public GameObject lockedPrefab;
    public MapPointerExlamation mapMissionPrefab;
    public GameObject playerPointerPrefab;
    public CloudsLockedBuilding cloudsBlockedPrefab;
    public Vector3 cloudsBlockedOffset;
    public GameObject DoingActivityPointer;
    public Animator animController;
    private BuildingType _buildingID;
    private bool isPlayerHere = false;
    private GameObject buildingPointer;
    private GameObject playerPointer;
    private MapPointerExlamation exclamationPointer;
    private int _unlockLevel;
    private CloudsLockedBuilding cloudsBlocked;

    // Start is called before the first frame update
    void Start()
    {
        if(camera == null)
            camera = GameObject.Find("CM vcamBuildingInteractable").GetComponent<CinemachineCamera>();
    }

    public void Init(BuildingType bType, int unlockLevel)
    {
        _buildingID = bType;
        _unlockLevel = unlockLevel;
        GameManager.RemoveChildren(gameObject);

        ReCheckPointer();
    }

    IEnumerator InitNextFrame()
    {
        yield return new WaitForEndOfFrame();
        if(GameManager.Instance.PlayerLevel < _unlockLevel)
        {
            buildingPointer = Instantiate(lockedPrefab, transform);
            cloudsBlocked = Instantiate(cloudsBlockedPrefab, transform.parent);
            var cloudsPos = cloudsBlocked.transform.localPosition;
            cloudsPos.y = this.transform.localPosition.y;
            cloudsPos += cloudsBlockedOffset;
            cloudsBlocked.transform.localPosition = cloudsPos;
            CheckPointer();
            yield break;
        }
        
        bool specialPointer = false;
        foreach (var p in pointerTypes)
        {
            if (p.buildingTypes.Contains(_buildingID))
            {
                buildingPointer = Instantiate(p.pointer, transform);
                //Instantiate(DoingActivityPointer, transform.parent);
                specialPointer = true;
                break;
            }
        }
        if (!specialPointer)
        {
            buildingPointer = Instantiate(pointerTypes[0].pointer, transform);
        }
        GameManager.Instance.playerData.OnPlayerLocationChange -= CheckPointer;
        GameManager.Instance.playerData.OnPlayerLocationChange += CheckPointer;
        CheckPointer();
    }

    public void ReCheckPointer()
    {
        StartCoroutine(InitNextFrame());
    }

    /* Update is called once per frame
    void Update(){
        if(PlayerData.playerLocation == buildingID)
        {
            if (!isPlayerHere)
            {
                //Set Player Pointer
                isPlayerHere = true;
                buildingPointer.SetActive(false);
                playerPointer = Instantiate(playerPointerPrefab, transform);
}
        }
        else
        {
            if (isPlayerHere)
            {
                //Set normal Pointer
                isPlayerHere = false;
                buildingPointer.SetActive(true);
                Destroy(playerPointer);
            }
        }
    }*/
    //{
    public void CheckPointer() { 
        if(PlayerData.playerLocation == _buildingID)
        {
            if (!isPlayerHere)
            {
                //Set Player Pointer
                isPlayerHere = true;
                buildingPointer.SetActive(false);
                playerPointer = Instantiate(playerPointerPrefab, transform);
                if (exclamationPointer) Destroy(exclamationPointer);
            }
        }
        else
        {
            if (isPlayerHere)
            {
                //Set normal Pointer
                isPlayerHere = false;
                buildingPointer.SetActive(true);
                Destroy(playerPointer);
                //if (exclamationPointer) Destroy(exclamationPointer);
            }
        }
    }

    public void SetPointerMission(bool ready)
    {
        if (PlayerData.playerLocation == _buildingID)
        {
            return;
        }
        //Todo, maybe differenciate between mission Ready and not ready
        if (buildingPointer != null) DestroyImmediate(buildingPointer);
        exclamationPointer = Instantiate(mapMissionPrefab, transform);
        exclamationPointer.SetReady(ready);
        buildingPointer = exclamationPointer.gameObject;
    }

    public void PlayUnlockAnim()
    {
        animController.SetTrigger("Unlock");
        buildingPointer.GetComponent<Locker3D>().PlayUnlockAnim();
        UIUtils.DelayedCall(3, ResetAnim, this);
        cloudsBlocked.CleanArea();
        Destroy(cloudsBlocked.gameObject, 5);
    }
    private void ResetAnim()
    {
        GameManager.RemoveChildren(gameObject);
        StartCoroutine(InitNextFrame());
        //Show Pointer
        animController.SetTrigger("reset");
    }
}

[System.Serializable]
public class PointerTypes
{
    public List<BuildingType> buildingTypes;
    public GameObject pointer;
}
