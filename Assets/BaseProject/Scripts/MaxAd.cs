using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class MaxAd : MonoBehaviour
{
    // MAX SDK is retained, with all legacy units removed. Only Google demo ads run.
    private string AppOpenAdUnitIdMAX => string.Empty;
    private string BannerAdUnitIdMAX => string.Empty;
    private string InterstitialAdUnitIdMAX => string.Empty;
    private string InterstitialAdBreakAdUnitIdMAX => string.Empty;
    private string RewardedAdUnitIdMAX => string.Empty;
    private string MrecAdUnitIdMAX => string.Empty;
    public bool _isDebug;

    public int retryAOAAttempt;
    public int retryAttempt;
    public int retryAdBreakAttempt;
    public int retryRewardAttempt;

    public int TYPE_REWARD = -1;

    public bool? initMaxSuccess;

    public static MaxAd instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Initialize()
    {
        if (!GoogleTestAds.MaxEnabled)
        {
            // Complete the existing startup wait without initializing MAX.
            initMaxSuccess = true;
            return;
        }
        //MaxSdk.SetHasUserConsent(true);
        //MaxSdk.SetIsAgeRestrictedUser(false);
        //MaxSdk.SetDoNotSell(false);
        //MaxSdk.SetSdkKey("YOUR_SDK_KEY");
        if (initMaxSuccess.HasValue)
            return;
        initMaxSuccess = false;
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            initMaxSuccess = true;

            if (_isDebug)
                MaxSdk.ShowMediationDebugger();
        };
        Debug.Log("AppLovin Initializing.");
        MaxSdk.InitializeSdk();
    }
    public void LoadAds()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        InitializeAOA();
        InitializeBannerAds();
        InitializeInterstitialAds();
        InitializeRewardedAds();
        InitializeMRecAds();
    }
    public void InitializeAOA()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAppOpenLoadedEvent;
        MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAppOpenClickedEvent;
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
        MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAppOpenLoadFailedEvent;
        MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        LoadAOAAd();
    }
    public void OnAppOpenLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }
    public void OnAppOpenClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }
    public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Invoke(nameof(LoadAOAAd), 2);
    }
    public void OnAppOpenLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        retryAOAAttempt++;
        double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAOAAttempt));
        Invoke(nameof(LoadAOAAd), (float)retryDelay);
    }
    public void LoadAOAAd()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.LoadAppOpenAd(AppOpenAdUnitIdMAX);
    }
    public bool IsAOAReady()
    {
        if (!GoogleTestAds.MaxEnabled) return false;
        return initMaxSuccess == true && MaxSdk.IsAppOpenAdReady(AppOpenAdUnitIdMAX);
    }
    public void ShowAOAIfReady()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        if (MaxSdk.IsAppOpenAdReady(AppOpenAdUnitIdMAX))
        {
            MaxSdk.ShowAppOpenAd(AppOpenAdUnitIdMAX);
        }
        else
        {
            MaxSdk.LoadAppOpenAd(AppOpenAdUnitIdMAX);
        }
    }

    #region Banner
    //bool initBanner;
    public void InitializeBannerAds()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        // Banners are automatically sized to 320�~50 on phones and 728�~90 on tablets
        // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
        MaxSdk.CreateBanner(BannerAdUnitIdMAX, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));

        //MaxSdk.SetBannerExtraParameter(BannerAdUnitIdMAX, "adaptive_banner", "true");

        // Set background or background color for banners to be fully functional
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitIdMAX, Color.white);

        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        //MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;

        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

        MaxSdk.LoadBanner(BannerAdUnitIdMAX);
        //initBanner = true;
    }
    public void ShowBanner()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.ShowBanner(BannerAdUnitIdMAX);
    }
    public void HideBanner()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.HideBanner(BannerAdUnitIdMAX);
    }
    public void DestroyBanner()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.DestroyBanner(BannerAdUnitIdMAX);
    }
    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        DestroyBanner();
        if (Common.ads_banner_fallback)
        {
            GameConfig.isMaxBanner = false;
            GameConfig.ShowBannerAd();
        }
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    #endregion

    #region Inter
    public void InitializeInterstitialAds()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        // Attach callback
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialAdFailedToDisplayEvent;
        //MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialAdRevenuePaidEvent;

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        // Load the first interstitial
        LoadInterstitial();
        //if (InterstitialAdUnitIdMAX != InterstitialAdBreakAdUnitIdMAX)
        //    LoadInterstitialAdBreak();
    }

    public bool IsInterstitialAvailable()
    {
        if (!GoogleTestAds.MaxEnabled) return false;
        return (MaxSdk.IsInterstitialReady(InterstitialAdUnitIdMAX));
    }
    private void LoadInterstitial()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.LoadInterstitial(InterstitialAdUnitIdMAX);
    }
    public void ShowInterstitial()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitIdMAX))
        {
            MaxSdk.ShowInterstitial(InterstitialAdUnitIdMAX);
        }
    }
    public bool IsInterstitialAdBreakAvailable()
    {
        if (!GoogleTestAds.MaxEnabled) return false;
        return (MaxSdk.IsInterstitialReady(InterstitialAdBreakAdUnitIdMAX));
    }
    private void LoadInterstitialAdBreak()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.LoadInterstitial(InterstitialAdBreakAdUnitIdMAX);
    }
    public void ShowInterstitialAdBreak()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        if (MaxSdk.IsInterstitialReady(InterstitialAdBreakAdUnitIdMAX))
        {
            MaxSdk.ShowInterstitial(InterstitialAdBreakAdUnitIdMAX);
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
        // Reset retry attempt
        if (adUnitId == InterstitialAdUnitIdMAX)
            retryAttempt = 0;
        else if (adUnitId == InterstitialAdBreakAdUnitIdMAX)
            retryAdBreakAttempt = 0;
    }

    private void OnInterstitialLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds)
        if (adUnitId == InterstitialAdUnitIdMAX)
        {
            retryAttempt++;
            double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttempt));
            Invoke(nameof(LoadInterstitial), (float)retryDelay);
        }
        else if (adUnitId == InterstitialAdBreakAdUnitIdMAX)
        {
            retryAdBreakAttempt++;
            double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAdBreakAttempt));
            Invoke(nameof(LoadInterstitialAdBreak), (float)retryDelay);
        }

    }
    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        GameConfig.SetDisableCommonAd(true);
        Time.timeScale = 0;
    }

    private void OnInterstitialAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. AppLovin recommends that you load the next ad.
        if (adUnitId == InterstitialAdUnitIdMAX)
            LoadInterstitial();
        else if (adUnitId == InterstitialAdBreakAdUnitIdMAX)
            LoadInterstitialAdBreak();
        //if (GameScript.instance && GameScript.instance.adbreakPopup.isAdBreak)
        //    GameScript.instance.adbreakPopup.Close();
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Time.timeScale = 1;
        GameConfig.SetDisableCommonAd(false);
        GameConfig.SetActionTime("ads_inter");

        // Interstitial ad is hidden. Pre-load the next ad.
        if (adUnitId == InterstitialAdUnitIdMAX)
            LoadInterstitial();
        else if (adUnitId == InterstitialAdBreakAdUnitIdMAX)
            LoadInterstitialAdBreak();
        //if (GameScript.instance && GameScript.instance.adbreakPopup.isAdBreak)
        //    GameScript.instance.adbreakPopup.Close();
    }
    private void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    #endregion

    #region Reward
    public void InitializeRewardedAds()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        // Attach callback
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        //MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        // Load the first rewarded ad
        LoadRewardedAd();
    }

    public bool IsRewardedAdAvailable()
    {
        if (!GoogleTestAds.MaxEnabled) return false;
        return (MaxSdk.IsRewardedAdReady(RewardedAdUnitIdMAX));
    }

    private void LoadRewardedAd()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.LoadRewardedAd(RewardedAdUnitIdMAX);
    }

    public void ShowRewardedAd()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        Debug.Log(MaxSdk.IsRewardedAdReady(RewardedAdUnitIdMAX));
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitIdMAX))
        {
            MaxSdk.ShowRewardedAd(RewardedAdUnitIdMAX);
        }
        else
        {
            LoadRewardedAd();
        }
    }
    Action rewardCallback;
    public void ShowRewardedAd(Action action)
    {
        if (!GoogleTestAds.MaxEnabled) return;
        Debug.Log(MaxSdk.IsRewardedAdReady(RewardedAdUnitIdMAX));
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitIdMAX))
        {
            rewardCallback = action;
            MaxSdk.ShowRewardedAd(RewardedAdUnitIdMAX);
        }
        else
        {
            LoadRewardedAd();
        }
    }
    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
        retryRewardAttempt = 0;
    }

    private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load 
        // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
        retryRewardAttempt++;
        double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryRewardAttempt));
        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        GameConfig.SetDisableCommonAd(true);
        Time.timeScale = 0;
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        Invoke(nameof(LoadRewardedAd), 2);
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Time.timeScale = 1;
        GameConfig.SetDisableCommonAd(false);
        if (Common.rewarded_reset_capping_time)
            GameConfig.SetActionTime("ads_inter");
        // Rewarded ad is hidden. Pre-load the next ad
        Invoke(nameof(LoadRewardedAd), 2);
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("receive reward");
        AdmobAd.instance.GetReward();
    }
    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }


    #endregion

    #region Mrec

    public void InitializeMRecAds()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
        MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
        MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;

        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

        MaxSdk.CreateMRec(MrecAdUnitIdMAX, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));

    }
    public void ShowMrec()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        // MRECs are sized to 300x250 on phones and tablets
        // var aspect = (float)Screen.height / Screen.width;
        float deviceScale = MobileAds.Utils.GetDeviceScale();
        Debug.Log("deviceScale " + deviceScale);
        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = anchorMin + safeArea.size;
        var width = Screen.safeArea.width * 0.5 / deviceScale;
        var height = Screen.safeArea.height / deviceScale;
        Debug.Log($"wid{width} hei{height}");
        var posX = (float)width - 150;
        float bannerHeight = MaxSdkUtils.GetAdaptiveBannerHeight();
        Debug.Log($"bannerHeight{bannerHeight}");
        var posY = (float)height - 255 - bannerHeight;
        Debug.Log($"posX{posX} posY{posY}");
        var posOnPixel = new Vector2(posX, posY);
        MaxSdk.UpdateMRecPosition(MrecAdUnitIdMAX, posOnPixel.x, posOnPixel.y);
        MaxSdk.ShowMRec(MrecAdUnitIdMAX);
    }
    public void HideMrec()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.HideMRec(MrecAdUnitIdMAX);
    }
    public void DestroyMrec()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.DestroyMRec(MrecAdUnitIdMAX);
    }
    //public bool MRecAdLoaded;
    public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        //MRecAdLoaded = true;
        //mrecLoadAttempt = 0;
    }

    //int mrecLoadAttempt;
    public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
    {
    }
    void LoadMrec()
    {
        if (!GoogleTestAds.MaxEnabled) return;
        MaxSdk.CreateMRec(MrecAdUnitIdMAX, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
    }

    public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
    }

    public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    #endregion

    private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
    {
        double revenue = impressionData.Revenue;
        Debug.Log(String.Format("{0} max impression {1}", impressionData.AdFormat, revenue));
    }
}
