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
    public override void SetupTweenCallback()
    {
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
        if (value)
        {
            avatarTab.SetActive(avatarToggle.isOn);
            frameTab.SetActive(frameToggle.isOn);
            flagTab.SetActive(flagToggle.isOn);
        }
    }
    public override void Open()
    {
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
        selectAvatar = avatar % UnityAuthentication.instance.avatarSprites.Length;
        for (int i = 0; i < btAvatars.Length; i++)
        {
            btAvatars[i].transform.GetChild(0).gameObject.SetActive(i == avatar);
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
