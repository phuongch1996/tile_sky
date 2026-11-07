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
    public bool IsHomeReturnTransition { get; private set; }
    private GameObject homeReturnOverlay;
    private Texture2D homeReturnSnapshot;
    private Sequence homeReturnTween;
    public List<MaskableGraphic> maskableGraphics;
    public Image loadProgress;
    public TextMeshProUGUI txLoadProgress;
    [SerializeField] private Sprite loadingStarSprite;
    private Image loadingStar;
    private readonly List<Image> rainbowTrail = new();
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
        SetupLoadingStar();
        UpdateLoadingProgress();
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
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnUpdate(UpdateLoadingProgress);
    }

    private void SetupLoadingStar()
    {
        if (loadingStarSprite == null)
            return;

        for (int i = 0; i < 7; i++)
        {
            var trail = CreateLoadingStar($"Rainbow Trail {i + 1}", 28f + i * 4f);
            trail.color = Color.HSVToRGB(i / 7f, 0.65f, 1f);
            rainbowTrail.Add(trail);
        }
        loadingStar = CreateLoadingStar("Loading Star", 110f);
        Canvas.ForceUpdateCanvases();
        UpdateLoadingStar();
    }

    private Image CreateLoadingStar(string name, float size)
    {
        var starObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        starObject.layer = loadProgress.gameObject.layer;
        starObject.transform.SetParent(loadProgress.transform, false);
        var star = starObject.GetComponent<Image>();
        star.sprite = loadingStarSprite;
        star.preserveAspect = true;
        star.raycastTarget = false;
        star.rectTransform.sizeDelta = Vector2.one * size;
        maskableGraphics.Add(star);
        return star;
    }

    private void LateUpdate()
    {
        UpdateLoadingStar();
    }

    private void UpdateLoadingStar()
    {
        if (loadingStar == null)
            return;

        float progress = Mathf.Clamp01(loadProgress.fillAmount);
        float time = Time.unscaledTime;
        var rect = loadingStar.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(progress, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(time * 4f) * 15f);
        rect.localScale = Vector3.one * (1f + Mathf.Sin(time * 6f) * 0.08f);

        float width = loadProgress.rectTransform.rect.width;
        for (int i = 0; i < rainbowTrail.Count; i++)
        {
            var trail = rainbowTrail[i];
            float distance = (7 - i) * 17f;
            trail.gameObject.SetActive(progress * width >= distance);
            var trailRect = trail.rectTransform;
            trailRect.anchorMin = trailRect.anchorMax = new Vector2(progress, 0.5f);
            trailRect.anchoredPosition = new Vector2(-distance, Mathf.Sin(time * 7f - i * 0.6f) * 7f);
            trailRect.localRotation = Quaternion.Euler(0, 0, time * 60f + i * 25f);
        }
    }

    void UpdateLoadingProgress()
    {
        txLoadProgress.text = $"Loading {(int)(loadProgress.fillAmount * 100)}%";
        UpdateLoadingStar();
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
        // Even if initialization finishes early, let the star pull the bar
        // all the way to 100% before dismissing the loading screen.
        yield return loadProgress.DOFillAmount(1f, Mathf.Max(0.1f, (1f - loadProgress.fillAmount) * 0.5f))
            .SetEase(Ease.Linear).SetUpdate(true)
            .OnUpdate(UpdateLoadingProgress).WaitForCompletion();
        UpdateLoadingProgress();
        yield return new WaitForSecondsRealtime(0.12f);
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

    public void StartHomeReturnTransition(float overlapDuration, int exitEdge, System.Action beforeUnload)
    {
        if (IsHomeReturnTransition)
            return;
        IsHomeReturnTransition = true;
        StartCoroutine(HomeReturnTransition(overlapDuration, exitEdge, beforeUnload));
    }

    private IEnumerator HomeReturnTransition(float overlapDuration, int exitEdge, System.Action beforeUnload)
    {
        yield return new WaitForEndOfFrame();
        try
        {
            homeReturnSnapshot = ScreenCapture.CaptureScreenshotAsTexture();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("Could not capture the Home transition: " + exception.Message);
        }

        // Release old pooled objects before Home can request any objects of its own.
        beforeUnload?.Invoke();
        if (homeReturnSnapshot == null)
        {
            IsHomeReturnTransition = false;
            StartLoad(GameConfig.homeScene, true);
            yield break;
        }

        homeReturnOverlay = new GameObject("Home Return Overlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        homeReturnOverlay.transform.SetParent(transform, false);
        // A nested Canvas keeps its default 100x100 rect unless explicitly stretched.
        // Match the persistent loading canvas so the captured frame never becomes a thumbnail.
        var overlayRect = (RectTransform)homeReturnOverlay.transform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;
        overlayRect.localScale = Vector3.one;
        var canvas = homeReturnOverlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        var loadingCanvas = GetComponent<Canvas>();
        if (loadingCanvas != null)
            canvas.sortingLayerID = loadingCanvas.sortingLayerID;
        canvas.sortingOrder = 32760;
        var imageObject = new GameObject("Outgoing Game", typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(homeReturnOverlay.transform, false);
        var image = imageObject.GetComponent<RawImage>();
        image.texture = homeReturnSnapshot;
        var imageRect = (RectTransform)image.transform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = imageRect.offsetMax = Vector2.zero;
        Canvas.ForceUpdateCanvases();

        graphicRaycaster.enabled = true;
        var previousPriority = Application.backgroundLoadingPriority;
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        var load = SceneManager.LoadSceneAsync(GameConfig.homeScene);
        while (!load.isDone)
            yield return null;
        Application.backgroundLoadingPriority = previousPriority;
        // Allow Home.Start to prepare its icons at scale zero before revealing them.
        yield return null;
        SetAlpha(0);
        graphicRaycaster.enabled = false;

        var directions = new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };
        var direction = exitEdge < 0 ? Vector2.up : directions[exitEdge];
        float distance = (direction.x == 0 ? overlayRect.rect.height : overlayRect.rect.width) * 0.08f;
        float duration = Mathf.Max(0.1f, overlapDuration);
        homeReturnTween = DOTween.Sequence().SetUpdate(true)
            .Append(homeReturnOverlay.GetComponent<CanvasGroup>().DOFade(0f, duration).SetEase(Ease.OutQuad))
            .Join(imageRect.DOAnchorPos(direction * distance, duration).SetEase(Ease.OutQuad));
        yield return homeReturnTween.WaitForCompletion();
        ClearHomeReturnOverlay();
        IsHomeReturnTransition = false;
    }

    private void ClearHomeReturnOverlay()
    {
        homeReturnTween?.Kill(false);
        homeReturnTween = null;
        if (homeReturnOverlay != null)
            Destroy(homeReturnOverlay);
        if (homeReturnSnapshot != null)
            Destroy(homeReturnSnapshot);
        homeReturnOverlay = null;
        homeReturnSnapshot = null;
    }

    private void OnDestroy()
    {
        ClearHomeReturnOverlay();
    }

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
            UpdateLoadingProgress();
            SetAlpha(1);
            loadProgress.DOFillAmount(1, LoadTime)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnUpdate(UpdateLoadingProgress)
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
