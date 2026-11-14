using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupEventTutorial : Popup
{
    public Button btPanel;
    [SerializeField] bool starCollectionTutorial;
    [SerializeField] TileSpritesSO collectionTiles;
    RectTransform collectionTutorialRoot;
    TextMeshProUGUI collectionFont;
    readonly List<Sprite> collectionSlices = new();
    public override void SetupTweenCallback()
    {
        EnsureStarCollectionTutorial();
        SetupTweenCallback(() =>
        {
            btPanel.onClick.AddListener(() => Close());
        });
    }
    public bool continueGame;
    public void OpenTutorial(bool continueGame = false)
    {
        this.continueGame = continueGame;
        Open();
    }
    public override void Open()
    {
        EnsureStarCollectionTutorial();
        FitStarCollectionTutorial();
        btPanel.onClick.RemoveAllListeners();
        base.Open();
    }

    public void EnsureStarCollectionTutorial()
    {
        if (!starCollectionTutorial || collectionTutorialRoot) return;
        var frame = Resources.Load<Sprite>("HeavenlyEvents/PanelWithoutTimer");
        var reward = Resources.Load<Sprite>("HeavenlyEvents/RewardCard");
        var symbols = Resources.Load<Texture2D>("HeavenlyEvents/Symbols");
        collectionFont = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!frame || !reward || !symbols || !collectionFont || !collectionTiles) return;

        var parent = btPanel.transform;
        var animation = CollectionRect("Star collection animation", parent, Vector2.zero, Vector2.zero);
        animation.anchorMin = Vector2.zero;
        animation.anchorMax = Vector2.one;
        animation.offsetMin = animation.offsetMax = Vector2.zero;
        collectionTutorialRoot = CollectionRect("Star collection tutorial", animation,
            new Vector2(1024, 1820), Vector2.zero);
        foreach (Transform child in parent)
            if (child != animation) child.gameObject.SetActive(false);
        if (bg) bg.gameObject.SetActive(false);
        isBGChange = false;
        btPanel.image.color = Color.clear;
        btPanel.image.raycastTarget = true;
        CollectionImage("Event frame", collectionTutorialRoot, frame, new Vector2(1024, 1820), Vector2.zero);
        CollectionLabel("Star collection", collectionTutorialRoot, new Vector2(760, 115), new Vector2(0, 540), 77);

        var star = Sprite.Create(symbols, new Rect(symbols.width * 0.5f, 0, symbols.width * 0.5f, symbols.height),
            Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        collectionSlices.Add(star);
        CollectionImage("Collectible star", collectionTutorialRoot, star, new Vector2(185, 195), new Vector2(-240, 230));
        CollectionLabel("Collect stars before\nthey disappear!", collectionTutorialRoot,
            new Vector2(340, 125), new Vector2(-240, 65), 39);
        CollectionLabel("\u2192", collectionTutorialRoot, new Vector2(120, 110), new Vector2(-20, 230), 95);

        var pile = CollectionRect("Pile of game items", collectionTutorialRoot, new Vector2(360, 300), new Vector2(220, 225));
        Vector2[] positions = {
            new(-80, 85), new(15, 100), new(95, 75), new(-125, 10), new(-35, 20), new(60, 20),
            new(135, -5), new(-105, -70), new(-10, -65), new(80, -80), new(-45, -125), new(40, -125)
        };
        var items = collectionTiles.GetAllSprites();
        for (int i = 0; i < positions.Length && items.Count > 0; i++)
        {
            var item = CollectionImage("Game item " + i, pile, items[(i * 5) % items.Count],
                Vector2.one * (i < 7 ? 120 : 125), positions[i]);
            item.rectTransform.localRotation = Quaternion.Euler(0, 0, (i % 5 - 2) * 8f);
        }
        CollectionLabel("Win levels!", collectionTutorialRoot, new Vector2(350, 75), new Vector2(220, 0), 46);
        CollectionLabel("\u2193", collectionTutorialRoot, new Vector2(120, 105), new Vector2(220, -95), 90);

        // Use the same cloud reward cards, gold icon and milestone diamonds as the event UI.
        // The tutorial's two examples intentionally have no connecting progress track.
        BuildCollectionReward(2, 100, -280, reward);
        BuildCollectionReward(1, 50, -520, reward);
        CollectionLabel("Claim prizes!", collectionTutorialRoot, new Vector2(650, 85), new Vector2(0, -710), 49);
        CollectionLabel("Tap anywhere to close", collectionTutorialRoot,
            new Vector2(650, 55), new Vector2(0, -790), 28);

        popupType = POPUP_TYPE.box;
        box = animation;
        FitStarCollectionTutorial();
    }

    void BuildCollectionReward(int milestone, int amount, float y, Sprite reward)
    {
        CollectionImage("Cloud reward " + milestone, collectionTutorialRoot, reward,
            new Vector2(390, 250), new Vector2(65, y));
        CollectionImage("Reward gold " + milestone, collectionTutorialRoot, collectionTiles.goldSprite,
            new Vector2(105, 100), new Vector2(65, y + 5));
        var quantity = CollectionLabel(amount.ToString(), collectionTutorialRoot,
            new Vector2(235, 65), new Vector2(65, y - 76), 53);
        quantity.outlineColor = new Color32(126, 20, 4, 255);
        var diamond = CollectionImage("Gold milestone " + milestone, collectionTutorialRoot, null,
            new Vector2(85, 85), new Vector2(-245, y));
        diamond.color = new Color32(255, 211, 29, 255);
        diamond.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
        var inset = CollectionImage("Milestone center", diamond.transform, null, new Vector2(66, 66), Vector2.zero);
        inset.color = milestone == 1 ? new Color32(0, 161, 255, 255) : new Color32(66, 205, 25, 255);
        CollectionLabel(milestone.ToString(), collectionTutorialRoot,
            new Vector2(100, 100), new Vector2(-245, y), 60);
    }

    static RectTransform CollectionRect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * 0.5f;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    static Image CollectionImage(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 position)
    {
        var image = CollectionRect(name, parent, size, position).gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = sprite != null;
        image.raycastTarget = false;
        return image;
    }

    TextMeshProUGUI CollectionLabel(string text, Transform parent, Vector2 size, Vector2 position, float fontSize)
    {
        var label = CollectionRect(text, parent, size, position).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = collectionFont.font;
        label.text = text;
        label.color = Color.white;
        label.fontSize = label.fontSizeMax = fontSize;
        label.fontSizeMin = fontSize * 0.7f;
        label.enableAutoSizing = true;
        label.alignment = TextAlignmentOptions.Center;
        label.outlineColor = new Color32(0, 30, 153, 255);
        label.outlineWidth = 0.18f;
        label.fontMaterial.SetTexture("_OutlineTex", Texture2D.whiteTexture);
        label.raycastTarget = false;
        return label;
    }

    void LateUpdate() => FitStarCollectionTutorial();

    void FitStarCollectionTutorial()
    {
        if (!collectionTutorialRoot) return;
        var parent = (RectTransform)collectionTutorialRoot.parent;
        float scale = Mathf.Min(parent.rect.width / 1024f, parent.rect.height / 1820f) * 0.94f;
        collectionTutorialRoot.localScale = Vector3.one * scale;
    }

    void OnDestroy()
    {
        foreach (var sprite in collectionSlices)
        {
            if (Application.isPlaying) Destroy(sprite);
            else DestroyImmediate(sprite);
        }
    }
    public override void Close()
    {
        base.Close();
        if (GameScript.instance && continueGame)
            GameScript.instance.ContinueGame();
    }
}
