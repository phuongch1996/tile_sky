using UnityEngine;
using UnityEngine.UI;

public class PopupChallengeInfo : Popup
{
    public Button btConfirm;
    public RectTransform rectTfm;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btConfirm.onClick.AddListener(() => Close());
        });
    }
    public override void Open()
    {
        btConfirm.onClick.RemoveAllListeners();
        rectTfm.anchoredPosition = Vector2.zero;
        base.Open();
    }
}
