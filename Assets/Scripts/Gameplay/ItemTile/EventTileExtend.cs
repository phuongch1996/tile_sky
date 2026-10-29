using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EventTileExtend : PoolObject
{
    public static List<ItemTile> eventTileList = new();
    public Sprite ungoldItemSprite;
    public static int eventCounter = 0;
    public static int eventTotal = 0;
    public SpriteRenderer[] goldPieces;
    public SpriteRenderer glow;
    public int counter;
    public ParticleSystem goldParticle;
    public ItemTile itemTile;
    Vector3[] goldPieceLocalPositions;
    Vector3[] goldPieceLocalScales;
    bool isCounterAnimating;
    public override void Active(float? timeActive = null)
    {
        base.Active(timeActive);
        isCounterAnimating = false;
        CacheGoldPieceTransforms();
        ResetGoldPiecesVisual();
    }
    public override void Inactive()
    {
        isCounterAnimating = false;
        ResetGoldPiecesVisual();
        base.Inactive();
        foreach (var renderer in goldPieces)
            renderer.gameObject.SetActive(true);
        itemTile = null;
    }
    public void SetCounter()
    {
        glow.gameObject.SetActive(false);
        eventTileList.Add(itemTile);
        counter = 4;
        ResetGoldPiecesVisual();
    }
    public void RestoreCounter(int value)
    {
        glow.gameObject.SetActive(false);
        if (!eventTileList.Contains(itemTile))
            eventTileList.Add(itemTile);

        counter = Mathf.Clamp(value, 0, goldPieces.Length);
        ResetGoldPiecesVisual();
        for (int i = 0; i < goldPieces.Length; i++)
            goldPieces[i].gameObject.SetActive(i < counter);
    }
    public void ReduceCounter()
    {
        Debug.Log("reduce counter");
        if (counter == 0)
            return;
        if (itemTile.state == ITEMTILE_STATE.TOUCHABLE)
        {
            int index = --counter;
            goldPieces[index].gameObject.SetActive(false);
            //Spine.Unity.SkeletonAnimation vaporizeFx = Instantiate(GameController.instance.vaporizeAnim, transform);
            //var vaporizeFx = PoolManager.instance.GetObject<PoolObjectSpine>(GameController.instance.vaporizeAnim, transform, 0.5f);
            //vaporizeFx.transform.localPosition = Vector3.zero;
            goldParticle.Play(true);
            ResetGoldPiecesVisual();
            itemTile.sprites.transform.DOKill();
            itemTile.sprites.transform.localScale = Vector3.one;
            itemTile.sprites.transform.localRotation = Quaternion.identity;
            itemTile.sprites.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);//.OnComplete(() => Destroy(vaporizeFx.gameObject));
        }
    }

    public void ResetGoldPiecesVisual()
    {
        if (isCounterAnimating)
            return;

        CacheGoldPieceTransforms();

        bool canStraightenParent = itemTile != null && itemTile.state != ITEMTILE_STATE.HOVERING;
        if (canStraightenParent)
        {
            itemTile.transform.localRotation = Quaternion.identity;
            if (itemTile.sprites != null)
            {
                itemTile.sprites.transform.localRotation = Quaternion.identity;
            }
        }

        transform.DOKill(false);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        if (goldPieces == null)
            return;

        for (int i = 0; i < goldPieces.Length; i++)
        {
            SpriteRenderer renderer = goldPieces[i];
            if (renderer == null)
                continue;

            Transform piece = renderer.transform;
            piece.DOKill(false);
            piece.localRotation = Quaternion.identity;
            if (goldPieceLocalPositions != null && i < goldPieceLocalPositions.Length)
                piece.localPosition = goldPieceLocalPositions[i];
            if (goldPieceLocalScales != null && i < goldPieceLocalScales.Length)
                piece.localScale = goldPieceLocalScales[i];
        }

        if (glow != null)
            glow.transform.localRotation = Quaternion.identity;
    }

    public void BeginCounterAnimation()
    {
        isCounterAnimating = true;
        transform.DOKill(false);
    }

    public void FinishCounterAnimation()
    {
        transform.DOKill(false);
        glow.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void CacheGoldPieceTransforms()
    {
        if (goldPieces == null)
            return;

        if (goldPieceLocalPositions != null && goldPieceLocalPositions.Length == goldPieces.Length)
            return;

        goldPieceLocalPositions = new Vector3[goldPieces.Length];
        goldPieceLocalScales = new Vector3[goldPieces.Length];
        for (int i = 0; i < goldPieces.Length; i++)
        {
            if (goldPieces[i] == null)
                continue;

            goldPieceLocalPositions[i] = goldPieces[i].transform.localPosition;
            goldPieceLocalScales[i] = goldPieces[i].transform.localScale;
        }
    }

    private void LateUpdate()
    {
        if (isCounterAnimating)
            return;

        if (itemTile == null || itemTile.state == ITEMTILE_STATE.HOVERING)
            return;

        ResetGoldPiecesVisual();
    }

    public bool EventUp { get { return counter > 0; } }
    public static Action<int, int> OnEventCounter;
    public static void EventCounter(bool increase)
    {
        if (increase)
            eventCounter++;
        else
            eventCounter--;
        OnEventCounter?.Invoke(eventCounter, eventTotal);
    }
    public static void EventCounterUpdate()
    {
        OnEventCounter?.Invoke(eventCounter, eventTotal);
    }
}
