using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PopupSettings : Popup
{
    public Button btClose;
    public Toggle toggleAudio;
    public Toggle toggleMusic;
    public Toggle toggleVibrate;
    public RectTransform togglesRectTfm;
    public Button btRateUs;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            btRateUs.onClick.AddListener(() => OpenRateUs());
        });
    }
    public override void Open()
    {
        btClose.onClick.RemoveAllListeners();
        btRateUs.onClick.RemoveAllListeners();
        //btRateUs.gameObject.SetActive(PlayerPrefs.GetInt(PopupRateUs.ratingKey, 0) == 0);
        //togglesRectTfm.anchoredPosition = new Vector2(0, btRateUs.gameObject.activeSelf ? 65 : 0);
        ToggleRate();
        toggleAudio.isOn = GameConfig.sfxToggle;
        toggleMusic.isOn = GameConfig.bgmToggle;
        toggleVibrate.isOn = GameConfig.vib;
        if (popupType == POPUP_TYPE.box && box is RectTransform boxRect)
        {
            // Opening must not reuse a scale left behind by an interrupted tween.
            panel.transform.localScale = Vector3.one;
            box.DOKill();
            boxRect.anchorMin = boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            panel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            var parentRect = (RectTransform)boxRect.parent;
            float scale = Mathf.Min(1f,
                parentRect.rect.width * 0.92f / boxRect.rect.width,
                parentRect.rect.height * 0.8f / boxRect.rect.height);
            box.localScale = Vector3.zero;
            box.DOScale(Vector3.one * scale, speed).SetEase(Ease.OutBack)
                .OnComplete(() => tweenCallback?.Invoke());
        }
        else
        {
            base.Open();
        }
    }
    public void ToggleRate()
    {
        //btRateUs.gameObject.SetActive(PlayerPrefs.GetInt(PopupRateUs.ratingKey, 0) == 0);
        btRateUs.gameObject.SetActive(!PopupRateUs.HasRating());
        togglesRectTfm.anchoredPosition = new Vector2(0, btRateUs.gameObject.activeSelf ? 0 : 0);
    }
    public override void Close()
    {
        if (box != null)
            box.DOKill();
        base.Close();
        if (GameScript.instance)
            GameScript.instance.ContinueGame();
    }
    public void ToggleAudio(bool isOn)
    {
        GameConfig.SetAudioToggle(isOn);
    }
    public void ToggleMusic(bool isOn)
    {
        GameConfig.SetMusicToggle(isOn);
    }
    public void ToggleVibrate(bool isOn)
    {
        GameConfig.SetVibrate(isOn);
    }
    void OpenRateUs()
    {
        if (HomeScript.instance)
            HomeScript.instance.OpenRateUs();
        if (GameScript.instance)
            GameScript.instance.OpenRateUs();
    }
}
