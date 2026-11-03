using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

[Serializable]
public class IAPValues
{
    public int goldAm;
    public int boostersAm;
    public bool removeAds;
    public IAPValues(int goldAm = 0, int boostersAm = 0, bool removeAds = false)
    {
        this.goldAm = goldAm;
        this.boostersAm = boostersAm;
        this.removeAds = removeAds;
    }
}
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    // No store is configured for this game yet. Never initialize the old catalog.
    public static bool PurchasingEnabled => false;
    public const string tilespace_removeads = "tilespace_removeads"; //300, 1, true  $0.99
    public const string tilespace_firstpack = "tilespace_firstpack"; //300, 3        $0.99
    public const string tilespace_megapack = "tilespace_megapack";   //1500, 15      $4.99
    public const string tilespace_ultipack = "tilespace_ultipack";   //3000, 30      $9.99
    public const string tilespace_gold1 = "tilespace_gold1";         //1200          $0.99
    public const string tilespace_gold2 = "tilespace_gold2";         //3900          $2.99
    public const string tilespace_gold3 = "tilespace_gold3";         //7800          $4.99
    public const string tilespace_piggybank = "tilespace_piggybank"; //piggybank     $2.99
    public const string tilespace_battlepass = "tilespace_battlepass"; //battlepass  $3.99
    public static readonly Dictionary<string, IAPValues> iapidList = new()
    {
        { tilespace_removeads, new(300, 1, true) },
        { tilespace_firstpack, new(300, 3) },
        { tilespace_megapack, new(1500, 15) },
        { tilespace_ultipack, new(3000, 30) },
        { tilespace_gold1, new(1200) },
        { tilespace_gold2, new(3900) },
        { tilespace_gold3, new(7800) },
    };
    public static IAPValues GetIAPValues(string iapid)
    {
        if (iapidList.TryGetValue(iapid, out var value))
            return value;
        else
            return new();
    }
    private IStoreController m_StoreController; // The Unity IAP Store Controller
    //private IExtensionProvider m_StoreExtensionProvider; // The Unity IAP Store Extension Provider
    IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
    IAppleExtensions m_AppleExtensions;

    public static IAPManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
        if (!PurchasingEnabled)
        {
            foreach (var purchase in FindObjectsByType<CodelessIAPButton>(FindObjectsInactive.Include))
            {
                purchase.enabled = false;
                var button = purchase.GetComponent<UnityEngine.UI.Button>();
                if (button) button.interactable = false;
            }
        }
    }
    private void Start()
    {
        if (PurchasingEnabled) StartCoroutine(InitializePurchasing());
    }
    public IEnumerator InitializePurchasing()
    {
        if (!PurchasingEnabled) yield break;
        yield return new WaitUntil(() => IsInitialized());
        Debug.Log("Init IAP");
        //if (IsInitialized())
        //{
        //    return;
        //}
        //Debug.Log("Init IAP");
        //ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        //IAPConfigurationHelper.PopulateConfigurationBuilder(ref builder, ProductCatalog.LoadDefaultCatalog());
        //UnityPurchasing.Initialize(this, builder);

        m_StoreController = CodelessIAPStoreListener.Instance.StoreController;
        m_GooglePlayStoreExtensions = CodelessIAPStoreListener.Instance.GetStoreExtensions<IGooglePlayStoreExtensions>();
        m_AppleExtensions = CodelessIAPStoreListener.Instance.GetStoreExtensions<IAppleExtensions>();
    }
    private bool IsInitialized()
    {
        return PurchasingEnabled && CodelessIAPStoreListener.initializationComplete;
        //return m_StoreController != null && m_StoreExtensionProvider != null;
    }
    public void OnProductsFetched(ProductCollection products)
    {
        Debug.Log("fetched products");
        foreach (var product in products.all)
        {
            Debug.Log($"{product.definition.id} - {product.definition.type}");
        }
    }
    void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("Unity IAP Initialized.");
    }

    void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("Unity IAP Initialization Failed: " + error);
    }

    void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("Unity IAP Initialization Failed: " + error + " Message: " + message);
    }

    void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        //try
        //{
        //    Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureDescription: {1}", product.definition.storeSpecificId, failureDescription.message));
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError(ex.Message);
        //}
        //DoneIAP(false);
    }
    public void OnPurchaseDetailedFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        try
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureDescription: {1}", product.definition.storeSpecificId, failureDescription.message));
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        DoneIAP(false);
    }

    void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        //try
        //{
        //    Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        //}
        //catch (Exception ex)
        //{
        //    Debug.LogError(ex.Message);
        //}
    }
    bool isRestore;
    PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        if (!PurchasingEnabled) return PurchaseProcessingResult.Pending;
        if (string.Equals(purchaseEvent.purchasedProduct.definition.id, tilespace_removeads, System.StringComparison.Ordinal))
        {
            Debug.Log("Non-consumable purchased or restored: " + purchaseEvent.purchasedProduct.definition.id);
            isRestore = true;
            // Grant access to non-consumable content
            //OnRemoveAds(true);
        }
        //{
        //    Debug.Log("Non-consumable purchased or restored: " + args.purchasedProduct.definition.id);
        //    isRestore = true;
        //    //OnPurchasePack(3300, 3);
        //    //OnStarterPack(true);
        //}
        //else
        //{
        //    Debug.Log("Unknown product purchased or restored: " + args.purchasedProduct.definition.id);
        //}
        //DoneIAP(true, false);
        return PurchaseProcessingResult.Complete;
    }
    public void OnPurchaseComplete(Product product)
    {
        if (!PurchasingEnabled) return;
        if (string.Equals(product.definition.id, tilespace_piggybank, System.StringComparison.Ordinal))
        {
            OnPiggyBank();
            OnPurchasePack(piggyBank);
        }
        else if (string.Equals(product.definition.id, tilespace_battlepass, System.StringComparison.Ordinal))
        {
            BattlePassManager.BuyBattlePassVIP();
        }
        else
        {
            if (string.Equals(product.definition.id, tilespace_removeads, System.StringComparison.Ordinal))
                OnRemoveAds();
            IAPValues iap = GetIAPValues(product.definition.id);
            OnPurchasePack(iap.goldAm, iap.boostersAm, iap.removeAds);
        }
        DoneIAP(true, product.definition.id);
    }
    public static bool IsRemoveAdsBought()
    {
        return PlayerPrefs.GetInt(tilespace_removeads, 0) != 0;
    }
    public static bool RemoveAdsBought()
    {
        if (IsRemoveAdsBought())
            return false;
        PlayerPrefs.SetInt(tilespace_removeads, 1);
        return true;
    }
    public void OnRemoveAds()
    {
        RemoveAdsBought();
        //OnPurchasePack(300, 1, true);
    }
    public static int piggyBank;
    public void OnPiggyBank()
    {
        piggyBank = GameConfig.GetPiggyBank();
        //OnPurchasePack(piggyBank);
        GameConfig.BoughtPiggyBank();
        GameConfig.ResetPiggyBank();
    }
    public void OnPurchasePack(int gold = 0, int booster = 0, bool removeads = false)
    {
        if (gold != 0)
        {
            GameConfig.AddGold(gold);
        }
        if (booster != 0)
        {
            GameConfig.AddAllPowerUps(booster);
        }
        if (removeads)
        {
            GameConfig.DestroyBannerAd();
            GameConfig.DestroyMrec();
            GameConfig.SetRemoveAds(true);
        }
    }
    public void OnIAPClick()
    {
        if (!PurchasingEnabled) return;
        isRestore = false;
        GameConfig.SetDisableCommonAd(true);
    }
    public void OnRestoreClick()
    {
        if (!PurchasingEnabled) return;
        isRestore = true;
        GameConfig.SetDisableCommonAd(true);
        RestorePurchases();
    }
    public void DoneIAP(bool successfull, string iapid = "")
    {
        GameConfig.SetDisableCommonAd(false);
        if (!isRestore)
        {
            StartCoroutine(IAPPopup(successfull, iapid));
        }
    }
    IEnumerator IAPPopup(bool successfull, string iapid = "")
    {
        yield return null;
        FindFirstObjectByType<PopupIAPDone>(FindObjectsInactive.Include).DoneIAP(successfull, iapid);
    }

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases failed: Unity IAP not initialized.");
            return;
        }
        //isRestore = true;
        // On Google Play, restoring purchases is handled automatically by calling the `ProcessPurchase` callback for each owned item.
#if UNITY_ANDROID
        m_GooglePlayStoreExtensions.RestoreTransactions(OnRestore);
#elif UNITY_IOS
        m_AppleExtensions.RestoreTransactions(OnRestore);
#endif
    }

    public void OnRestore(bool success, string error)
    {
        var restoreMessage = "";
        if (success)
        {
            // This does not mean anything was restored,
            // merely that the restoration process succeeded.
            restoreMessage = "Restore Successful";
        }
        else
        {
            // Restoration failed.
            restoreMessage = $"Restore Failed with error: {error}";
        }
        Debug.Log(restoreMessage);
    }
}
