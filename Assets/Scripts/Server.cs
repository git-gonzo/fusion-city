#define ENABLE_UPDATE_FUNCTION_CALLBACK
//#define ENABLE_LATEUPDATE_FUNCTION_CALLBACK
//#define ENABLE_FIXEDUPDATE_FUNCTION_CALLBACK


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Assets.Scripts.MergeBoard;
using Unity.Services.CloudSave;
using Newtonsoft.Json;
using Unity.Services.Authentication;

public enum ActionType
{
    Unknown = 0,
    ShopPurchase = 1,
    ShopVideoAd = 2,
	JobComplete = 3,
	ActivityComplete = 4,
	SkipTravel = 5,
	SkipJob = 6,
	SkipActivity = 7,
	StartActivity= 8,
	BuyVehicle=9,
	BuyBuilding=10,
	UpgradeBuilding=11
}

public class Server :MonoBehaviour
{
    public Action<string> OnQueryResult;
    public Action<bool> OnGiftSent;
     private MergeBoardModel mergeModel => GameManager.Instance.mergeModel;
    public IEnumerator AfterLogin()
    {

        while (!GameServerConfig.Instance || !GameServerConfig.Instance.isLogedin)
        {
            yield return null;
        }
        Debug.Log("After login");
        JSONArray result = GameServerConfig.Instance.resultLogin;
        //TODO: If player was not in the server, send the data
        if (result[0].AsObject["result"] == "NewPlayer")
        {
            //Debug.Log("Bien, nuevo playerID = " + result[0].AsObject["playerID"]);
            PlayerData.playerID = result[0].AsObject["playerID"];
        } 
        else if (result[0].AsObject["result"] == "Notification")
        {
            //Debug.Log("Notification = " + result[0].AsObject["message"]);
            GameManager.Instance.PopupsManager.ShowPopupYesNo("Attention", result[0].AsObject["message"],PopupManager.PopupType.ok );
            if (result[0].AsObject["goldUpdated"] == 1) PlayerData.coins = result[0].AsObject["newGold"];
            if (result[0].AsObject["gemsUpdated"] == 1) PlayerData.gems = result[0].AsObject["newGems"];
            if (result[0].AsObject["fameUpdated"] == 1) PlayerData.famePoints = result[0].AsObject["newFame"];
            if (result[0].AsObject["levelUpdated"] == 1) GameManager.Instance.playerData.level = result[0].AsObject["newLevel"];
        }
    }
    
    public void SendAction(ActionType action, RewardType resource, int amount, int targetID = 0)
    {
        string url = "https://mylittlelifesim.com/game/sendAction.php?playerID="
                + PlayerData.playerID
                + "&action=" + (int)action
                + "&resource=" + (int)resource
                + "&amount=" + amount
                + "&targetID=" + targetID;
        StartCoroutine(SendRequest(url));
    }

    [Serializable]
    public class PendingGift 
    {
        public string sender;
        public int itemType;
        public int itemLevel;
    }
    public async void CheckPendingGifts()
    {
        await Globals.IsSignedIn();
        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "pendingGifts"});
        var pendingGifts = new List<PendingGift>();
        foreach (var result in results)
        {
            if (result.Key == "pendingGifts")
            {
                pendingGifts = JsonConvert.DeserializeObject<List<PendingGift>>(result.Value.Value.GetAsString());
            }
        }
        if (pendingGifts.Count == 0) return;

        var rewards = new List<RewardData>();
        var players = new List<String>();
        foreach (var result in pendingGifts)
        {
            var pieceGift = new PieceDiscovery((PieceType)result.itemType, result.itemLevel);
            rewards.Add(new RewardData(pieceGift));
            if(!players.Contains(result.sender)) players.Add(result.sender);
        }
        await CloudSaveService.Instance.Data.Player.DeleteAsync("pendingGifts");
        GameManager.Instance.PopupsManager.ShowGiftStealReceivedPopup(rewards, players, true);
    }

    [Serializable]
    public class PendingSteal
    {
        public string robbedBy;
        public int itemType;
        public int itemLevel;
    }
    public async void CheckPendingSteal()
    {
        await Globals.IsSignedIn();
        var results = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "pendingSteal" });
        var pendingSteal = new List<PendingSteal>();
        foreach (var result in results)
        {
            if (result.Key == "pendingSteal")
            {
                pendingSteal = JsonConvert.DeserializeObject<List<PendingSteal>>(result.Value.Value.GetAsString());
            }
        }
        if (pendingSteal.Count == 0) return;

        var itemsStolen = new List<RewardData>();
        var players = new List<String>();
        foreach (var result in pendingSteal)
        {
            var pieceStolen = new PieceDiscovery((PieceType)result.itemType, result.itemLevel);
            itemsStolen.Add(new RewardData(pieceStolen));
            if (!players.Contains(result.robbedBy)) players.Add(result.robbedBy);
            //Remove from Storage
            if (mergeModel.storage.Find(p => p.pType == pieceStolen.pType) != null)
            {
                mergeModel.storage.RemoveAt(mergeModel.storage.FindIndex(p => p.pType == pieceStolen.pType));
                GameManager.Instance.UpdateStorageButtons();
            }
        }

        await CloudSaveService.Instance.Data.Player.DeleteAsync("pendingSteal");
        GameManager.Instance.PopupsManager.ShowGiftStealReceivedPopup(itemsStolen, players, false);
    }
        
    public void ChangeName()
    {
        string url = "https://www.mylittlelifesim.com/game/change_name_merge.php?playerid="
            + PlayerData.playerID
            + "&playername=" + (String.IsNullOrEmpty(PlayerData.playerName) ? "SinNombre" : PlayerData.playerName);
        StartCoroutine(SendRequest(url));
    }

    IEnumerator SendRequest(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        GameManager.Log("Update URL: " + url + " Result: " + request.downloadHandler.text);
    }


    IEnumerator SendRequestState(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        GameManager.Log("Update URL: " + url + " Result: " + request.downloadHandler.text);
        var giftsStealResult = request.downloadHandler.text;
        JSONArray result = JSON.Parse(giftsStealResult) as JSONArray;
        if(result!= null && result.Count > 0)
        {
            GameManager.Instance.ProcessGiftSteal(result);
        }
    }
    IEnumerator SendRequestScore(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        GameManager.Log("Send Score URL: " + url + " Result: " + request.downloadHandler.text);
    }
    



    void Awake()
    {
        UnityThread.initUnityThread();
    }
}

public class UnityThread : MonoBehaviour
{
    //our (singleton) instance
    private static UnityThread instance = null;


    ////////////////////////////////////////////////UPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueUpdateFunc then executed from there
    private static List<System.Action> actionQueuesUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesUpdateFunc to be executed
    List<System.Action> actionCopiedQueueUpdateFunc = new List<System.Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteUpdateFunc = true;


    ////////////////////////////////////////////////LATEUPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueLateUpdateFunc then executed from there
    private static List<System.Action> actionQueuesLateUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesLateUpdateFunc to be executed
    List<System.Action> actionCopiedQueueLateUpdateFunc = new List<System.Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteLateUpdateFunc = true;



    ////////////////////////////////////////////////FIXEDUPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueFixedUpdateFunc then executed from there
    private static List<System.Action> actionQueuesFixedUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesFixedUpdateFunc to be executed
    List<System.Action> actionCopiedQueueFixedUpdateFunc = new List<System.Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteFixedUpdateFunc = true;


    //Used to initialize UnityThread. Call once before any function here
    public static void initUnityThread(bool visible = false)
    {
        if (instance != null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            // add an invisible game object to the scene
            GameObject obj = new GameObject("MainThreadExecuter");
            if (!visible)
            {
                obj.hideFlags = HideFlags.HideAndDontSave;
            }

            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<UnityThread>();
        }
    }

    public void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }

    //////////////////////////////////////////////COROUTINE IMPL//////////////////////////////////////////////////////
#if (ENABLE_UPDATE_FUNCTION_CALLBACK)
    public static void executeCoroutine(IEnumerator action)
    {
        if (instance != null)
        {
            executeInUpdate(() => instance.StartCoroutine(action));
        }
    }

    ////////////////////////////////////////////UPDATE IMPL////////////////////////////////////////////////////
    public static void executeInUpdate(System.Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesUpdateFunc)
        {
            actionQueuesUpdateFunc.Add(action);
            noActionQueueToExecuteUpdateFunc = false;
        }
    }

    public void Update()
    {
        if (noActionQueueToExecuteUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueUpdateFunc queue
        actionCopiedQueueUpdateFunc.Clear();
        lock (actionQueuesUpdateFunc)
        {
            //Copy actionQueuesUpdateFunc to the actionCopiedQueueUpdateFunc variable
            actionCopiedQueueUpdateFunc.AddRange(actionQueuesUpdateFunc);
            //Now clear the actionQueuesUpdateFunc since we've done copying it
            actionQueuesUpdateFunc.Clear();
            noActionQueueToExecuteUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueUpdateFunc
        for (int i = 0; i < actionCopiedQueueUpdateFunc.Count; i++)
        {
            actionCopiedQueueUpdateFunc[i].Invoke();
        }
    }
#endif

    ////////////////////////////////////////////LATEUPDATE IMPL////////////////////////////////////////////////////
#if (ENABLE_LATEUPDATE_FUNCTION_CALLBACK)
    public static void executeInLateUpdate(System.Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesLateUpdateFunc)
        {
            actionQueuesLateUpdateFunc.Add(action);
            noActionQueueToExecuteLateUpdateFunc = false;
        }
    }


    public void LateUpdate()
    {
        if (noActionQueueToExecuteLateUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueLateUpdateFunc queue
        actionCopiedQueueLateUpdateFunc.Clear();
        lock (actionQueuesLateUpdateFunc)
        {
            //Copy actionQueuesLateUpdateFunc to the actionCopiedQueueLateUpdateFunc variable
            actionCopiedQueueLateUpdateFunc.AddRange(actionQueuesLateUpdateFunc);
            //Now clear the actionQueuesLateUpdateFunc since we've done copying it
            actionQueuesLateUpdateFunc.Clear();
            noActionQueueToExecuteLateUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueLateUpdateFunc
        for (int i = 0; i < actionCopiedQueueLateUpdateFunc.Count; i++)
        {
            actionCopiedQueueLateUpdateFunc[i].Invoke();
        }
    }
#endif

    ////////////////////////////////////////////FIXEDUPDATE IMPL//////////////////////////////////////////////////
#if (ENABLE_FIXEDUPDATE_FUNCTION_CALLBACK)
    public static void executeInFixedUpdate(System.Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesFixedUpdateFunc)
        {
            actionQueuesFixedUpdateFunc.Add(action);
            noActionQueueToExecuteFixedUpdateFunc = false;
        }
    }

    public void FixedUpdate()
    {
        if (noActionQueueToExecuteFixedUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueFixedUpdateFunc queue
        actionCopiedQueueFixedUpdateFunc.Clear();
        lock (actionQueuesFixedUpdateFunc)
        {
            //Copy actionQueuesFixedUpdateFunc to the actionCopiedQueueFixedUpdateFunc variable
            actionCopiedQueueFixedUpdateFunc.AddRange(actionQueuesFixedUpdateFunc);
            //Now clear the actionQueuesFixedUpdateFunc since we've done copying it
            actionQueuesFixedUpdateFunc.Clear();
            noActionQueueToExecuteFixedUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueFixedUpdateFunc
        for (int i = 0; i < actionCopiedQueueFixedUpdateFunc.Count; i++)
        {
            actionCopiedQueueFixedUpdateFunc[i].Invoke();
        }
    }
#endif

    public void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
