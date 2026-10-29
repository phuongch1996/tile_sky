using System;
using Unity.Services.Core;
using UnityEngine;

public class UGSManager : MonoBehaviour
{
    public static UGSManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    async void Start()
    {
        try
        {
            Debug.Log("Init UGS");
            await UnityServices.InitializeAsync();
            // UGS initialized, now proceed to initialize Unity IAP
            IAPManager.instance.InitializePurchasing();
        }
        catch (Exception exception)
        {
            // An error occurred during services initialization.
            Debug.LogError($"Unity Services initialization failed: {exception.Message}");
        }
    }
}
