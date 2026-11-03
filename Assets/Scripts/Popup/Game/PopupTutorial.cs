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
        if (modeTutorial != null)
            modeTutorial.gameObject.SetActive(false);
        HideOptionalPopup(addSlotPopup);
        HideOptionalPopup(hardPopup);
        HideOptionalPopup(superHardPopup);
        if (NewModeRules.IsExtendedMode(tutIndex))
        {
            OpenNewModeTutorial(tutIndex);
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

    void OpenNewModeTutorial(int mode)
    {
        if (modeTutorial == null)
        {
            var go = new GameObject("New mode instructions", typeof(TextMeshProUGUI));
            go.transform.SetParent(tutorials[0].transform.parent, false);
            modeTutorial = go.GetComponent<TextMeshProUGUI>();
            modeTutorial.font = txHard != null ? txHard.font : TMP_Settings.defaultFontAsset;
            modeTutorial.fontSize = 42;
            modeTutorial.alignment = TextAlignmentOptions.Center;
            modeTutorial.raycastTarget = false;
            var rect = modeTutorial.rectTransform;
            rect.anchorMin = new Vector2(0.08f, 0.15f);
            rect.anchorMax = new Vector2(0.92f, 0.85f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        modeTutorial.text = mode switch
        {
            9 => "LIMITED TRAY\n\nSome tray slots are locked. Match triples before the remaining slots fill up.",
            10 => "POISON\n\nMatching a POISON triple sends one random ordinary tile to your tray, then shuffles the board.",
            11 => "BOMB\n\nExposed bombs count down on each selection. At zero, each bomb sends one random tile to the tray. Collect a bomb before it expires.",
            12 => "UNCOVER TRAYS\n\nRemove the tiles covering every orange tray to win. Remaining tiles clear automatically.",
            13 => "TARGET TILES\n\nMatch the TARGET triple to reveal the next target. Complete all rounds to win.",
            14 => "COIN TILES\n\nMatch every COIN triple to win. Remaining tiles clear automatically.",
            _ => ""
        };
        modeTutorial.gameObject.SetActive(true);
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
        if (modeTutorial != null) modeTutorial.gameObject.SetActive(false);
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
        if (modeTutorial != null) modeTutorial.gameObject.SetActive(false);
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
        btPanel.onClick.RemoveAllListeners();
        btPanel.interactable = true;
        Invoke(nameof(ClosePanel), 5);
        base.Open();
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
        base.Close();
        if (GameScript.instance)
            GameScript.instance.ContinueGame();
    }
}
