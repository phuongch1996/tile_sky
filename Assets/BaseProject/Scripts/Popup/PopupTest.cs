using UnityEngine.UI;


public class PopupTest : Popup
{
    public Button btClose;
    public Button btPanel;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            GameConfig.ShowMrec();
            btClose.onClick.AddListener(() => Close());
            btPanel.onClick.AddListener(() => Close());
        });
    }
    public override void Open()
    {
        //GameConfig.ShowMrec();
        btClose.onClick.RemoveAllListeners();
        btPanel.onClick.RemoveAllListeners();
        base.Open();
    }
    public override void Close()
    {
        GameConfig.HideMrec();
        base.Close();
    }
}
