using DG.Tweening;
using UnityEngine;

public class GoldTileVariant : ItemTile
{
    public static int goldCount = 0;
    public SpriteRenderer gold;
    static readonly Vector3 GoldLocalPosition = new(0f, 0.05f, 0.06f);

    public override void Active(float? timeActive = null)
    {
        base.Active(timeActive);
        ResetGoldVisual();
    }

    public override void Inactive()
    {
        ResetGoldVisual();
        base.Inactive();
    }

    public override void DefaultValue()
    {
        base.DefaultValue();
        ResetGoldVisual();
    }

    public override void InitItemTile(int floor, int sortingIndex, Vector2 tilePos)
    {
        ResetGoldVisual();
        base.InitItemTile(floor, sortingIndex, tilePos);
        ResetGoldVisual();
    }

    public override void SetType(int type)
    {
        if (type == -1)
        {
            tileType = -1;
            SetGold(true);
        }
        else
        {
            base.SetType(type);
            SetGold(false);
        }
    }
    public void SetGold(bool isGold)
    {
        ResetGoldVisual();
        gold.gameObject.SetActive(isGold);
        item.gameObject.SetActive(!isGold);
    }

    public override void SetState()
    {
        base.SetState();
        ResetGoldVisual();
    }

    public override void MouseDown()
    {
        ResetGoldVisual();
        base.MouseDown();
    }

    public override void MouseUp()
    {
        base.MouseUp();
        ResetGoldVisual();
    }

    public override void MouseUpAsButton()
    {
        ResetGoldVisual();
        base.MouseUpAsButton();
    }

    public override void MoveTileToSlot(bool delay = false)
    {
        ResetGoldSlotVisual();
        base.MoveTileToSlot(false);
        ResetGoldSlotVisual();
    }

    public override void PlayTileAppearAnimation(float delay)
    {
        if (state == ITEMTILE_STATE.SLOTTED || state == ITEMTILE_STATE.DONE)
            return;

        PlayStraightGoldAppear(delay);
    }
    public override void SetSortingLayer(string layer)
    {
        base.SetSortingLayer(layer);
        gold.sortingLayerName = layer;
    }
    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        gold.sortingOrder = order;
    }

    public void ResetGoldVisual()
    {
        if (gold == null)
            return;

        gold.transform.DOKill(false);
        gold.transform.localRotation = Quaternion.identity;
        gold.transform.localScale = Vector3.one;
        gold.transform.localPosition = GoldLocalPosition;
    }

    void ResetGoldSlotVisual()
    {
        ResetGoldVisual();
        if (sprites == null)
            return;

        sprites.transform.DOKill(false);
        sprites.transform.localPosition = Vector3.zero;
        sprites.transform.localRotation = Quaternion.identity;
        sprites.transform.localScale = Vector3.one;
    }

    void StraightenGoldTile()
    {
        if (state == ITEMTILE_STATE.HOVERING || state == ITEMTILE_STATE.DONE)
            return;

        transform.localRotation = Quaternion.identity;

        if (sprites != null)
        {
            sprites.localRotation = Quaternion.identity;
        }
    }

    void PlayStraightGoldAppear(float delay)
    {
        StopTileAppearAnimation();
        transform.DOKill(false);

        Vector3 finalPosition = transform.localPosition;
        transform.localPosition = finalPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;
        glow.gameObject.SetActive(true);

        float duration = GameController.instance != null ? 0.5f * GameController.instance.TileAppearDurationMultiplier : 0.5f;
        transform.DOScale(Vector3.one, duration)
            .SetDelay(delay)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (state == ITEMTILE_STATE.SLOTTED || state == ITEMTILE_STATE.DONE)
                    return;

                transform.localPosition = finalPosition;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                glow.gameObject.SetActive(false);
                ResetGoldVisual();
            });
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (gold == null || !gold.gameObject.activeInHierarchy)
            return;

        if (state == ITEMTILE_STATE.DONE)
            return;

        StraightenGoldTile();

        if (gold.transform.localRotation != Quaternion.identity ||
            gold.transform.localScale != Vector3.one ||
            gold.transform.localPosition != GoldLocalPosition)
        {
            ResetGoldVisual();
        }
    }
}
