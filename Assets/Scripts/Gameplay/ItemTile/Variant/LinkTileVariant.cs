using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class LinkTileVariant : ItemTile
{
    public static List<LinkTileVariant> linkTilesList = new();
    bool Touchable { get { return base.state == ITEMTILE_STATE.TOUCHABLE && linkedTile.state == ITEMTILE_STATE.TOUCHABLE; } }
    public LinkTileVariant linkedTile;
    public bool unLinked;
    int rldu;
    public SpriteRenderer[] stickySprites;
    public SpriteRenderer stickyFull;
    public override void DefaultValue()
    {
        unLinked = false;
        linkedTile = null;
        stickyFull = null;
        foreach (var renderer in stickySprites)
        {
            renderer.gameObject.SetActive(false);
            renderer.color = Color.white;
        }
        base.DefaultValue();
    }
    public void LinkTileInit(int index, Transform parent, Vector2 tilePos)
    {
        this.tilePos = tilePos;
        linkTilesList[index] = this;
        if (index % 2 == 0)
        {
            //stickyFull = Instantiate(GameController.instance.stickyFull, parent);
            stickyFull = PoolManager.instance.GetObject<PoolObjectSpriteRenderer>(GameController.instance.stickyFull, parent).spriteRenderer;
            stickyFull.transform.localScale = new Vector3(0.75f, 0, 0);
        }
        else
        {
            stickyFull = linkTilesList[linkTilesList.IndexOf(this) - 1].stickyFull;
            linkedTile = linkTilesList[linkTilesList.IndexOf(this) - 1];
            linkTilesList[linkTilesList.IndexOf(this) - 1].linkedTile = this;
            if (tilePos.x == linkedTile.tilePos.x)
            {
                bool check = tilePos.y > linkedTile.tilePos.y;
                stickyFull.transform.localPosition = new Vector3(tilePos.x, tilePos.y + (check ? -0.5f : 0.5f), 0);
                stickyFull.transform.eulerAngles = new Vector3(0, 0, -90);
                SetRLDU(check ? 2 : 3);
                linkedTile.SetRLDU(check ? 3 : 2);
            }
            else if (tilePos.y == linkedTile.tilePos.y)
            {
                bool check = tilePos.x < linkedTile.tilePos.x;
                stickyFull.transform.localPosition = new Vector3(tilePos.x + (check ? 0.5f : -0.5f), tilePos.y, 0);
                stickyFull.transform.eulerAngles = new Vector3(0, 0, 0);
                SetRLDU(check ? 0 : 1);
                linkedTile.SetRLDU(check ? 1 : 0);
            }
        }
    }
    void SetRLDU(int rldu)
    {
        this.rldu = rldu;
        stickyFull.gameObject.SetActive(false);
        stickySprites[rldu].gameObject.SetActive(true);
    }
    public void Init()
    {
        if (stickyFull == null)
            return;

        stickyFull.gameObject.SetActive(false);
        stickySprites[rldu].gameObject.SetActive(true);
    }
    public override void SetState()
    {
        if (unLinked || linkedTile == null || stickyFull == null)
        {
            base.SetState();
            unLinked = true;
            return;
        }

        base.SetState();
        if (state != ITEMTILE_STATE.DONE && linkedTile.state != ITEMTILE_STATE.DONE)
        {
            stickyFull.transform.localScale = new Vector3(0.75f, 1, 1);
            stickyFull.gameObject.SetActive(true);
            stickySprites[rldu].gameObject.SetActive(false);
        }
        else
        {
            stickyFull.gameObject.SetActive(false);
            stickySprites[rldu].gameObject.SetActive(false);
            unLinked = true;
        }
    }
    public override void ToggleTouchable(bool touchable)
    {
        base.ToggleTouchable(touchable);
        if (unLinked || linkedTile == null || stickyFull == null)
            return;

        if (state == ITEMTILE_STATE.TOUCHABLE || linkedTile.state == ITEMTILE_STATE.TOUCHABLE)
            stickyFull.color = Color.white;
        else
            stickyFull.color = Color.gray;
    }
    public override void SetSortingLayer(string layer)
    {
        base.SetSortingLayer(layer);
        if (stickyFull != null)
            stickyFull.sortingLayerName = layer;
        foreach (var stickySprite in stickySprites)
            if (stickySprite != null)
                stickySprite.sortingLayerName = layer;
    }
    public override void SetSortingOrder(int order)
    {
        base.SetSortingOrder(order);
        if (stickyFull != null)
            stickyFull.sortingOrder = order + 10;
        foreach (var stickySprite in stickySprites)
            if (stickySprite != null)
                stickySprite.sortingOrder = order;
    }
    public override void MouseDown()
    {
        if (unLinked)
        {
            base.MouseDown();
            return;
        }
        if (GameController.instance.gameState != GAME_STATE.PLAYING || !GameController.instance.IsSlotLinkAvailable)
            return;
        if (state == ITEMTILE_STATE.TOUCHABLE && linkedTile.state == ITEMTILE_STATE.TOUCHABLE)
        {
            isHolding = true;
            holdAnim.SetActive(true);
            linkedTile.holdAnim.SetActive(true);
            SetSortingLayer("Hovering");
            linkedTile.SetSortingLayer("Hovering");
            state = ITEMTILE_STATE.HOVERING;
            //linkedTile.state = ITEMTILE_STATE.HOVERING;
            sprites.transform.DOKill();
            linkedTile.sprites.transform.DOKill();
            stickyFull.transform.DOKill();
            sprites.transform.DOScale(Vector3.one * 1.25f, 0.25f).SetEase(Ease.OutBack);
            linkedTile.sprites.transform.DOScale(Vector3.one * 1.25f, 0.25f).SetEase(Ease.OutBack);
            stickyFull.transform.DOScale(new Vector3(0.75f * 1.25f, 1.25f, 1.25f), 0.25f).SetEase(Ease.OutBack);
            switch (rldu)
            {
                case 0:
                    sprites.transform.DOLocalMove(new Vector2(-0.125f, 0), 0.25f).SetEase(Ease.OutBack);
                    linkedTile.sprites.transform.DOLocalMove(new Vector2(0.125f, 0), 0.25f).SetEase(Ease.OutBack);
                    break;
                case 1:
                    sprites.transform.DOLocalMove(new Vector2(0.125f, 0), 0.25f).SetEase(Ease.OutBack);
                    linkedTile.sprites.transform.DOLocalMove(new Vector2(-0.125f, 0), 0.25f).SetEase(Ease.OutBack);
                    break;
                case 2:
                    sprites.transform.DOLocalMove(new Vector2(0, 0.125f), 0.25f).SetEase(Ease.OutBack);
                    linkedTile.sprites.transform.DOLocalMove(new Vector2(0, -0.125f), 0.25f).SetEase(Ease.OutBack);
                    break;
                case 3:
                    sprites.transform.DOLocalMove(new Vector2(0, -0.125f), 0.25f).SetEase(Ease.OutBack);
                    linkedTile.sprites.transform.DOLocalMove(new Vector2(0, 0.125f), 0.25f).SetEase(Ease.OutBack);
                    break;
                default:
                    break;
            }
        }
    }
    public override void UndoMove()
    {
        if (state == ITEMTILE_STATE.SLOTTED && linkedTile.state == ITEMTILE_STATE.SLOTTED)
        {
            GameController.instance.gameState = GAME_STATE.UNDO;
            gameObject.transform.SetParent(GameController.instance.floorTransformList[floor - 1]);
            linkedTile.gameObject.transform.SetParent(GameController.instance.floorTransformList[linkedTile.floor - 1]);
            gameObject.transform.DOScale(Vector3.one * 1.25f, 0.35f);
            linkedTile.gameObject.transform.DOScale(Vector3.one * 1.25f, 0.35f);
            //Debug.Log(sortingIndex);
            glow.gameObject.SetActive(true);
            linkedTile.glow.gameObject.SetActive(true);

            gameObject.transform.DOLocalMove(new Vector3(tilePos.x, tilePos.y, sortingIndex / -10f), 0.25f).SetDelay(0.35f)
                .OnStart(() =>
                {
                    GameController.instance.slottedItems.Remove(this);
                    GameController.instance.undoList.Remove(this);
                    GameController.instance.slottedItems.Remove(linkedTile);
                    GameController.instance.undoList.Remove(linkedTile);
                    foreach (ItemTile tile in GameController.instance.slottedItems)
                    {
                        _ = tile.Slotted;
                    }
                    GameController.instance.Danger();
                    gameObject.transform.DOScale(Vector3.one, 0.25f);
                    //GameScript.instance.PowerUpsInteractable();
                })
                .OnComplete(() =>
                {
                    SetSortingLayer("Floor" + floor);
                    SetSortingOrder(sortingIndex);
                    glow.gameObject.SetActive(false);
                    itemCollider.enabled = true;
                    SetState();
                    //if (Config.gameMode == GAME_MODE.tile_snail)
                    //    StartCoroutine(SnailScript.instance.Snail());
                    //else 
                    //if (GameConfig.gameMode == GAME_MODE.tile_freeze)
                    //    StartCoroutine(GameController.instance.CheckFullFreeze());
                    //GameScript.instance.PowerUpsInteractable();
                    //GameController.instance.gameState = GAME_STATE.PLAYING;
                });
            linkedTile.gameObject.transform.DOLocalMove(new Vector3(linkedTile.tilePos.x, linkedTile.tilePos.y, linkedTile.sortingIndex / -10f), 0.25f).SetDelay(0.351f)
                .OnStart(() =>
                {
                    linkedTile.gameObject.transform.DOScale(Vector3.one, 0.25f);
                })
                .OnComplete(() =>
                {
                    linkedTile.SetSortingLayer("Floor" + linkedTile.floor);
                    linkedTile.SetSortingOrder(linkedTile.sortingIndex);
                    linkedTile.glow.gameObject.SetActive(false);
                    linkedTile.itemCollider.enabled = true;
                    linkedTile.SetState();
                    //if (Config.gameMode == GAME_MODE.tile_snail)
                    //    StartCoroutine(SnailScript.instance.Snail());
                    //else 
                    if (GameConfig.gameMode == GAME_MODE.tile_freeze)
                        StartCoroutine(GameController.instance.CheckFullFreeze());
                    GameScript.instance.PowerUpsInteractable();
                    GameController.instance.gameState = GAME_STATE.PLAYING;
                });

        }
        else
        {
            base.UndoMove();
            unLinked = true;
        }
    }
    public override void MouseUp()
    {
        if (unLinked)
        {
            base.MouseUp();
            return;
        }
        if (GameController.instance.gameState != GAME_STATE.PLAYING || !GameController.instance.IsSlotLinkAvailable)
            return;
        if (state == ITEMTILE_STATE.HOVERING)
        {
            isHolding = false;
            holdAnim.SetActive(false);
            linkedTile.holdAnim.SetActive(false);
            SetSortingLayer("Floor" + floor);
            linkedTile.SetSortingLayer("Floor" + floor);
            state = ITEMTILE_STATE.TOUCHABLE;
            sprites.transform.DOKill();
            linkedTile.sprites.transform.DOKill();
            stickyFull.transform.DOKill();
            sprites.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
            linkedTile.sprites.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
            sprites.transform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
            linkedTile.sprites.transform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
            stickyFull.transform.DOScale(new Vector3(0.75f, 1, 1), 0.25f).SetEase(Ease.OutBack);
        }
    }
    public override void MouseUpAsButton()
    {
        if (unLinked)
        {
            base.MouseUpAsButton();
            return;
        }
        if (GameController.instance.gameState != GAME_STATE.PLAYING || !GameController.instance.IsSlotLinkAvailable)
            return;

        if ((state == ITEMTILE_STATE.TOUCHABLE || state == ITEMTILE_STATE.HOVERING) && (linkedTile.state == ITEMTILE_STATE.TOUCHABLE || linkedTile.state == ITEMTILE_STATE.HOVERING))
        {
            GameController.instance.gameState = GAME_STATE.STICKY;
            SetSortingLayer("Hovering");
            linkedTile.SetSortingLayer("Hovering");
            sprites.transform.DOKill();
            linkedTile.sprites.transform.DOKill();
            stickyFull.transform.DOKill();
            Vector3 scale = sprites.transform.localScale;
            switch (rldu)
            {
                case 0:
                    transform.DOBlendableLocalMoveBy(new Vector3(-0.25f * scale.x, 0, 0), 0.5f);
                    linkedTile.transform.DOBlendableLocalMoveBy(new Vector3(0.25f * scale.x, 0, 0), 0.5f);
                    break;
                case 1:
                    transform.DOBlendableLocalMoveBy(new Vector3(0.25f * scale.x, 0, 0), 0.5f);
                    linkedTile.transform.DOBlendableLocalMoveBy(new Vector3(-0.25f * scale.x, 0, 0), 0.5f);
                    break;
                case 2:
                    transform.DOBlendableLocalMoveBy(new Vector3(0, 0.25f * scale.x, 0), 0.5f);
                    linkedTile.transform.DOBlendableLocalMoveBy(new Vector3(0, -0.25f * scale.x, 0), 0.5f);
                    break;
                case 3:
                    transform.DOBlendableLocalMoveBy(new Vector3(0, -0.25f * scale.x, 0), 0.5f);
                    linkedTile.transform.DOBlendableLocalMoveBy(new Vector3(0, 0.25f * scale.x, 0), 0.5f);
                    break;
                default:
                    break;
            }
            stickyFull.transform.DOScale(new Vector3(1.5f * scale.x, scale.x, scale.x), 0.51f).OnComplete(() => DoneSticky());
        }
    }
    void DoneSticky()
    {
        if (sortingIndex < linkedTile.sortingIndex)
        {
            InvokeUpAsButton();
            linkedTile.InvokeUpAsButton();
        }
        else
        {
            linkedTile.InvokeUpAsButton();
            InvokeUpAsButton();
        }
        //Spine.Unity.SkeletonAnimation vaporizeFx = Instantiate(GameController.instance.vaporizeAnim, stickyFull.transform);
        //var vaporizeFx = PoolManager.instance.GetObject<PoolObjectSpine>(GameController.instance.vaporizeAnim, stickyFull.transform, 0.5f);
        //vaporizeFx.transform.localPosition = Vector3.zero;
        stickyFull.DOFade(0, 0.5f);
        //GameController.instance.StickyFade(stickyFull, vaporizeFx);
        stickySprites[rldu].gameObject.SetActive(true);
        stickySprites[rldu].DOKill();
        stickySprites[rldu].DOFade(0, 1).SetDelay(0.5f);
        linkedTile.stickySprites[linkedTile.rldu].gameObject.SetActive(true);
        linkedTile.stickySprites[linkedTile.rldu].DOKill();
        linkedTile.stickySprites[linkedTile.rldu].DOFade(0, 1).SetDelay(0.5f);

        GameController.instance.gameState = GAME_STATE.PLAYING;
    }
}
