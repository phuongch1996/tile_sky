using System.Collections.Generic;
using UnityEngine;

public class RandomTileVariant : ItemTile
{
    public static List<RandomTileVariant> randomTilesList = new();
    public SpriteRenderer rndOb;
    public bool isRevealed;
    public override void DefaultValue()
    {
        isRevealed = false;
        rndOb.gameObject.SetActive(true);
        base.DefaultValue();
    }
    public override void MouseUpAsButton()
    {
        if (!GameController.instance.UIClickable || !GameController.instance.IsSlotAvailable)
            return;
        if (state == ITEMTILE_STATE.TOUCHABLE || state == ITEMTILE_STATE.HOVERING)
        {
            rndOb.gameObject.SetActive(false);
            isRevealed = true;
        }
        base.MouseUpAsButton();
    }
    public void RestoreRevealed(bool revealed)
    {
        isRevealed = revealed;
        rndOb.gameObject.SetActive(!revealed);
    }
    public override void SetSortingLayer(string layer)
    {
        base.SetSortingLayer(layer);
        rndOb.sortingLayerName = layer;
    }
    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        rndOb.sortingOrder = order;
    }
}
