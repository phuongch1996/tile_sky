/// <summary>Compatibility component for old scenes; Quest can no longer open.</summary>
public class PopupQuest : Popup
{
    private void Awake()
    {
        HideQuest();
    }

    public override void SetupTweenCallback()
    {
        HideQuest();
    }

    public override void Open()
    {
        HideQuest();
    }

    private void HideQuest()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}