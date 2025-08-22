using Assets.Scripts.MergeBoard;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AddressableAssets.Addressables;

public class AdminGivePieces : MonoBehaviour
{
    public RewardItemUniversal rewardItemUniversal;
    public StorageItemController storeItemPrefab;
    public Transform piecesContainer;
    public List<PieceDiscovery> pieces;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.RemoveChildren(piecesContainer.gameObject);
        for (var i = 0; i < pieces.Count; i++)
        {
            var reward = new RewardData(new PieceDiscovery(pieces[i].pType, pieces[i].Lvl));
            var item = Instantiate(rewardItemUniversal, piecesContainer);
            item.InitReward(reward, GameManager.Instance.topBar);
            item.AddButton();
            item.ResetItemScale();
            item.SetStateNormal();
            item.amountText.gameObject.SetActive(false);
            item.transform.DOScale(Vector3.zero, 0);
            item.transform.DOScale(Vector3.one, 0.2f).SetDelay(0.01f * i);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}

