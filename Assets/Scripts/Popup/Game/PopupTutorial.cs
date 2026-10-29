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
        HideOptionalPopup(addSlotPopup);
        HideOptionalPopup(hardPopup);
        HideOptionalPopup(superHardPopup);
        if (tutIndex < 0 || tutIndex >= tutorials.Length)
            Close();
        tutorials[tutIndex].SetActive(true);
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
