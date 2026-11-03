using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Transform tfm;
    public Selectable bt;
    public float scale;
    Tween pressTween;
    Vector3 restingScale;
    bool ownsScale;
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
        if (!bt) bt = GetComponent<Selectable>();
        if (!tfm) tfm = transform;
        if (!bt || !bt.IsInteractable() || !bt.IsActive())
            return;
        // Layouts can change the scale after prefab initialization.
        if (!ownsScale) restingScale = tfm.localScale;
        ownsScale = true;
        pressTween?.Kill();
        pressTween = tfm.DOScale(restingScale * 0.95f, 0.05f).SetUpdate(true);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        RestoreScale();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => RestoreScale();

    void RestoreScale()
    {
        if (!ownsScale || !tfm) return;
        pressTween?.Kill();
        pressTween = tfm.DOScale(restingScale, 0.05f).SetUpdate(true)
            .OnComplete(() => { ownsScale = false; pressTween = null; });
    }

    void OnDisable()
    {
        pressTween?.Kill();
        pressTween = null;
        if (ownsScale && tfm) tfm.localScale = restingScale;
        ownsScale = false;
    }

    //void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    //{
    //    if (sfx)
    //        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.BUTTON);
    //    //throw new System.NotImplementedException();
    //}
}
