using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadingCamera : MonoBehaviour
{
    const float MaxInitialLoadingDuration = 2f;

    public static LoadingCamera instance;
    public List<MaskableGraphic> maskableGraphics;
    public Image loadProgress;
    public TextMeshProUGUI txLoadProgress;
    public GraphicRaycaster graphicRaycaster;
#if UNITY_IOS
    readonly static bool disableLoadingStart = true;
#else
    readonly static bool disableLoadingStart = false;
#endif
    readonly static bool disableLoading = true;
    public TextMeshProUGUI txLoadingDesc;
    public readonly string[] loadingDesc = new string[]
    {
        "Every match brings you one step closer to the world beyond.",
        "A great journey begins with a single tile.",
        "Exploration is not just about going far, but seeing something new.",
        "Beyond every challenge lies a new horizon waiting to unfold.",
        "The world feels bigger when you dare to begin.",
        "Match today, discover tomorrow.",
        "Every land holds a story waiting to be found.",
        "Be patient - something wonderful is about to appear.",
        "Every small victory unlocks a greater adventure.",
        "The best adventure is always the one ahead.",
    };
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
        txLoadingDesc.text = GameConfig.GetRandom(loadingDesc);
        loadProgress.fillAmount = 0;
        graphicRaycaster.enabled = true;
        videoPlayer.prepareCompleted += (video) =>
        {
            video.Play();
            videoPrepared = true;
        };
        videoPlayer.Prepare();
    }
    public VideoPlayer videoPlayer;
    bool videoPrepared;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!GameConfig.isGameLoaded)
        {
            StartInitialLoadingProgress();
            StartCoroutine(CompleteLoadingAfterTimeout());
            FirebaseManager.instance.RemoteConfig();
            if (GameConfig.CHEAT_ENABLED)
            {
                CompleteLoading();
                return;
            }
            if (!GameConfig.isUMP)
            {
                UMPManager.Instance.InitUMP(() =>
                {
                    Debug.Log("InitUMP");
                    AdmobAd.instance.InitializeGoogleMobileAds();
                    MaxAd.instance.Initialize();

                    StartCoroutine(LoadAdmobAd());
                    StartCoroutine(LoadMaxAd());

                    GameConfig.SetActionTime("ads_inter");
                    GameConfig.SetActionTime("ads_collaps");
                    if (disableLoadingStart)
                    {
                        CompleteLoading();
                        return;
                    }
                    StartCoroutine(LoadGameStart());
                    GameConfig.isUMP = true;
                }, () =>
                {
                    Debug.Log("InitUMPPause");
                    Time.timeScale = 0;
                }, () =>
                {
                    Debug.Log("InitUMPResume");
                    Time.timeScale = 1;
                });
            }
            else
            {
                StartCoroutine(LoadAdmobAd());
                StartCoroutine(LoadMaxAd());
                if (disableLoadingStart)
                {
                    CompleteLoading();
                    return;
                }
                StartCoroutine(LoadGameStart());
            }
        }
    }

    void StartInitialLoadingProgress()
    {
        loadProgress.DOKill();
        loadProgress.DOFillAmount(1, MaxInitialLoadingDuration)
            .SetUpdate(true)
            .OnUpdate(UpdateLoadingProgress);
    }

    void UpdateLoadingProgress()
    {
        txLoadProgress.text = $"Loading {(int)(loadProgress.fillAmount * 100)}%";
    }

    IEnumerator CompleteLoadingAfterTimeout()
    {
        yield return new WaitForSecondsRealtime(MaxInitialLoadingDuration);
        CompleteLoading();
    }

    void CompleteLoading()
    {
        if (!loadFinished)
            StartCoroutine(DoneLoading());
    }

    IEnumerator LoadAdmobAd()
    {
        yield return new WaitUntil(() => AdmobAd._isInitialized == true);
        AdmobAd.instance.LoadAds();
    }
    IEnumerator LoadMaxAd()
    {
        yield return new WaitUntil(() => MaxAd.instance.initMaxSuccess == true);
        MaxAd.instance.LoadAds();
    }

    IEnumerator LoadGameStart()
    {
        if (GameConfig.CHEAT_ENABLED)
        {
            CompleteLoading();
            yield break;
        }

        int open_count = PlayerPrefs.GetInt("open_count", 0);
        open_count++;
        PlayerPrefs.SetInt("open_count", open_count);

        yield return new WaitUntil(() => loadFinished || FirebaseManager.instance.isRemoteConfig || FirebaseManager.instance.isRemoteFailed);
        if (loadFinished)
            yield break;

        yield return new WaitUntil(() => loadFinished || (videoPrepared && UnityAuthentication.isSignedIn.HasValue));
        if (loadFinished)
            yield break;

        if (!GameConfig.IsAdsRemoved() &&
            GameConfig.IsActionAvailable("ads_aoa", Common.ads_aoa_duration, true) &&
            GameConfig.GetCurrentLevel() >= Common.ads_aoa_start_level &&
            Common.ads_aoa_show == true &&
            Common.ads_aoa_open_count >= 0 && Common.ads_aoa_open_count <= open_count)
        {
            GameConfig.ShowAppOpenAd();
        }
        CompleteLoading();
    }
    bool loadFinished;
    IEnumerator DoneLoading()
    {
        if (!loadFinished)
            loadFinished = true;
        else yield break;
        loadProgress.DOKill();
        loadProgress.fillAmount = 1;
        UpdateLoadingProgress();
        EventManager.instance.GetEventTimer();
        txLoadingDesc.gameObject.SetActive(false);
        GameConfig.SetBannerType();
        GameConfig.ShowBannerAd();
        GameConfig.isGameLoaded = true;
        if (GameConfig.GetCurrentLevel() == 1)
        {
            GameConfig.level = 1;
            PoolManager.instance.ReleaseAllObjects();
            StartLoad(GameConfig.gameScene, true);
            yield break;
        }
        if (HomeScript.instance)
            HomeScript.instance.HomeUI();
        SetAlphaImmediate(0);
        graphicRaycaster.enabled = false;
    }

    void SetAlphaImmediate(float alpha)
    {
        foreach (var img in maskableGraphics)
        {
            img.DOKill();
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }
    public void SetAlpha(float alpha)
    {
        foreach (var img in maskableGraphics)
        {
            img.DOKill();
            img.DOFade(alpha, 0.05f);
        }
    }

    int LoadTime => Common.config_loading_time;
    public void StartLoad(string sceneToLoad, bool forceDisableLoading = false)
    {
        if (disableLoading || LoadTime <= 0 || forceDisableLoading)
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            graphicRaycaster.enabled = true;
            StartCoroutine(LoadYourAsyncScene(sceneToLoad));
        }
        else
        {
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            graphicRaycaster.enabled = true;
            loadProgress.fillAmount = 0;
            txLoadProgress.text = $"Loading {(int)(loadProgress.fillAmount * 100)}%";
            SetAlpha(1);
            loadProgress.DOFillAmount(1, LoadTime)
                .OnUpdate(() => txLoadProgress.text = $"Loading {(int)(loadProgress.fillAmount * 100)}%")
                .OnComplete(() =>
                {
                    StartCoroutine(LoadYourAsyncScene(sceneToLoad));
                });
        }
    }
    IEnumerator LoadYourAsyncScene(string sceneToLoad)
    {
        //videoPlayer.Prepare();
        yield return new WaitForEndOfFrame();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        SetAlpha(0);
        graphicRaycaster.enabled = false;
        //videoPlayer.Play();
    }
}
