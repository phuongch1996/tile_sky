using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUnlock : Popup
{
    public Button btGoToFeature;
    public Button btNextLevel;
    Sprite orangeButtonFace;
    Popup openedFeature;
    public GameObject[] features;
    readonly string[] featureDesc = new string[]
    {
        "Log in daily to earn special rewards.",//dailyreward
        //"Log in daily to receive free prizes.",
        "Watch ads to get free coins.",//freegold
        "Collect coins in the piggy bank and get a big reward when it's full.",//piggybank
        //"Log in for 7 days to earn special rewards.",
        //"Join events to earn exciting rewards.",
        //"A new challenge every day with rewards."
        "Win the match, score points, climb to the top.",//rank
        "Unlock rewards upon clearing the level.",//battlepass
    };
    public TextMeshProUGUI txDesc;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btGoToFeature.onClick.AddListener(() => GoToFeature());
        });
    }
    int feature;
    public void OpenUnlock(int feature)
    {
        this.feature = feature;
        foreach (var go in features)
            go.SetActive(false);
        features[feature].SetActive(true);
        txDesc.text = featureDesc[feature];
        Open();
    }
    public override void Open()
    {
        DetachFeatureClose();
        ApplyWinButtons();
        btGoToFeature.onClick.RemoveAllListeners();
        btNextLevel.onClick.RemoveAllListeners();
        btNextLevel.gameObject.SetActive(false);
        base.Open();
    }

    void ApplyWinButtons()
    {
        if (!orangeButtonFace)
        {
            var atlas = Resources.Load<Texture2D>("HeavenlyWin/Buttons");
            if (!atlas) return;
            orangeButtonFace = CreateButtonFace(atlas);
        }
        StyleUnlockButton(btGoToFeature, orangeButtonFace);
    }

    static Sprite CreateButtonFace(Texture2D atlas)
    {
        var rect = new Rect(atlas.width * 0.11f, atlas.height * 0.01f,
            atlas.width * 0.78f, atlas.height * 0.48f);
        return Sprite.Create(atlas, rect, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
    }

    static void StyleUnlockButton(Button button, Sprite face)
    {
        ((RectTransform)button.transform).sizeDelta = new Vector2(580, 205);
        button.image.sprite = face;
        button.image.type = Image.Type.Simple;
        button.image.preserveAspect = false;
        button.image.color = Color.white;
        button.targetGraphic = button.image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = ColorBlock.defaultColorBlock;
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1);
        button.colors = colors;
        foreach (var label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 7);
            rect.sizeDelta = new Vector2(520, 150);
            label.enableAutoSizing = true;
            label.fontSizeMin = 40;
            label.fontSizeMax = 80;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }
    }

    void OnDestroy()
    {
        DetachFeatureClose();
        if (orangeButtonFace) Destroy(orangeButtonFace);
    }
    void GoToFeature()
    {
        if (openedFeature)
            return;

        var game = GameScript.instance;
        Popup target = feature switch
        {
            0 => game.dailyLoginPopup,
            1 => game.freeGoldPopup,
            2 => game.piggyBankPopup,
            3 => game.rankPopup,
            4 => game.battlePassPopup,
            _ => null
        };
        if (!target)
            return;

        Close();
        openedFeature = target;
        openedFeature.Closed += OnFeatureClosed;
        openedFeature.Open();
    }

    void OnFeatureClosed()
    {
        DetachFeatureClose();
        GameScript.instance.Next();
    }

    void DetachFeatureClose()
    {
        if (openedFeature)
            openedFeature.Closed -= OnFeatureClosed;
        openedFeature = null;
    }
}
