using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
public class IceTileVariant : ItemTile
{
    public static List<IceTileVariant> iceTilesList = new();
    public SpriteRenderer[] icePieces;
    public int counter;
    public ParticleSystem iceParticle;
    public static bool iceBreak = false;
    public List<Vector3> icesPos;
    private void OnValidate()
    {
        icesPos = new();
        foreach (var renderer in icePieces)
            icesPos.Add(renderer.transform.localPosition);
    }
    public override void DefaultValue()
    {
        for (int i = 0; i < icePieces.Length; i++)
        {
            icePieces[i].gameObject.SetActive(true);
            icePieces[i].color = Color.white;
            icePieces[i].transform.localPosition = icesPos[i];
        }
        base.DefaultValue();
    }
    public void SetCounter()
    {
        iceTilesList.Add(this);
        counter = 3;
    }
    public void RestoreCounter(int value)
    {
        if (!iceTilesList.Contains(this))
            iceTilesList.Add(this);

        counter = Mathf.Clamp(value, 0, 3);
        for (int i = 0; i < icePieces.Length; i++)
            icePieces[i].gameObject.SetActive(true);

        if (counter <= 2 && icePieces.Length > 3)
            icePieces[3].gameObject.SetActive(false);
        if (counter <= 1 && icePieces.Length > 2)
            icePieces[2].gameObject.SetActive(false);
        if (counter <= 0)
        {
            if (icePieces.Length > 0)
                icePieces[0].gameObject.SetActive(false);
            if (icePieces.Length > 1)
                icePieces[1].gameObject.SetActive(false);
        }
    }
    public void ReduceCounter(bool loop = false)
    {
        if (counter == 0)
            return;
        if (state == ITEMTILE_STATE.TOUCHABLE || state == ITEMTILE_STATE.HOVERING)
        {
            counter -= 1;
            if (counter == 0)
            {
                icePieces[0].sortingLayerName = "Slotted";
                icePieces[1].sortingLayerName = "Slotted";
                Vector3 pos0 = icePieces[0].transform.localPosition;
                Vector3 pos1 = icePieces[1].transform.localPosition;
                Vector3[] path0 = new Vector3[] { pos0, pos0, pos0 + new Vector3(0.5f, 0.25f), pos0 + new Vector3(1, -0.25f), pos0 + new Vector3(1.75f, -3), pos0 + new Vector3(3, -10) };
                Vector3[] path1 = new Vector3[] { pos1, pos1, pos1 + new Vector3(-0.5f, 0.25f), pos1 + new Vector3(-1, -0.25f), pos1 + new Vector3(-1.75f, -3), pos1 + new Vector3(-3, -10) };
                icePieces[0].transform.DOLocalPath(path0, 0.75f, PathType.CatmullRom).SetEase(Ease.InCirc);
                icePieces[1].transform.DOLocalPath(path1, 0.75f, PathType.CatmullRom).SetEase(Ease.InCirc);
                icePieces[0].transform.DOBlendableRotateBy(new Vector3(0, 0, -360), 0.75f, RotateMode.FastBeyond360).SetEase(Ease.InCirc);
                icePieces[1].transform.DOBlendableRotateBy(new Vector3(0, 0, 360), 0.75f, RotateMode.FastBeyond360).SetEase(Ease.InCirc);
                icePieces[0].DOFade(0, 0.75f).OnComplete(() =>
                {
                    icePieces[0].gameObject.SetActive(false);
                    icePieces[0].transform.localPosition = icesPos[0];
                });
                icePieces[1].DOFade(0, 0.75f).OnComplete(() =>
                {
                    icePieces[1].gameObject.SetActive(false);
                    icePieces[1].transform.localPosition = icesPos[1];
                });
            }
            else
            {
                int index = counter + 1;
                icePieces[index].sortingLayerName = "Slotted";
                Vector3 pos = icePieces[index].transform.localPosition;
                int i = index % 2 == 0 ? 1 : -1;
                Vector3[] path = new Vector3[] { pos, pos, pos + new Vector3(0.5f * i, 0.25f), pos + new Vector3(1 * i, -0.25f), pos + new Vector3(1.75f * i, -3), pos + new Vector3(3 * i, -10) };
                icePieces[index].transform.DOLocalPath(path, 0.75f, PathType.CatmullRom).SetEase(Ease.InCirc);
                icePieces[index].transform.DOBlendableRotateBy(new Vector3(0, 0, index % 2 == 0 ? -360 : 360), 0.75f, RotateMode.FastBeyond360).SetEase(Ease.InCirc);
                icePieces[index].DOFade(0, 0.75f).OnComplete(() =>
                {
                    icePieces[index].gameObject.SetActive(false);
                    icePieces[index].transform.localPosition = icesPos[index];
                });
            }
            if (!loop)
            {
                iceBreak = true;
                //Spine.Unity.SkeletonAnimation vaporizeFx = Instantiate(GameController.instance.vaporizeAnim, transform);
                //var vaporizeFx = PoolManager.instance.GetObject<PoolObjectSpine>(GameController.instance.vaporizeAnim, transform, 0.5f);
                //vaporizeFx.transform.localPosition = Vector3.zero;
                iceParticle.Play(true);
                sprites.transform.DOKill();
                sprites.transform.localScale = Vector3.one;
                sprites.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);//.OnComplete(() => Destroy(vaporizeFx.gameObject));
            }
        }
    }
    // One adjacent collection breaks every remaining ice piece, including old saves.
    public static void UnfreezeNeighbours(ItemTile collectedTile)
    {
        foreach (var iceTile in iceTilesList)
        {
            if (iceTile == null || iceTile == collectedTile || iceTile.Touchable ||
                iceTile.floor != collectedTile.floor ||
                (iceTile.state != ITEMTILE_STATE.TOUCHABLE && iceTile.state != ITEMTILE_STATE.HOVERING))
                continue;

            Vector2 offset = iceTile.tilePos - collectedTile.tilePos;
            if (NewModeRules.AreIceNeighbours(iceTile.floor, collectedTile.floor, offset.x, offset.y))
                iceTile.InstaUnfreeze();
        }
    }
    public static void UnfreezeIsolatedTiles(List<ItemTile> board)
    {
        foreach (var iceTile in iceTilesList)
        {
            if (iceTile == null || iceTile.Touchable || iceTile.state == ITEMTILE_STATE.SLOTTED ||
                iceTile.state == ITEMTILE_STATE.DONE || !board.Contains(iceTile))
                continue;

            bool hasNeighbour = board.Exists(tile => tile != iceTile &&
                tile.state != ITEMTILE_STATE.SLOTTED && tile.state != ITEMTILE_STATE.DONE &&
                NewModeRules.AreIceNeighbours(iceTile.floor, tile.floor,
                    tile.tilePos.x - iceTile.tilePos.x, tile.tilePos.y - iceTile.tilePos.y));
            if (hasNeighbour)
                continue;

            if (iceTile.state == ITEMTILE_STATE.TOUCHABLE || iceTile.state == ITEMTILE_STATE.HOVERING)
                iceTile.InstaUnfreeze();
            else
                iceTile.RestoreCounter(0);
        }
    }
    public void InstaUnfreeze()
    {
        if (Touchable || (state != ITEMTILE_STATE.TOUCHABLE && state != ITEMTILE_STATE.HOVERING))
            return;

        while (counter > 0)
        {
            ReduceCounter(true);
        }
        //Spine.Unity.SkeletonAnimation vaporizeFx = Instantiate(GameController.instance.vaporizeAnim, transform);
        //var vaporizeFx = PoolManager.instance.GetObject<PoolObjectSpine>(GameController.instance.vaporizeAnim, transform, 0.5f);
        //vaporizeFx.transform.localPosition = Vector3.zero;
        iceParticle.Play(true);
        sprites.transform.DOKill();
        sprites.transform.localScale = Vector3.one;
        sprites.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);//.OnComplete(() => Destroy(vaporizeFx.gameObject));
    }
    public bool Touchable { get { return counter <= 0; } }
    public override void MouseDown()
    {
        if (!Touchable)
            return;
        base.MouseDown();
    }
    public override void MouseUpAsButton()
    {
        if (!Touchable)
        {
            int rnd = GameConfig.GetRandom(-1, 1);
            DOTween.Sequence().Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, -10 * rnd), 0.125f))
                   .Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, 10 * rnd), 0.25f))
                   .Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.125f));
            return;
        }
        base.MouseUpAsButton();
    }
    public override void SetSortingLayer(string layer)
    {
        //Debug.Log("iceLayer");
        base.SetSortingLayer(layer);
        foreach (var renderer in icePieces)
            renderer.sortingLayerName = layer;
    }
    public override void SetSortingOrder(int order)
    {
        //Debug.Log("iceOrder");
        base.SetSortingOrder(order);
        foreach (var renderer in icePieces)
            renderer.sortingOrder = order;
    }
}
