#pragma warning disable 0649
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaView : MonoBehaviour
{
    private void OnValidate()
    {
        gameObject.name = "SafeAreaView";
    }
    private void OnRectTransformDimensionsChange()
    {
        var rect = GetComponent<RectTransform>();
        var safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
    }
}
