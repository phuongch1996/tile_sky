using UnityEngine;
using UnityEngine.UI;

// Popups have separate Safe Area parents from the Home feature buttons.
// Match screen coordinates and visible artwork rather than local offsets.
public class HomeAlignedBackButton : MonoBehaviour
{
    const float SettingsVisibleWidth = 181f / 256f;
    const float BackVisibleWidth = 1036f / 1254f;
    RectTransform settings;
    RectTransform back;

    public static void Configure(Button button, Button settingsButton)
    {
        if (!button || !settingsButton) return;
        var sprite = Resources.Load<Sprite>("HeavenlyBattlePass/Back");
        if (!sprite) return;
        var alignment = button.GetComponent<HomeAlignedBackButton>();
        if (!alignment) alignment = button.gameObject.AddComponent<HomeAlignedBackButton>();
        alignment.settings = (RectTransform)settingsButton.transform;
        alignment.back = (RectTransform)button.transform;
        foreach (Transform child in button.transform) child.gameObject.SetActive(false);
        button.image.sprite = sprite;
        button.image.overrideSprite = null;
        button.image.type = Image.Type.Simple;
        button.image.preserveAspect = true;
        button.image.color = Color.white;
        button.image.enabled = true;
        button.transition = Selectable.Transition.None;
        alignment.back.localScale = Vector3.one;
        alignment.back.localRotation = Quaternion.identity;
        alignment.back.SetAsLastSibling();
        alignment.Align();
    }

    void LateUpdate() => Align();

    void Align()
    {
        if (!settings || !back || !(back.parent is RectTransform parent)) return;
        var sourceCanvas = settings.GetComponentInParent<Canvas>().rootCanvas;
        var targetCanvas = back.GetComponentInParent<Canvas>().rootCanvas;
        var sourceCamera = sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : sourceCanvas.worldCamera;
        var targetCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera;
        var center = RectTransformUtility.WorldToScreenPoint(sourceCamera, settings.TransformPoint(settings.rect.center));
        var edge = RectTransformUtility.WorldToScreenPoint(sourceCamera,
            settings.TransformPoint(settings.rect.center + Vector2.right * settings.rect.width * 0.5f));
        float visibleHalfWidth = Vector2.Distance(center, edge) * SettingsVisibleWidth;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, center, targetCamera, out var localCenter)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,
                center + Vector2.right * visibleHalfWidth, targetCamera, out var localEdge)) return;
        back.anchorMin = back.anchorMax = Vector2.one * 0.5f;
        back.pivot = Vector2.one * 0.5f;
        back.localPosition = new Vector3(localCenter.x, localCenter.y, 0);
        back.sizeDelta = Vector2.one * (Vector2.Distance(localCenter, localEdge) * 2f / BackVisibleWidth);
    }
}
