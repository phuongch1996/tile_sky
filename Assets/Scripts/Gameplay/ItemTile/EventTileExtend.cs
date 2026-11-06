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
        if (!eventTileList.Contains(itemTile))
            eventTileList.Add(itemTile);
        counter = 4;
        foreach (var piece in goldPieces) piece.gameObject.SetActive(true);
        ResetGoldPiecesVisual();
    }
    public void RestoreCounter(int value)
    {
        glow.gameObject.SetActive(false);
        if (!eventTileList.Contains(itemTile))
            eventTileList.Add(itemTile);

        // Retain the serialized counter for compatibility; partial damage no longer exists.
        counter = value > 0 ? 4 : 0;
        ResetGoldPiecesVisual();
        foreach (var piece in goldPieces) piece.gameObject.SetActive(true);
        gameObject.SetActive(counter > 0);
    }
    public bool TryCollect()
    {
        if (!EventUp) return false;
        counter = 0;
        EventCounter(true);
        return true;
    }

    public void UndoCollection()
    {
        if (EventUp) return;
        transform.DOKill(false);
        isCounterAnimating = false;
        transform.SetParent(itemTile.sprites, false);
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(true);
        goldParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        SetCounter();
        EventCounter(false);
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
