using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonSFX : MonoBehaviour, IPointerClickHandler
{
    public Selectable bt;
    private void OnValidate()
    {
        bt = GetComponent<Selectable>();
#if UNITY_EDITOR
        if (bt == null || GetComponent<ButtonSFX>() != this)
        {
            ButtonSFX component = this;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (component == null)
                    return;

                RemoveFromEditorSelection(component);
                DestroyImmediate(component, true);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            };
        }
#endif
    }

#if UNITY_EDITOR
    private static void RemoveFromEditorSelection(Component component)
    {
        Object[] selectedObjects = UnityEditor.Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
            return;

        bool changed = false;
        System.Collections.Generic.List<Object> aliveObjects = new System.Collections.Generic.List<Object>(selectedObjects.Length);
        foreach (Object selectedObject in selectedObjects)
        {
            Component selectedComponent = selectedObject as Component;
            if (selectedObject == null || selectedObject == component || selectedObject == component.gameObject ||
                (selectedComponent != null && selectedComponent.gameObject == component.gameObject))
            {
                changed = true;
                continue;
            }

            aliveObjects.Add(selectedObject);
        }

        if (changed)
            UnityEditor.Selection.objects = aliveObjects.ToArray();
    }
#endif

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (bt != null && bt.interactable)
            AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.BUTTON);
    }
}
