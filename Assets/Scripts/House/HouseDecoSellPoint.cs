using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Newtonsoft.Json;

[System.Serializable]
public class MuebleOfType
{
    public MuebleType muebleType;
    public string muebleName;
    //public SO_Mueble soMueble;
    //public GameObject instance;
    public void RemoveMueble()
    {
        muebleName = "";
        //soMueble = null;
        //instance = null;
    }
}

public class MuebleSelected
{
    public MuebleOfType mueble;
    public GameObject instance;
    public int index;
    public MuebleSelected(int atIndex)
    {
        mueble = new MuebleOfType();
        index = atIndex;
    }
}


public class HouseDecoSellPoint : MonoBehaviour
{
    public string muebleID;
    public GameObject SellPoint;
    public Color SellPointColor;
    public GameObject selected;
    public GameObject preview;
    public List<SO_Mueble> muebles;

    
    public List<string> activeMueblesByName;
    public List<MuebleSelected> activeMuebles;

    List<MuebleIndex> indexes;
    LowerbarShop lowerbarShop;
    int currentIndex;
    int selectedIndex;
    bool initialized = false;
    private GameObject currentMueble;


    private void Start()
    {
        activeMuebles = new List<MuebleSelected>();
        LoadActiveMuebles();
    }

    private void LoadActiveMuebles()
    {
        var jsonMuebles = PlayerPrefs.GetString("mueblesIn_" + this.name);
        activeMueblesByName = JsonConvert.DeserializeObject<List<string>>(jsonMuebles);

        if (activeMueblesByName == null)
        {
            //Debug.Log("No hay muebles activados en zona " + this.name);
            activeMueblesByName = new List<string>();
            return;
        }
        Debug.Log("Cargando " + activeMueblesByName.Count + " muebles en zona " + this.name);
        for (var i = 0; i < activeMueblesByName.Count; i++)
        {
            var index = GetMuebleIndex(activeMueblesByName[i]);
            if (index == -1) continue;
            CreateActiveMuebleInstance(index);
        }

    }

    public void OnOpen()
    {
        lowerbarShop = GameManager.Instance.GetLowerBarShop().GetComponent<LowerbarShop>();

        if (muebles.Count == 0) return;

        Init();
        CreateIndexes();

        if (activeMueblesByName != null && activeMueblesByName.Count > 0)
        {
            MarkIndexesActive(true);
            currentIndex = GetMuebleIndex(activeMueblesByName[0]);
            Debug.Log("Loading active mueble at index " + currentIndex);
            indexes[currentIndex].IsIndex = true;
        }
        RefreshState();
    }

    void Init()
    {
        lowerbarShop.btnNext.gameObject.SetActive(muebles.Count > 1);
        lowerbarShop.btnPrev.gameObject.SetActive(muebles.Count > 1);
        ///Remove Listeners
        lowerbarShop.btnNext.onClick.RemoveAllListeners();
        lowerbarShop.btnPrev.onClick.RemoveAllListeners();
        lowerbarShop.btnSelect.onClick.RemoveAllListeners();
        lowerbarShop.btnBuy.onClick.RemoveAllListeners();
        lowerbarShop.btnClose.onClick.RemoveAllListeners();
        ///Add Listeners
        lowerbarShop.btnNext.onClick.AddListener(MoveNext);
        lowerbarShop.btnPrev.onClick.AddListener(MovePrev);
        lowerbarShop.btnBuy.onClick.AddListener(BuyItem);
        lowerbarShop.btnSelect.onClick.AddListener(SelectItem);
        lowerbarShop.btnClose.onClick.AddListener(OnClose);
    }

    public void CreateIndexes()
    {
        indexes = new List<MuebleIndex>();
        ClearChilds(lowerbarShop.itemIndexContainer.transform);
        for(var i=0; i < muebles.Count; i++)
        {
            var ind = Instantiate(lowerbarShop.itemIndexPrefab, lowerbarShop.itemIndexContainer.transform).GetComponent<MuebleIndex>();
            ind.Owned = muebles[i].owned;
            ind.Locked = !muebles[i].unlocked;
            ind.Index = i+1;
            indexes.Add(ind);
        }
    }

    void LoadItem(int index)
    {
        ClearChilds(preview.transform);
        currentMueble = Instantiate(muebles[index].prefab, preview.transform);
        currentMueble.transform.localPosition = Vector3.zero;
        CheckEnoughCurrency();
    }

    void RefreshState()
    {
        AllIndexesOff();

        MarkIndexesActive();
        indexes[currentIndex].IsIndex = true;
        lowerbarShop.btnBuy.gameObject.SetActive(!indexes[currentIndex].Owned && selectedIndex != currentIndex);
        lowerbarShop.btnSelect.gameObject.SetActive(indexes[currentIndex].Owned && !IsIndexActive());
        lowerbarShop.btnBuyText.text = muebles[currentIndex].price.ToString();
        lowerbarShop.itemTitle.text = muebles[currentIndex].muebleName;
        lowerbarShop.iconCoins.SetActive(muebles[currentIndex].currency == RewardType.Coins);
        lowerbarShop.iconGems.SetActive(muebles[currentIndex].currency == RewardType.Gems);

        selected.SetActive(IsIndexActive());
        preview.SetActive(!IsIndexActive());
    }

    void MarkIndexesActive(bool loadItem = false)
    {
        foreach (var m in activeMueblesByName)
        {
            var i = GetMuebleIndex(m);
            indexes[i].IsSelected = true;
            if(loadItem)LoadItem(i);
        }
    }

    bool IsIndexActive()
    {
        for(var i = 0; i< activeMuebles.Count; i++)
        {
            if(currentIndex == activeMuebles[i].index)
            {
                return true;
            }
        }

        return false;
    }

    bool CheckEnoughCurrency()
    {
        if ((muebles[currentIndex].currency == RewardType.Gems && PlayerData.gems < muebles[currentIndex].price) || 
            (muebles[currentIndex].currency == RewardType.Coins && PlayerData.coins < muebles[currentIndex].price))
        {
            lowerbarShop.btnBuyText.color = Color.red;
            return false;
        }
        else
        {
            lowerbarShop.btnBuyText.color = Color.white;
        }
        return true;
    }

    void AllIndexesOff()
    {
        for (var i = 0; i < indexes.Count; i++)
        {
            //indexes[i].GetComponent<MuebleIndex>().IsSelected = false;
            indexes[i].IsIndex = false;
        }
    }

    void MoveNext()
    {
        currentIndex++;
        if (currentIndex == muebles.Count) currentIndex = 0;
        LoadItem(currentIndex);
        RefreshState();
    }
    void MovePrev()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = muebles.Count-1;
        LoadItem(currentIndex);
        RefreshState();
    }
    void BuyItem()
    {
        if (!CheckEnoughCurrency()) return;
        muebles[currentIndex].owned = true;
        var r = new RewardData(muebles[currentIndex].currency, muebles[currentIndex].price * -1);
        GameManager.Instance.AddRewardToPlayer(r);
        indexes[currentIndex].Owned = true;
        RefreshState();
    }
    void SelectItem()
    {
        //Remove Activated of same type
        for(var i = 0;  i < activeMuebles.Count; i++)
        {
            if(activeMuebles[i].mueble.muebleType == muebles[currentIndex].type)
            {
                RemoveMuebleFromSavingList(activeMuebles[i].mueble.muebleName);
            }
        }

        CreateActiveMuebleInstance(currentIndex);
        RefreshState();
        //lowerbarShop.btnSelect.gameObject.SetActive(indexes[currentIndex].Owned && !IsIndexActive());
        SaveMueble();
    }

    private void CreateActiveMuebleInstance(int index)
    {
        Debug.Log("Creating Mueble " + muebles[index].muebleName + ", Index " + index);
        var typeint = (int)muebles[index].type;
        var type = muebles[index].type;
        var activeMueble = activeMuebles.Find(a => a.mueble.muebleType == type);
        if (activeMueble != null)
        {
            Destroy(activeMueble.instance);
            activeMuebles.Remove(activeMueble);
            activeMueble.index = index;
        }
        else
        {
            activeMueble = new MuebleSelected(index);
        }
        activeMueble.mueble.muebleType = type;
        activeMueble.mueble.muebleName = muebles[index].muebleName;
        activeMueble.instance = Instantiate(muebles[index].prefab, selected.transform);
        activeMueble.instance.transform.localPosition = Vector3.zero;
        activeMuebles.Add(activeMueble);
        AddMuebleToSavingList(activeMueble.mueble.muebleName);
    }

    private void AddMuebleToSavingList(string muebleName)
    {
        if (activeMueblesByName.Contains(muebleName)) return;
        activeMueblesByName.Add(muebleName);
    }
    private void RemoveMuebleFromSavingList(string muebleName)
    {
        if (activeMueblesByName.Contains(muebleName))
        {
            var oldIndex = GetMuebleIndex(muebleName);
            indexes[oldIndex].IsSelected = false;
            activeMueblesByName.Remove(muebleName);
        }
    }

    private void SaveMueble()
    {
        var jsonMuebles = JsonConvert.SerializeObject(activeMueblesByName);
        PlayerPrefs.SetString("mueblesIn_" + this.name, jsonMuebles);
    }


    private int GetMuebleIndex(string muebleName)
    {
        for (var i = 0; i < muebles.Count; i++)
        {
            if (muebles[i].muebleName == muebleName)
            {
                return i;
            }
        }
        return -1;
    }

    private void OnClose()
    {
        currentIndex = selectedIndex;
        preview.SetActive(false);
        selected.SetActive(true);
        GameManager.Instance.HideLowerBarShop();
    }


    void ClearChilds(Transform target)
    {
        for (var i = 0; i < target.childCount; i++)
        {
            Destroy(target.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        //Initialize
        if (!initialized && GameManager.Instance != null)
        {
            initialized = true;
            GameManager.Instance.OnSelectShop -= ShowSellPoint;
            GameManager.Instance.OnSelectShop += ShowSellPoint;
        }
    }

    void ShowSellPoint(bool value)
    {
        if (!SellPoint) return;
        SellPoint.SetActive(value);
        if (value)
        {
            var c = SellPointColor;
            c.a = 0;
            SellPoint.GetComponent<Renderer>().material.color = c;
            var tween = DOTween.Sequence();
            tween.SetLoops(-1);
            tween.Append(
                SellPoint.GetComponent<Renderer>().material.DOColor(SellPointColor, 1)
            );
            tween.Append(
                SellPoint.GetComponent<Renderer>().material.DOColor(c, 1)
            );
        }
        else
        {
            SellPoint.GetComponent<Renderer>().material.DOKill();
        }
    }
}
