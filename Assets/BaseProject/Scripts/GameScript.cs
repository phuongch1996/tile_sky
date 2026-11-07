using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float homeExitDuration = 1f;
    [SerializeField, Min(0.1f)] private float levelEntranceDuration = 0.85f;
    public bool IsLevelEntering { get; private set; }
    private Sequence levelEntranceSequence;
    private readonly Dictionary<Transform, Vector3> levelEntrancePositions = new();
    public bool IsReturningHome { get; private set; }
    private Sequence homeExitSequence;

    [FormerlySerializedAs("toggleUI")]
    public GameObject hideUI;
    public GameObject uiux;
    public List<GameObject> uiToggleable;
    bool isSelectedUIHidden;
    readonly Dictionary<GameObject, bool> uiActiveStates = new();

    public void ToggleUI()
    {
        SetSelectedUIVisible(isSelectedUIHidden);
    }

    public void HideSelectedUI()
    {
        SetSelectedUIVisible(false);
    }

    void SetSelectedUIVisible(bool visible)
    {
        isSelectedUIHidden = !visible;

        if (visible)
        {
            GameConfig.ShowBannerAd();
        }
        else
        {
            GameConfig.HideBannerAd();
            GameConfig.HideMrec();
        }

        SetUITreeVisible(uiux, visible);

        foreach (GameObject ui in uiToggleable)
        {
            SetUITreeVisible(ui, visible);
        }

        if (visible)
            uiActiveStates.Clear();
    }

    void SetUITreeVisible(GameObject ui, bool visible)
    {
        if (ui == null)
            return;

        if (ShouldKeepHideUIButtonVisible(ui))
        {
            ui.SetActive(true);
            foreach (Transform child in ui.transform)
            {
                SetUITreeVisible(child.gameObject, visible);
            }
            return;
        }

        if (visible)
        {
            if (uiActiveStates.TryGetValue(ui, out bool wasActive))
                ui.SetActive(wasActive);
            else
                ui.SetActive(true);
        }
        else
        {
            if (!uiActiveStates.ContainsKey(ui))
                uiActiveStates.Add(ui, ui.activeSelf);
            ui.SetActive(false);
        }
    }

    bool ShouldKeepHideUIButtonVisible(GameObject ui)
    {
        if (hideUI == null)
            return false;

        return ui == hideUI || hideUI.transform.IsChildOf(ui.transform);
    }
    public static GameScript instance;
    private void Awake()
    {
        instance = this;
        PrepareTrayCounter(goldTileCountGO.GetComponent<RectTransform>(), new Vector2(260f, 130f));
        PrepareTrayCounter(comboCountGO.GetComponent<RectTransform>(), new Vector2(260f, 130f));
        foreach (var popup in FindObjectsByType<Popup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            popup.SetupTweenCallback();
        }
        if (hideUI != null)
            hideUI.SetActive(true);
    }
    //private void Start()
    //{
    //GameConfig.ShowCollapseBannerAd();
    //GameConfig.InterStartLevel();
    //}
    private void Start()
    {
        if (!GameConfig.isGameLoaded)
            return;
        PowerUpsStrings();
        AudioManager.instance.PlayBGMGame();
        GameConfig.ShowCollapseBannerAd();
    }
    //public List<Button> powerButtons;
    public CanvasBG canvasBG;
    private void OnValidate()
    {
        //var popupList = FindObjectsByType<Popup>(FindObjectsInactive.Include, FindObjectsSortMode.None).OrderBy(x => x.transform.GetSiblingIndex()).ToList();
        var popupList = FindObjectsByType<Popup>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        popupList.Sort((a, b) => GameConfig.CompareHierarchy(a.transform, b.transform));
        canvasBG.images = new() { canvasBG.bg };
        canvasBG.bgImages = new() { canvasBG.bg };
        foreach (var popup in popupList)
        {
            if (popup.bg)
            {
                canvasBG.images.Add(popup.bg);
                if (popup.isBGChange)
                    canvasBG.bgImages.Add(popup.bg);
            }
        }
    }
    [Header("undo")]
    public Button btUndo;
    public Image undoImg;
    public Image[] undoAnim;
    public Sprite[] undoSprites;
    public TextMeshProUGUI undoTxt;
    public GameObject undoCounterGO;
    public GameObject undoBuyCoinGO;
    public TextMeshProUGUI undoPriceTxt;
    public GameObject undoWatchAdGO;
    [Header("shuffle")]
    public Button btShuffle;
    public Image shuffleImg;
    public Image[] shuffleAnim;
    public Sprite[] shuffleSprites;
    public TextMeshProUGUI shuffleTxt;
    public GameObject shuffleCounterGO;
    public GameObject shuffleBuyCoinGO;
    public TextMeshProUGUI shufflePriceTxt;
    public GameObject shuffleWatchAdGO;
    [Header("suggest")]
    public Button btSuggest;
    public Image suggestImg;
    public Image[] suggestAnim;
    public Sprite[] suggestSprites;
    public TextMeshProUGUI suggestTxt;
    public GameObject suggestCounterGO;
    public GameObject suggestBuyCoinGO;
    public TextMeshProUGUI suggestPriceTxt;
    public GameObject suggestWatchAdGO;
    [Header("addslott")]
    public Button btAddSlot;
    public Image addSlotImg;
    public Sprite[] addSlotSprites;
    public TextMeshProUGUI addSlotTxt;
    public GameObject addSlotCounterGO;
    public GameObject addSlotBuyCoinGO;
    public TextMeshProUGUI addSlotPriceTxt;
    public GameObject addSlotWatchAdGO;
    public GameObject addSlotWatchAdGO_;
    [Space]
    public TopBarScript topBarScript;
    public GameObject levelGO;
    public GameObject levelTextGO;
    public TextMeshProUGUI txLevel;
    public GameObject levelTagGO;
    public Sprite[] levelTagSprites; //0:gold 1:hard 2:super
    public Image levelTagImg;
    public TextMeshProUGUI txLevelTag;
    [Space]
    public GameObject goldTileCountGO;
    public TextMeshProUGUI txGoldTileCounter;
    [Space]
    public GameObject comboCountGO;
    public TextMeshProUGUI txComboCounter;
    [SerializeField] private SpriteRenderer objectiveTrayFrame;
    private RectTransform modeObjectiveCounter;
    [Space]
    public GameObject challengeTimerGO;
    public TextMeshProUGUI txChallengeTimer;
    public float timer;

    private void OnEnable()
    {
        undoPriceTxt.text = $"{GameConfig.powerPrices[0]}";
        suggestPriceTxt.text = $"{GameConfig.powerPrices[1]}";
        shufflePriceTxt.text = $"{GameConfig.powerPrices[2]}";
        HideAddSlotPowerUp();
        PowerUpsStrings();
        GameConfig.OnGoldChange += OnGoldChange;
        GameConfig.OnPowerUpsChange += PowerUpsStrings;
        EventTileExtend.OnEventCounter += OnEventCounter;
        GameTextScript.OnComboCounter += OnComboCounter;
    }
    private void OnDisable()
    {
        levelEntranceSequence?.Kill(false);
        FinishLevelEntrance();
        homeExitSequence?.Kill(false);
        homeExitSequence = null;
        GameConfig.OnGoldChange -= OnGoldChange;
        GameConfig.OnPowerUpsChange -= PowerUpsStrings;
    }
    void OnGoldChange(int gold, bool increase)
    {
        PowerUpsStrings();
    }
    public void TopBarUpdateGolds(Transform pos, int goldsGain, int mod = 1, Action callback = null)
    {
        StartCoroutine(topBarScript.UpdateGolds(pos, goldsGain, mod, callback));
    }
    public void LevelText()
    {
        txLevel.text = $"Level {GameConfig.level}";
        int diff = LevelManager.instance.GetLevelDifficulty(GameConfig.level);
        levelTagGO.SetActive(false);
        // Apply this at runtime as well as in the scene so loaded UI state
        // cannot bring back the level frame or difficulty banner.
        if (levelGO.TryGetComponent<Image>(out var levelFrame))
            levelFrame.enabled = false;
        levelTagImg.enabled = false;
        Color32 outlineColor = diff switch
        {
            1 => new Color32(220, 35, 45, 255),
            2 => new Color32(140, 45, 200, 255),
            _ => new Color32(25, 65, 125, 255)
        };
        StyleLevelText(txLevel, outlineColor);
        levelGO.SetActive(true);
        levelGO.transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private static void StyleLevelText(TextMeshProUGUI text, Color32 outlineColor)
    {
        text.color = Color.white;
        text.outlineColor = outlineColor;
        text.outlineWidth = 0.175f;
        // The original material uses a red outline texture. Use white so the
        // per-text outline color also renders correctly for Super Hard levels.
        text.fontMaterial.SetTexture("_OutlineTex", Texture2D.whiteTexture);
    }
    public void ChallengeText()
    {
        levelGO.SetActive(false);
        timer = 0;
        TimeSpan ts = TimeSpan.FromSeconds(timer);
        txChallengeTimer.text = $"{Mathf.Floor((float)ts.TotalMinutes):00}:{ts:ss}";
        challengeTimerGO.SetActive(true);
    }
    private void Update()
    {
        if (!GameConfig.isGameLoaded)
            return;
        if (GameController.instance.gameState == GAME_STATE.START ||
            GameController.instance.gameState == GAME_STATE.PAUSE ||
            GameController.instance.gameState == GAME_STATE.WIN ||
            GameController.instance.gameState == GAME_STATE.WIN_POPUP ||
            GameController.instance.gameState == GAME_STATE.LOSE ||
            GameController.instance.gameState == GAME_STATE.LOSE_POPUP)
            return;
        if (challengeTimerGO.activeSelf)
        {
            timer += Time.deltaTime;
            TimeSpan ts = TimeSpan.FromSeconds(timer);
            txChallengeTimer.text = $"{Mathf.Floor((float)ts.TotalMinutes):00}:{ts:ss}";
        }
    }
    public void OnEventCounter(int count, int total)
    {
        txGoldTileCounter.text = $"{count}/{total}";
    }

    private void LateUpdate()
    {
        if (!IsLevelEntering && !IsReturningHome)
            PositionTrayCounters();
    }

    public void RegisterModeObjectiveCounter(RectTransform counter)
    {
        modeObjectiveCounter = counter;
        PrepareTrayCounter(counter, new Vector2(850f, 64f));
        PositionTrayCounters();
    }

    private void PrepareTrayCounter(RectTransform counter, Vector2 size)
    {
        if (objectiveTrayFrame == null || counter == null)
            return;
        // Keep counters independent of the level label and its visibility.
        counter.SetParent(levelGO.transform.parent, false);
        counter.anchorMin = counter.anchorMax = new Vector2(0.5f, 0.5f);
        counter.pivot = new Vector2(0.5f, 0f);
        counter.sizeDelta = size;
    }

    private void PositionTrayCounters()
    {
        if (objectiveTrayFrame == null || Camera.main == null)
            return;
        var parent = (RectTransform)levelGO.transform.parent;
        var canvas = parent.GetComponentInParent<Canvas>();
        var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Bounds trayBounds = objectiveTrayFrame.bounds;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(new Vector3(
            trayBounds.center.x, trayBounds.max.y, trayBounds.center.z));
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, uiCamera, out var point))
            return;

        Vector2 position = point + Vector2.up * 8f;
        var goldCounter = (RectTransform)goldTileCountGO.transform;
        var comboCounter = (RectTransform)comboCountGO.transform;
        goldCounter.anchoredPosition = position;
        comboCounter.anchoredPosition = position;
        if (modeObjectiveCounter != null)
        {
            float extraHeight = goldTileCountGO.activeSelf || comboCountGO.activeSelf ? 130f : 0f;
            modeObjectiveCounter.anchoredPosition = position + Vector2.up * extraHeight;
        }
    }
    int GetCurrentEventTarget()
    {
        int prizeClaimed = EventManager.GetEventPrizeClaimed();
        try
        {
            return EventManager.eventType switch
            {
                EventType.combo_event_1 => EventManager.eventComboPrizes[prizeClaimed].collect,
                EventType.streak_event_1 => EventManager.eventStreakPrizes[prizeClaimed].collect + EventManager.GetStreakAdditive(),
                _ => 0
            };
        }
        catch
        {
            return 0;
        }
    }
    public void HideEventCounters()
    {
        goldTileCountGO.SetActive(false);
        comboCountGO.SetActive(false);
        if (!GameConfig.isChallengeLevel)
            levelTextGO.SetActive(true);
    }
    public void EventGoalCounter()
    {
        HideEventCounters();
        if (!EventManager.instance.isEvent || GameConfig.isChallengeLevel)
            return;

        switch (EventManager.eventType)
        {
            case EventType.collect_event_1:
                OnEventCounter(EventTileExtend.eventCounter, EventTileExtend.eventTotal);
                levelTextGO.SetActive(false);
                goldTileCountGO.SetActive(true);
                break;
            case EventType.combo_event_1:
                OnComboCounter(EventManager.GetComboCount() + GameTextScript.comboCount);
                int comboTarget = GetCurrentEventTarget();
                if (comboTarget > 0)
                    txComboCounter.text = $"{EventManager.GetComboCount() + GameTextScript.comboCount}/{comboTarget}";
                levelTextGO.SetActive(false);
                comboCountGO.SetActive(true);
                break;
            case EventType.streak_event_1:
                int streakTarget = GetCurrentEventTarget();
                txComboCounter.text = streakTarget > 0 ? $"{EventManager.GetCurrentStreak()}/{streakTarget}" : $"{EventManager.GetCurrentStreak()}";
                levelTextGO.SetActive(false);
                comboCountGO.SetActive(true);
                break;
        }
    }
    public void GoldTileCounter()
    {
        HideEventCounters();
        OnEventCounter(EventTileExtend.eventCounter, EventTileExtend.eventTotal);
        levelTextGO.SetActive(false);
        goldTileCountGO.SetActive(true);
        GameController.instance.eventParent.SetParent(GameController.instance.eventParentUI);
        GameController.instance.eventParent.localPosition = Vector3.zero;
    }
    public void GoldTileUpdate(EventTileExtend eventTile)
    {
        eventTile.BeginCounterAnimation();
        eventTile.glow.gameObject.SetActive(true);
        eventTile.transform.SetParent(GameController.instance.eventParent, true);
        eventTile.transform.DORotate(Vector3.zero, 0);
        eventTile.transform.DOScale(GameController.instance.slotParent.localScale.x * 1.5f, 0.35f);
        Vector3 halfLocalPos = eventTile.transform.localPosition / 2;
        Vector3 curveLocation = halfLocalPos + (halfLocalPos.x < 0 ? new Vector3(halfLocalPos.y, -halfLocalPos.x) : new Vector3(-halfLocalPos.y, halfLocalPos.x)) / 4;
        eventTile.transform.DOScale(Vector3.zero, 0.5f).SetDelay(0.5f);
        eventTile.transform.DOLocalRotate(new Vector3(0, 0, 360 * (halfLocalPos.x < 0 ? -1 : 1)), 0.35f, RotateMode.FastBeyond360).SetDelay(0.5f);
        eventTile.transform.DOLocalPath(new Vector3[] { curveLocation, Vector3.zero }, 0.35f, PathType.CatmullRom).SetDelay(0.5f)
            .OnComplete(() =>
            {
                EventTileExtend.EventCounterUpdate();
                eventTile.FinishCounterAnimation();
            });
    }
    public void OnComboCounter(int count)
    {
        if (EventManager.eventType == EventType.combo_event_1)
        {
            int target = GetCurrentEventTarget();
            txComboCounter.text = target > 0 ? $"{EventManager.GetComboCount() + count}/{target}" : $"{EventManager.GetComboCount() + count}";
        }
        else
        {
            txComboCounter.text = $"{count}";
        }
    }
    public void ComboCounter()
    {
        HideEventCounters();
        OnComboCounter(GameTextScript.comboCount);
        levelTextGO.SetActive(false);
        comboCountGO.SetActive(true);
    }
    #region powerUI
    void PowerUpsStrings()
    {
        var powerUps = GameConfig.GetPowerUps();
        if (GameConfig.level >= 2 || GameConfig.isChallengeLevel)
        {
            //btUndo.interactable = true;
            if (powerUps[0] != 0)
            {
                undoTxt.text = powerUps[0].ToString();
                //undoTxt.transform.parent.GetComponent<Image>().sprite = powerTxtSprite[0];
                undoImg.sprite = undoSprites[0];
                btUndo.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                undoCounterGO.SetActive(true);
                undoBuyCoinGO.SetActive(false);
                undoWatchAdGO.SetActive(false);
            }
            else
            {
                undoTxt.text = "";
                //undoTxt.transform.parent.GetComponent<Image>().sprite = powerTxtSprite[1];
                undoImg.sprite = undoSprites[0];
                btUndo.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                undoCounterGO.SetActive(false);
                bool powerUpBuyable = GameConfig.GetGold() >= GameConfig.powerPrices[0];
                undoBuyCoinGO.SetActive(powerUpBuyable);
                undoWatchAdGO.SetActive(!powerUpBuyable);
            }
        }
        else
        {
            btUndo.transition = Selectable.Transition.None;
            undoImg.sprite = undoSprites[1];
            btUndo.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            btUndo.interactable = false;
            undoTxt.text = "";
            //undoTxt.transform.parent.gameObject.SetActive(false);
            undoCounterGO.SetActive(false);
            undoBuyCoinGO.SetActive(false);
            undoWatchAdGO.SetActive(false);
        }

        //if (Config.GetTutorial() >= 3 && 
        if (GameConfig.level >= 2 || GameConfig.isChallengeLevel)
        {
            //btShuffle.interactable = true;
            if (powerUps[2] != 0)
            {
                shuffleTxt.text = powerUps[2].ToString();
                //shuffleTxt.transform.parent.GetComponent<Image>().sprite = powerTxtSprite[0];
                shuffleImg.sprite = shuffleSprites[0];
                btShuffle.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                shuffleCounterGO.SetActive(true);
                shuffleBuyCoinGO.SetActive(false);
                shuffleWatchAdGO.SetActive(false);
            }
            else
            {
                shuffleTxt.text = "";
                //shuffleTxt.transform.parent.GetComponent<Image>().sprite = powerTxtSprite[1];
                shuffleImg.sprite = shuffleSprites[0];
                btShuffle.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                shuffleCounterGO.SetActive(false);
                bool powerUpBuyable = GameConfig.GetGold() >= GameConfig.powerPrices[2];
                shuffleBuyCoinGO.SetActive(powerUpBuyable);
                shuffleWatchAdGO.SetActive(!powerUpBuyable);
            }
        }
        else
        {
            btShuffle.transition = Selectable.Transition.None;
            shuffleImg.sprite = shuffleSprites[1];
            btShuffle.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            btShuffle.interactable = false;
            shuffleTxt.text = "";
            //shuffleTxt.transform.parent.gameObject.SetActive(false);
            shuffleCounterGO.SetActive(false);
            shuffleBuyCoinGO.SetActive(false);
            shuffleWatchAdGO.SetActive(false);
        }


        //if (Config.GetTutorial() >= 2 && 
        if (GameConfig.level >= 2 || GameConfig.isChallengeLevel)
        {
            //btSuggest.interactable = true;
            if (powerUps[1] != 0)
            {
                suggestTxt.text = powerUps[1].ToString();
                //suggestTxt.transform.parent.GetComponent<Image>().sprite = powerTxtSprite[0];
                suggestImg.sprite = suggestSprites[0];
                btSuggest.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                suggestCounterGO.SetActive(true);
                suggestBuyCoinGO.SetActive(false);
                suggestWatchAdGO.SetActive(false);
            }
            else
            {
                suggestTxt.text = "";
                //suggestTxt.transform.parent.GetComponent<Image>().sprite = powerTxtSprite[1];
                suggestImg.sprite = suggestSprites[0];
                btSuggest.transform.GetChild(1).GetComponent<Image>().color = Color.white;
                suggestCounterGO.SetActive(false);
                bool powerUpBuyable = GameConfig.GetGold() >= GameConfig.powerPrices[2];
                suggestBuyCoinGO.SetActive(powerUpBuyable);
                suggestWatchAdGO.SetActive(!powerUpBuyable);
            }
        }
        else
        {
            btSuggest.transition = Selectable.Transition.None;
            suggestImg.sprite = suggestSprites[1];
            btSuggest.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            btSuggest.interactable = false;
            suggestTxt.text = "";
            //suggestTxt.transform.parent.gameObject.SetActive(false);
            suggestCounterGO.SetActive(false);
            suggestBuyCoinGO.SetActive(false);
            suggestWatchAdGO.SetActive(false);
        }
        HideAddSlotPowerUp();
    }
    public void PowerUpsInteractable()
    {
        if (GameConfig.level >= 2 || GameConfig.isChallengeLevel)
        {
            btUndo.interactable = GameController.instance.undoList.Count > 0;
            undoImg.DOFade(btUndo.interactable ? 1 : 0.5f, 0);
        }

        if (GameConfig.level >= 2 || GameConfig.isChallengeLevel)
        {
            btShuffle.interactable = GameController.instance.itemTiles.Count - GameController.instance.slottedItems.Count > 0;
            shuffleImg.DOFade(btShuffle.interactable ? 1 : 0.5f, 0);
        }

        if (GameConfig.level >= 2 || GameConfig.isChallengeLevel)
        {
            try
            {
                int count = GameController.instance.SuggestItem().Count;
                btSuggest.interactable = count > 0;
                suggestImg.DOFade(btSuggest.interactable ? 1 : 0.5f, 0);
                Debug.Log("suggest count " + count);
            }
            catch (Exception ex)
            {
                Debug.Log("suggest exception " + ex);
                btSuggest.interactable = false;
                suggestImg.DOFade(btSuggest.interactable ? 1 : 0.5f, 0);
            }
        }

        HideAddSlotPowerUp();
    }
    void HideAddSlotPowerUp()
    {
        if (btAddSlot)
        {
            btAddSlot.interactable = false;
            btAddSlot.gameObject.SetActive(false);
        }
        if (addSlotTxt)
            addSlotTxt.text = "";
        if (addSlotCounterGO)
            addSlotCounterGO.SetActive(false);
        if (addSlotBuyCoinGO)
            addSlotBuyCoinGO.SetActive(false);
        if (addSlotWatchAdGO)
            addSlotWatchAdGO.SetActive(false);
        if (addSlotWatchAdGO_)
            addSlotWatchAdGO_.SetActive(false);
    }

    bool isItemsAnim;
    Coroutine itemsAnimCoroutine;
    public IEnumerator StartItemsAnim()
    {
        yield return new WaitForSeconds(5);
        if (isItemsAnim || GameController.instance.gameState != GAME_STATE.PLAYING)
            yield break;
        List<Image[]> anims = new();
        if (btShuffle.interactable)
        {
            anims.Add(shuffleAnim);
        }
        if (btSuggest.interactable)
        {
            anims.Add(suggestAnim);
        }
        //if (btUndo.interactable)
        //{
        //    anims.Add(undoAnim);
        //}
        //List<string> animNames = new();
        //foreach (var anim in anims)
        //{
        //    animNames.Add(anim[0].name);
        //    animNames.Add(anim[1].name);
        //}
        //Debug.Log(string.Join("|", animNames));
        if (anims.Count > 0)
        {
            Image[] usingAnims = GameConfig.GetRandom(anims.ToArray());
            foreach (var anim in usingAnims)
                anim.DOFade(1, 0.75f).SetLoops(-1, LoopType.Yoyo);
            isItemsAnim = true;
        }
        GameController.instance.IdleAnimPlay();
        //GameController.instance.IdleChallenge();
    }
    public void EndItemsAnim()
    {
        if (itemsAnimCoroutine != null)
        {
            StopCoroutine(itemsAnimCoroutine);
            itemsAnimCoroutine = null;
        }
        isItemsAnim = false;
        shuffleAnim[0].DOKill();
        shuffleAnim[1].DOKill();
        shuffleAnim[0].DOFade(0, 0);
        shuffleAnim[1].DOFade(0, 0);
        suggestAnim[0].DOKill();
        suggestAnim[1].DOKill();
        suggestAnim[0].DOFade(0, 0);
        suggestAnim[1].DOFade(0, 0);
        undoAnim[0].DOKill();
        undoAnim[1].DOKill();
        undoAnim[0].DOFade(0, 0);
        undoAnim[1].DOFade(0, 0);
        itemsAnimCoroutine = StartCoroutine(StartItemsAnim());
    }
    #endregion


    //ad break
    //public PopupAdBreak adbreakPopup;
    //public float timer = 0;
    //private void Update()
    //{
    //    //check if playing
    //    if (winPopup.Showing || losePopup.Showing)
    //        return;

    //    timer += Time.deltaTime;
    //    if (timer > Common.ads_inter_ad_break_time)
    //    {
    //        if (!adbreakPopup.Showing && GameConfig.InterAvailable(true))
    //            adbreakPopup.Open();
    //    }
    //}
    public void Revive()
    {
        GameConfig.reviveCount++;
        GameController.instance.OnClickRevive();
    }
    public void Restart()
    {
        if (IsReturningHome)
            return;
        GameController.instance.ClearSavedProgress();
        GameConfig.reviveCount = 0;
        GameConfig.loseCount++;
        PoolManager.instance.ReleaseAllObjects();
        LoadingCamera.instance.StartLoad(GameConfig.gameScene, true);
    }
    public void Quit()
    {
        if (IsReturningHome)
            return;
        GameController.instance.ClearSavedProgress();
        Home();
    }
    public void Home(bool clearSavedProgress = true)
    {
        if (IsReturningHome)
            return;
        if (clearSavedProgress && GameController.instance)
            GameController.instance.ClearSavedProgress();
        IsReturningHome = true;
        GameConfig.reviveCount = 0;
        GameConfig.loseCount = 0;
        GameConfig.boosterCount = 0;
        // Cancel old callbacks before moving tiles, and keep the saved board unchanged.
        StopAllCoroutines();
        if (GameController.instance != null)
        {
            GameController.instance.StopAllCoroutines();
            GameController.instance.gameState = GAME_STATE.PAUSE;
        }
        DOTween.KillAll();
        AnimateReturnHome();
    }

    public void AnimateLevelEntrance()
    {
        if (IsReturningHome || IsLevelEntering)
            return;

        PositionTrayCounters();

        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None)
            .Where(button => button.gameObject.scene == gameObject.scene &&
                button.GetComponentInParent<Popup>() == null);
        var candidates = GetLevelUIElements(buttons).Distinct().ToList();
        var targets = candidates.Where(target => !candidates.Any(parent => parent != target && target.IsChildOf(parent)))
            .OrderByDescending(target => target.position.y).ThenBy(target => target.position.x).ToList();
        if (targets.Count == 0)
            return;

        IsLevelEntering = true;
        Canvas.ForceUpdateCanvases();
        float duration = Mathf.Max(0.1f, levelEntranceDuration);
        float slideDuration = targets.Count > 1 ? duration * 0.6f : duration;
        float stagger = targets.Count > 1 ? (duration - slideDuration) / (targets.Count - 1) : 0f;
        var camera = Camera.main;
        levelEntranceSequence = DOTween.Sequence().SetUpdate(true);
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            target.DOKill(false);
            levelEntrancePositions[target] = target.localPosition;
            var destination = target.position;
            target.position = target is RectTransform rect && rect.GetComponentInParent<Canvas>() != null
                ? HomeScript.GetUIExitPosition(rect)
                : GetWorldExitPosition(target, camera);
            levelEntranceSequence.Insert(i * stagger,
                target.DOMove(destination, slideDuration).SetEase(Ease.OutBack));
        }
        // Restore the layout both on completion and if another scene transition interrupts it.
        levelEntranceSequence.OnComplete(FinishLevelEntrance).OnKill(FinishLevelEntrance);
    }

    private void FinishLevelEntrance()
    {
        foreach (var item in levelEntrancePositions)
        {
            if (item.Key != null)
                item.Key.localPosition = item.Value;
        }
        levelEntrancePositions.Clear();
        levelEntranceSequence = null;
        IsLevelEntering = false;
    }

    private List<Transform> GetLevelUIElements(IEnumerable<Button> buttons)
    {
        var elements = buttons.Select(button => button.transform).ToList();
        var tray = GameController.instance != null ? GameController.instance.slotParent : null;
        if (tray != null && tray.gameObject.activeInHierarchy)
            elements.Add(tray);
        foreach (var counter in new[] { levelGO, goldTileCountGO, comboCountGO, challengeTimerGO })
        {
            if (counter != null && counter.activeInHierarchy)
                elements.Add(counter.transform);
        }
        if (modeObjectiveCounter != null && modeObjectiveCounter.gameObject.activeInHierarchy)
            elements.Add(modeObjectiveCounter);
        if (topBarScript != null && topBarScript.goldsCounter != null && topBarScript.goldsCounter.activeInHierarchy)
            elements.Add(topBarScript.goldsCounter.transform);
        return elements;
    }

    private void AnimateReturnHome()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None)
            .Where(button => button.gameObject.scene == gameObject.scene).ToList();
        foreach (var button in buttons)
            button.interactable = false;

        // Include pooled tiles in the tray even if they have moved outside the board hierarchy.
        var tiles = FindObjectsByType<ItemTile>(FindObjectsSortMode.None)
            .Where(tile => tile.gameObject.scene == gameObject.scene ||
                (GameController.instance != null &&
                 ((GameController.instance.itemTiles != null && GameController.instance.itemTiles.Contains(tile)) ||
                  (GameController.instance.slottedItems != null && GameController.instance.slottedItems.Contains(tile)))))
            .ToList();
        foreach (var tile in tiles)
        {
            tile.StopTileAppearAnimation();
            tile.CancelInvoke();
            tile.StopAllCoroutines();
            foreach (var collider in tile.GetComponentsInChildren<Collider2D>())
                collider.enabled = false;
        }

        var ui = GetLevelUIElements(buttons);

        // Move open popup content as a group and clear its dimmer so the board exit remains visible.
        foreach (var popup in FindObjectsByType<Popup>(FindObjectsSortMode.None))
        {
            if (popup.gameObject.scene != gameObject.scene || popup.panel == null || !popup.Showing)
                continue;
            if (popup.bg != null)
                popup.bg.DOFade(0f, 0.2f).SetUpdate(true);
            if (popup.box != null && popup.box.gameObject.activeInHierarchy)
                ui.Add(popup.box);
        }

        var roots = ui.Distinct().Where(target => target.gameObject.activeInHierarchy).ToList();
        roots = roots.Where(target => !roots.Any(parent => parent != target && target.IsChildOf(parent))).ToList();
        var candidates = tiles.Select(tile => tile.transform).Concat(roots).Distinct().ToList();
        var targets = candidates.Where(target => !candidates.Any(parent => parent != target && target.IsChildOf(parent)))
            .OrderByDescending(target => target.position.y).ThenBy(target => target.position.x).ToList();
        Canvas.ForceUpdateCanvases();

        float duration = Mathf.Max(0.1f, homeExitDuration);
        float slideDuration = targets.Count > 1 ? duration * 0.4f : duration;
        float stagger = targets.Count > 1 ? (duration - slideDuration) / (targets.Count - 1) : 0f;
        var camera = Camera.main;
        // Pick once per return: nearest edge (original), up, down, left or right.
        var exitEdges = new[] { -1, 2, 3, 0, 1 };
        int exitEdge = exitEdges[UnityEngine.Random.Range(0, exitEdges.Length)];
        homeExitSequence = DOTween.Sequence().SetUpdate(true);
        homeExitSequence.AppendInterval(duration);
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            target.DOKill(false);
            var destination = target is RectTransform rect && rect.GetComponentInParent<Canvas>() != null
                ? HomeScript.GetUIExitPosition(rect, exitEdge)
                : GetWorldExitPosition(target, camera, exitEdge);
            homeExitSequence.Insert(i * stagger, target.DOMove(destination, slideDuration).SetEase(Ease.InBack));
        }
        float overlapDuration = duration * 0.35f;
        homeExitSequence.InsertCallback(duration - overlapDuration, () =>
        {
            // Preserve the outgoing frame while Home loads, then overlap its last slide/fade
            // with the new Home entrance. Keep a single scene active for gameplay managers.
            homeExitSequence.Pause();
            LoadingCamera.instance.StartHomeReturnTransition(overlapDuration, exitEdge, () =>
            {
                homeExitSequence?.Kill(false);
                homeExitSequence = null;
                DOTween.KillAll();
                PoolManager.instance.ReleaseAllObjects();
            });
        });
    }

    private static Vector3 GetWorldExitPosition(Transform target, Camera camera, int exitEdge = -1)
    {
        if (camera == null)
            return target.position + Vector3.down * 30f;

        var bounds = new Bounds(target.position, Vector3.zero);
        foreach (var sprite in target.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sprite.enabled)
                bounds.Encapsulate(sprite.bounds);
        }
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (int x = 0; x < 2; x++)
        for (int y = 0; y < 2; y++)
        for (int z = 0; z < 2; z++)
        {
            var corner = camera.WorldToScreenPoint(new Vector3(x == 0 ? bounds.min.x : bounds.max.x,
                y == 0 ? bounds.min.y : bounds.max.y, z == 0 ? bounds.min.z : bounds.max.z));
            min = Vector2.Min(min, corner);
            max = Vector2.Max(max, corner);
        }
        var screen = camera.pixelRect;
        var distances = new[] { max.x - screen.xMin, screen.xMax - min.x,
            screen.yMax - min.y, max.y - screen.yMin };
        var directions = new[] { Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        int edge = exitEdge;
        if (edge < 0)
        {
            edge = 0;
            for (int i = 1; i < distances.Length; i++)
            {
                if (distances[i] < distances[edge])
                    edge = i;
            }
        }
        var position = camera.WorldToScreenPoint(target.position);
        return camera.ScreenToWorldPoint(position + directions[edge] * (Mathf.Max(0f, distances[edge]) + 64f));
    }
    bool nextLevelLoading;
    public void Next()
    {
        if (nextLevelLoading || IsReturningHome)
            return;
        nextLevelLoading = true;
        int completedLevel = GameConfig.level;
        try
        {
            GameController.instance.ClearSavedProgress();
            GameConfig.reviveCount = 0;
            GameConfig.loseCount = 0;
            GameConfig.boosterCount = 0;
            // Stop callbacks before returning their targets to the persistent pool.
            DOTween.KillAll();
            PoolManager.instance.ReleaseAllObjects();
            GameConfig.level = completedLevel + 1;
            LoadingCamera.instance.StartLoad(GameConfig.gameScene);
        }
        catch
        {
            GameConfig.level = completedLevel;
            nextLevelLoading = false;
            throw;
        }
    }
    public void ContinueGame()
    {
        if (IsReturningHome)
            return;
        GameController.instance.gameState = GAME_STATE.PLAYING;
    }
    public PopupTutorial tutorialPopup;
    public void OpenTutorial(GAME_MODE gameMode)
    {
        GameController.instance.gameState = GAME_STATE.PAUSE;
        tutorialPopup.OpenTutorial(LevelManager.GetGameModeIndex(gameMode));
    }
    public void OpenHardPopup(int diff)
    {
        if (diff == 1)
            tutorialPopup.OpenHardPopup();
        else if (diff == 2)
            tutorialPopup.OpenSuperHardPopup();
        else
            ContinueGame();
    }
    public void OpenAddSlotPopup()
    {
    }
    public PopupQuit quitPopup;
    public void OpenQuit()
    {
        if (!GameController.instance.UIClickable)
            return;
        GameController.instance.SaveProgress();
        Home(false);
    }
    public PopupWin winPopup;
    public PopupWinChallenge winChallengePopup;
    public void Win()
    {
        GameController.instance.ClearSavedProgress();
        GameController.instance.gameState = GAME_STATE.WIN_POPUP;
        winPopup.Open();
    }
    public void WinChallenge()
    {
        GameController.instance.ClearSavedProgress();
        GameController.instance.gameState = GAME_STATE.WIN_POPUP;
        winChallengePopup.OpenWinChallenge(timer);
    }
    public PopupChallengeInfo challengeInfoPopup;
    public void OnChallengeInfo()
    {
        challengeInfoPopup.Open();
    }
    public PopupLose losePopup;
    public void Lose()
    {
        GameController.instance.ClearSavedProgress();
        GameController.instance.gameState = GAME_STATE.LOSE_POPUP;
        losePopup.Open();
    }
    public PopupBonus bonusPopup;
    public void OpenBonus(Action callback)
    {
        bonusPopup.OpenBonus(false, callback);
    }
    public PopupMapPreview mapPreviewPopup;
    public void OnPreviewMap(int bgIndex, Action callback)
    {
        mapPreviewPopup.OpenMapPreview(bgIndex, 0, true, callback, true);
    }
    public PopupSettings settingsPopup;
    public void OnSettings()
    {
        if (!GameController.instance.UIClickable)
            return;
        GameController.instance.gameState = GAME_STATE.PAUSE;
        settingsPopup.Open();
    }
    public PopupRateUs rateUsPopup;
    public void OpenRateUs()
    {
        rateUsPopup.Open();
    }
    public PopupShop shopPopup;
    public void OnShop()
    {
        if (!GameController.instance.UIClickable)
            return;
        GameController.instance.gameState = GAME_STATE.PAUSE;
        shopPopup.Open();
    }
    public PopupCollectionEvent collectionEventPopup;
    public PopupStreakEvent streakEventPopup;
    public PopupComboEvent comboEventPopup;
    public void OnOpenEvent()
    {
        switch (EventManager.eventType)
        {
            case EventType.collect_event_1:
                collectionEventPopup.Open();
                break;
            case EventType.streak_event_1:
                streakEventPopup.Open();
                break;
            case EventType.combo_event_1:
                comboEventPopup.Open();
                break;
        }
    }
    public PopupEventTutorial collectionTutorialPopup;
    public PopupEventTutorial comboTutorialPopup;
    public void OpenEventTutorial()
    {
        switch (EventManager.eventType)
        {
            case EventType.collect_event_1:
                collectionTutorialPopup.OpenTutorial();
                break;
            case EventType.combo_event_1:
                comboTutorialPopup.OpenTutorial();
                break;
        }
    }
    public void OnStartGameEventTutorial()
    {
        switch (EventManager.eventType)
        {
            case EventType.collect_event_1:
                collectionTutorialPopup.OpenTutorial(true);
                break;
            case EventType.combo_event_1:
                comboTutorialPopup.OpenTutorial(true);
                break;
        }
    }
    public PopupUnlock unlockPopup;
    public void OpenUnlock(int unlock)
    {
        unlockPopup.OpenUnlock(unlock);
    }
    public PopupDailyLogin dailyLoginPopup;
    public void OnDailyLogin()
    {
        dailyLoginPopup.Open();
    }
    public PopupFreeGold freeGoldPopup;
    public void OnOpenFreeGold()
    {
        freeGoldPopup.Open();
    }
    public void OnAddGold()
    {
        GameConfig.AddGold(100000);
    }
    public PopupPiggyBank piggyBankPopup;
    public void OnOpenPiggyBank()
    {
        piggyBankPopup.Open();
    }
    public PopupRank rankPopup;
    public void OnOpenRank()
    {
        rankPopup.Open();
    }
    public PopupBattlePass battlePassPopup;
    public void OnOpenBattlePass()
    {
        battlePassPopup.Open();
    }
}
