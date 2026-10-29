using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdmobAd : MonoBehaviour
{
    //default ad id
#if UNITY_IOS
        private const string _AppOpenAdUnitId = "ca-app-pub-3940256099942544/5575463023";
        private const string _BannerAdUnitId = "ca-app-pub-3940256099942544/2435281174";
        private const string _BannerCollapseAdUnitId = "ca-app-pub-3940256099942544/2435281174";
        private const string _InterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
        private const string _InterstitialAdBreakAdUnitId = "ca-app-pub-3940256099942544/4411468910";
        private const string _RewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
        private const string _NativeAdUnitId = "ca-app-pub-3940256099942544/3986624511";
#else
    //testads
    private const string _AppOpenAdUnitIdTest = "ca-app-pub-3940256099942544/9257395921";
    private const string _BannerAdUnitIdTest = "ca-app-pub-3940256099942544/9214589741";
    private const string _BannerCollapseAdUnitIdTest = "ca-app-pub-3940256099942544/9214589741";
    private const string _InterstitialAdUnitIdTest = "ca-app-pub-3940256099942544/1033173712";
    private const string _InterstitialAdBreakAdUnitIdTest = "ca-app-pub-3940256099942544/1033173712";
    private const string _RewardedAdUnitIdTest = "ca-app-pub-3940256099942544/5224354917";
    private const string _NativeAdUnitIdTest = "ca-app-pub-3940256099942544/2247696110";
    //real
    private const string _AppOpenAdUnitId = "ca-app-pub-2266966365312947/3200462064";
    private const string _BannerAdUnitId = "ca-app-pub-2266966365312947/1588831640";
    private const string _BannerCollapseAdUnitId = "ca-app-pub-2266966365312947/9275749972";
    private const string _InterstitialAdUnitId = "ca-app-pub-2266966365312947/3967582978";
    private const string _InterstitialAdBreakAdUnitId = "ca-app-pub-2266966365312947/3967582978";
    private const string _RewardedAdUnitId = "ca-app-pub-2266966365312947/6508764643";
    //
    //private const string _NativeAdUnitId = "ca-app-pub-3940256099942544/2247696110";
#endif
    const bool TESTAD = false;
    private string AppOpenAdUnitId
    {
        get
        {
            if (TESTAD)
                return _AppOpenAdUnitIdTest;
            if (!string.IsNullOrEmpty(Common.ads_aoa_id_admob))
                return Common.ads_aoa_id_admob;
            return _AppOpenAdUnitId;
        }
    }
    private string BannerAdUnitId
    {
        get
        {
            if (TESTAD)
                return _BannerAdUnitIdTest;
            if (!string.IsNullOrEmpty(Common.ads_banner_id_admob))
                return Common.ads_banner_id_admob;
            return _BannerAdUnitId;
        }
    }
    private string BannerCollapseAdUnitId
    {
        get
        {
            if (TESTAD)
                return _BannerCollapseAdUnitIdTest;
            if (!string.IsNullOrEmpty(Common.ads_collapse_id_admob))
                return Common.ads_collapse_id_admob;
            return _BannerCollapseAdUnitId;
        }
    }
    private string InterstitialAdUnitId
    {
        get
        {
            if (TESTAD)
                return _InterstitialAdUnitIdTest;
            if (!string.IsNullOrEmpty(Common.ads_inter_id_admob))
                return Common.ads_inter_id_admob;
            return _InterstitialAdUnitId;
        }
    }
    private string InterstitialAdBreakAdUnitId
    {
        get
        {
            if (TESTAD)
                return _InterstitialAdBreakAdUnitIdTest;
            if (!string.IsNullOrEmpty(Common.ads_inter_ad_break_id_admob))
                return Common.ads_inter_ad_break_id_admob;
            return _InterstitialAdBreakAdUnitId;
        }
    }
    private string RewardedAdUnitId
    {
        get
        {
            if (TESTAD)
                return _RewardedAdUnitIdTest;
            if (!string.IsNullOrEmpty(Common.ads_reward_id_admob))
                return Common.ads_reward_id_admob;
            return _RewardedAdUnitId;
        }
    }
    //unused
    private string NativeAdUnitId
    {
        get
        {
            //if (TESTAD)
            return _NativeAdUnitIdTest;
            // if (!string.IsNullOrEmpty(Common.ads_native_id_admob))
            //    return Common.ads_native_id_admob;
            //return _NativeAdUnitId;
        }
    }

    internal static List<string> TestDeviceIds = new List<string>()
        {
            AdRequest.TestDeviceSimulator,
            //"46DE0836A7D5532C5AD898EB9CEE6E12",
            "39B8E82D05395A4049872B3A496BD7BD",
            "A645389550666D563AF1F8AB5F9C7CCA"
            //"TestDeviceIds"
        };
    public static bool? _isInitialized;

    public static AdmobAd instance;
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
    /// <summary>
    /// Demonstrates how to configure Google Mobile Ads Unity plugin.
    /// </summary>
    private void Start()
    {
        // On Android, Unity is paused when displaying interstitial or rewarded video.
        // This setting makes iOS behave consistently with Android.
        MobileAds.SetiOSAppPauseOnBackground(true);

        // When true all events raised by GoogleMobileAds will be raised
        // on the Unity main thread. The default value is false.
        // https://developers.google.com/admob/unity/quick-start#raise_ad_events_on_the_unity_main_thread
        //MobileAds.RaiseAdEventsOnUnityMainThread = true;

        // Configure your RequestConfiguration with Child Directed Treatment
        // and the Test Device Ids.
        MobileAds.SetRequestConfiguration(new RequestConfiguration
        {
            TestDeviceIds = TestDeviceIds
        });
    }

    /// <summary>
    /// Initializes the Google Mobile Ads Unity plugin.
    /// </summary>
    public void InitializeGoogleMobileAds()
    {
        // The Google Mobile Ads Unity plugin needs to be run only once and before loading any ads.
        if (_isInitialized.HasValue)
        {
            return;
        }

        _isInitialized = false;
        // Initialize the Google Mobile Ads Unity plugin.
        Debug.Log("Google Mobile Ads Initializing.");
        MobileAds.Initialize((InitializationStatus initstatus) =>
        {
            if (initstatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                _isInitialized = null;
                return;
            }

            // If you use mediation, you can check the status of each adapter.
            var adapterStatusMap = initstatus.getAdapterStatusMap();
            if (adapterStatusMap != null)
            {
                foreach (var item in adapterStatusMap)
                {
                    Debug.Log(string.Format("Adapter {0} is {1}",
                        item.Key,
                        item.Value.InitializationState));
                }
            }
            Debug.Log("Google Mobile Ads initialization complete.");
            _isInitialized = true;
        });
    }
    public void LoadAds()
    {
        LoadAppOpenAd();
        LoadInterstitialAd();
        //LoadInterstitialAdAdBreak();
        LoadRewardedAd();
        //LoadNativeOverlayAd();
    }

    /// <summary>
    /// Opens the AdInspector.
    /// </summary>
    public void OpenAdInspector()
    {
        Debug.Log("Opening ad Inspector.");
        MobileAds.OpenAdInspector((AdInspectorError error) =>
        {
            // If the operation failed, an error is returned.
            if (error != null)
            {
                Debug.Log("Ad Inspector failed to open with error: " + error);
                return;
            }

            Debug.Log("Ad Inspector opened successfully.");
        });
    }

    #region aoa
    // App open ads can be preloaded for up to 4 hours.
    private readonly TimeSpan TIMEOUT = TimeSpan.FromHours(4);
    private DateTime _expireTime;
    private AppOpenAd _appOpenAd;

    private void OnEnable()
    {
        // Use the AppStateEventNotifier to listen to application open/close events.
        // This is used to launch the loaded ad when we open the app.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    private void OnDestroy()
    {
        // Always unlisten to events when complete.
        AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    }

    public bool IsAppOpenAdAvailable
    {
        get
        {
            return (_appOpenAd != null
                    && _appOpenAd.CanShowAd()
                    && DateTime.Now < _expireTime);
        }
    }

    /// <summary>
    /// Loads the ad.
    /// </summary>
    public void LoadAppOpenAd()
    {
        // Clean up the old ad before loading a new one.
        if (_appOpenAd != null)
        {
            DestroyAppOpenAd();
        }

        Debug.Log("Loading app open ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        AppOpenAd.Load(AppOpenAdUnitId, adRequest, (AppOpenAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("App open ad failed to load an ad with error : " + error.GetResponseInfo());
                Invoke(nameof(LoadAppOpenAd), 100);
                return;
            }

            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: App open ad load event fired with " +
                               " null ad and null error.");
                return;
            }

            // The operation completed successfully.
            Debug.Log("App open ad loaded with response : " + ad.GetResponseInfo());
            _appOpenAd = ad;

            // App open ads can be preloaded for up to 4 hours.
            _expireTime = DateTime.Now + TIMEOUT;

            // Register to ad events to extend functionality.
            RegisterEventHandlers(ad);

            // Inform the UI that the ad is ready.
        });
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowAppOpenAd()
    {
        // App open ads can be preloaded for up to 4 hours.
        if (_appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _expireTime)
        {
            Debug.Log("Showing app open ad.");
            _appOpenAd.Show();
        }
        else
        {
            Debug.LogError("App open ad is not ready yet.");
        }
        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyAppOpenAd()
    {
        if (_appOpenAd != null)
        {
            Debug.Log("Destroying app open ad.");
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }
        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogAppOpenResponseInfo()
    {
        if (_appOpenAd != null)
        {
            var responseInfo = _appOpenAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }

    private void OnAppStateChanged(AppState state)
    {
        Debug.Log("App State changed to : " + state);

        // If the app is Foregrounded and the ad is available, show it.
        if (state == AppState.Foreground)
        {
            if (Common.ads_aoa_resume_game_show)
                GameConfig.ShowAppOpenAd();
        }
    }
    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            try
            {
                Debug.Log(String.Format("Appopen ad paid {0} {1}.",
                    (double)adValue.Value / 1000000,
                    adValue.CurrencyCode));

                var trueValue = (double)adValue.Value / 1000000;

                string currencyCode = adValue.CurrencyCode;
                ResponseInfo responseInfo = ad.GetResponseInfo();
                string responseId = responseInfo.GetResponseId();

                AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
                string adSourceId = loadedAdapterResponseInfo.AdSourceId;
                string adSourceInstanceId = loadedAdapterResponseInfo.AdSourceInstanceId;
                string adSourceInstanceName = loadedAdapterResponseInfo.AdSourceInstanceName;
                string adSourceName = loadedAdapterResponseInfo.AdSourceName;
                string adapterClassName = loadedAdapterResponseInfo.AdapterClassName;
                long latencyMillis = loadedAdapterResponseInfo.LatencyMillis;
                Dictionary<string, string> credentials = loadedAdapterResponseInfo.AdUnitMapping;

                string adId = ad.GetAdUnitID();

                var impressionParameters = new[] {
                        new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                        new Firebase.Analytics.Parameter("ad_source", adSourceName),
                        new Firebase.Analytics.Parameter("ad_unit_name", adId),
                        new Firebase.Analytics.Parameter("ad_format", "APPOPEN"),
                        new Firebase.Analytics.Parameter("value", trueValue),
                        new Firebase.Analytics.Parameter("currency", currencyCode),
                };
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            try
            {
                Debug.Log("App open ad recorded an impression.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            try
            {
                Debug.Log("App open ad was clicked.");
                string adId = ad.GetAdUnitID();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
            //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_aoa_click");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            try
            {
                Debug.Log("App open ad full screen content opened.");
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_aoa_show");
                // Inform the UI that the ad is consumed and not ready.
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            try
            {
                Debug.Log("App open ad full screen content closed.");
                // It may be useful to load a new ad when the current one is complete.
                Invoke(nameof(LoadAppOpenAd), 2);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            try
            {
                Debug.LogError("App open ad failed to open full screen content with error : "
                                + error.GetMessage());

                //var impressionParameters = new[] {
                //    new Firebase.Analytics.Parameter("errormsg", error.ToString())
                //};
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_aoa_fail", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
    }
    #endregion

    #region banner
    private BannerView _bannerView;

    int retryAttemptBanner = 0;

    bool _isCollapsible;
    /// <summary>
    /// Creates the banner view and loads a banner ad.
    /// </summary>
    public void LoadBannerAd(bool isCollapsible)
    {
        Debug.Log("Creating banner view.");
        // If we already have a banner, destroy the old one.
        if (_bannerView != null)
        {
            DestroyBannerAd();
        }
        _isCollapsible = isCollapsible;
        // Create a 320x50 banner at top of the screen.
        //_bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        _bannerView = new BannerView(_isCollapsible ? BannerCollapseAdUnitId : BannerAdUnitId, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), AdPosition.Bottom);

        // Listen to events the banner may raise.
        ListenToAdEvents();

        Debug.Log("Banner view created.");
        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Create an extra parameter that aligns the bottom of the expanded ad to the
        // bottom of the bannerView.
        if (isCollapsible)
            adRequest.Extras.Add("collapsible", "bottom");

        // Send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Showing banner view.");
            _bannerView.Show();
        }
        else
            LoadBannerAd(false);
    }

    /// <summary>
    /// Hides the ad.
    /// </summary>
    public void HideBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Hiding banner view.");
            _bannerView.Hide();
        }
    }

    /// <summary>
    /// Destroys the ad.
    /// When you are finished with a BannerView, make sure to call
    /// the Destroy() method before dropping your reference to it.
    /// </summary>
    public void DestroyBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogBannerResponseInfo()
    {
        if (_bannerView != null)
        {
            var responseInfo = _bannerView.GetResponseInfo();
            if (responseInfo != null)
            {
                UnityEngine.Debug.Log(responseInfo);
            }
        }
    }
    void BannerLoad()
    {
        GameConfig.ShowBannerAd();
    }
    //bool isBannerLoaded;
    //bool lockMaxBanner = false;
    /// <summary>
    /// Listen to events the banner may raise.
    /// </summary>
    private void ListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
            try
            {
                Debug.Log("Collapsible:" + _bannerView.IsCollapsible());
                Debug.Log("Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
                //isBannerLoaded = true;
                retryAttemptBanner = 0;
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_banner_load");
                // Inform the UI that the ad is ready.
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            try
            {
                Debug.LogError("Banner view failed to load an ad with error : " + error.GetResponseInfo());
                DestroyBannerAd();
                if (!Common.ads_banner_fallback)
                {
                    retryAttemptBanner++;
                    double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttemptBanner));
                    Invoke(nameof(BannerLoad), (float)retryDelay);
                }
                else
                {
                    GameConfig.isMaxBanner = true;
                    GameConfig.ShowBannerAd();
                }
                //var impressionParameters = new[] {
                //    new Firebase.Analytics.Parameter("errormsg", error.ToString())
                //};
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_banner_fail", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            try
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    (double)adValue.Value / 1000000,
                    adValue.CurrencyCode));

                //double trueValue = (double)adValue.Value / 1000000D;

                var trueValue = (double)adValue.Value / 1000000;

                string currencyCode = adValue.CurrencyCode;
                ResponseInfo responseInfo = _bannerView.GetResponseInfo();
                string responseId = responseInfo.GetResponseId();

                AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
                string adSourceId = loadedAdapterResponseInfo.AdSourceId;
                string adSourceInstanceId = loadedAdapterResponseInfo.AdSourceInstanceId;
                string adSourceInstanceName = loadedAdapterResponseInfo.AdSourceInstanceName;
                string adSourceName = loadedAdapterResponseInfo.AdSourceName;
                string adapterClassName = loadedAdapterResponseInfo.AdapterClassName;
                long latencyMillis = loadedAdapterResponseInfo.LatencyMillis;
                Dictionary<string, string> credentials = loadedAdapterResponseInfo.AdUnitMapping;

                string adId = _bannerView.GetAdUnitID();

                var impressionParameters = new[] {
                        new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                        new Firebase.Analytics.Parameter("ad_source", adSourceName),
                        new Firebase.Analytics.Parameter("ad_unit_name", adId),//_isCollapsible ? BannerCollapseAdUnitId : BannerAdUnitId),
                        new Firebase.Analytics.Parameter("ad_format", "BANNER"),
                        new Firebase.Analytics.Parameter("value", trueValue),
                        new Firebase.Analytics.Parameter("currency", currencyCode), // All AppLovin revenue is sent in USD
                };
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
            try
            {
                Debug.Log("Banner view recorded an impression.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
            try
            {
                Debug.Log("Banner view was clicked.");
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_banner_click");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            try
            {
                Debug.Log("Banner view full screen content opened.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            try
            {
                Debug.Log("Banner view full screen content closed.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
    }
    #endregion

    #region interstitial
    private InterstitialAd _interstitialAd;

    public bool IsInterstitialAvailable
    {
        get
        {
            return (_interstitialAd != null
                    && _interstitialAd.CanShowAd());
        }
    }

    /// <summary>
    /// Loads the ad.
    /// </summary>
    int retryAttempt = 0;
    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            DestroyInterstitialAd();
        }

        Debug.Log("Loading interstitial ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        InterstitialAd.Load(InterstitialAdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("Interstitial ad failed to load an ad with error : " + error.GetResponseInfo());
                retryAttempt++;
                double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttempt));
                Invoke(nameof(LoadInterstitialAd), (float)retryDelay);
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                retryAttempt++;
                double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttempt));
                Invoke(nameof(LoadInterstitialAd), (float)retryDelay);
                return;
            }

            retryAttempt = 0;
            // The operation completed successfully.
            Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

            _interstitialAd = ad;

            // Register to ad events to extend functionality.
            RegisterEventHandlers(ad);

            // Inform the UI that the ad is ready.
        });
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }

        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            Debug.Log("Destroying interstitial ad.");
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogInterstitialResponseInfo()
    {
        if (_interstitialAd != null)
        {
            var responseInfo = _interstitialAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }
    bool isInterAdBreak;
    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            try
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));

                var trueValue = (double)adValue.Value / 1000000;

                string currencyCode = adValue.CurrencyCode;
                ResponseInfo responseInfo = ad.GetResponseInfo();
                string responseId = responseInfo.GetResponseId();

                AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
                string adSourceId = loadedAdapterResponseInfo.AdSourceId;
                string adSourceInstanceId = loadedAdapterResponseInfo.AdSourceInstanceId;
                string adSourceInstanceName = loadedAdapterResponseInfo.AdSourceInstanceName;
                string adSourceName = loadedAdapterResponseInfo.AdSourceName;
                string adapterClassName = loadedAdapterResponseInfo.AdapterClassName;
                long latencyMillis = loadedAdapterResponseInfo.LatencyMillis;
                Dictionary<string, string> credentials = loadedAdapterResponseInfo.AdUnitMapping;

                var impressionParameters = new[] {
                        new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                        new Firebase.Analytics.Parameter("ad_source", adSourceName),
                        new Firebase.Analytics.Parameter("ad_unit_name", ad.GetAdUnitID()),
                        new Firebase.Analytics.Parameter("ad_format", "INTER"),
                        new Firebase.Analytics.Parameter("value", trueValue),
                        new Firebase.Analytics.Parameter("currency", currencyCode), // All AppLovin revenue is sent in USD
                };
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            try
            {
                Debug.Log("Interstitial ad recorded an impression.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            try
            {
                Debug.Log("Interstitial ad was clicked.");
                string adId = ad.GetAdUnitID();
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            try
            {
                Debug.Log("Interstitial ad full screen content opened.");
                string adId = ad.GetAdUnitID();
                Debug.Log(adId);
                if (adId == InterstitialAdUnitId)
                    isInterAdBreak = false;
                else if (adId == InterstitialAdBreakAdUnitId)
                    isInterAdBreak = true;
                GameConfig.SetDisableCommonAd(true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            try
            {
                Debug.Log("Interstitial ad full screen content closed.");
                GameConfig.SetDisableCommonAd(false);
                GameConfig.SetActionTime("ads_inter");
                string adId = ad.GetAdUnitID();
                Debug.Log(adId);
                if (!isInterAdBreak)
                    Invoke(nameof(LoadInterstitialAd), 2);
                else
                    Invoke(nameof(LoadInterstitialAdAdBreak), 2);
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Debug.Log("excecute in update");
                    //if (GameScript.instance && GameScript.instance.adbreakPopup.isAdBreak)
                    //    GameScript.instance.adbreakPopup.Close();
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            try
            {
                Debug.LogError("Interstitial ad failed to open full screen content with error : "
                    + error.GetMessage());
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    //if (GameScript.instance && GameScript.instance.adbreakPopup.isAdBreak)
                    //    GameScript.instance.adbreakPopup.Close();
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
    }

    #endregion

    #region adbreak_interstitial
    private InterstitialAd _interstitialAdAdBreak;

    public bool IsInterstitialAdBreakAvailable
    {
        get
        {
            return (_interstitialAdAdBreak != null
                    && _interstitialAdAdBreak.CanShowAd());
        }
    }

    /// <summary>
    /// Loads the ad.
    /// </summary>
    int retryAttemptAdBreak = 0;
    public void LoadInterstitialAdAdBreak()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAdAdBreak != null)
        {
            DestroyInterstitialAdAdBreak();
        }

        Debug.Log("Loading interstitial ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        InterstitialAd.Load(InterstitialAdBreakAdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("Interstitial ad failed to load an ad with error : " + error.GetResponseInfo());
                retryAttemptAdBreak++;
                double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttemptAdBreak));
                Invoke(nameof(LoadInterstitialAdAdBreak), (float)retryDelay);
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                retryAttemptAdBreak++;
                double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttemptAdBreak));
                Invoke(nameof(LoadInterstitialAdAdBreak), (float)retryDelay);
                return;
            }

            retryAttemptAdBreak = 0;
            // The operation completed successfully.
            Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());

            //#if UNITY_IOS
            //            MirrorAdTracker.Instance.TrackAdLoad(InterstitialAdUnitIdAdBreak, "Admob", "interstitial");
            //#endif
            _interstitialAdAdBreak = ad;

            // Register to ad events to extend functionality.
            RegisterEventHandlers(ad);

            // Inform the UI that the ad is ready.
        });
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowInterstitialAdAdBreak()
    {
        if (_interstitialAdAdBreak != null && _interstitialAdAdBreak.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAdAdBreak.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }

        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyInterstitialAdAdBreak()
    {
        if (_interstitialAdAdBreak != null)
        {
            Debug.Log("Destroying interstitial ad.");
            _interstitialAdAdBreak.Destroy();
            _interstitialAdAdBreak = null;
        }

        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogInterstitialResponseInfoAdBreak()
    {
        if (_interstitialAdAdBreak != null)
        {
            var responseInfo = _interstitialAdAdBreak.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }

    #endregion

    #region rewarded
    private RewardedAd _rewardedAd;

    /// <summary>
    /// Loads the ad.
    /// </summary>
    int rewardRetryAttempt = 0;
    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            DestroyRewardedAd();
        }

        Debug.Log("Loading rewarded ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedAd.Load(RewardedAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("Rewarded ad failed to load an ad with error : " + error.GetResponseInfo());
                rewardRetryAttempt++;
                double retryDelay = Mathf.Pow(2, Mathf.Min(6, rewardRetryAttempt));
                Invoke(nameof(LoadRewardedAd), (float)retryDelay);
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                rewardRetryAttempt++;
                double retryDelay = Mathf.Pow(2, Mathf.Min(6, rewardRetryAttempt));
                Invoke(nameof(LoadRewardedAd), (float)retryDelay);
                return;
            }
            rewardRetryAttempt = 0;

            // The operation completed successfully.
            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
            _rewardedAd = ad;

            // Register to ad events to extend functionality.
            RegisterEventHandlers(ad);

            // Inform the UI that the ad is ready.
        });
    }
    public bool IsRewardedAdAvailable
    {
        get
        {
            return _rewardedAd != null && _rewardedAd.CanShowAd();
        }
    }
    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowRewardedAd()//int rewardType)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            Debug.Log("Showing rewarded ad.");
            _rewardedAd.Show((Reward reward) =>
            {
                MobileAdsEventExecutor.ExecuteInUpdate(() =>
                {
                    Debug.Log("excecute in update");
                    GetReward();
                });
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
        }

        // Inform the UI that the ad is not ready.
    }
    public int TYPE_REWARD = -1;
    public void GetReward()
    {
        Debug.Log(TYPE_REWARD);
        // The rewarded ad displayed and the user should receive the reward.
        if (TYPE_REWARD != -1)
        {
            switch (TYPE_REWARD)
            {
                case 0:
                    Debug.Log("receive rewarded");
                    GameController.instance.DonePowerAd();
                    //reward
                    break;
                case 1:
                    if (HomeScript.instance)
                        HomeScript.instance.dailyLoginPopup.ClaimPrize(true);
                    if (GameScript.instance)
                        GameScript.instance.dailyLoginPopup.ClaimPrize(true);
                    break;
                case 2:
                    if (HomeScript.instance)
                        HomeScript.instance.collectionEventPopup.ClaimPrize(true);
                    if (GameScript.instance)
                        GameScript.instance.collectionEventPopup.ClaimPrize(true);
                    break;
                case 3:
                    if (HomeScript.instance)
                        HomeScript.instance.streakEventPopup.ClaimPrize(true);
                    if (GameScript.instance)
                        GameScript.instance.streakEventPopup.ClaimPrize(true);
                    break;
                case 4:
                    if (HomeScript.instance)
                        HomeScript.instance.comboEventPopup.ClaimPrize(true);
                    if (GameScript.instance)
                        GameScript.instance.comboEventPopup.ClaimPrize(true);
                    break;
                case 5:
                    GameScript.instance.losePopup.Revive();
                    break;
                case 6://bonus ads
                    if (HomeScript.instance)
                        HomeScript.instance.bonusPopup.Collect(true);
                    if (GameScript.instance)
                        GameScript.instance.bonusPopup.Collect(true);
                    break;
                case 7:
                    if (HomeScript.instance)
                        HomeScript.instance.freeGoldPopup.DoneAds();
                    if (GameScript.instance)
                        GameScript.instance.freeGoldPopup.DoneAds();
                    break;
                case 8:
                    HomeScript.instance.battlePassPopup.AdsClaim();
                    break;
                case 9:
                    if (GameScript.instance)
                        GameScript.instance.winPopup.ClaimHomeRewardAds();
                    break;
                default:
                    break;
            }
            QuestManager.QuestComplete(QUEST_TYPE.WATCH_X_ADS, 1);
            TYPE_REWARD = -1;
        }
    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyRewardedAd()
    {
        if (_rewardedAd != null)
        {
            Debug.Log("Destroying rewarded ad.");
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogRewardedResponseInfo()
    {
        if (_rewardedAd != null)
        {
            var responseInfo = _rewardedAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            try
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));

                var trueValue = (double)adValue.Value / 1000000;

                string currencyCode = adValue.CurrencyCode;
                ResponseInfo responseInfo = ad.GetResponseInfo();
                string responseId = responseInfo.GetResponseId();

                AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
                string adSourceId = loadedAdapterResponseInfo.AdSourceId;
                string adSourceInstanceId = loadedAdapterResponseInfo.AdSourceInstanceId;
                string adSourceInstanceName = loadedAdapterResponseInfo.AdSourceInstanceName;
                string adSourceName = loadedAdapterResponseInfo.AdSourceName;
                string adapterClassName = loadedAdapterResponseInfo.AdapterClassName;
                long latencyMillis = loadedAdapterResponseInfo.LatencyMillis;
                Dictionary<string, string> credentials = loadedAdapterResponseInfo.AdUnitMapping;
                string adId = ad.GetAdUnitID();

                var impressionParameters = new[] {
                        new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                        new Firebase.Analytics.Parameter("ad_source", adSourceName),
                        new Firebase.Analytics.Parameter("ad_unit_name", adId),
                        new Firebase.Analytics.Parameter("ad_format", "REWARDED"),
                        new Firebase.Analytics.Parameter("value", trueValue),
                        new Firebase.Analytics.Parameter("currency", currencyCode), // All AppLovin revenue is sent in USD
                };
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            try
            {
                Debug.Log("Rewarded ad recorded an impression.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            try
            {
                Debug.Log("Rewarded ad was clicked.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            try
            {
                Debug.Log("Rewarded ad full screen content opened.");
                GameConfig.SetDisableCommonAd(true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            try
            {
                Debug.Log("Rewarded ad full screen content closed.");
                Debug.Log("excecute in update");
                GameConfig.SetDisableCommonAd(false);
                if (Common.rewarded_reset_capping_time)
                    GameConfig.SetActionTime("ads_inter");
                Invoke(nameof(LoadRewardedAd), 2);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            try
            {
                Debug.LogError("Rewarded ad failed to open full screen content with error : "
                    + error.GetMessage());
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
    }

    #endregion

    #region native_overlay


    private NativeOverlayAd _nativeOverlayAd;

    /// <summary>
    /// Loads the ad.
    /// </summary>
    public void LoadNativeOverlayAd()
    {
        // Clean up the old ad before loading a new one.
        if (_nativeOverlayAd != null)
        {
            DestroyNativeOverlayAd();
        }

        Debug.Log("Loading native overlay ad.");

        // Create a request used to load the ad.
        var adRequest = new AdRequest();

        // Optional: Define native ad options.
        var options = new NativeAdOptions
        {
            AdChoicesPlacement = AdChoicesPlacement.TopLeftCorner,
            MediaAspectRatio = MediaAspectRatio.Any,
        };

        // Send the request to load the ad.
        NativeOverlayAd.Load(NativeAdUnitId, adRequest, options,
            (NativeOverlayAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError("Native Overlay ad failed to load an ad " +
                               " with error: " + error);
                    //Invoke(nameof(LoadNativeOverlayAd), 5);

                    //if (error.GetCode() == 0)
                    //    Init.Instance.ads_type_native = 0;
                    return;
                }

                // The ad should always be non-null if the error is null, but
                // double-check to avoid a crash.
                if (ad == null)
                {
                    Debug.LogError("Unexpected error: Native Overlay ad load event " +
                               " fired with null ad and null error.");
                    return;
                }

                // The operation completed successfully.
                Debug.Log("Native Overlay ad loaded with response : " +
                           ad.GetResponseInfo());
                _nativeOverlayAd = ad;

                // Register to ad events to extend functionality.
                RegisterEventHandlers(ad);
                //RenderAd();
            });
    }
    /// <summary>
    /// Renders the ad.
    /// </summary>
    public void RenderNativeOverlayAd()
    {
        if (_nativeOverlayAd != null)
        {
            Debug.Log("Rendering Native Overlay ad.");

            // Define a native template style with a custom style.
            NativeTemplateStyle style = new();
            //if (Common.ads_native_type == 1)
            //{
            //    style = new NativeTemplateStyle
            //    {
            //        TemplateId = NativeTemplateId.Small
            //    };
            //}
            //else
            //{
            //    style = new NativeTemplateStyle
            //    {
            //        TemplateId = NativeTemplateId.Medium
            //    };
            //}
            // Renders a native overlay ad at the default size
            // and anchored to the bottom of the screne.

            Debug.Log("dpi " + Screen.dpi);
            float deviceScale = MobileAds.Utils.GetDeviceScale();
            Debug.Log("deviceScale " + deviceScale);
            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = anchorMin + safeArea.size;
            var width = Screen.width * 0.5 / deviceScale;
            var height = (Screen.height - anchorMin.y) / deviceScale;
            Debug.Log($"wid{width} hei{height}");
            var posX = (float)width - 150;
            float bannerHeight = MaxSdkUtils.GetAdaptiveBannerHeight();
            Debug.Log($"bannerHeight{bannerHeight}");
            var posY = (float)height - 255 - bannerHeight;
            Debug.Log($"posX{posX} posY{posY}");
            var posOnPixel = new Vector2(posX, posY);
            //if (Common.ads_native_type == 1)
            //    _nativeOverlayAd.RenderTemplate(style, 0, (int)(height - width * 2 / 3f - bannerHeight - 5));
            //else if (Common.ads_native_type == 2)
            //    _nativeOverlayAd.RenderTemplate(style, AdPosition.Bottom);
            //else
            _nativeOverlayAd.RenderTemplate(style, new AdSize((int)(300 * deviceScale), (int)(250 * deviceScale)), (int)posOnPixel.x, (int)posOnPixel.y);
        }
    }
    /// <summary>
    /// Shows the ad.
    /// </summary>
    public bool ShowNativeOverlayAd()
    {
        if (_nativeOverlayAd != null)
        {
            Debug.Log("Showing Native Overlay ad.");
            _nativeOverlayAd.Show();
            return true;
        }
        else
        {
            LoadNativeOverlayAd();
            return false;
        }
    }
    /// <summary>
    /// Hides the ad.
    /// </summary>
    public void HideNativeOverlayAd()
    {
        if (_nativeOverlayAd != null)
        {
            Debug.Log("Hiding Native Overlay ad.");
            _nativeOverlayAd.Hide();
        }
    }
    /// <summary>
    /// Destroys the native overlay ad.
    /// </summary>
    public void DestroyNativeOverlayAd()
    {
        if (_nativeOverlayAd != null)
        {
            Debug.Log("Destroying native overlay ad.");
            _nativeOverlayAd.Destroy();
            _nativeOverlayAd = null;
        }
    }
    private void RegisterEventHandlers(NativeOverlayAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            try
            {
                Debug.Log(String.Format("Native Overlay ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));

                var trueValue = (double)adValue.Value / 1000000;

                string currencyCode = adValue.CurrencyCode;
                ResponseInfo responseInfo = ad.GetResponseInfo();
                string responseId = responseInfo.GetResponseId();

                AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
                string adSourceName = loadedAdapterResponseInfo.AdSourceName;

                var impressionParameters = new[] {
                        new Firebase.Analytics.Parameter("ad_platform", "Admob"),
                        new Firebase.Analytics.Parameter("ad_source", adSourceName),
                        new Firebase.Analytics.Parameter("ad_unit_name", NativeAdUnitId),
                        new Firebase.Analytics.Parameter("ad_format", "NATIVE"),
                        new Firebase.Analytics.Parameter("value", trueValue),
                        new Firebase.Analytics.Parameter("currency", currencyCode), // All AppLovin revenue is sent in USD
                };
                //Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            try
            {
                Debug.Log("Native Overlay ad recorded an impression.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            try
            {
                Debug.Log("Native Overlay ad was clicked.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            try
            {
                Debug.Log("Native Overlay ad full screen content opened.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            try
            {
                Debug.Log("Native Overlay ad full screen content closed.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        };
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogResponseInfoNativeOverlay()
    {
        if (_nativeOverlayAd != null)
        {
            var responseInfo = _nativeOverlayAd.GetResponseInfo();
            if (responseInfo != null)
            {
                Debug.Log(responseInfo);
            }
        }
    }
    //private void OnApplicationQuit()
    //{
    //    PlayerPrefs.DeleteKey("ads_native_time");
    //}
    #endregion

}
