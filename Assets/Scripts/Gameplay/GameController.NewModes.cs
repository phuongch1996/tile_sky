using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public partial class GameController
{
    bool newModeReady;
    bool newModeEffectBusy;
    bool newModeClearing;
    bool newModeWaveQueued;
    bool newModeClearBoardFallback;
    int newModePoisonPending;
    int newModeWinningRounds;
    int newModeRoundTarget;
    readonly List<ModeTrayProgress> modeTrays = new();
    readonly List<GameObject> modeTrayVisuals = new();
    TextMeshProUGUI modeObjective;

    bool HasNewMode => NewModeRules.IsExtendedMode((int)gameMode);
    int ModeSlotCount => gameMode == GAME_MODE.tile_missslot
        ? BaseSlotCount - NewModeRules.LockedSlots(parameter) : BaseSlotCount;

    public bool NewModeInputBlocked => HasNewMode && newModeReady &&
        (newModeEffectBusy || newModeClearing || newModeWaveQueued || newModePoisonPending > 0 ||
         PendingModeBombs().Count > 0);

    List<TileModeMarker> PendingModeBombs() => itemTiles == null ? new List<TileModeMarker>() : itemTiles
        .Where(IsModeBoardTile).Select(tile => tile.GetComponent<TileModeMarker>())
        .Where(marker => marker != null && marker.kind == TileModeKind.Bomb &&
            marker.bombMoves == 0 && !marker.bombResolved).ToList();

    bool IsModeBoardTile(ItemTile tile) => tile != null && tile.gameObject.activeSelf &&
        tile.state != ITEMTILE_STATE.SLOTTED && tile.state != ITEMTILE_STATE.DONE &&
        !slottedItems.Contains(tile);

    bool ModeTilesSettled() => match3Items.Count == 0 && !itemTiles.Any(tile =>
        tile.state == ITEMTILE_STATE.HOVERING) && slottedItems.All(tile =>
        tile.transform.localPosition.sqrMagnitude < 0.001f);

    void InitializeNewMode()
    {
        if (!HasNewMode) return;
        newModeWinningRounds = 0;
        newModeRoundTarget = NewModeRules.TargetRounds(parameter);
        newModePoisonPending = 0;
        newModeWaveQueued = false;
        newModeClearBoardFallback = false;
        newModeClearing = false;
        newModeEffectBusy = false;
        modeTrays.Clear();
        var candidates = itemTiles.Where(tile => !tile.IsEventTile && tile.tileType >= 0).ToList();
        switch (gameMode)
        {
            case GAME_MODE.tile_poison:
                foreach (var tile in RandomModeTiles(candidates, parameter > 0 ? parameter : 3))
                    GetModeMarker(tile).Configure(tile, TileModeKind.Poison);
                break;
            case GAME_MODE.tile_bomb:
                foreach (var floor in candidates.GroupBy(tile => tile.floor))
                    foreach (var tile in RandomModeTiles(floor.ToList(), parameter > 0 ? parameter : 1))
                        GetModeMarker(tile).Configure(tile, TileModeKind.Bomb,
                            NewModeRules.BombBudget(leveljson.bombMoves));
                break;
            case GAME_MODE.tile_tray:
                var floors = candidates.Select(tile => tile.floor).Distinct()
                    .OrderBy(_ => UnityEngine.Random.value).Take(parameter > 0 ? parameter : 3);
                foreach (int floor in floors)
                {
                    var tile = RandomModeTiles(candidates.Where(t => t.floor == floor).ToList(), 1)[0];
                    modeTrays.Add(new ModeTrayProgress { floor = floor, x = tile.tilePos.x, y = tile.tilePos.y });
                }
                CreateModeTrayVisuals();
                break;
            case GAME_MODE.tile_winning:
                newModeClearBoardFallback = !CreateWinningWave();
                break;
            case GAME_MODE.tile_gold_2:
                // Reserve whole triples, so normal tile counts remain solvable.
                var triples = CompleteModeTriples(candidates);
                int coinCount = NewModeRules.CoinTileCount(parameter, triples.Count);
                newModeClearBoardFallback = coinCount == 0;
                foreach (var tile in triples.Take(coinCount / 3).SelectMany(group => group))
                    tile.SetType(NewModeRules.CoinTileType);
                if (coinCount == 0)
                    Debug.LogWarning("Coin mode has no complete ordinary triple; clear the board to win.");
                break;
        }
        newModeReady = true;
        CreateModeObjective();
        CreateLockedSlotLabels();
    }

    static TileModeMarker GetModeMarker(ItemTile tile) =>
        tile.GetComponent<TileModeMarker>() ?? tile.gameObject.AddComponent<TileModeMarker>();

    static List<ItemTile> RandomModeTiles(List<ItemTile> tiles, int count) =>
        tiles.OrderBy(_ => UnityEngine.Random.value).Take(Mathf.Clamp(count, 0, tiles.Count)).ToList();

    List<List<ItemTile>> CompleteModeTriples(IEnumerable<ItemTile> tiles)
    {
        var eligible = tiles.Where(tile => !tile.IsEventTile && tile.tileType >= 0)
            .OrderBy(_ => UnityEngine.Random.value).ToList();
        return NewModeRules.CompleteTripleIndices(eligible.Select(tile => tile.tileType).ToList())
            .Select(indices => indices.Select(index => eligible[index]).ToList()).ToList();
    }

    bool CreateWinningWave()
    {
        var triples = CompleteModeTriples(itemTiles.Where(IsModeBoardTile));
        if (triples.Count == 0) return false;
        foreach (var tile in triples[0])
        {
            tile.SetType(NewModeRules.WinningTileType);
            tile.sprites.DOPunchScale(Vector3.one * 0.15f, 0.3f);
        }
        return true;
    }

    public bool TrySetNewModeTileType(ItemTile tile, int type)
    {
        if (!HasNewMode) return false;
        if (!NewModeRules.IsSpecialType((int)gameMode, type))
        {
            var previous = tile.GetComponent<TileModeMarker>();
            if (previous != null && (previous.kind == TileModeKind.Winning || previous.kind == TileModeKind.Coin))
                previous.ResetMarker();
            return false;
        }
        tile.tileType = type;
        var sprite = type == NewModeRules.CoinTileType
            ? LevelManager.instance.tileSpritesSO.goldSprite
            : itemSpritesUsing[0];
        tile.item.sprite = sprite;
        float edge = Mathf.Max(sprite.rect.width, sprite.rect.height);
        tile.item.size = new Vector2(sprite.rect.width / edge, sprite.rect.height / edge) * 0.825f;
        tile.item.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
        GetModeMarker(tile).Configure(tile, type == NewModeRules.WinningTileType
            ? TileModeKind.Winning : TileModeKind.Coin);
        return true;
    }

    public void OnNewModeTileCollected(ItemTile selected, bool automatic)
    {
        if (!HasNewMode || !newModeReady || automatic) return;
        AdvanceModeBombs(new List<ItemTile> { selected });
    }

    void AdvanceModeBombs(List<ItemTile> excluded)
    {
        if (gameMode != GAME_MODE.tile_bomb) return;
        foreach (var tile in itemTiles)
        {
            if (excluded.Contains(tile) || !IsModeBoardTile(tile) ||
                (tile.state != ITEMTILE_STATE.TOUCHABLE && tile.state != ITEMTILE_STATE.HOVERING)) continue;
            var marker = tile.GetComponent<TileModeMarker>();
            if (marker != null && marker.kind == TileModeKind.Bomb)
                marker.bombMoves = NewModeRules.BombAfterSelection(marker.bombMoves, true, false);
        }
    }

    void OnNewModeHintStarted(List<ItemTile> selected)
    {
        if (HasNewMode && newModeReady) AdvanceModeBombs(selected);
    }

    public void OnNewModeTileArrived()
    {
        if (HasNewMode && newModeReady && !newModeEffectBusy && ModeTilesSettled())
            EvaluateModeObjectives();
    }

    void OnNewModeMatchCleared(List<ItemTile> matched)
    {
        if (!HasNewMode || !newModeReady || newModeClearing) return;
        if (gameMode == GAME_MODE.tile_poison && matched.Any(tile =>
            tile.TryGetComponent(out TileModeMarker marker) && marker.kind == TileModeKind.Poison))
            newModePoisonPending++;
        if (gameMode == GAME_MODE.tile_winning && matched.Any(tile => tile.tileType == NewModeRules.WinningTileType))
        {
            newModeWinningRounds++;
            newModeWaveQueued = newModeWinningRounds < newModeRoundTarget;
        }
        EvaluateModeObjectives();
    }

    void LateUpdate()
    {
        if (!HasNewMode || !newModeReady || newModeClearing) return;
        UpdateModeObjective();
        if (gameState != GAME_STATE.PLAYING || newModeEffectBusy || !ModeTilesSettled()) return;
        if (EvaluateModeObjectives()) return;
        if (newModeWaveQueued)
        {
            newModeWaveQueued = false;
            newModeClearBoardFallback = !CreateWinningWave();
            SaveProgress();
        }
        if (newModePoisonPending > 0 || PendingModeBombs().Count > 0)
            StartCoroutine(ResolveNewModeEffects());
    }

    bool EvaluateModeObjectives()
    {
        if (newModeClearing) return true;
        if (gameState == GAME_STATE.LOSE_POPUP || gameState == GAME_STATE.WIN_POPUP) return false;
        bool won = false;
        if (gameMode == GAME_MODE.tile_winning)
            won = newModeWinningRounds >= newModeRoundTarget;
        if (gameMode == GAME_MODE.tile_gold_2)
            won = !newModeClearBoardFallback && !itemTiles.Any(tile => tile.tileType == NewModeRules.CoinTileType);
        if (gameMode == GAME_MODE.tile_tray)
        {
            for (int i = 0; i < modeTrays.Count; i++)
            {
                var tray = modeTrays[i];
                if (tray.cleared || IsTrayCovered(tray)) continue;
                tray.cleared = true;
                if (i < modeTrayVisuals.Count && modeTrayVisuals[i] != null)
                    modeTrayVisuals[i].transform.DOScale(0, 0.25f).SetEase(Ease.InBack);
            }
            won = modeTrays.Count > 0 && modeTrays.All(tray => tray.cleared);
        }
        if (!won) return false;
        newModeClearing = true;
        newModeWaveQueued = false;
        gameState = GAME_STATE.WIN;
        ClearSavedProgress();
        StartCoroutine(ClearNewModeBoard());
        return true;
    }

    bool IsTrayCovered(ModeTrayProgress tray)
    {
        var trayFloor = floorTransformList[tray.floor - 1];
        foreach (var tile in itemTiles.Where(IsModeBoardTile))
        {
            if (tile.floor < tray.floor) continue;
            Vector3 boardPosition = floorTransformList[tile.floor - 1].TransformPoint(tile.tilePos);
            Vector3 relative = trayFloor.InverseTransformPoint(boardPosition);
            if (NewModeRules.CoversTray(relative.x - tray.x, relative.y - tray.y)) return true;
        }
        return false;
    }

    IEnumerator ResolveNewModeEffects()
    {
        newModeEffectBusy = true;
        try
        {
            while (itemTiles.Count > 0 && !newModeClearing)
            {
                bool poison = newModePoisonPending > 0;
                var bombs = PendingModeBombs();
                if (!poison && bombs.Count == 0) break;
                if (poison) newModePoisonPending--;
                foreach (var bomb in bombs) bomb.bombResolved = true;
                var candidates = itemTiles.Where(IsModeBoardTile).Where(tile => !tile.IsEventTile && tile.tileType >= 0)
                    .Where(tile => !tile.TryGetComponent(out TileModeMarker marker) ||
                        (marker.kind != TileModeKind.Poison &&
                         (marker.kind != TileModeKind.Bomb || marker.bombMoves <= 0))).ToList();
                int freeSlots = Mathf.Max(0, SlotCount - slottedItems.Count);
                int count = Mathf.Min(freeSlots, poison ? 1 : bombs.Count);
                var forcedTiles = RandomModeTiles(candidates, count);
                if (forcedTiles.Count == 0) continue;
                gameState = GAME_STATE.SHUFFLE; // Existing state blocks manual tile input.
                foreach (var tile in forcedTiles)
                {
                    if (!IsSlotAvailable) break;
                    tile.MoveTileToSlot(true);
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(0.8f);
                while (match3Items.Count > 0 && itemTiles.Count > 0) yield return null;
                while (gameState == GAME_STATE.PAUSE) yield return null;
                if (gameState == GAME_STATE.LOSE || gameState == GAME_STATE.LOSE_POPUP ||
                    gameState == GAME_STATE.WIN || gameState == GAME_STATE.WIN_POPUP) yield break;
                if (poison)
                {
                    // Preserve the reference mechanic: force one normal tile, then shuffle.
                    foreach (var tile in itemTiles.Where(IsModeBoardTile))
                    {
                        tile.state = ITEMTILE_STATE.START;
                        tile.InitItemTile(tile.floor, tile.sortingIndex, tile.tilePos);
                    }
                    ShuffleItems();
                    yield return new WaitUntil(() => gameState != GAME_STATE.SHUFFLE);
                }
                else gameState = GAME_STATE.PLAYING;
            }
        }
        finally { newModeEffectBusy = false; }
        if (gameState == GAME_STATE.PLAYING) SaveProgress();
    }

    IEnumerator ClearNewModeBoard()
    {
        foreach (var tile in itemTiles) tile.itemCollider.enabled = false;
        yield return new WaitUntil(() => match3Items.Count == 0);
        var remaining = new List<ItemTile>(itemTiles);
        foreach (var tile in remaining)
        {
            tile.transform.DOKill();
            tile.sprites.DOKill();
            tile.transform.DOScale(0, 0.25f).SetEase(Ease.InBack);
        }
        yield return new WaitForSeconds(0.3f);
        foreach (var tile in remaining) tile.gameObject.SetActive(false);
        itemTiles.Clear();
        slottedItems.Clear();
        undoList.Clear();
        // The existing WinCoroutine owns the final animation, rewards and victory popup.
    }

    void CreateModeTrayVisuals()
    {
        foreach (var previous in modeTrayVisuals) if (previous != null) Destroy(previous);
        modeTrayVisuals.Clear();
        for (int i = 0; i < modeTrays.Count; i++)
        {
            var tray = modeTrays[i];
            var go = new GameObject("Objective tray " + (i + 1), typeof(SpriteRenderer));
            go.transform.SetParent(floorTransformList[tray.floor - 1], false);
            go.transform.localPosition = new Vector3(tray.x, tray.y, 0.1f);
            go.transform.localScale = Vector3.one * 2.2f;
            var sprite = go.GetComponent<SpriteRenderer>();
            sprite.sprite = LevelManager.instance.tileSpritesSO.bgSprite;
            sprite.color = new Color(1f, 0.65f, 0.1f, 0.8f);
            sprite.sortingLayerName = "Floor" + tray.floor;
            sprite.sortingOrder = -100;
            go.SetActive(!tray.cleared);
            modeTrayVisuals.Add(go);
        }
    }

    void CreateLockedSlotLabels()
    {
        if (gameMode != GAME_MODE.tile_missslot) return;
        for (int i = SlotCount; i < BaseSlotCount; i++)
        {
            Transform slot = GetSlotTransform(i);
            if (slot == null) continue;
            var go = new GameObject("Locked slot", typeof(TextMeshPro));
            go.transform.SetParent(slot, false);
            var text = go.GetComponent<TextMeshPro>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = "X";
            text.fontSize = 6;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1, 0.25f, 0.2f);
            text.rectTransform.sizeDelta = Vector2.one;
            text.renderer.sortingLayerName = "Slotted";
            text.renderer.sortingOrder = 100;
        }
    }

    void CreateModeObjective()
    {
        var source = GameScript.instance.txLevel;
        if (source == null) return;
        if (modeObjective == null)
        {
            var go = new GameObject("Mode objective", typeof(TextMeshProUGUI));
            go.transform.SetParent(source.transform.parent, false);
            modeObjective = go.GetComponent<TextMeshProUGUI>();
            modeObjective.font = source.font;
            modeObjective.fontSize = 28;
            modeObjective.alignment = TextAlignmentOptions.Center;
            modeObjective.raycastTarget = false;
            var rect = modeObjective.rectTransform;
            rect.anchorMin = source.rectTransform.anchorMin;
            rect.anchorMax = source.rectTransform.anchorMax;
            rect.pivot = source.rectTransform.pivot;
            rect.anchoredPosition = source.rectTransform.anchoredPosition + Vector2.down * 75;
            rect.sizeDelta = new Vector2(850, 64);
            if (gameMode is GAME_MODE.tile_tray or GAME_MODE.tile_winning or GAME_MODE.tile_gold_2)
                GameScript.instance.RegisterModeObjectiveCounter(rect);
        }
        UpdateModeObjective();
    }

    void UpdateModeObjective()
    {
        if (modeObjective == null) return;
        modeObjective.text = gameMode switch
        {
            GAME_MODE.tile_missslot => "Match 3 - " + SlotCount + " tray slots",
            GAME_MODE.tile_poison => "POISON triple: adds one tile, then shuffles",
            GAME_MODE.tile_bomb => "BOMB at zero: adds one tile to the tray",
            GAME_MODE.tile_tray => "Uncover trays: " + modeTrays.Count(t => t.cleared) + "/" + modeTrays.Count,
            GAME_MODE.tile_winning => newModeClearBoardFallback ? "Clear the remaining tiles" :
                "TARGET triples: " + newModeWinningRounds + "/" + newModeRoundTarget,
            GAME_MODE.tile_gold_2 => "COIN tiles left: " + itemTiles.Count(t => t.tileType == NewModeRules.CoinTileType),
            _ => ""
        };
    }

    ModeProgressData CaptureNewModeProgress() => !HasNewMode ? null : new ModeProgressData
    {
        winningRounds = newModeWinningRounds, roundTarget = newModeRoundTarget,
        clearBoardFallback = newModeClearBoardFallback, trays = modeTrays,
        waveQueued = newModeWaveQueued
    };

    void RestoreNewModeProgress(GameplayProgressData data)
    {
        if (!HasNewMode) return;
        var saved = data.modeProgress;
        newModeWinningRounds = saved.winningRounds;
        newModeRoundTarget = saved.roundTarget;
        newModeClearBoardFallback = saved.clearBoardFallback;
        newModeWaveQueued = saved.waveQueued;
        modeTrays.Clear();
        if (saved.trays != null) modeTrays.AddRange(saved.trays);
        newModeReady = true;
        CreateModeTrayVisuals();
        CreateModeObjective();
        CreateLockedSlotLabels();
    }

    void RestoreNewModeTile(ItemTile tile, GameplayTileProgressData data)
    {
        if (HasNewMode && data.modeKind != 0)
            GetModeMarker(tile).Configure(tile, (TileModeKind)data.modeKind, data.bombMoves, data.bombResolved);
    }

    void ResumeAfterModeHint()
    {
        // Release only Hint's lock, preserving pause, win/lose and mode-transition states.
        if (gameState == GAME_STATE.SUGGEST && !newModeClearing)
            gameState = GAME_STATE.PLAYING;
        GameScript.instance.PowerUpsInteractable();
    }
}

[Serializable]
public sealed class ModeProgressData
{
    public int winningRounds;
    public int roundTarget;
    public bool clearBoardFallback;
    public bool waveQueued;
    public List<ModeTrayProgress> trays = new();
}

[Serializable]
public sealed class ModeTrayProgress
{
    public int floor;
    public float x;
    public float y;
    public bool cleared;
}
