using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupEditPlayer : Popup
{
    public TMP_InputField inputFieldName;
    public GameObject editName;
    public Button btConfirm;
    public Button[] btAvatars;
    public Button[] btFrames;
    public Button[] btFlags;
    public Toggle avatarToggle;
    public GameObject avatarTab;
    public Toggle frameToggle;
    public GameObject frameTab;
    public Toggle flagToggle;
    public GameObject flagTab;
    Button btEditName;
    RectTransform heavenlyRoot;
    bool heavenlyLayout;

    void EnsureHeavenlyLayout()
    {
        if (heavenlyLayout || !HeavenlyInfoArt.Panel || HeavenlyInfoArt.Avatars.Length != 9)
            return;

        // Retain the input field, font and save button's existing behaviour.
        heavenlyRoot = NewRect("Heavenly Info", panel.transform, new Vector2(1024, 1536), Vector2.zero);
        var art = heavenlyRoot.gameObject.AddComponent<Image>();
        art.sprite = HeavenlyInfoArt.Panel;
        art.raycastTarget = false;
        inputFieldName.transform.SetParent(heavenlyRoot, false);
        btConfirm.transform.SetParent(heavenlyRoot, false);
        foreach (Transform child in panel.transform)
            if (child != heavenlyRoot)
                child.gameObject.SetActive(false);
        box = heavenlyRoot;
        // Fit uses the root's scale, so animate opacity instead of overwriting that scale.
        popupType = POPUP_TYPE.fade;

        var nameRect = (RectTransform)inputFieldName.transform;
        PlaceRect(nameRect, new Vector2(580, 90), new Vector2(-37, 252));
        nameRect.gameObject.SetActive(true);
        var nameImage = inputFieldName.GetComponent<Image>();
        if (nameImage) nameImage.color = Color.clear;
        inputFieldName.characterLimit = 24;
        inputFieldName.lineType = TMP_InputField.LineType.SingleLine;
        inputFieldName.textComponent.color = Color.white;
        inputFieldName.textComponent.fontSize = 46;
        inputFieldName.textComponent.enableAutoSizing = true;
        inputFieldName.textComponent.fontSizeMin = 26;
        inputFieldName.textComponent.fontSizeMax = 46;
        inputFieldName.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
        inputFieldName.textComponent.outlineColor = new Color32(112, 29, 43, 255);
        inputFieldName.textComponent.outlineWidth = 0.18f;
        inputFieldName.caretColor = Color.white;
        // Remove the old field's art and use the entire new name slot for text.
        foreach (var graphic in inputFieldName.GetComponentsInChildren<Graphic>(true))
            if (graphic != nameImage && graphic != inputFieldName.textComponent && graphic != inputFieldName.placeholder)
                graphic.gameObject.SetActive(false);
        if (inputFieldName.textViewport)
            StretchRect(inputFieldName.textViewport);
        StretchRect(inputFieldName.textComponent.rectTransform);
        if (inputFieldName.placeholder)
            inputFieldName.placeholder.gameObject.SetActive(false);

        var pencil = NewRect("editName", heavenlyRoot, new Vector2(110, 110), new Vector2(295, 252));
        pencil.gameObject.AddComponent<Image>().color = Color.clear;
        editName = pencil.gameObject;
        SetupEditNameButton();

        PlaceRect((RectTransform)btConfirm.transform, new Vector2(490, 155), new Vector2(0, -591));
        btConfirm.gameObject.SetActive(true);
        foreach (Transform child in btConfirm.transform)
            child.gameObject.SetActive(false);
        btConfirm.image.sprite = null;
        btConfirm.image.color = Color.clear;
        btConfirm.transition = Selectable.Transition.None;
        // Discard old persistent calls; Save is wired once by SetupTweenCallback.
        btConfirm.onClick = new Button.ButtonClickedEvent();

        btAvatars = new Button[9];
        for (int i = 0; i < btAvatars.Length; i++)
        {
            var rect = NewRect("Avatar " + i, heavenlyRoot, new Vector2(195, 187),
                new Vector2(-229 + (i % 3) * 229, 23 - (i / 3) * 198));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = HeavenlyInfoArt.Avatars[i];
            image.preserveAspect = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            btAvatars[i] = button;

            var selection = NewRect("Selected", rect, new Vector2(205, 197), Vector2.zero);
            // A bright four-sided outline remains visible around the portrait.
            SelectionEdge(selection, new Vector2(205, 4), new Vector2(0, 98));
            SelectionEdge(selection, new Vector2(205, 4), new Vector2(0, -98));
            SelectionEdge(selection, new Vector2(4, 197), new Vector2(-102, 0));
            SelectionEdge(selection, new Vector2(4, 197), new Vector2(102, 0));
            selection.gameObject.SetActive(false);
        }
        heavenlyLayout = true;
        FitHeavenlyLayout();
    }

    static void SelectionEdge(RectTransform parent, Vector2 size, Vector2 position)
    {
        var edge = NewRect("Selection glow", parent, size, position).gameObject.AddComponent<Image>();
        edge.color = new Color32(255, 234, 94, 255);
        edge.raycastTarget = false;
    }

    static RectTransform NewRect(string objectName, Transform parent, Vector2 size, Vector2 position)
    {
        var rect = new GameObject(objectName, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        PlaceRect(rect, size, position);
        return rect;
    }

    static void PlaceRect(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    static void StretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    void LateUpdate()
    {
        if (heavenlyLayout && Showing)
            FitHeavenlyLayout();
    }

    void FitHeavenlyLayout()
    {
        var parent = (RectTransform)heavenlyRoot.parent;
        var safe = Screen.safeArea;
        float width = parent.rect.width * safe.width / Mathf.Max(1, Screen.width);
        float height = parent.rect.height * safe.height / Mathf.Max(1, Screen.height);
        float scale = Mathf.Min(width / 1024f, height / 1536f) * 0.97f;
        heavenlyRoot.localScale = Vector3.one * scale;
        heavenlyRoot.anchoredPosition = new Vector2(
            (safe.center.x / Mathf.Max(1, Screen.width) - 0.5f) * parent.rect.width,
            (safe.center.y / Mathf.Max(1, Screen.height) - 0.5f) * parent.rect.height);
    }

    public override void SetupTweenCallback()
    {
        EnsureHeavenlyLayout();
        SetupTweenCallback(() =>
        {
            SetupEditNameButton();
            if (btEditName)
                btEditName.onClick.AddListener(EditName);
            btConfirm.onClick.AddListener(() => Close());
            for (int i = 0; i < btAvatars.Length; i++)
                AvatarButton(btAvatars[i], i);
            for (int i = 0; i < btFrames.Length; i++)
                FrameButton(btFrames[i], i);
            for (int i = 0; i < btFlags.Length; i++)
                FlagButton(btFlags[i], i);
        });
    }
    System.Action callback;
    public void OpenWithCallback(System.Action callback = null)
    {
        this.callback = callback;
        Open();
    }
    public void OpenTab(bool value)
    {
        if (heavenlyLayout) return;
        if (value)
        {
            avatarTab.SetActive(avatarToggle.isOn);
            frameTab.SetActive(frameToggle.isOn);
            flagTab.SetActive(flagToggle.isOn);
        }
    }
    public override void Open()
    {
        EnsureHeavenlyLayout();
        SetupEditNameButton();
        if (btEditName)
            btEditName.onClick.RemoveAllListeners();
        btConfirm.onClick.RemoveAllListeners();
        inputFieldName.richText = false;
        inputFieldName.SetTextWithoutNotify(GameConfig.GetDisplayName());
        inputFieldName.caretPosition = inputFieldName.text.Length;
        inputFieldName.selectionAnchorPosition = inputFieldName.caretPosition;
        inputFieldName.selectionFocusPosition = inputFieldName.caretPosition;

        foreach (var bt in btAvatars)
            bt.onClick.RemoveAllListeners();
        foreach (var bt in btFrames)
            bt.onClick.RemoveAllListeners();
        foreach (var bt in btFlags)
            bt.onClick.RemoveAllListeners();

        SelectAvatar(GameConfig.GetPlayerAvatar());
        SelectFrame(GameConfig.GetPlayerFrame());
        SelectFlag(GameConfig.GetPlayerFlag());

        avatarToggle.onValueChanged.RemoveAllListeners();
        frameToggle.onValueChanged.RemoveAllListeners();
        flagToggle.onValueChanged.RemoveAllListeners();

        OpenTab(true);

        avatarToggle.onValueChanged.AddListener(OpenTab);
        frameToggle.onValueChanged.AddListener(OpenTab);
        flagToggle.onValueChanged.AddListener(OpenTab);

        base.Open();
    }
    void SetupEditNameButton()
    {
        if (!editName)
            editName = FindChildByName(transform, "editName")?.gameObject;
        if (!editName)
            return;

        Graphic graphic = editName.GetComponent<Graphic>();
        if (graphic)
            graphic.raycastTarget = true;

        btEditName = editName.GetComponent<Button>();
        if (!btEditName)
            btEditName = editName.AddComponent<Button>();
        btEditName.transition = Selectable.Transition.None;
        if (graphic)
            btEditName.targetGraphic = graphic;
    }
    Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindChildByName(child, childName);
            if (result)
                return result;
        }

        return null;
    }
    void EditName()
    {
        inputFieldName.Select();
        inputFieldName.ActivateInputField();
        inputFieldName.caretPosition = inputFieldName.text.Length;
        inputFieldName.selectionAnchorPosition = inputFieldName.caretPosition;
        inputFieldName.selectionFocusPosition = inputFieldName.caretPosition;
    }
    void AvatarButton(Button bt, int avatar)
    {
        bt.onClick.AddListener(() => SelectAvatar(avatar));
    }
    void FrameButton(Button bt, int frame)
    {
        bt.onClick.AddListener(() => SelectFrame(frame));
    }
    void FlagButton(Button bt, int flag)
    {
        bt.onClick.AddListener(() => SelectFlag(flag));
    }
    public int selectAvatar;
    public int selectFrame;
    public int selectFlag;
    public void SelectAvatar(int avatar)
    {
        selectAvatar = heavenlyLayout ? Mathf.Max(0, avatar) : avatar % UnityAuthentication.instance.avatarSprites.Length;
        for (int i = 0; i < btAvatars.Length; i++)
        {
            btAvatars[i].transform.GetChild(0).gameObject.SetActive(i == (heavenlyLayout ? avatar % 9 : avatar));
        }
    }
    public void SelectFrame(int frame)
    {
        selectFrame = frame % UnityAuthentication.instance.frameSprites.Length;
        for (int i = 0; i < btFrames.Length; i++)
        {
            btFrames[i].transform.GetChild(0).gameObject.SetActive(i == frame);
        }
    }
    public void SelectFlag(int flag)
    {
        selectFlag = flag % UnityAuthentication.instance.flagSprites.Length;
        for (int i = 0; i < btFlags.Length; i++)
        {
            btFlags[i].transform.GetChild(0).gameObject.SetActive(i == flag);
        }
    }
    public override void Close()
    {
        var (notDefault, newName) = GameConfig.SetDisplayName(inputFieldName.text);
        //if (notDefault && Config.IsChangeNameBonus())
        //{
        //    Config.ChangeNameBonus();
        //    HomeUI.instance.CoinsStarsUpdateCoins(changeNameBonusOB.transform, 100, 20);
        //}
        bool newAvatar = GameConfig.SetPlayerAvatar(selectAvatar);
        bool newFrame = GameConfig.SetPlayerFrame(selectFrame);
        bool newFlag = GameConfig.SetPlayerFlag(selectFlag);
        if (newName || newAvatar || newFrame || newFlag)
            UnityAuthentication.instance.UpdateMetadata();
        base.Close();
        callback?.Invoke();
    }
}

// Shared by the editor popup, home profile and leaderboard portraits.
public static class HeavenlyInfoArt
{
    static Sprite panelSprite;
    static Sprite[] avatarSprites;
    public static Sprite Panel => panelSprite ? panelSprite : (panelSprite = Resources.Load<Sprite>("HeavenlyInfo/InfoPanel"));
    public static Sprite[] Avatars
    {
        get
        {
            if (avatarSprites != null) return avatarSprites;
            var atlas = Resources.Load<Texture2D>("HeavenlyInfo/AvatarAtlas");
            if (!atlas) return System.Array.Empty<Sprite>();
            avatarSprites = new Sprite[9];
            float cellWidth = atlas.width / 3f;
            float cellHeight = atlas.height / 3f;
            for (int i = 0; i < 9; i++)
            {
                avatarSprites[i] = Sprite.Create(atlas,
                    new Rect(i % 3 * cellWidth, (2 - i / 3) * cellHeight, cellWidth, cellHeight),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                avatarSprites[i].name = "Heavenly Avatar " + i;
            }
            return avatarSprites;
        }
    }
}
