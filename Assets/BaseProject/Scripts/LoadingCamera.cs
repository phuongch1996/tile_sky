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
    private CanvasGroup shootingStarGroup;
    private CanvasGroup loadingProgressGroup;
    private RectTransform shootingStarRoot;
    private readonly List<Image> shootingStars = new();
    private readonly List<Sprite> loadingSprites = new();
    private readonly List<Texture2D> loadingTextures = new();
    public GraphicRaycaster graphicRaycaster;
#if UNITY_IOS
    readonly static bool disableLoadingStart = true;
#else
    readonly static bool disableLoadingStart = false;
#endif
    readonly static bool disableLoading = true;
    public TextMeshProUGUI txLoadingDesc;
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
        HideLoadingText();
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
        var art = Resources.Load<Sprite>("TileSkyLoading/ShootingStar");
        if (art) loadingStarSprite = art;
        ApplyLoadingColors();
        if (!loadingStarSprite) return;
        var go = new GameObject("Loading shooting star shower", typeof(RectTransform), typeof(CanvasGroup));
        go.layer = gameObject.layer;
        shootingStarRoot = go.GetComponent<RectTransform>();
        shootingStarRoot.SetParent(transform, false);
        shootingStarRoot.anchorMin = Vector2.zero;
        shootingStarRoot.anchorMax = Vector2.one;
        shootingStarRoot.offsetMin = shootingStarRoot.offsetMax = Vector2.zero;
        shootingStarGroup = go.GetComponent<CanvasGroup>();
        shootingStarGroup.blocksRaycasts = shootingStarGroup.interactable = false;
        for (int i = 0; i < 10; i++)
            shootingStars.Add(CreateLoadingStar("Shooting star " + (i + 1), 210 + i % 4 * 55));
    }

    private Image CreateLoadingStar(string name, float size)
    {
        var starObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        starObject.layer = gameObject.layer;
        starObject.transform.SetParent(shootingStarRoot, false);
        var star = starObject.GetComponent<Image>();
        star.sprite = loadingStarSprite;
        star.preserveAspect = true;
        star.raycastTarget = false;
        star.rectTransform.anchorMin = star.rectTransform.anchorMax = Vector2.zero;
        star.rectTransform.sizeDelta = new Vector2(size, size / 3f);
        return star;
    }

    void HideLoadingText()
    {
        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            text.text = string.Empty;
            text.gameObject.SetActive(false);
        }
        txLoadProgress.text = txLoadingDesc.text = string.Empty;
        txLoadProgress.gameObject.SetActive(false);
        txLoadingDesc.gameObject.SetActive(false);
    }

    void ApplyLoadingColors()
    {
        var background = GetComponent<CanvasBG>();
        if (background && background.bg) background.bg.color = new Color32(210, 228, 255, 255);
        var frame = loadProgress.transform.parent.GetComponent<Image>();
        if (frame)
        {
            loadingProgressGroup = frame.GetComponent<CanvasGroup>();
            if (!loadingProgressGroup) loadingProgressGroup = frame.gameObject.AddComponent<CanvasGroup>();
            loadingProgressGroup.blocksRaycasts = loadingProgressGroup.interactable = false;
            frame.sprite = CreateLoadingBarSprite(false);
            frame.type = Image.Type.Sliced;
            frame.preserveAspect = false;
            frame.color = new Color32(26, 40, 87, 255);
        }
        // The old sliced fill was a child shown through the mask, outside maskableGraphics.
        // The new gradient renders directly on loadProgress, so retire that legacy artwork.
        foreach (Transform child in loadProgress.transform)
            child.gameObject.SetActive(false);
        var oldMask = loadProgress.GetComponent<Mask>();
        if (oldMask) oldMask.enabled = false;
        loadProgress.sprite = CreateLoadingBarSprite(true);
        loadProgress.color = Color.white;
        loadProgress.type = Image.Type.Filled;
        loadProgress.fillMethod = Image.FillMethod.Horizontal;
        loadProgress.fillOrigin = 0;
    }

    Sprite CreateLoadingBarSprite(bool gradient)
    {
        const int width = 256, height = 32;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "Loading sky pill", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float centerX = Mathf.Clamp(x + 0.5f, 16, width - 16);
                float alpha = Mathf.Clamp01(16 - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(centerX, 16)));
                Color color = gradient ? Color.Lerp(new Color32(92, 241, 239, 255), new Color32(172, 143, 255, 255), x / (float)(width - 1)) : Color.white;
                if (gradient) color = Color.Lerp(color, Color.white, Mathf.Clamp01((y / 31f - 0.55f) * 0.65f));
                color.a = alpha;
                pixels[y * width + x] = color;
            }
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f,
            100, 0, SpriteMeshType.FullRect, new Vector4(16, 16, 16, 16));
        loadingTextures.Add(texture);
        loadingSprites.Add(sprite);
        return sprite;
    }

    private void LateUpdate()
    {
        UpdateLoadingStar();
    }

    private void UpdateLoadingStar()
    {
        if (!shootingStarGroup || shootingStarGroup.alpha <= 0.001f) return;
        Vector2 size = shootingStarRoot.rect.size;
        float angle = Mathf.Atan2(size.y * 0.85f, size.x) * Mathf.Rad2Deg;
        for (int i = 0; i < shootingStars.Count; i++)
        {
            float phase = Mathf.Repeat(Time.unscaledTime * (0.43f + i % 3 * 0.08f) + i * 0.137f, 1);
            var start = new Vector2(0.2f + Mathf.Repeat(i * 0.618f, 1), 0.75f + Mathf.Repeat(i * 0.382f, 1) * 0.6f);
            Vector2 position = start + new Vector2(-1, -0.85f) * phase;
            var star = shootingStars[i];
            star.rectTransform.anchoredPosition = Vector2.Scale(position, size);
            star.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
            star.color = new Color(1, 1, 1, Mathf.Clamp01(phase / 0.08f) * Mathf.Clamp01((1 - phase) / 0.18f));
        }
    }

    void UpdateLoadingProgress()
    {
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
        // Finish the progress bar before dismissing the sky, even on a fast initialization.
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
        if (loadingProgressGroup)
        {
            loadingProgressGroup.DOKill();
            loadingProgressGroup.alpha = alpha;
        }
        if (shootingStarGroup)
        {
            shootingStarGroup.DOKill();
            shootingStarGroup.alpha = alpha;
        }
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
        if (loadingProgressGroup)
        {
            loadingProgressGroup.DOKill();
            loadingProgressGroup.DOFade(alpha, 0.05f).SetUpdate(true);
        }
        if (shootingStarGroup)
        {
            shootingStarGroup.DOKill();
            shootingStarGroup.DOFade(alpha, 0.05f).SetUpdate(true);
        }
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
        if (shootingStarGroup) shootingStarGroup.DOKill();
        if (loadingProgressGroup) loadingProgressGroup.DOKill();
        foreach (var sprite in loadingSprites) if (sprite) Destroy(sprite);
        foreach (var texture in loadingTextures) if (texture) Destroy(texture);
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
