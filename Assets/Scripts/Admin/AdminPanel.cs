using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminPanel : MonoBehaviour
{
    public AdminGivePieces AdminGivePiecesScreen;
    private AdminGivePieces AdminGivePiecesScreenInstance;
    public void AdminShowGiveItems(bool value)
    {
        AdminGivePiecesScreenInstance ??= Instantiate(AdminGivePiecesScreen,transform);
        AdminGivePiecesScreenInstance.gameObject.SetActive(value);
    }
}
