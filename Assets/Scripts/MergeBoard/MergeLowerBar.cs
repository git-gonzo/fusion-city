using Assets.Scripts.MergeBoard;
using UnityEngine;

public class MergeLowerBar : MonoBehaviour
{
    public SideBarButton btnMap;
    public SideBarButton btnMergeMissions;
    public SideBarButton[] btnMissions;
    public ButtonMergeStore[] btnsStorage;

    public void UpdateStorageBtn(int current, int max, int gifts)
    {
        foreach (var btnStorage in btnsStorage)
        {
            btnStorage.txtItemsCount.text = $"{current}/{max}";
            btnStorage.SetBubbleCounter(gifts);
        }
    }
    public void UpdateMissionsBtn(int count, bool isGreen)
    {
        foreach (var btnMission in btnMissions)
        {
            btnMission.SetBubbleCounter(count, isGreen);
        }
    }

    public void SetMissionsVisible(bool value)
    {
        foreach (var btnMission in btnMissions)
        {
            btnMission.gameObject.SetActive(value);
        }
    }
}
