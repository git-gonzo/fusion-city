using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeopleContainer : MonoBehaviour
{
    public GameObject listItemPrefab;
    public Transform listContainer;
    public TextMeshProUGUI txtTitle;

    
    public void FillPeopleInBuilding(BuildingType buildingType)
    {
        GameManager.Instance.mapManager.playerInputEnable = false;   
        var building = GameManager.Instance.mapManager.GetBuildingInteractiveFromType(buildingType);
        
        //SET TITLE
        if (GameServerConfig.Instance.ConfigHasBuilding(buildingType))
            GameServerConfig.Instance.SetBuildingLocTitle(buildingType, txtTitle, "_name");
        else
            txtTitle.text = building.buildingData.buildingName;

        GameManager.RemoveChildren(listContainer.gameObject);
        foreach (var def in building._playersInBuilding)
        {
            if (def.isPlayer) continue; 
            var item = Instantiate(listItemPrefab, listContainer).GetComponent<LeaderBoardItenFameView>();
            item.Init(def);
            item.SetPeopleCallback(() => 
            {
                GameManager.Instance.mapManager.popupOtherPlayer.Show(def);
                OnClose();
            });
        }
    }

    public void OnClose()
    {
        GameManager.Instance.mapManager.playerInputEnable = true;
        gameObject.SetActive(false);
    }
}
