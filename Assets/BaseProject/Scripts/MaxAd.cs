using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class MaxAd : MonoBehaviour
{
    //#if UNITY_IOS
    //        private const string _AppOpenAdUnitIdMAX = "";
    //        private const string _BannerAdUnitIdMAX = "";
    //        private const string _InterstitialAdUnitIdMAX = "";
    //        private const string _InterstitialAdBreakAdUnitIdMAX = "";
    //        private const string _RewardedAdUnitIdMAX = "";
    //        private const string _MrecAdUnitIdMAX = "";
    //#else
    private const string _AppOpenAdUnitIdMAX = "3f59155b46ae8266";
    private const string _BannerAdUnitIdMAX = "8367e62f680d47eb";
    private const string _InterstitialAdUnitIdMAX = "79c0e12015a527af";
    private const string _InterstitialAdBreakAdUnitIdMAX = "79c0e12015a527af";
    private const string _RewardedAdUnitIdMAX = "d546a75822759418";
    private const string _MrecAdUnitIdMAX = "eec5f6d6387b00e4";
    //#endif
    private string AppOpenAdUnitIdMAX
    {
        get
        {
            if (!string.IsNullOrEmpty(Common.ads_aoa_id_max))
                return Common.ads_aoa_id_max;
            return _AppOpenAdUnitIdMAX;
        }
    }
    private string BannerAdUnitIdMAX
    {
        get
        {
            if (!string.IsNullOrEmpty(Common.ads_banner_id_max))
                return Common.ads_banner_id_max;
            return _BannerAdUnitIdMAX;
        }
    }
    private string InterstitialAdUnitIdMAX
    {
        get
        {
            if (!string.IsNullOrEmpty(Common.ads_inter_id_max))
                return Common.ads_inter_id_max;
            return _InterstitialAdUnitIdMAX;
        }
    }
    private string InterstitialAdBreakAdUnitIdMAX
    {
        get
        {
            if (!string.IsNullOrEmpty(Common.ads_inter_ad_break_id_max))
                return Common.ads_inter_ad_break_id_max;
            return _InterstitialAdBreakAdUnitIdMAX;
        }
    }
    private string RewardedAdUnitIdMAX
    {
        get
        {
            if (!string.IsNullOrEmpty(Common.ads_reward_id_max))
                return Common.ads_reward_id_max;
            return _RewardedAdUnitIdMAX;
        }
    }
    private string MrecAdUnitIdMAX
    {
        get
        {
            if (!string.IsNullOrEmpty(Common.ads_mrec_id_max))
                return Common.ads_mrec_id_max;
            return _MrecAdUnitIdMAX;
        }
    }

    private readonly TimeSpan APPOPEN_TIMEOUT = TimeSpan.FromHours(4);
    private DateTime appOpenExpireTime;

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
        //MaxSdk.SetHasUserConsent(true);
        //MaxSdk.SetIsAgeRestrictedUser(false);
        //MaxSdk.SetDoNotSell(false);
        //MaxSdk.SetSdkKey("YOUR_SDK_KEY");
        if (initMaxSuccess.HasValue)
            return;
        initMaxSuccess = false;
        //MaxSdk.SetTestDeviceAdvertisingIdentifiers(new string[] { "349fbed8-8b3e-919b-0258-839f61fb18cc" });
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
        InitializeAOA();
        InitializeBannerAds();
        InitializeInterstitialAds();
        InitializeRewardedAds();
        InitializeMRecAds();
    }
    public void InitializeAOA()
    {
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
        MaxSdk.LoadAppOpenAd(AppOpenAdUnitIdMAX);
    }
    public bool IsAOAReady()
    {
        return initMaxSuccess == true && MaxSdk.IsAppOpenAdReady(AppOpenAdUnitIdMAX);
    }
    public void ShowAOAIfReady()
    {
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
        // Banners are automatically sized to 320Å~50 on phones and 728Å~90 on tablets
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
        MaxSdk.ShowBanner(BannerAdUnitIdMAX);
    }
    public void HideBanner()
    {
        MaxSdk.HideBanner(BannerAdUnitIdMAX);
    }
    public void DestroyBanner()
    {
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
        return (MaxSdk.IsInterstitialReady(InterstitialAdUnitIdMAX));
    }
    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(InterstitialAdUnitIdMAX);
    }
    public void ShowInterstitial()
    {
        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitIdMAX))
        {
            //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_inter_show");
            MaxSdk.ShowInterstitial(InterstitialAdUnitIdMAX);
        }
    }
    public bool IsInterstitialAdBreakAvailable()
    {
        return (MaxSdk.IsInterstitialReady(InterstitialAdBreakAdUnitIdMAX));
    }
    private void LoadInterstitialAdBreak()
    {
        MaxSdk.LoadInterstitial(InterstitialAdBreakAdUnitIdMAX);
    }
    public void ShowInterstitialAdBreak()
    {
        if (MaxSdk.IsInterstitialReady(InterstitialAdBreakAdUnitIdMAX))
        {
            //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_inter_show");
            MaxSdk.ShowInterstitial(InterstitialAdBreakAdUnitIdMAX);
        }
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready for you to show. MaxSdk.IsInterstitialReady(adUnitId) now returns 'true'
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_inter_load");
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
        //var impressionParameters = new[] {
        //    new Firebase.Analytics.Parameter("errormsg", errorInfo.ToString())
        //};
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_inter_fail", impressionParameters);
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
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_inter_click");
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
        return (MaxSdk.IsRewardedAdReady(RewardedAdUnitIdMAX));
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedAdUnitIdMAX);
    }

    public void ShowRewardedAd()
    {
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
        MaxSdk.HideMRec(MrecAdUnitIdMAX);
    }
    public void DestroyMrec()
    {
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
        var impressionParameters = new[] {
            new Firebase.Analytics.Parameter("ad_platform", "AppLovin"),
            new Firebase.Analytics.Parameter("ad_source", impressionData.NetworkName),
            new Firebase.Analytics.Parameter("ad_unit_name", impressionData.AdUnitIdentifier),
            new Firebase.Analytics.Parameter("ad_format", impressionData.AdFormat),
            new Firebase.Analytics.Parameter("value", revenue),
            new Firebase.Analytics.Parameter("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
    }
}
