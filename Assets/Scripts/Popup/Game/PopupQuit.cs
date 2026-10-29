using UnityEngine;
using UnityEngine.UI;

public class PopupQuit : Popup
{
    public Button btQuit;
    public Button btContinue;
    public GameObject textDesc;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btQuit.onClick.AddListener(() => Quit());
            btContinue.onClick.AddListener(() => Close());
        });
    }
    public override void Open()
    {
        btQuit.onClick.RemoveAllListeners();
        btContinue.onClick.RemoveAllListeners();
        textDesc.SetActive(!GameConfig.isChallengeLevel);
        base.Open();
    }
    public override void Close()
    {
        base.Close();
        if (GameScript.instance)
            GameScript.instance.ContinueGame();
    }
    void Quit()
    {
        GameScript.instance.Quit();
    }
}
