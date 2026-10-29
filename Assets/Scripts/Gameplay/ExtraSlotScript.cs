using UnityEngine;
using UnityEngine.EventSystems;

public class ExtraSlotScript : MonoBehaviour, IPointerClickHandler
{
    public GameObject extraOn;
    public Animator animCon;
    public void SetIsExtra(bool isExtra, bool anim = true)
    {
        if (extraOn)
            extraOn.SetActive(false);
        if (anim && animCon)
        {
            animCon.SetTrigger("extra");
        }
        else if (animCon)
            animCon.gameObject.SetActive(false);
    }
    private void MouseUpAsButton()
    {
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        MouseUpAsButton();
    }
}
