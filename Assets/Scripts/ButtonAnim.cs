using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler//, IPointerClickHandler
{
    public Transform tfm;
    public Selectable bt;
    public float scale;
    //bool sfx = true;
    private void OnValidate()
    {
        tfm = GetComponent<Transform>();
        bt = GetComponent<Selectable>();
        scale = tfm.localScale.x;
#if UNITY_EDITOR
        if (bt == null || GetComponent<ButtonAnim>() != this)
        {
            ButtonAnim component = this;
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

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if (!bt.interactable)
            return;
        tfm.DOKill();
        tfm.DOScale(0.95f * scale, 0.05f);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (!bt.interactable)
            return;
        tfm.DOKill();
        tfm.DOScale(1 * scale, 0.05f);
    }

    //void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    //{
    //    if (sfx)
    //        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.BUTTON);
    //    //throw new System.NotImplementedException();
    //}
}
