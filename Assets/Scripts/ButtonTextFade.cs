using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ButtonTextFade : Button
{
    public List<MaskableGraphic> maskables;
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        maskables = GetComponentsInChildren<MaskableGraphic>().ToList();
        maskables.Remove(GetComponent<MaskableGraphic>());
    }
#endif
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        foreach (var mask in maskables)
            mask.color = state == SelectionState.Disabled ? colors.disabledColor : colors.normalColor;
    }
}
