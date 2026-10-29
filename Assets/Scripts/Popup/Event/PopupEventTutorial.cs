using UnityEngine.UI;

public class PopupEventTutorial : Popup
{
    public Button btPanel;
    public override void SetupTweenCallback()
    {
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
        btPanel.onClick.RemoveAllListeners();
        base.Open();
    }
    public override void Close()
    {
        base.Close();
        if (GameScript.instance && continueGame)
            GameScript.instance.ContinueGame();
    }
}
