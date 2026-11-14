using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTutorial : Popup
{
    [Header("0 => GAME_MODE.normal\n" +
        "1 => GAME_MODE.tile_random\n" +
        "2 => GAME_MODE.tile_double\n" +
        "3 => GAME_MODE.tile_freeze\n" +
        "4 => GAME_MODE.tile_gold\n" +
        "5 => GAME_MODE.tile_2up\n" +
        "6 => GAME_MODE.tile_rainbow\n" +
        "7 => GAME_MODE.tile_fog\n" +
        "//8 => GAME_MODE.tile_color")]
    public GameObject[] tutorials;
    [SerializeField] private Sprite tutorialFrameSprite;
    public Button btPanel;
    public GameObject addSlotPopup;
    public GameObject hardPopup;
    public GameObject superHardPopup;
    const string hardFormat = "<color=#CF0000>{0}%</color> players\npass this level";
    public TextMeshProUGUI txHard;
    const string superHardFormat = "<color=#CF00CF>{0}%</color> players\npass this level";
    public TextMeshProUGUI txSuperHard;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btPanel.onClick.AddListener(() => ClosePanel());
        });
    }
    public void OpenTutorial(int tutIndex)
    {
        foreach (var tut in tutorials)
            tut.SetActive(false);
        HideOptionalPopup(newModePanel);
        HideOptionalPopup(addSlotPopup);
        HideOptionalPopup(hardPopup);
        HideOptionalPopup(superHardPopup);
        // Revised modes use text-only instructions; legacy tile diagrams describe old rules.
        if (tutIndex == 5 || NewModeRules.IsExtendedMode(tutIndex))
        {
            OpenTextModeTutorial(tutIndex);
            return;
        }
        if (tutIndex < 0 || tutIndex >= tutorials.Length)
        {
            Close();
            return;
        }
        tutorials[tutIndex].SetActive(true);
        Open();
    }
    TextMeshProUGUI modeTutorial;
    TextMeshProUGUI modeTutorialTitle;
    GameObject newModePanel;

    void OpenTextModeTutorial(int mode)
    {
        if (modeTutorial == null)
        {
            newModePanel = new GameObject("Text-only mode tutorial", typeof(RectTransform), typeof(Image));
            newModePanel.transform.SetParent(box, false);
            var go = new GameObject("New mode instructions", typeof(TextMeshProUGUI));
            go.transform.SetParent(newModePanel.transform, false);
            modeTutorial = go.GetComponent<TextMeshProUGUI>();
            modeTutorial.font = txHard != null ? txHard.font : TMP_Settings.defaultFontAsset;
            modeTutorial.fontSize = 42;
            modeTutorial.alignment = TextAlignmentOptions.Center;
            modeTutorial.raycastTarget = false;
            var rect = modeTutorial.rectTransform;
            rect.anchorMin = new Vector2(0.12f, 0.3f);
            rect.anchorMax = new Vector2(0.88f, 0.72f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var titleObject = new GameObject("Mode title", typeof(TextMeshProUGUI));
            titleObject.transform.SetParent(newModePanel.transform, false);
            modeTutorialTitle = titleObject.GetComponent<TextMeshProUGUI>();
            modeTutorialTitle.font = modeTutorial.font;
            modeTutorialTitle.alignment = TextAlignmentOptions.Center;
            modeTutorialTitle.raycastTarget = false;
            modeTutorialTitle.rectTransform.anchorMin = modeTutorialTitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        }
        string instructions = mode switch
        {
            5 => "2UP TILES\n\nTwo random tiles start in your tray. Match a triple containing either 2UP tile to clear one extra triple. Clear both 2UP tiles before a new pair appears.",
            9 => "LIMITED TRAY\n\nOne tray slot is locked. Match triples before the six remaining slots fill up.",
            10 => "POISON\n\nMatching a POISON triple sends one random ordinary tile to your tray, then shuffles the board.",
            11 => "BOMB\n\nExposed bombs count down on each selection. At zero, each bomb sends one random tile to the tray. Collect a bomb before it expires.",
            12 => "HEAVEN GATES\n\nRemove the tiles covering every heaven gate to win. Remaining tiles clear automatically.",
            13 => "TARGET TILES\n\nMatch 3 eggs to hatch bunnies. Match 3 bunnies in shells, then 3 carrot bunnies. Complete all 3 stages to win.",
            14 => "COIN TILES\n\nMatch every COIN triple to win. Remaining tiles clear automatically.",
            _ => ""
        };
        int separator = instructions.IndexOf("\n\n", System.StringComparison.Ordinal);
        modeTutorialTitle.text = separator < 0 ? instructions : instructions.Substring(0, separator);
        modeTutorial.text = separator < 0 ? "" : instructions.Substring(separator + 2);
        newModePanel.SetActive(true);
        Open();
    }
    const string hard = "hard_";
    int GetHardLevelPercentage()
    {
        if (PlayerPrefs.HasKey(hard + GameConfig.level))
        {
            return PlayerPrefs.GetInt(hard + GameConfig.level);
        }
        else
        {
            int rnd = Random.Range(60, 91);
            PlayerPrefs.SetInt(hard + GameConfig.level, rnd);
            return rnd;
        }
    }
    public void OpenHardPopup()
    {
        HideOptionalPopup(newModePanel);
        foreach (var tut in tutorials)
            tut.SetActive(false);
        HideOptionalPopup(addSlotPopup);
        HideOptionalPopup(superHardPopup);
        ShowOptionalPopup(hardPopup);
        int percentage = GetHardLevelPercentage();
        txHard.text = string.Format(hardFormat, percentage);
        Open();
    }
    public void OpenSuperHardPopup()
    {
        HideOptionalPopup(newModePanel);
        foreach (var tut in tutorials)
            tut.SetActive(false);
        HideOptionalPopup(addSlotPopup);
        HideOptionalPopup(hardPopup);
        ShowOptionalPopup(superHardPopup);
        int percentage = GetHardLevelPercentage();
        txSuperHard.text = string.Format(superHardFormat, percentage);
        Open();
    }
    void HideOptionalPopup(GameObject popup)
    {
        if (popup)
            popup.SetActive(false);
    }
    void ShowOptionalPopup(GameObject popup)
    {
        if (popup)
            popup.SetActive(true);
    }
    public void OpenAddSlotPopup()
    {
    }
    public override void Open()
    {
        foreach (var tutorial in tutorials)
        {
            HideTileIllustrations(tutorial);
            ApplySettingsFrame(tutorial);
        }
        ApplySettingsFrame(hardPopup);
        ApplySettingsFrame(superHardPopup);
        ApplySettingsFrame(newModePanel);
        closePanel = false;
        CancelInvoke(nameof(ClosePanel));
        box.DOKill();
        box.localScale = Vector3.one;
        btPanel.onClick.RemoveAllListeners();
        btPanel.interactable = true;
        Invoke(nameof(ClosePanel), 5);
        base.Open();
    }

    static void HideTileIllustrations(GameObject tutorial)
    {
        if (!tutorial) return;
        // Each mode panel has two direct text children; all other children are tile diagrams.
        foreach (Transform child in tutorial.transform)
            if (!child.TryGetComponent<TextMeshProUGUI>(out _))
                child.gameObject.SetActive(false);
    }

    private void ApplySettingsFrame(GameObject tutorial)
    {
        if (tutorial == null || tutorialFrameSprite == null)
            return;

        var frame = tutorial.GetComponent<Image>();
        frame.sprite = tutorialFrameSprite;
        frame.type = Image.Type.Simple;
        frame.preserveAspect = false;
        frame.color = Color.white;
        var rect = (RectTransform)tutorial.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1000f, 1000f * tutorialFrameSprite.rect.height / tutorialFrameSprite.rect.width);
        var parentRect = (RectTransform)rect.parent;
        float scale = Mathf.Min(1f, parentRect.rect.width * 0.92f / rect.rect.width,
            parentRect.rect.height * 0.8f / rect.rect.height);
        rect.localScale = Vector3.one * scale;

        foreach (var text in tutorial.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.color = Color.white;
            text.outlineColor = new Color32(15, 45, 125, 255);
            text.outlineWidth = 0.18f;
            text.fontMaterial.SetTexture("_OutlineTex", Texture2D.whiteTexture);
            if (text.transform.parent == rect && text.rectTransform.anchorMin.y > 0.8f)
            {
                var titleRect = text.rectTransform;
                titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.84f);
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = Vector2.zero;
                titleRect.sizeDelta = new Vector2(400f, 100f);
                text.enableAutoSizing = true;
                text.fontSizeMin = 28f;
                text.fontSizeMax = 64f;
            }
        }
    }
    bool closePanel;
    void ClosePanel()
    {
        if (closePanel)
            return;
        closePanel = true;
        btPanel.interactable = false;
        box.DOScale(0, speed * 0.75f).OnComplete(() => Close());
    }
    public override void Close()
    {
        CancelInvoke(nameof(ClosePanel));
        box.DOKill();
        base.Close();
        if (GameScript.instance)
            GameScript.instance.ContinueGame();
    }
}
