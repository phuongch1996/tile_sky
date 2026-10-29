using DG.Tweening;
using EasyButtons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public Transform gameParent;
    public Transform floorParent;
    public List<Transform> floorTransformList;
    public Transform slotParent;
    public List<Transform> slotTransformList;
    //public Transform slotPosTfm;
    [Header("Play Area Fit")]
    [SerializeField] private RectTransform bottomRect;
    [SerializeField] private float slotBottomPaddingPixels = 45f;
    [SerializeField] private float bottomTopPaddingPixels = 15f;
    [SerializeField] private float playAreaHorizontalPaddingPixels = 20f;
    [SerializeField] private float boardTilePadding = 0.8f;

    public PoolObject itemPrefab;
    public PoolObject itemRandomPrefab;
    public PoolObject itemLinkPrefab;
    public PoolObject itemIcePrefab;
    public PoolObject itemGoldPrefab;
    public PoolObject eventExtendPrefab;

    public Transform eventParent;
    public Transform eventParentUI;

    public PoolObject stickyFull;

    public ExtraSlotScript extraSlotScript;

    public GameTextScript gameTextScript;

    public BrokenAnimScript brokenAnimScript;

    public GameObject lowOnSpaceOB;
    public GameObject challengeAnim;
    public PoolObject likeAnim;
    //public PoolObjectSpine vaporizeAnim;
    const int BaseSlotCount = 7;
    const int MatchOverflowSlotCount = 3;
    public bool IsSlotAvailable { get { return slottedItems.Count - match3Items.Count < SlotCount && slottedItems.Count < ActiveSlotTransformCount; } }
    public bool IsSlotLinkAvailable { get { return slottedItems.Count - match3Items.Count < SlotCount - 1 && slottedItems.Count < ActiveSlotTransformCount - 1; } }
    private int SlotCount => BaseSlotCount;
    private int ActiveSlotTransformCount => slotTransformList == null ? 0 : Mathf.Min(BaseSlotCount + PendingMatchSlotCount, slotTransformList.Count);
    private int PendingMatchSlotCount => match3Items == null ? 0 : Mathf.Min(match3Items.Count, MatchOverflowSlotCount);
    public bool IsDanger { get { return SlotCount - slottedItems.Count <= 1 && IsSlotAvailable && match3Items.Count == 0; } }
    public bool IsChallengeAnim { get { return SlotCount - slottedItems.Count <= 2 && IsSlotAvailable && match3Items.Count == 0; } }
    public readonly List<int> slotsAvailable = new() { 0, 1, 2, 3, 4, 5, 6 };
    public List<ItemTile> itemTiles;
    public List<ItemTile> slottedItems;
    public List<ItemTile> undoList;
    public List<ItemTile> match3Items;
    public List<Sprite> itemSpritesUsing;
    public GAME_STATE gameState;
    public bool UIClickable { get { return gameState == GAME_STATE.PLAYING && !itemTiles.Exists(x => x.state == ITEMTILE_STATE.HOVERING); } }
    [HideInInspector]
    public int totalTileCount;
    [HideInInspector]
    public List<Vector3Int[]> connectedList;
    public int itemTypeCount;
    public int floorCount;
    public int parameter;
    public GAME_MODE gameMode;
    public bool isGoldTile;
    public bool isComboEvent;
    public float waitTimeToStart;

    [Header("Tile Effects")]
    [SerializeField] private Transform effectsParent;
    [SerializeField] private TileTapParticlePool tileTapParticlePool;
    [SerializeField] private ParticleSystem tileTapParticlePrefab;
    [SerializeField] private ParticleSystem tripleMergeBoomParticlePrefab;
    [SerializeField] private TripleTileMergeFx tripleTileMergeFx;
    [Header("Tile Appear Animation")]
    [SerializeField] private TILE_APPEAR_ANIM tileAppearAnim;
    [SerializeField] private float tileAppearDurationMultiplier = 1.25f;
    [SerializeField] private float tileAppearDelayStep = 0.015f;
    [SerializeField] private float maxTileAppearDelay = 0.45f;

    private Vector3 slotParentBaseScale = Vector3.one;
    private Tween trayBounceTween;

    const string Match3FxSortingLayer = TileOnRuntimeFx.SlottedSortingLayer;
    const int Match3FxSortingOrder = 500;
    const float Match3DurationMultiplier = 7f / 8f;
    public TILE_APPEAR_ANIM TileAppearAnim => tileAppearAnim;
    public float TileAppearDurationMultiplier => tileAppearDurationMultiplier;

    #region startgame
    private void Awake()
    {
        if (!GameConfig.isGameLoaded)
            return;
        instance = this;
        GameConfig.isExtraSlot = false;
        ResetSevenSlotBoard();
        EnsureTileEffects();
        GameConfig.piggyAdd = 0;
        gameState = GAME_STATE.START;
    }
    private void Start()
    {
        if (!GameConfig.isGameLoaded)
            return;
        Starting();
        //StartCoroutine(Starting());
    }
    int GetBoardSizeFromStringLength(int length)
    {
        int size = 2;
        do
        {
            size += 2;
        } while (size * (size - 2) < length);
        return size;
    }
    (Vector2Int min, Vector2Int max) BoardBoundary(int boardSize)
    {
        Vector2Int min = new(-boardSize / 2 + 1, -(boardSize - 2) / 2 + 1);
        Vector2Int max = new(boardSize / 2 - 2, (boardSize + 2) / 2);
        //Debug.Log($"min{min} max{max}");
        return (min, max);
    }
    Dictionary<int, List<Vector2Int>> GetTileMaps(List<string> data)
    {
        Dictionary<int, List<Vector2Int>> maps = new();
        int size = GetBoardSizeFromStringLength(data[0].Length);
        (boardMin, boardMax) = BoardBoundary(size);
        for (int i = 0; i < data.Count; i++)
        {
            List<Vector2Int> tiles = new();
            int[] limit = (i % 2 == 0) ? new int[4] { boardMin.x, boardMax.x, boardMin.y, boardMax.y } : new int[4] { boardMin.x, boardMax.x - 1, boardMin.y, boardMax.y - 1 };
            int x = limit[0];
            //int y = limit[3] - 1;
            int y = limit[3];
            foreach (char c in data[i])
            {
                //Debug.Log($"{c},x={x},y={y}");
                if (c == '1')
                    tiles.Add(new Vector2Int(x, y));
                x++;
                if (x > limit[1])
                {
                    x = limit[0];
                    y--;
                    //if (y == limit[2] - 1)
                    //    y = limit[3] - 1;
                }
            }
            maps.Add(i, tiles);
        }
        return maps;
    }
    (int minX, int maxX, int minY, int maxY) Bound(List<Vector2Int> v2List)
    {
        int minX = v2List.Min(v => v.x);
        int maxX = v2List.Max(v => v.x);
        int minY = v2List.Min(v => v.y);
        int maxY = v2List.Max(v => v.y);
        return (minX, maxX, minY, maxY);
    }
    public (int Count, List<Vector3Int[]> Connected) GetMapValue(Dictionary<int, List<Vector2Int>> maps)
    {
        int count = 0;
        List<Vector3Int[]> result = new();

        List<Vector3Int> indexer = new();
        for (int i = 0; i < maps.Count; i++)
        {

            for (int y = boardMax.y; y >= boardMin.y; y--)
            {
                for (int x = boardMin.x; x <= boardMax.x; x++)
                {
                    //Debug.Log("x:" + x + "   y:" + y);
                    if (maps.TryGetValue(i, out var value) && value.Exists(v2 => v2 == new Vector2Int(x, y)))
                    {
                        count++;
                        indexer.Add(new Vector3Int(x, y, i));
                        if (indexer.Contains(new Vector3Int(x - 1, y, i)))
                        {
                            result.Add(new Vector3Int[] { new Vector3Int(x - 1, y, i), new Vector3Int(x, y, i) });
                        }
                        if (indexer.Contains(new Vector3Int(x, y + 1, i)))
                        {
                            result.Add(new Vector3Int[] { new Vector3Int(x, y + 1, i), new Vector3Int(x, y, i) });
                        }
                    }
                }
            }
        }
        return (count, result);
    }
    public List<int> GetTileIndexesFromListV3(Dictionary<int, List<Vector2Int>> maps, List<Vector3Int> posList)
    {
        int count = 0;
        List<int> result = new();
        for (int i = 0; i < maps.Count; i++)
        {
            for (int y = boardMax.y; y >= boardMin.y; y--)
            {
                for (int x = boardMin.x; x <= boardMax.x; x++)
                {
                    //Debug.Log("x:" + x + "   y:" + y);
                    if (maps.TryGetValue(i, out var value) && value.Exists(v2 => v2 == new Vector2Int(x, y)))
                    {
                        count++;
                        if (posList.Contains(new Vector3Int(x, y, i)))
                            result.Add(count);
                    }
                }
            }
        }
        return result;
    }

    Leveljson leveljson;
    Vector2Int boardMin;
    Vector2Int boardMax;
    float zoomies;

    void Starting()
    {
        EnsureTileEffects();
        RandomTileAppearAnimation();
        itemTiles = new();
        if (!GameConfig.isChallengeLevel)
        {
            leveljson = LevelManager.instance.levelsSO.GetLevel(GameConfig.level);
        }
        else
        {
            GameConfig.level = 0;
            if (GameConfig.isTesting)
                leveljson = LevelManager.instance.levelsSO.GetChallengeLevel(UnityAuthentication.instance.dailyChallengeTestDayOfYear);
            else
            {
                if (!GameConfig.IsDailyChallenge)
                {
                    //goto home;
                    GameScript.instance.Home();
                    return;
                }
                leveljson = LevelManager.instance.levelsSO.GetChallengeLevel(GameConfig.GetIndexDate(UnityAuthentication.instance.dailyChallengeDateIndex.Value));
            }
        }
        parameter = leveljson.param;
        gameMode = LevelManager.GetGameMode(leveljson.mode);
        GameConfig.gameMode = gameMode;
        if (GameConfig.gameMode != GAME_MODE.tile_gold)
        {
            itemTypeCount = Mathf.Max(leveljson.itemTypeCount - GameConfig.Handicap(), 2);
            isGoldTile = EventManager.instance.IsCurrentLevelEvent();
            isComboEvent = EventManager.instance.IsComboEvent();
        }
        else
        {
            itemTypeCount = Mathf.Max(leveljson.itemTypeCount, 2);
            isGoldTile = false;
        }
        var tilemaps = GetTileMaps(leveljson.data);
        var (Count, Connected) = GetMapValue(tilemaps);
        totalTileCount = Count;
        connectedList = Connected;
        int gameModeCount;
        List<int> gameModeTileIndexes = new();
        List<Vector3Int> doubleParams = new();
        IceTileVariant.iceTilesList = new();
        LinkTileVariant.linkTilesList = new();
        RandomTileVariant.randomTilesList = new();
        GoldTileVariant.goldCount = 0;

        EventTileExtend.eventTileList = new();
        EventTileExtend.eventCounter = 0;
        EventTileExtend.eventTotal = 0;
        GameTextScript.comboCount = 0;
        GameScript.instance.HideEventCounters();

        if (!GameConfig.isChallengeLevel)
            GameScript.instance.LevelText();
        else
            GameScript.instance.ChallengeText();

        switch (GameConfig.gameMode)
        {
            case GAME_MODE.tile_random:
                gameModeCount = parameter;
                gameModeTileIndexes = GameConfig.GetRandomCountFromTotal(gameModeCount, 1, totalTileCount);
                break;
            case GAME_MODE.tile_freeze:
                gameModeCount = parameter;
                gameModeTileIndexes = GameConfig.GetRandomCountFromTotal(gameModeCount, 1, totalTileCount);
                break;
            case GAME_MODE.tile_double:
                gameModeCount = parameter;
                List<Vector3Int[]> connect = new();
                connect.AddRange(connectedList);
                Debug.Log("connect count " + connect.Count);
                List<Vector3Int[]> rnd = GameConfig.GetRandomCountV3(gameModeCount, connect.ToArray());
                foreach (var r in rnd)
                {
                    Debug.Log("double Rnd" + r[0] + r[1]);
                    doubleParams.Add(r[0]);
                    doubleParams.Add(r[1]);
                    LinkTileVariant.linkTilesList.Add(null);
                    LinkTileVariant.linkTilesList.Add(null);
                }
                break;
        }

        List<int> eventTileIndexes = new();
        if (isGoldTile)
        {
            EventTileExtend.eventTotal = GameConfig.GetRandom(3, 6, 9);
            List<int> exclude = new();
            exclude.AddRange(gameModeTileIndexes);
            exclude.AddRange(GetTileIndexesFromListV3(tilemaps, doubleParams));
            eventTileIndexes = GameConfig.GetRandomCountFromTotalExclude(EventTileExtend.eventTotal, 1, totalTileCount, exclude);
            GameScript.instance.GoldTileCounter();
        }
        if (isComboEvent)
            GameScript.instance.ComboCounter();
        if (!isGoldTile && !isComboEvent && EventManager.instance.isEvent && EventManager.eventType == EventType.streak_event_1)
            GameScript.instance.EventGoalCounter();
        GameConfig.isNewGame = true;
        if (GameConfig.gameMode == GAME_MODE.tile_gold)
            itemSpritesUsing = LevelManager.instance.tileSpritesSO.GetAllSprites();
        else
            itemSpritesUsing = LevelManager.instance.tileSpritesSO.GetUsingSprites(itemTypeCount);

        if (!GameConfig.isChallengeLevel && GameplayProgressSave.TryLoad(out var progressData) && RestoreProgress(progressData, tilemaps))
        {
            SetCameraOrthoSize();
            StartCoroutine(WaitRestoredProgress());
            return;
        }

        int indexOnMap = 0;
        zoomies = 10;
        waitTimeToStart = 0.6f;
        Debug.Log(tilemaps.Count);
        for (int i = 0; i < tilemaps.Count; i++)
        {
            if (!tilemaps.TryGetValue(i, out List<Vector2Int> v2List) || v2List.Count <= 0)
                continue;
            var (minX, maxX, minY, maxY) = Bound(v2List);
            float[] zoomParams = new float[] { 10, 10, 10, 10 };
            if (i % 2 == 0)
            {
                zoomParams[0] = Mathf.Min(zoomParams[0], minY + 3);
                zoomParams[1] = Mathf.Min(zoomParams[1], 6 - maxY);
                zoomParams[2] = Mathf.Min(zoomParams[2], minX + 4);
                zoomParams[3] = Mathf.Min(zoomParams[3], 3 - maxX);
            }
            else
            {
                zoomParams[0] = Mathf.Min(zoomParams[0], minY + 3.5f);
                zoomParams[1] = Mathf.Min(zoomParams[1], 5.5f - maxY);
                zoomParams[2] = Mathf.Min(zoomParams[2], minX + 4.5f);
                zoomParams[3] = Mathf.Min(zoomParams[3], 2.5f - maxX);
            }
            Debug.Log($"zoomParams {zoomParams[0]} {zoomParams[1]} {zoomParams[2]} {zoomParams[3]}");
            if (zoomies > Mathf.Min(zoomParams))
                zoomies = Mathf.Min(zoomParams);

            //if (GameManager.instance.tileSpawnToggle.isOn)
            //    waitTimeToStart += 0.05f;
            //for (int y = maxY; y >= minY; y--)
            //{
            //    for (int x = minX; x <= maxX; x++)
            //    {
            //
            //    }
            //}
            foreach (var v2 in v2List)
            {
                floorCount = i;
                //if (!GameManager.instance.tileSpawnToggle.isOn)
                waitTimeToStart += 0.01f;
                //else
                //    waitTimeToStart = 1 + timeFloorSpawn * floorCount;
                indexOnMap++;
                ItemTile item;
                if (gameModeTileIndexes.Contains(indexOnMap))
                {
                    switch (GameConfig.gameMode)
                    {
                        case GAME_MODE.tile_random:
                            //item = Instantiate(itemRandomVariant, floorTransformList[i]);
                            item = PoolManager.instance.GetObject<ItemTile>(itemRandomPrefab, floorTransformList[i]);
                            RandomTileVariant.randomTilesList.Add(item.GetComponent<RandomTileVariant>());
                            break;
                        case GAME_MODE.tile_freeze:
                            //item = Instantiate(itemIceVariant, floorTransformList[i]);
                            item = PoolManager.instance.GetObject<ItemTile>(itemIcePrefab, floorTransformList[i]);
                            item.GetComponent<IceTileVariant>().SetCounter();
                            break;
                        default:
                            //item = Instantiate(itemGameObject, floorTransformList[i]);
                            item = PoolManager.instance.GetObject<ItemTile>(itemPrefab, floorTransformList[i]);
                            break;
                    }
                }
                else if (doubleParams.Contains(new Vector3Int(v2.x, v2.y, i)))
                {
                    Debug.Log("doubleParam " + new Vector3Int(v2.x, v2.y, i));
                    //item = Instantiate(itemLinkVariant, floorTransformList[i]);
                    item = PoolManager.instance.GetObject<ItemTile>(itemLinkPrefab, floorTransformList[i]);
                    item.GetComponent<LinkTileVariant>().LinkTileInit(doubleParams.IndexOf(new Vector3Int(v2.x, v2.y, i)), floorTransformList[i], new Vector2(v2.x, v2.y));
                }
                else
                {
                    if (GameConfig.gameMode == GAME_MODE.tile_gold)
                        //item = Instantiate(itemGoldVariant, floorTransformList[i]);
                        item = PoolManager.instance.GetObject<ItemTile>(itemGoldPrefab, floorTransformList[i]);
                    else
                        //item = Instantiate(itemGameObject, floorTransformList[i]);
                        item = PoolManager.instance.GetObject<ItemTile>(itemPrefab, floorTransformList[i]);
                }
                if (GameConfig.gameMode != GAME_MODE.tile_gold && eventTileIndexes.Contains(indexOnMap))
                {
                    item.SetEventTile();
                }
                item.InitItemTile(i + 1, indexOnMap, new Vector2(v2.x, v2.y));
                itemTiles.Add(item);
            }
        }
        SetCameraOrthoSize();
        //StartCoroutine(SetCameraOrthoSize());
        GenerateTiles();
    }

    void EnsureTileEffects()
    {
        if (effectsParent == null)
        {
            Transform existing = transform.Find("Tile Effects");
            if (existing != null)
            {
                effectsParent = existing;
            }
            else
            {
                GameObject effectRoot = new("Tile Effects");
                effectRoot.transform.SetParent(transform, false);
                effectsParent = effectRoot.transform;
            }
        }

        if (tileTapParticlePool == null)
            tileTapParticlePool = GetComponentInChildren<TileTapParticlePool>(true);
        if (tileTapParticlePool == null)
        {
            GameObject tapFxObject = new("Tile Tap Particle Pool");
            tapFxObject.transform.SetParent(effectsParent, false);
            tileTapParticlePool = tapFxObject.AddComponent<TileTapParticlePool>();
        }
        tileTapParticlePool.SetParticlePrefab(tileTapParticlePrefab);

        if (tripleTileMergeFx == null)
            tripleTileMergeFx = GetComponentInChildren<TripleTileMergeFx>(true);
        if (tripleTileMergeFx == null)
        {
            GameObject mergeFxObject = new("Triple Tile Merge Fx");
            mergeFxObject.transform.SetParent(effectsParent, false);
            tripleTileMergeFx = mergeFxObject.AddComponent<TripleTileMergeFx>();
        }
        tripleTileMergeFx.SetBoomParticlePrefab(tripleMergeBoomParticlePrefab);

        CaptureSlotParentBaseScale();
    }

    public void PlayTileTapEffect(Vector3 worldPosition)
    {
        EnsureTileEffects();
        tileTapParticlePool?.PlayAt(worldPosition);
    }

    public void BounceTray(int tileCount = 1)
    {
        if (slotParent == null)
            return;

        trayBounceTween?.Kill(false);
        ResetTrayScale();

        trayBounceTween = DOTween.Sequence()
            .Append(slotParent.DOPunchScale(Vector3.one * Mathf.Clamp(tileCount, 1, 3) * 0.035f, 0.18f, 6, 0.55f))
            .Append(slotParent.DOScale(slotParentBaseScale, 0.08f))
            .OnKill(ResetTrayScale)
            .OnComplete(ResetTrayScale);
    }

    void CaptureSlotParentBaseScale()
    {
        if (slotParent == null)
            return;

        if (trayBounceTween != null && trayBounceTween.IsActive())
            return;

        slotParentBaseScale = slotParent.localScale;
    }

    void ResetTrayScale()
    {
        if (slotParent == null)
            return;

        slotParent.localScale = slotParentBaseScale;
        trayBounceTween = null;
    }

    public float GetTileAppearDelay(int appearIndex)
    {
        return Mathf.Min(Mathf.Max(0, appearIndex) * tileAppearDelayStep, maxTileAppearDelay);
    }

    [Button]
    public void PlaySelectedTileAppearAnimation()
    {
        RandomTileAppearAnimation();

        int appearIndex = 0;
        foreach (ItemTile tile in itemTiles)
        {
            if (!CanPlayTileAppearAnimation(tile))
                continue;

            tile.PlayTileAppearAnimation(GetTileAppearDelay(appearIndex));
            appearIndex++;
        }
    }

    bool CanPlayTileAppearAnimation(ItemTile tile)
    {
        return tile != null
            && tile.gameObject.activeInHierarchy
            && tile.state != ITEMTILE_STATE.SLOTTED
            && tile.state != ITEMTILE_STATE.DONE
            && (slottedItems == null || !slottedItems.Contains(tile));
    }

    void RandomTileAppearAnimation()
    {
        Array values = Enum.GetValues(typeof(TILE_APPEAR_ANIM));
        tileAppearAnim = (TILE_APPEAR_ANIM)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        Debug.Log($"Tile appear animation: {tileAppearAnim}");
    }
    bool RestoreProgress(GameplayProgressData data, Dictionary<int, List<Vector2Int>> tilemaps)
    {
        if (data.level != GameConfig.level || data.tiles == null || data.tiles.Count == 0)
            return false;
        if (data.gameMode != (int)GameConfig.gameMode)
            return false;
        if (GameConfig.gameMode == GAME_MODE.tile_gold && !data.tiles.Any(tile => tile.tileType == -1))
            return false;

        parameter = data.parameter;
        itemTypeCount = data.itemTypeCount;
        gameMode = (GAME_MODE)data.gameMode;
        GameConfig.gameMode = gameMode;
        isGoldTile = data.isGoldTile && EventManager.instance.IsCurrentLevelEvent();
        isComboEvent = data.isComboEvent && EventManager.instance.IsComboEvent();
        clearAnimPlay = data.clearAnimPlay;
        GameTextScript.comboCount = data.comboCount;
        EventTileExtend.eventCounter = isGoldTile ? data.eventCounter : 0;
        EventTileExtend.eventTotal = isGoldTile ? data.eventTotal : 0;
        GameScript.instance.HideEventCounters();
        if (isGoldTile)
            GameScript.instance.GoldTileCounter();
        if (isComboEvent)
            GameScript.instance.ComboCounter();
        if (!isGoldTile && !isComboEvent && EventManager.instance.isEvent && EventManager.eventType == EventType.streak_event_1)
            GameScript.instance.EventGoalCounter();

        RestoreItemSprites(data);
        CalculateZoom(tilemaps);

        Dictionary<ItemTile, GameplayTileProgressData> tileData = new();
        foreach (var tileProgress in data.tiles.OrderBy(x => x.sortingIndex))
        {
            ItemTile item = PoolManager.instance.GetObject<ItemTile>(GetPrefabForProgress(tileProgress.prefabType, gameMode), floorTransformList[Mathf.Max(tileProgress.floor - 1, 0)]);
            item.InitItemTile(tileProgress.floor, tileProgress.sortingIndex, new Vector2(tileProgress.tilePosX, tileProgress.tilePosY));
            item.transform.DOKill();
            item.sprites.transform.DOKill();
            item.transform.localScale = Vector3.one;
            item.sprites.transform.localScale = Vector3.one;
            item.sprites.transform.localPosition = Vector3.zero;
            item.sprites.transform.localRotation = Quaternion.identity;
            itemTiles.Add(item);
            tileData[item] = tileProgress;
        }

        RestoreLinkedTiles(tileData);
        foreach (var kvp in tileData)
            ApplyTileProgress(kvp.Key, kvp.Value);

        foreach (var slottedTile in tileData.Keys
            .Where(tile => IsValidSlotIndex(tileData[tile].slottedIndex))
            .OrderBy(tile => tileData[tile].slottedIndex))
        {
            slottedItems.Add(slottedTile);
        }

        SnapSlottedTilesToSlots();

        undoList.Clear();
        foreach (int sortingIndex in data.undoSortingIndexes)
        {
            ItemTile undoTile = itemTiles.Find(x => x.sortingIndex == sortingIndex);
            if (undoTile != null && slottedItems.Contains(undoTile))
                undoList.Add(undoTile);
        }

        return true;
    }

    void SnapSlottedTilesToSlots()
    {
        if (slottedItems == null || slotTransformList == null)
            return;

        int count = Mathf.Min(slottedItems.Count, ActiveSlotTransformCount, BaseSlotCount);
        for (int i = 0; i < count; i++)
        {
            ItemTile tile = slottedItems[i];
            if (tile == null)
                continue;

            tile.ForceSnapToSlot(i);
        }
    }

    IEnumerator SnapSlottedTilesAfterRestoreFrame()
    {
        float endTime = Time.time + 1f;
        while (Time.time < endTime)
        {
            SnapSlottedTilesToSlots();
            yield return null;
        }

        yield return new WaitForFixedUpdate();
        SnapSlottedTilesToSlots();
    }

    bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0
            && slotIndex < ActiveSlotTransformCount
            && GetSlotTransform(slotIndex) != null;
    }

    public Transform GetSlotTransform(int slotIndex)
    {
        if (slotTransformList == null || slotIndex < 0)
            return null;

        if (slotIndex >= slotTransformList.Count)
            return null;

        return slotTransformList[slotIndex];
    }

    void RestoreItemSprites(GameplayProgressData data)
    {
        if (data.itemSpriteIndexes == null || data.itemSpriteIndexes.Count == 0)
            return;

        List<Sprite> allSprites = LevelManager.instance.tileSpritesSO.GetAllSprites();
        List<Sprite> restoredSprites = new();
        foreach (int spriteIndex in data.itemSpriteIndexes)
        {
            if (spriteIndex >= 0 && spriteIndex < allSprites.Count)
                restoredSprites.Add(allSprites[spriteIndex]);
        }

        if (restoredSprites.Count > 0)
            itemSpritesUsing = restoredSprites;
    }

    void CalculateZoom(Dictionary<int, List<Vector2Int>> tilemaps)
    {
        zoomies = 10;
        foreach (KeyValuePair<int, List<Vector2Int>> kvp in tilemaps)
        {
            int i = kvp.Key;
            List<Vector2Int> v2List = kvp.Value;
            if (v2List == null || v2List.Count <= 0)
                continue;

            var (minX, maxX, minY, maxY) = Bound(v2List);
            float[] zoomParams = new float[] { 10, 10, 10, 10 };
            if (i % 2 == 0)
            {
                zoomParams[0] = Mathf.Min(zoomParams[0], minY + 3);
                zoomParams[1] = Mathf.Min(zoomParams[1], 6 - maxY);
                zoomParams[2] = Mathf.Min(zoomParams[2], minX + 4);
                zoomParams[3] = Mathf.Min(zoomParams[3], 3 - maxX);
            }
            else
            {
                zoomParams[0] = Mathf.Min(zoomParams[0], minY + 3.5f);
                zoomParams[1] = Mathf.Min(zoomParams[1], 5.5f - maxY);
                zoomParams[2] = Mathf.Min(zoomParams[2], minX + 4.5f);
                zoomParams[3] = Mathf.Min(zoomParams[3], 2.5f - maxX);
            }
            if (zoomies > Mathf.Min(zoomParams))
                zoomies = Mathf.Min(zoomParams);
        }
    }

    PoolObject GetPrefabForProgress(int prefabType, GAME_MODE restoredGameMode)
    {
        if (restoredGameMode == GAME_MODE.tile_gold)
            return itemGoldPrefab;

        return prefabType switch
        {
            1 => itemRandomPrefab,
            2 => itemLinkPrefab,
            3 => itemIcePrefab,
            4 => itemGoldPrefab,
            _ => itemPrefab,
        };
    }

    void RestoreLinkedTiles(Dictionary<ItemTile, GameplayTileProgressData> tileData)
    {
        var linkGroups = tileData
            .Where(kvp => kvp.Key.TryGetComponent(out LinkTileVariant _) && !kvp.Value.linkUnlinked && kvp.Value.slottedIndex < 0)
            .GroupBy(kvp => kvp.Value.linkKey);

        foreach (var group in linkGroups)
        {
            var linkedTiles = group.OrderBy(kvp => kvp.Value.sortingIndex).ToList();
            if (linkedTiles.Count != 2)
            {
                foreach (var kvp in linkedTiles)
                    kvp.Key.GetComponent<LinkTileVariant>().unLinked = true;
                continue;
            }

            int startIndex = LinkTileVariant.linkTilesList.Count;
            LinkTileVariant.linkTilesList.Add(null);
            LinkTileVariant.linkTilesList.Add(null);
            for (int i = 0; i < linkedTiles.Count; i++)
            {
                var tileProgress = linkedTiles[i].Value;
                linkedTiles[i].Key.GetComponent<LinkTileVariant>().LinkTileInit(startIndex + i, floorTransformList[tileProgress.floor - 1], new Vector2(tileProgress.tilePosX, tileProgress.tilePosY));
            }
        }
    }

    void ApplyTileProgress(ItemTile item, GameplayTileProgressData tileProgress)
    {
        if (tileProgress.isEventTile && isGoldTile && GameConfig.gameMode != GAME_MODE.tile_gold)
        {
            item.SetEventTile();
            item.eventTileExtend.RestoreCounter(tileProgress.eventCounter);
        }
        else if (tileProgress.isRainbow)
        {
            item.SetRainbow();
        }
        else
        {
            item.SetType(GetRestoredTileType(tileProgress.tileType));
        }

        item.is2Up = tileProgress.is2Up;
        item.floor = tileProgress.floor;
        item.sortingIndex = tileProgress.sortingIndex;
        item.tilePos = new Vector2(tileProgress.tilePosX, tileProgress.tilePosY);
        item.state = (ITEMTILE_STATE)tileProgress.state;

        if (item.TryGetComponent(out RandomTileVariant randomTile))
            randomTile.RestoreRevealed(tileProgress.randomRevealed);
        if (item.TryGetComponent(out IceTileVariant iceTile))
            iceTile.RestoreCounter(tileProgress.iceCounter);
        if (item.TryGetComponent(out LinkTileVariant linkTile) && (tileProgress.linkUnlinked || tileProgress.slottedIndex >= 0))
            linkTile.unLinked = true;

        bool isSlotted = IsValidSlotIndex(tileProgress.slottedIndex);
        Transform parent = isSlotted ? GetSlotTransform(tileProgress.slottedIndex) : floorTransformList[tileProgress.floor - 1];
        item.transform.SetParent(parent, false);
        item.transform.localScale = Vector3.one;
        item.transform.localPosition = isSlotted ? Vector3.zero : new Vector3(tileProgress.tilePosX, tileProgress.tilePosY, tileProgress.sortingIndex / -10f);
        item.sprites.transform.localScale = Vector3.one;
        item.sprites.transform.localPosition = Vector3.zero;
        item.sprites.transform.localRotation = Quaternion.identity;
        item.itemCollider.enabled = !isSlotted && item.state == ITEMTILE_STATE.TOUCHABLE;
        item.shadow.gameObject.SetActive(!isSlotted && item.state == ITEMTILE_STATE.HIDDEN);
        item.glow.gameObject.SetActive(item.is2Up);
        item.SetSortingLayer(isSlotted ? "Slotted" : "Floor" + tileProgress.floor);
        item.SetSortingOrder(isSlotted ? tileProgress.slottedIndex : tileProgress.sortingIndex);
        if (!isSlotted && item.TryGetComponent(out LinkTileVariant restoredLink) && !restoredLink.unLinked && restoredLink.linkedTile != null &&
            item.state == ITEMTILE_STATE.TOUCHABLE && restoredLink.linkedTile.state == ITEMTILE_STATE.TOUCHABLE)
        {
            restoredLink.SetState();
        }
    }

    int GetRestoredTileType(int tileType)
    {
        if (GameConfig.gameMode == GAME_MODE.tile_gold && tileType == -1)
            return -1;

        if (tileType >= 0 || itemSpritesUsing == null || itemSpritesUsing.Count == 0)
            return tileType;

        return Mathf.Abs(tileType) % itemSpritesUsing.Count;
    }

    IEnumerator WaitRestoredProgress()
    {
        SnapSlottedTilesToSlots();
        StartCoroutine(SnapSlottedTilesAfterRestoreFrame());
        RefreshRestoredTileStates();
        yield return new WaitForFixedUpdate();
        SnapSlottedTilesToSlots();
        foreach (ItemTile tile in itemTiles)
        {
            if (!slottedItems.Contains(tile))
                tile.shadow.gameObject.SetActive(tile.state == ITEMTILE_STATE.HIDDEN);
        }
        Danger();
        StartCoroutine(WinCoroutine());
        GameConfig.SetActionTime(GameConfig.startLevelKey);
        if (GameConfig.gameMode == GAME_MODE.tile_freeze)
            StartCoroutine(CheckFullFreeze(true));
        GameScript.instance.PowerUpsInteractable();
        gameState = GAME_STATE.PLAYING;
        SaveProgress();
    }

    void RefreshRestoredTileStates()
    {
        foreach (ItemTile tile in itemTiles)
        {
            if (slottedItems.Contains(tile))
                continue;

            tile.sprites.transform.DOKill();
            tile.sprites.transform.localScale = Vector3.one;
            tile.sprites.transform.localPosition = Vector3.zero;
            tile.sprites.transform.localRotation = Quaternion.identity;
            tile.SetSortingLayer("Floor" + tile.floor);
            tile.SetSortingOrder(tile.sortingIndex);
            tile.SetState();
        }
    }
    //private void OnRectTransformDimensionsChange()
    //{
    //    SetCameraOrthoSize();
    //}
    [Button]
    public void SetCameraOrthoSize()
    {
        Debug.Log("zoomies = " + zoomies);
        float zoom = 10 - zoomies * 2;
        Debug.Log("zoomies = " + zoom);
        gameParent.localPosition = new Vector3(0, 6, 0);
        float legacyScale = 10 / zoom;
        floorParent.localScale = Vector3.one * legacyScale;
        slotParent.localPosition = Vector3.zero;
        floorParent.localPosition = new Vector3(0, -8 - 2 * (legacyScale - 1), 0);
        float cameraSize = zoom <= 10 ? 9 : (zoom - 1) * (10 / zoom);
        Debug.Log("zoomcamsize = " + cameraSize);
        Camera.main.orthographicSize = Mathf.Max((float)Screen.height / Screen.width * cameraSize / 2, cameraSize);
        FitBoardToPlayArea(legacyScale);
        ResetSevenSlotBoard();
        CaptureSlotParentBaseScale();
        //slotParent.gameObject.SetActive(true);
    }

    void FitBoardToPlayArea(float fallbackScale)
    {
        Camera cam = Camera.main;
        if (cam == null || gameParent == null || floorParent == null || itemTiles == null || itemTiles.Count == 0)
            return;

        Bounds boardBounds = GetBoardLocalBounds();
        if (boardBounds.size.x <= 0 || boardBounds.size.y <= 0)
            return;

        float slotBottomScreenY = GetSevenSlotsBottomScreenY(cam);
        float bottomTopScreenY = GetBottomTopScreenY(cam);

        float areaTopScreenY = slotBottomScreenY - slotBottomPaddingPixels;
        float areaBottomScreenY = bottomTopScreenY + bottomTopPaddingPixels;
        if (areaTopScreenY <= areaBottomScreenY)
            return;

        float areaLeftScreenX = playAreaHorizontalPaddingPixels;
        float areaRightScreenX = Screen.width - playAreaHorizontalPaddingPixels;
        if (areaRightScreenX <= areaLeftScreenX)
            return;

        Vector3 areaBottomLeftWorld = ScreenToBoardPlaneWorld(cam, new Vector2(areaLeftScreenX, areaBottomScreenY));
        Vector3 areaTopRightWorld = ScreenToBoardPlaneWorld(cam, new Vector2(areaRightScreenX, areaTopScreenY));
        float areaWidth = Mathf.Abs(areaTopRightWorld.x - areaBottomLeftWorld.x);
        float areaHeight = Mathf.Abs(areaTopRightWorld.y - areaBottomLeftWorld.y);
        if (areaWidth <= 0 || areaHeight <= 0)
            return;

        float maxAllowedScale = Mathf.Min(areaWidth / boardBounds.size.x, areaHeight / boardBounds.size.y);
        if (float.IsNaN(maxAllowedScale) || float.IsInfinity(maxAllowedScale) || maxAllowedScale <= 0)
            maxAllowedScale = fallbackScale;

        Vector3 appliedScale = Vector3.one * maxAllowedScale;
        floorParent.localScale = appliedScale;

        Vector3 areaCenterWorld = (areaBottomLeftWorld + areaTopRightWorld) * 0.5f;
        Vector3 areaCenterInGame = gameParent.InverseTransformPoint(areaCenterWorld);
        floorParent.localPosition = areaCenterInGame - Vector3.Scale(boardBounds.center, appliedScale);
    }

    Bounds GetBoardLocalBounds()
    {
        bool hasBounds = false;
        Bounds bounds = new(Vector3.zero, Vector3.zero);

        foreach (ItemTile tile in itemTiles)
        {
            if (tile == null || tile.state == ITEMTILE_STATE.SLOTTED || tile.state == ITEMTILE_STATE.DONE)
                continue;

            Vector3 center = GetTileStableCenterInFloorParent(tile);
            Bounds tileBounds = new(center, Vector3.one * boardTilePadding * 2f);
            if (!hasBounds)
            {
                bounds = tileBounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(tileBounds);
            }
        }

        if (!hasBounds)
            bounds = new(Vector3.zero, new Vector3(1f, 1f, 0f));

        return bounds;
    }

    Vector3 GetTileStableCenterInFloorParent(ItemTile tile)
    {
        Transform tileFloor = null;
        int floorIndex = tile.floor - 1;
        if (floorTransformList != null && floorIndex >= 0 && floorIndex < floorTransformList.Count)
            tileFloor = floorTransformList[floorIndex];

        if (tileFloor == null)
            tileFloor = tile.transform.parent;

        if (tileFloor == null || floorParent == null)
            return new Vector3(tile.tilePos.x, tile.tilePos.y, 0f);

        Vector3 stableWorldPosition = tileFloor.TransformPoint(new Vector3(tile.tilePos.x, tile.tilePos.y, 0f));
        return floorParent.InverseTransformPoint(stableWorldPosition);
    }

    float GetSevenSlotsBottomScreenY(Camera cam)
    {
        if (slotTransformList == null || slotTransformList.Count == 0)
            return Screen.height;

        float bottom = Screen.height;
        int count = Mathf.Min(BaseSlotCount, slotTransformList.Count);
        for (int i = 0; i < count; i++)
        {
            Transform slot = GetSlotTransform(i);
            if (slot == null)
                continue;

            Renderer renderer = slot.GetComponentInChildren<Renderer>(true);
            if (renderer != null && renderer.bounds.size.y > 0f)
            {
                bottom = Mathf.Min(bottom, cam.WorldToScreenPoint(renderer.bounds.min).y);
                continue;
            }

            float halfHeight = 0.6f * Mathf.Abs(slot.lossyScale.y);
            bottom = Mathf.Min(bottom, cam.WorldToScreenPoint(slot.position + Vector3.down * halfHeight).y);
        }

        return bottom;
    }

    float GetBottomTopScreenY(Camera cam)
    {
        RectTransform rect = GetBottomRect();
        if (rect == null)
            return 0f;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        float top = 0f;
        for (int i = 0; i < corners.Length; i++)
            top = Mathf.Max(top, RectTransformUtility.WorldToScreenPoint(uiCamera, corners[i]).y);

        return top;
    }

    RectTransform GetBottomRect()
    {
        if (bottomRect != null)
            return bottomRect;

        GameObject bottomObject = GameObject.Find("Bottom");
        if (bottomObject == null)
            return null;

        bottomRect = bottomObject.GetComponent<RectTransform>();
        return bottomRect;
    }

    Vector3 ScreenToBoardPlaneWorld(Camera cam, Vector2 screenPosition)
    {
        float zDistance = Mathf.Abs(cam.transform.position.z - gameParent.position.z);
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, zDistance));
        world.z = gameParent.position.z;
        return world;
    }
    void ResetSevenSlotBoard()
    {
        GameConfig.isExtraSlot = false;
        slotsAvailable.Clear();
        for (int i = 0; i < BaseSlotCount; i++)
            slotsAvailable.Add(i);

        if (extraSlotScript)
            extraSlotScript.SetIsExtra(false, false);
    }
    public void GenerateTiles()
    {
        Debug.Log("item count " + itemTiles.Count);
        int mod = itemTiles.Count % 3;
        if (mod != 0)
        {
            Debug.LogError("Insufficient tile count");
            for (int i = 0; i < mod; i++)
            {
                ItemTile item = itemTiles[^1];
                itemTiles.Remove(item);
                PoolManager.instance.ReleaseObject(item);
            }
        }
        Debug.Log("item count " + itemTiles.Count);
        List<ItemTile> itemList_temp = new(itemTiles);
        bool isEvent = false;
        if (EventTileExtend.eventTileList.Count > 0)
        {
            isEvent = true;
            foreach (var eventTile in EventTileExtend.eventTileList)
            {
                itemList_temp.Remove(eventTile);
                itemTiles.Remove(eventTile);
            }
        }
        List<ItemTile> itemList_shuffle = new();

        for (int i = 0; i < itemTiles.Count; i++)
        {
            ItemTile item = itemList_temp[Random.Range(0, itemList_temp.Count)];
            itemList_temp.Remove(item);
            itemList_shuffle.Add(item);
        }
        if (GameConfig.gameMode == GAME_MODE.tile_rainbow)
        {
            List<ItemTile> rainbowTiles = itemList_shuffle.Take(parameter * 3).ToList();
            itemList_shuffle.RemoveAll(x => rainbowTiles.Contains(x));
            foreach (var rb in rainbowTiles)
                rb.SetRainbow();
        }
        List<ItemTile> notGoldTiles = new();
        bool overflow = itemTypeCount > itemSpritesUsing.Count;
        for (int i = 0; i < itemList_shuffle.Count; i++)
        {
            if (GameConfig.gameMode != GAME_MODE.tile_gold)
            {
                if (!overflow)
                {
                    int itemType = (i / 3) % itemTypeCount;
                    itemList_shuffle[i].SetType(itemType);
                }
                else
                    notGoldTiles.Add(itemList_shuffle[i]);
            }
            else
            {
                int itemType = (i / 3) % itemTypeCount;
                if (itemType != 0)
                    itemList_shuffle[i].SetType(-1);
                else
                    notGoldTiles.Add(itemList_shuffle[i]);
            }
            //itemList_shuffle[i].SetBG();
        }
        if (notGoldTiles.Count > 0)
        {
            for (int i = 0; i < notGoldTiles.Count; i++)
            {
                Debug.Log(i / 3);
                Debug.Log(itemSpritesUsing.Count);
                int itemType = (i / 3) % itemSpritesUsing.Count;
                notGoldTiles[i].SetType(itemType);
            }
        }
        if (isEvent)
            itemTiles.AddRange(EventTileExtend.eventTileList);
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTimeToStart);
        foreach (ItemTile tile in itemTiles)
            if (!slottedItems.Contains(tile))
                tile.SetState();
        yield return new WaitForFixedUpdate();
        foreach (ItemTile tile in itemTiles)
        {
            tile.shadow.gameObject.SetActive(tile.state == ITEMTILE_STATE.HIDDEN);
        }
        Danger();
        StartCoroutine(WinCoroutine());
        yield return null;
        //if toggle ui
        //GamePlayScript.instance.cheatUIGO.SetActive(GameFunc.cheatEnabled);
        StartPlaying();
    }
    void StartPlaying()
    {
        //if (Config.gameMode == GAME_MODE.timeCD ||
        //if (Config.gameMode == GAME_MODE.tile_gold)
        //    timerCDScript.StartCountdown();
        //else if (Config.gameMode == GAME_MODE.tile_snail)
        //    StartCoroutine(SnailScript.instance.Snail(true));
        //else 
        GameConfig.SetActionTime(GameConfig.startLevelKey);

        bool skipPlayCheck = false;
        if (GameConfig.gameMode == GAME_MODE.tile_freeze)
            StartCoroutine(CheckFullFreeze(true));
        if (GameConfig.gameMode == GAME_MODE.tile_2up)
        {
            skipPlayCheck = true;
            StartCoroutine(Up2());
        }
        //Config.OnItemThemeChange += OnThemeChange;

        bool interStartLevel = GameConfig.InterStartLevel();
        if (!GameConfig.isChallengeLevel)
        {
            FirebaseManager.instance.LogLevelStart(GameConfig.level);
            int diff = LevelManager.instance.GetLevelDifficulty(GameConfig.level);
            if (PlayerPrefs.GetInt("tutorial_" + GameConfig.gameMode, 0) == 0)
            {
                //gameState = GAME_STATE.PAUSE;
                PlayerPrefs.SetInt("tutorial_" + GameConfig.gameMode, 1);
                GameScript.instance.OpenTutorial(GameConfig.gameMode);
                skipPlayCheck = true;
            }
            else if (diff != 0)
            {
                GameScript.instance.OpenHardPopup(diff);
                skipPlayCheck = true;
            }
            else if (isGoldTile && EventManager.GetEventTutorial())
            {
                EventManager.SetEventTutorial();
                GameScript.instance.OnStartGameEventTutorial();
                skipPlayCheck = true;
            }
            else if (isComboEvent && EventManager.GetEventTutorial())
            {
                EventManager.SetEventTutorial();
                GameScript.instance.OnStartGameEventTutorial();
                skipPlayCheck = true;
            }
            else
            {
                //if (Config.InterStartLevel() && Config.InterAvailable())
                if (interStartLevel && GameConfig.InterAvailable())
                {
                    GameConfig.ShowInterstitialAd();
                }
            }
        }
        else
        {
            //Config.InterStartLevel();
            if (PlayerPrefs.GetInt("tutorial_" + GameConfig.gameMode, 0) == 0)
            {
                //gameState = GAME_STATE.PAUSE;
                PlayerPrefs.SetInt("tutorial_" + GameConfig.gameMode, 1);
                GameScript.instance.OpenTutorial(GameConfig.gameMode);
                skipPlayCheck = true;
            }
        }
        GameScript.instance.PowerUpsInteractable();
        Debug.Log("skipPlayCheck " + skipPlayCheck);
        SaveProgress();
        if (skipPlayCheck)
            return;
        gameState = GAME_STATE.PLAYING;
    }
    IEnumerator WinCoroutine()
    {
        yield return new WaitUntil(() => itemTiles.Count == 0);
        gameState = GAME_STATE.WIN;
        //gameTextScript.Clear();
        Instantiate(clearAnim, clearAnim.transform.parent).SetActive(true);
        yield return new WaitForSeconds(1.05f);
        if (!GameConfig.isChallengeLevel)
            GameScript.instance.Win();
        else
            GameScript.instance.WinChallenge();
        //if (!Config.isChallengeLevel)
        //    GameUI.instance.OnWin();
        //else
        //    GameUI.instance.OnWinChallenge();
    }
    #endregion

    int powerAd;
    public void DonePowerAd()
    {
        switch (powerAd)
        {
            case 0:
                UndoPower();
                break;
            case 1:
                SuggestPower(SuggestItem());
                break;
            case 2:
                ShufflePower();
                break;
            case 3:
                break;
        }
    }
    #region undo
    public void OnClickUndo()
    {
        if (gameState != GAME_STATE.PLAYING)
            return;
        //if (Config.GetTutorial() == 1 && GameController.instance.gameState == GAME_STATE.TUTORIAL)
        //{
        //    undoList[^1].UndoMove();
        //    Config.SetTutorial(2);
        //    LevelPopup.instance.tutorialPowerPopup.ExitTutorialPower();
        //    return;
        //}
        GameScript.instance.EndItemsAnim();
        if (!UIClickable || match3Items.Count != 0 || undoList.Count <= 0)
            return;
        if (GameConfig.UsePowers(0))
        {
            UndoPower();
        }
        else
        {
            if (GameConfig.UseGold(GameConfig.powerPrices[0]))
            {
                UndoPower();
            }
            else
            {
                powerAd = 0;
                GameConfig.ShowRewardedAd(0);
            }
        }
    }
    public void UndoPower()
    {
        undoList[^1].UndoMove();
        QuestManager.QuestComplete(QUEST_TYPE.USE_X_UNDOS, 1);
    }
    #endregion

    #region shuffle
    public void OnClickShuffle()
    {
        if (gameState != GAME_STATE.PLAYING)
            return;
        GameScript.instance.EndItemsAnim();
        if (!UIClickable || match3Items.Count != 0)
            return;
        if (GameConfig.UsePowers(2))
        {
            ShufflePower();
        }
        else
        {
            if (GameConfig.UseGold(GameConfig.powerPrices[2]))
            {
                ShufflePower();
            }
            else
            {
                powerAd = 2;
                GameConfig.ShowRewardedAd(0);
            }
        }
    }
    public void ShufflePower()
    {
        gameState = GAME_STATE.SHUFFLE;
        foreach (ItemTile tile in itemTiles)
        {
            if (tile.state == ITEMTILE_STATE.SLOTTED)
                continue;
            tile.state = ITEMTILE_STATE.START;
            tile.InitItemTile(tile.floor, tile.sortingIndex, tile.tilePos);
        }
        ShuffleItems();
        QuestManager.QuestComplete(QUEST_TYPE.USE_X_SHUFFLE, 1);
    }
    public void ShuffleItems()
    {
        List<ItemTile> itemList_temp = new(itemTiles);
        if (gameState == GAME_STATE.SHUFFLE)
            itemList_temp.RemoveAll(x => slottedItems.Contains(x));
        itemList_temp.RemoveAll(x => x.IsEventTile);

        Dictionary<int, int> itemTypeCount = new();
        List<int> itemTypeList = new();
        //itemTypeCount.Keys.ToList();
        foreach (ItemTile item in itemList_temp)
        {
            if (itemTypeCount.ContainsKey(item.tileType))
                itemTypeCount[item.tileType] += 1;
            else
            {
                itemTypeCount.Add(item.tileType, 1);
                itemTypeList.Add(item.tileType);
            }
        }
        List<ItemTile> itemList_shuffle = new();
        int listCount = itemList_temp.Count;
        for (int i = 0; i < listCount; i++)
        {
            ItemTile item = itemList_temp[Random.Range(0, itemList_temp.Count)];
            itemList_temp.Remove(item);
            itemList_shuffle.Add(item);
        }
        Debug.Log(itemTypeList);
        for (int i = 0; i < itemList_shuffle.Count; i++)
        {
            int itemType = itemTypeList[Random.Range(0, itemTypeList.Count)];
            itemList_shuffle[i].SetType(itemType);
            itemTypeCount[itemType] -= 1;

            Debug.Log($"{itemType}, {itemTypeCount[itemType]}");
            if (itemTypeCount[itemType] == 0)
                itemTypeList.Remove(itemType);
        }
        Debug.Log(itemTypeList);
        StartCoroutine(DoneShuffle());
    }
    public IEnumerator DoneShuffle()
    {
        yield return new WaitForSeconds(waitTimeToStart);
        foreach (ItemTile tile in itemTiles)
            if (!slottedItems.Contains(tile))
                tile.SetState();
        yield return new WaitForFixedUpdate();
        foreach (ItemTile tile in itemTiles)
        {
            if (!tile.is2Up)
                tile.shadow.gameObject.SetActive(tile.state == ITEMTILE_STATE.HIDDEN);
        }
        if (GameConfig.gameMode == GAME_MODE.tile_2up && !slottedItems.Exists(x => x.is2Up))
            StartCoroutine(Up2());
        GameScript.instance.PowerUpsInteractable();
        gameState = GAME_STATE.PLAYING;
        SaveProgress();
    }
    #endregion

    #region hint/suggest
    public void OnClickSuggest()
    {
        if (gameState != GAME_STATE.PLAYING)
            return;
        GameScript.instance.EndItemsAnim();
        List<ItemTile> suggestTiles;
        try
        {
            suggestTiles = SuggestItem();
            if (suggestTiles.Count <= 0)
            {
                gameState = GAME_STATE.PLAYING;
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            gameState = GAME_STATE.PLAYING;
            return;
        }
        if (!UIClickable || match3Items.Count != 0)
            return;
        try
        {
            if (suggestTiles.Count > 0)
            {
                if (GameConfig.UsePowers(1))
                {
                    SuggestPower(suggestTiles);
                }
                else
                {
                    if (GameConfig.UseGold(GameConfig.powerPrices[1]))
                    {
                        SuggestPower(suggestTiles);
                    }
                    else
                    {
                        powerAd = 1;
                        GameConfig.ShowRewardedAd(0);
                    }
                }
            }
            else
            {
                gameState = GAME_STATE.PLAYING;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            gameState = GAME_STATE.PLAYING;
        }
    }
    public void SuggestPower(List<ItemTile> suggestTiles)
    {
        gameState = GAME_STATE.SUGGEST;
        StartCoroutine(MoveSuggestTiles(suggestTiles));
        QuestManager.QuestComplete(QUEST_TYPE.USE_X_HINT, 1);
    }
    IEnumerator MoveSuggestTiles(List<ItemTile> suggestTiles)
    {
        yield return new WaitForEndOfFrame();

        foreach (var eventTile in EventTileExtend.eventTileList)
        {
            eventTile.eventTileExtend.ReduceCounter();
            yield return null;
        }
        if (TryGetHintMergeTiles(suggestTiles, out List<ItemTile> hintMergeTiles))
        {
            if (GameConfig.gameMode == GAME_MODE.tile_freeze)
            {
                foreach (ItemTile tile in suggestTiles)
                    IceTileVariant.UnfreezeNeighbours(tile);
            }
            yield return Remove3MatchWithHint(hintMergeTiles);
            if (GameConfig.gameMode == GAME_MODE.tile_freeze)
                StartCoroutine(CheckFullFreeze());
            yield return new WaitForSeconds(0.35f);
            gameState = GAME_STATE.PLAYING;
            SaveProgress();
            yield break;
        }
        foreach (ItemTile tile in suggestTiles)
        {
            tile.MoveTileToSlot(true);
            yield return new WaitForSeconds(0.05f);
        }
        if (GameConfig.gameMode == GAME_MODE.tile_freeze)
            StartCoroutine(CheckFullFreeze());
        yield return new WaitForSeconds(1.5f);
        gameState = GAME_STATE.PLAYING;
        SaveProgress();
    }

    bool TryGetHintMergeTiles(List<ItemTile> suggestTiles, out List<ItemTile> mergeTiles)
    {
        mergeTiles = new();
        if (suggestTiles == null || suggestTiles.Count == 0 || suggestTiles.Count > 3)
            return false;

        int tileType = suggestTiles[0].tileType;
        if (tileType < 0)
            return false;

        if (!suggestTiles.TrueForAll(tile => tile != null &&
            tile.tileType == tileType &&
            tile.state != ITEMTILE_STATE.SLOTTED &&
            !slottedItems.Contains(tile)))
            return false;

        List<ItemTile> matchingSlottedTiles = slottedItems
            .Where(tile => tile != null &&
                !match3Items.Contains(tile) &&
                !tile.is2Up &&
                tile.tileType == tileType)
            .ToList();

        mergeTiles.AddRange(matchingSlottedTiles);
        mergeTiles.AddRange(suggestTiles);

        if (mergeTiles.Count != 3 || mergeTiles.Distinct().Count() != 3)
        {
            mergeTiles.Clear();
            return false;
        }

        return true;
    }

    public IEnumerator Remove3MatchWithHint(List<ItemTile> tiles)
    {
        if (tiles == null || tiles.Count == 0)
            yield break;

        foreach (ItemTile tile in tiles)
        {
            if (tile == null)
                continue;

            tile.state = ITEMTILE_STATE.DONE;
            if (tile.itemCollider != null)
                tile.itemCollider.enabled = false;
            if (!match3Items.Contains(tile))
                match3Items.Add(tile);
        }

        undoList.RemoveAll(x => tiles.Contains(x));
        yield return Remove3Match(tiles, true);
    }

    public List<ItemTile> SuggestItem()
    {
        List<ItemTile> itemList_temp = new(itemTiles);
        //if (gameState == GAME_STATE.SHUFFLE)
        itemList_temp.RemoveAll(x => x.state == ITEMTILE_STATE.SLOTTED || slottedItems.Contains(x));
        switch (GameConfig.gameMode)
        {
            case GAME_MODE.tile_random:
                itemList_temp.RemoveAll(x => x.TryGetComponent(out RandomTileVariant random) && !random.isRevealed);
                break;
            case GAME_MODE.tile_freeze:
                itemList_temp.RemoveAll(x => x.TryGetComponent(out IceTileVariant ice) && ice.counter > 0);
                break;
            case GAME_MODE.tile_double:
                itemList_temp.RemoveAll(x => x.TryGetComponent(out LinkTileVariant link) && !link.GetComponent<LinkTileVariant>().unLinked);
                break;
            //case GAME_MODE.tile_snail:
            //    itemList_temp.RemoveAll(x => SnailScript.instance.currentIndex == x.sortingIndex);
            //    break;
            case GAME_MODE.tile_2up:
                int up2Type = slottedItems.Find(x => x.is2Up).tileType;
                itemList_temp.RemoveAll(x => x.tileType == up2Type);
                break;
            case GAME_MODE.tile_rainbow:
                itemList_temp.RemoveAll(x => x.isRainbow);
                break;
            default:
                break;
        }
        itemList_temp.RemoveAll(x => x.IsEventTile);

        itemList_temp.Reverse();
        Dictionary<int, List<ItemTile>> dicItems = new();
        Dictionary<int, List<ItemTile>> dicItems_all = new();
        //List<int> itemTypeList = new();
        //itemTypeCount.Keys.ToList();
        foreach (ItemTile item in itemList_temp)
        {
            if (item.state == ITEMTILE_STATE.TOUCHABLE)
            {
                if (dicItems.ContainsKey(item.tileType))
                    dicItems[item.tileType].Add(item);
                else
                    dicItems.Add(item.tileType, new List<ItemTile> { item });

                if (dicItems_all.ContainsKey(item.tileType))
                    dicItems_all[item.tileType].Add(item);
                else
                    dicItems_all.Add(item.tileType, new List<ItemTile> { item });
            }

            if (item.state == ITEMTILE_STATE.HIDDEN)
            {
                if (dicItems_all.ContainsKey(item.tileType))
                    dicItems_all[item.tileType].Add(item);
                else
                    dicItems_all.Add(item.tileType, new List<ItemTile> { item });
            }
        }

        List<ItemTile> suggestTiles = new();
        if (slottedItems.Count > 0)
        {
            //suggestTiles.Clear();
            List<int> tileType_Checked = new();
            for (int i = 0; i < slottedItems.Count; i++)
            {
                if (slottedItems[i].is2Up)
                    continue;
                int tileType = slottedItems[i].tileType;
                if (tileType < 0)
                    continue;
                if (tileType_Checked.Contains(tileType))
                    break;
                tileType_Checked.Add(tileType);
                //int tileTypeCount
                //TODO: itemTypeCount == 1/2
                int typeCount = slottedItems.FindAll(x => x.tileType == tileType).Count;
                if (typeCount == 1)
                {
                    if (slottedItems.Count <= SlotCount - 2 && dicItems.ContainsKey(tileType))
                    {
                        List<ItemTile> listHaveItem = dicItems[tileType];
                        if (listHaveItem.Count >= 2)
                        {
                            suggestTiles.Add(listHaveItem[0]);
                            suggestTiles.Add(listHaveItem[1]);
                            return suggestTiles;
                        }
                    }
                }
                else if (typeCount == 2)
                {
                    if (slottedItems.Count <= SlotCount - 1 && dicItems.ContainsKey(tileType))
                    {
                        List<ItemTile> listHaveItem = dicItems[tileType];
                        if (listHaveItem.Count >= 1)
                        {
                            suggestTiles.Add(listHaveItem[0]);
                            return suggestTiles;
                        }
                    }
                }
            }
        }
        if (slottedItems.Count <= SlotCount - 3)
        {
            foreach (KeyValuePair<int, List<ItemTile>> kvp in dicItems)
            {
                if (kvp.Value.Count >= 3)
                {
                    suggestTiles.Add(kvp.Value[0]);
                    suggestTiles.Add(kvp.Value[1]);
                    suggestTiles.Add(kvp.Value[2]);
                    return suggestTiles;
                }
            }
        }

        if (slottedItems.Count > 0)
        {
            //suggestTiles.Clear();
            List<int> tileType_Checked = new();
            for (int i = 0; i < slottedItems.Count; i++)
            {
                if (slottedItems[i].is2Up)
                    continue;
                int tileType = slottedItems[i].tileType;
                if (tileType < 0)
                    continue;
                if (tileType_Checked.Contains(tileType))
                    break;
                tileType_Checked.Add(tileType);
                //int tileTypeCount
                //TODO: itemTypeCount == 1/2
                int typeCount = slottedItems.FindAll(x => x.tileType == tileType).Count;
                if (typeCount == 1)
                {
                    if (slottedItems.Count <= SlotCount - 2 && dicItems_all.ContainsKey(tileType))
                    {
                        List<ItemTile> listHaveItem = dicItems_all[tileType];
                        if (listHaveItem.Count >= 2)
                        {
                            suggestTiles.Add(listHaveItem[0]);
                            suggestTiles.Add(listHaveItem[1]);
                            return suggestTiles;
                        }
                    }
                }
                else if (typeCount == 2)
                {
                    if (slottedItems.Count <= SlotCount - 1 && dicItems_all.ContainsKey(tileType))
                    {
                        List<ItemTile> listHaveItem = dicItems_all[tileType];
                        if (listHaveItem.Count >= 1)
                        {
                            suggestTiles.Add(listHaveItem[0]);
                            return suggestTiles;
                        }
                    }
                }
            }
        }
        //else
        //{
        //if (slottedItems.Count >= slotCount - 1)
        //    {
        //        List<int> tileType_Checked = new();
        //        for (int i = 0; i < slottedItems.Count; i++)
        //        {
        //            int tileType = slottedItems[i].tileType;
        //            if (tileType_Checked.Contains(tileType))
        //                break;
        //            tileType_Checked.Add(tileType);

        //            int typeCount = slottedItems.FindAll(x => x.tileType == tileType).Count;
        //            if (typeCount == 2 && dicItems_all.ContainsKey(tileType))
        //            {
        //                List<ItemTile> listHaveItem = dicItems_all[tileType];
        //                if (listHaveItem.Count >= 1)
        //                {
        //                    suggestTiles.Add(listHaveItem[^1]);
        //                    return suggestTiles;
        //                }
        //            }
        //        }
        //        return suggestTiles;
        //    }
        //    else
        //    {
        //        //List<int> tileType_Checked = new();
        //        for (int i = 0; i < slottedItems.Count; i++)
        //        {
        //            int tileType = slottedItems[i].tileType;
        //            //if (tileType_Checked.Contains(tileType))
        //            //    break;
        //            //tileType_Checked.Add(tileType);
        //            int typeCount = slottedItems.FindAll(x => x.tileType == tileType).Count;
        //            if (typeCount == 1 && dicItems_all.ContainsKey(tileType))
        //            {
        //                List<ItemTile> listHaveItem = dicItems_all[tileType];
        //                if (listHaveItem.Count >= 2)
        //                {
        //                    suggestTiles.Add(listHaveItem[^1]);
        //                    suggestTiles.Add(listHaveItem[^2]);
        //                    return suggestTiles;
        //                }
        //            }
        //            else if (typeCount == 2 && dicItems_all.ContainsKey(tileType))
        //            {
        //                List<ItemTile> listHaveItem = dicItems_all[tileType];
        //                if (listHaveItem.Count >= 1)
        //                {
        //                    suggestTiles.Add(listHaveItem[^1]);
        //                    return suggestTiles;
        //                }
        //            }
        //        }
        //        return suggestTiles;
        //    }
        //}

        if (slottedItems.Count <= SlotCount - 3)
        {
            foreach (KeyValuePair<int, List<ItemTile>> kvp in dicItems_all)
            {
                if (kvp.Value.Count >= 3)
                {
                    suggestTiles.Add(kvp.Value[0]);
                    suggestTiles.Add(kvp.Value[1]);
                    suggestTiles.Add(kvp.Value[2]);
                    return suggestTiles;
                }
            }
        }
        return suggestTiles;
    }
    #endregion

    #region addslot
    public void OnClickAddSlot()
    {
    }
    public void SetExtraSlot(bool anim)
    {
        ResetSevenSlotBoard();
        GameScript.instance.PowerUpsInteractable();
        Danger();
        SaveProgress();
    }
    #endregion

    public void OnClickRevive()
    {
        GameScript.instance.EndItemsAnim();
        undoList.Clear();
        slottedItems.Clear();
        gameState = GAME_STATE.SHUFFLE;
        foreach (ItemTile tile in itemTiles)
        {
            if (!slottedItems.Contains(tile))
            {
                tile.is2Up = false;
                tile.state = ITEMTILE_STATE.START;
                tile.InitItemTile(tile.floor, tile.sortingIndex, tile.tilePos);
            }
        }
        clearAnimPlay = false;
        ShuffleItems();
    }
    public IEnumerator CheckFullFreeze(bool isStart = false)
    {
        List<ItemTile> touchableTiles;
        if (isStart)
        {
            yield return new WaitUntil(() => gameState == GAME_STATE.PLAYING);
        }
        touchableTiles = itemTiles.FindAll(x => x.state == ITEMTILE_STATE.TOUCHABLE || x.state == ITEMTILE_STATE.HOVERING);
        bool fullFreeze = true;
        Debug.Log(touchableTiles.Count);
        foreach (var tile in touchableTiles)
        {
            if (!tile.TryGetComponent(out IceTileVariant freezeTile) || freezeTile.Touchable)
            {
                Debug.Log(tile.sortingIndex);
                Debug.Log(tile.state);
                fullFreeze = false;
            }
        }
        Debug.Log("fullFreeze " + fullFreeze);
        if (fullFreeze)
        {
            foreach (var tile in touchableTiles)
            {
                tile.GetComponent<IceTileVariant>().InstaUnfreeze();
                yield return null;
            }
            //SoundManager.instance.PlaySFXOneShot(SoundManager.SFX.ICE_BREAK);
        }
    }
    public IEnumerator Up2()
    {
        gameState = GAME_STATE.UP2;
        yield return new WaitForSeconds(0.1f);
        if (slottedItems.Exists(x => x.is2Up))
        {
            gameState = GAME_STATE.PLAYING;
            yield break;
        }
        List<ItemTile> itemList_temp = new(itemTiles);
        itemList_temp.RemoveAll(x => x.state == ITEMTILE_STATE.SLOTTED || slottedItems.Exists(y => y.tileType == x.tileType) || x.IsEventTile);
        itemList_temp.Reverse();
        Dictionary<int, List<ItemTile>> dicItems_all = new();
        foreach (ItemTile item in itemList_temp)
        {
            if (dicItems_all.ContainsKey(item.tileType))
                dicItems_all[item.tileType].Add(item);
            else
                dicItems_all.Add(item.tileType, new List<ItemTile> { item });
        }
        if (dicItems_all.Count > 0)
        {
            int randomType = GameConfig.GetRandom(dicItems_all.Keys.ToArray());
            if (dicItems_all[randomType].Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    slottedItems.Insert(0, dicItems_all[randomType][i]);
                    dicItems_all[randomType][i].itemCollider.enabled = false;
                    dicItems_all[randomType][i].SetSortingLayer("Slotted");
                    dicItems_all[randomType][i].state = ITEMTILE_STATE.SLOTTED;
                    dicItems_all[randomType][i].is2Up = true;
                    dicItems_all[randomType][i].glow.gameObject.SetActive(true);
                    dicItems_all[randomType][i].shadow.color = new Color(1, 1, 1, 0.2f);
                    dicItems_all[randomType][i].shadow.gameObject.SetActive(true);
                    dicItems_all[randomType][i].gameObject.transform.DOScale(Vector3.one * 1.75f, 0.35f);
                }
                yield return new WaitForSeconds(0.35f);
                foreach (ItemTile tile in slottedItems)
                    _ = tile.Slotted;
            }
        }
        GameScript.instance.PowerUpsInteractable();
        if (gameState != GAME_STATE.PAUSE)
            gameState = GAME_STATE.PLAYING;
        SaveProgress();
    }
    public IEnumerator Rainbow()
    {
        gameState = GAME_STATE.RAINBOW;
        yield return new WaitForSeconds(0.1f);
        List<ItemTile> itemList_temp = new(itemTiles);
        itemList_temp.RemoveAll(x => x.state == ITEMTILE_STATE.SLOTTED || slottedItems.Exists(y => y.tileType == x.tileType) || x.IsEventTile || x.isRainbow);
        //itemList_temp.Reverse();
        Dictionary<int, List<ItemTile>> dicItems_all = new();
        List<int> rainbowable = new();
        foreach (ItemTile item in itemList_temp)
        {
            if (dicItems_all.ContainsKey(item.tileType))
                dicItems_all[item.tileType].Add(item);
            else
                dicItems_all.Add(item.tileType, new List<ItemTile> { item });
            if (dicItems_all[item.tileType].Count >= 3 && !rainbowable.Contains(item.tileType))
            {
                rainbowable.Add(item.tileType);
            }
        }
        if (rainbowable.Count > 0)
        {
            int randomType = GameConfig.GetRandom(rainbowable.ToArray());
            for (int i = 0; i < 3; i++)
            {
                dicItems_all[randomType][i].MoveTileToSlot(true);
                yield return new WaitForSeconds(0.05f);
                //dicItems_all[randomType][1].MoveTileToSlot(true);
                //dicItems_all[randomType][2].MoveTileToSlot(true);
            }
        }
        yield return new WaitForSeconds(1.5f);
        gameState = GAME_STATE.PLAYING;
        SaveProgress();
    }
    public void Danger()
    {
        lowOnSpaceOB.SetActive(IsDanger);
        return;
    }
    bool dangerAnim;
    Sequence dangerSeq;
    void DangerSlotAnim()
    {
        dangerAnim = true;
        dangerSeq = DOTween.Sequence()
            .Insert(3, slotParent.DOScale(1.05f, 0.25f))
            .Append(slotParent.DOScale(1f, 0.25f))
            .Append(slotParent.DOScale(1.05f, 0.25f))
            .Append(slotParent.DOScale(1f, 0.25f))
            .SetLoops(-1, LoopType.Restart);
    }
    void DangerSlotAnimKill()
    {
        dangerAnim = false;
        dangerSeq.Kill();
        slotParent.DOScale(1, 0.1f);
    }
    bool challengeAnimPlayed;
    public void IdleChallenge()
    {
        if (!challengeAnimPlayed && IsChallengeAnim)
        {
            challengeAnimPlayed = true;
            challengeAnim.SetActive(true);
        }
    }
    public void IdleAnimPlay()
    {
        if (!dangerAnim)
            DangerSlotAnim();
    }
    public void IdleAnimKill()
    {
        //idleTime = 0;
        if (dangerAnim)
            DangerSlotAnimKill();
    }
    int likeAnimActive = 0;
    bool isLikeAnim;
    public void LikeAnim(Vector2 position)
    {
        if (isLikeAnim)
            return;
        likeAnimActive++;
        if (likeAnimActive >= 2)
        {
            isLikeAnim = true;
            var like = PoolManager.instance.GetObject<PoolObject>(likeAnim, this.transform, 1.5f);
            like.transform.position = position;
            Invoke(nameof(DoneLike), 1);
        }
    }
    void DoneLike()
    {
        likeAnimActive = 0;
        isLikeAnim = false;
    }
    public GameObject clearAnim;
    bool clearAnimPlay;
    bool isClearAnim;
    IEnumerator PlayClearAnim()
    {
        if (!clearAnimPlay || isClearAnim)
            yield break;
        isClearAnim = true;
        clearAnim.SetActive(true);
        yield return new WaitForSeconds(5);
        clearAnim.SetActive(false);
        clearAnimPlay = false;
        isClearAnim = false;
    }
    public PoolObjectSpriteRenderer comboAnim;
    void OnComboAnim(Vector2 pos, int count)
    {
        //var anim = Instantiate(comboAnim);
        var anim = PoolManager.instance.GetObject<PoolObjectSpriteRenderer>(comboAnim, this.transform, 1.5f).spriteRenderer;
        var text = anim.GetComponentInChildren<TextMeshPro>();
        text.text = $"+{count}";
        text.color = anim.color;
        anim.transform.position = pos;
        anim.transform.DOBlendableMoveBy(Vector3.up, 1);
        anim.DOFade(0, 1);
        text.DOFade(0, 1);//.OnComplete(() => Destroy(anim.gameObject));
    }
    public (bool isMatch3, List<ItemTile> matching) Check3Match()
    {
        bool result = false;
        List<ItemTile> matching = new();
        foreach (ItemTile checkTile in slottedItems)
        {
            int index = slottedItems.IndexOf(checkTile);
            if (index >= 2)
            {
                if (!match3Items.Contains(slottedItems[index - 1]) && slottedItems[index - 1]._slotted == checkTile._slotted - 1 && slottedItems[index - 1].tileType == checkTile.tileType &&
                    !match3Items.Contains(slottedItems[index - 2]) && slottedItems[index - 2]._slotted == checkTile._slotted - 2 && slottedItems[index - 2].tileType == checkTile.tileType)
                {
                    //Debug.Log("matchcheck");
                    matching = new List<ItemTile>() { slottedItems[index], slottedItems[index - 2], slottedItems[index - 1] };
                    match3Items.AddRange(matching);
                    undoList.RemoveAll(x => match3Items.Contains(x));
                    result = true;
                }
                //else
                //{
                //    //Debug.Log("matchfail");
                //}
            }
        }
        return (result, matching);
    }
    public void Remove3Match(ItemTile matchTile)
    {
        Debug.Log($"{matchTile.state} {matchTile.sortingIndex}");
        if (matchTile.state == ITEMTILE_STATE.DONE || match3Items.IndexOf(matchTile) % 3 != 0)
            return;
        Debug.Log("remove match 3");
        int index = slottedItems.IndexOf(matchTile);
        matchTile.state = ITEMTILE_STATE.DONE;
        slottedItems[index - 1].state = ITEMTILE_STATE.DONE;
        slottedItems[index - 2].state = ITEMTILE_STATE.DONE;
        List<ItemTile> match3Tiles = new() { slottedItems[index - 2], slottedItems[index - 1], slottedItems[index] };
        //Debug.Log(match3Tiles.Count);
        if (slottedItems[index].TryGetComponent(out GoldTileVariant goldTile) && goldTile.tileType == -1)
            GoldTileVariant.goldCount += 3;

        StartCoroutine(Remove3Match(match3Tiles));

        if (GameConfig.GetCurrentLevel() >= 7 && Random.Range(0, 2) == 0)
            GameConfig.piggyAdd += 5;
    }

    GameObject SpawnMatch3Fx(Vector3 position)
    {
        Transform parent = slotParent != null ? slotParent : transform;
        ParticleSystem fallback = TileOnRuntimeFx.CreateBurstParticle(
            parent,
            "Match Break Fx",
            new Color(1f, 0.82f, 0.38f, 1f),
            22,
            0.5f,
            0.28f,
            0.06f,
            0.18f,
            1.55f,
            Match3FxSortingLayer,
            Match3FxSortingOrder);
        fallback.transform.position = position;
        fallback.Play(true);
        return fallback.gameObject;
    }

    List<GameObject> BrokenMatchTiles(List<ItemTile> tiles)
    {
        EnsureTileEffects();

        List<GameObject> fallbackFxs = new();
        foreach (ItemTile tile in tiles)
        {
            if (tile == null)
                continue;

            GameObject itemBurstFx = MatchedItemBurstFx.Play(tile, slotParent != null ? slotParent : transform, Match3FxSortingLayer, Match3FxSortingOrder);
            fallbackFxs.Add(itemBurstFx != null ? itemBurstFx : SpawnMatch3Fx(tile.transform.position));
        }

        return fallbackFxs;
    }

    IEnumerator PlayTrayMeteorMatchFx(List<ItemTile> tiles, List<GameObject> fxs)
    {
        EnsureTileEffects();

        const float nextMeteorDelay = 0.25f * Match3DurationMultiplier;
        Transform parent = slotParent != null ? slotParent : transform;
        List<ItemTile> orderedTiles = tiles
            .Where(tile => tile != null)
            .OrderBy(tile => tile.transform.position.x)
            .ToList();
        int count = Mathf.Min(3, orderedTiles.Count);
        int completedCount = 0;
        for (int i = 0; i < count; i++)
        {
            ItemTile tile = orderedTiles[i];
            StartCoroutine(PlaySingleTrayMeteorBreakFx(tile, parent, fxs, i, () => completedCount++));

            if (i < count - 1)
                yield return new WaitForSeconds(nextMeteorDelay);
        }

        while (completedCount < count)
            yield return null;
    }

    IEnumerator PlaySingleTrayMeteorBreakFx(ItemTile tile, Transform parent, List<GameObject> fxs, int sequenceIndex, Action onComplete)
    {
        if (tile == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        Transform visual = TileOnRuntimeFx.VisualRoot(tile);
        visual?.DOKill();
        Tween pulseTween = visual?.DOScale(1.14f, 0.05f * Match3DurationMultiplier).SetEase(Ease.OutQuad);

        Tween meteorTween = MeteorImpactFx.Play(
            tile.transform.position,
            parent,
            Match3FxSortingLayer,
            Match3FxSortingOrder + 120 + sequenceIndex * 20,
            sequenceIndex);
        if (meteorTween != null)
            yield return meteorTween.WaitForCompletion();
        else
            yield return new WaitForSeconds(0.22f * Match3DurationMultiplier);

        pulseTween?.Kill();
        GameObject itemBurstFx = MatchedItemBurstFx.Play(
            tile,
            parent,
            Match3FxSortingLayer,
            Match3FxSortingOrder + sequenceIndex * 40);
        fxs.Add(itemBurstFx != null ? itemBurstFx : SpawnMatch3Fx(tile.transform.position));

        visual?.DOScale(0f, 0.08f * Match3DurationMultiplier).SetEase(Ease.InBack);
        onComplete?.Invoke();
    }

    IEnumerator Remove3Match(List<ItemTile> tiles, bool useHintMergeFx = false)
    {
        if (itemTiles.Count == 3)
        {
            gameState = GAME_STATE.WIN;
            StartCoroutine(OnWin(tiles));
            yield break;
        }
        bool is2Up = tiles.Exists(x => x.is2Up);
        bool isRainbow = tiles.Exists(x => x.isRainbow);
        if (slottedItems.Count > 3)
            clearAnimPlay = true;
        bool playClearAnim = slottedItems.Count == 3 && !isRainbow && !is2Up;
        //isRemove3Match = true;
        int combo = 0;
        if (!isRainbow)
            combo = gameTextScript.ShowText();
        if (combo > 0 && isComboEvent)
        {
            OnComboAnim(tiles[1].transform.position, combo);
        }
        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.BREAK);
        List<GameObject> fxs = new();
        if (useHintMergeFx && tripleTileMergeFx != null)
        {
            Tween mergeTween = tripleTileMergeFx.Play(tiles, () =>
            {
                for (int i = 0; i < tiles.Count; i++)
                {
                    Transform visual = TileOnRuntimeFx.VisualRoot(tiles[i]);
                    visual?.DOScale(0, 0.1f * Match3DurationMultiplier);
                }
            }, false, false);
            if (mergeTween != null)
                yield return mergeTween.WaitForCompletion();
        }
        else
        {
            yield return PlayTrayMeteorMatchFx(tiles, fxs);
        }
        //if (tiles[0].TryGetComponent(out GoldTileVariant goldVarient) && goldVarient.tileType == -1)
        //{
        //    //StartCoroutine(brokenAnimScript.BrokenTile(3, tiles[1].transform.position));
        //    brokenAnimScript.BrokenTile(3, tiles[1].transform.position);
        //}
        //else
        //{
        //    Sprite itemSprite = tiles[0].item.sprite;
        //    int color = tileSpritesSO.itemColorSprites.FindIndex(ics => ics.itemSprites.Contains(itemSprite));
        //    if (color == -1 || color > 3) color = 4;
        //    //StartCoroutine(brokenAnimScript.BrokenTile(color, tiles[1].transform.position));
        //    brokenAnimScript.BrokenTile(color, tiles[1].transform.position);
        //}
        itemTiles.RemoveAll(x => tiles.Contains(x));
        yield return new WaitForSeconds(0.25f * Match3DurationMultiplier);
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            ItemTile item = tiles[i];
            slottedItems.Remove(item);
            match3Items.Remove(item);
            item.gameObject.SetActive(false);
        }
        bool isLose = match3Items.Count == 0 && !IsSlotAvailable;
        //bool isLose = !GameController.instance.IsSlotAvailable;
        if (isLose)
            gameState = GAME_STATE.LOSE;
        foreach (ItemTile tile in slottedItems)
            _ = tile.Slotted;
        Danger();

        //if (match3Items.Count == 0 && !IsSlotAvailable)
        if (gameState == GAME_STATE.LOSE)
        {
            ClearSavedProgress();
            GameScript.instance.Lose();
        }
        if (is2Up)
            StartCoroutine(Up2());
        if (isRainbow)
            StartCoroutine(Rainbow());
        //isRemove3Match = false;
        GameScript.instance.PowerUpsInteractable();
        yield return new WaitForSeconds(0.25f * Match3DurationMultiplier);
        if (playClearAnim)
            StartCoroutine(PlayClearAnim());
        SaveProgress();
        yield return new WaitForSeconds(0.75f * Match3DurationMultiplier);
        for (int i = 0; i < fxs.Count; i++)
        {
            if (fxs[i] != null)
                Destroy(fxs[i]);
        }
        yield return null;
    }
    IEnumerator OnWin(List<ItemTile> tiles)
    {
        Transform slot0 = GetSlotTransform(0);
        Transform slot3 = GetSlotTransform(3);
        Transform slot4 = GetSlotTransform(4);
        if (slot0 == null || slot3 == null || slot4 == null)
            yield break;

        if (tiles[0].transform.position != slot0.transform.position)
        {
            for (int i = 0; i < 3; i++)
            {
                Transform slot = GetSlotTransform(i);
                if (slot != null)
                    tiles[i].transform.DOMove(slot.transform.position, 0.1f * Match3DurationMultiplier);
            }
            yield return new WaitForSeconds(0.1f * Match3DurationMultiplier);
        }
        tiles[0].SetSortingOrder(10);
        tiles[0].transform.DORotate(new Vector3(0, 0, 360 * (-1)), 0.25f * Match3DurationMultiplier, RotateMode.FastBeyond360);
        tiles[0].transform.DOMoveY(slot0.transform.position.y + (-3), 0.25f * Match3DurationMultiplier);
        tiles[0].transform.DOMoveY(slot0.transform.position.y, 0.25f * Match3DurationMultiplier).SetDelay(0.25f * Match3DurationMultiplier);
        tiles[1].transform.DOMove(slot3.transform.position, 0.25f * Match3DurationMultiplier);
        tiles[1].transform.DOMove(slot0.transform.position, 0.25f * Match3DurationMultiplier).SetDelay(0.25f * Match3DurationMultiplier);
        tiles[2].transform.DOMove(slot4.transform.position, 0.25f * Match3DurationMultiplier);
        tiles[2].transform.DOMove(slot0.transform.position, 0.25f * Match3DurationMultiplier).SetDelay(0.25f * Match3DurationMultiplier);
        yield return new WaitForSeconds(0.5f * Match3DurationMultiplier);

        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.BREAK);
        Tween rocketTween = FinalMatchRocketFx.Play(
            tiles,
            slotParent != null ? slotParent : transform,
            Match3FxSortingLayer,
            Match3FxSortingOrder + 200);
        if (rocketTween != null)
            yield return rocketTween.WaitForCompletion();
        else
        {
            List<GameObject> fxs = new();
            yield return PlayTrayMeteorMatchFx(tiles, fxs);
            for (int i = 0; i < fxs.Count; i++)
            {
                if (fxs[i] != null)
                    Destroy(fxs[i]);
            }
        }
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            ItemTile item = tiles[i];
            itemTiles.Remove(item);
            slottedItems.Remove(item);
            match3Items.Remove(item);
            item.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.2f * Match3DurationMultiplier);
        yield return null;
    }

    public void SaveProgress()
    {
        if (GameConfig.isChallengeLevel)
        {
            GameplayProgressSave.Clear();
            return;
        }

        if (itemTiles == null || itemTiles.Count == 0 || match3Items.Count > 0)
            return;

        if (gameState == GAME_STATE.WIN || gameState == GAME_STATE.WIN_POPUP ||
            gameState == GAME_STATE.LOSE || gameState == GAME_STATE.LOSE_POPUP)
            return;

        GameplayProgressData data = new()
        {
            level = GameConfig.level,
            gameMode = (int)GameConfig.gameMode,
            itemTypeCount = itemTypeCount,
            parameter = parameter,
            isGoldTile = isGoldTile,
            isComboEvent = isComboEvent,
            isExtraSlot = GameConfig.isExtraSlot,
            clearAnimPlay = clearAnimPlay,
            comboCount = GameTextScript.comboCount,
            eventCounter = EventTileExtend.eventCounter,
            eventTotal = EventTileExtend.eventTotal,
            itemSpriteIndexes = GetUsingSpriteIndexes(),
            undoSortingIndexes = undoList.Select(x => x.sortingIndex).ToList(),
            tiles = new List<GameplayTileProgressData>()
        };

        foreach (ItemTile tile in itemTiles)
            data.tiles.Add(CreateTileProgress(tile));

        GameplayProgressSave.Save(data);
    }

    public void ClearSavedProgress()
    {
        GameplayProgressSave.Clear();
    }

    List<int> GetUsingSpriteIndexes()
    {
        List<Sprite> allSprites = LevelManager.instance.tileSpritesSO.GetAllSprites();
        List<int> indexes = new();
        foreach (Sprite sprite in itemSpritesUsing)
            indexes.Add(allSprites.IndexOf(sprite));
        return indexes;
    }

    GameplayTileProgressData CreateTileProgress(ItemTile tile)
    {
        int slottedIndex = slottedItems.IndexOf(tile);
        GameplayTileProgressData data = new()
        {
            prefabType = GetPrefabType(tile),
            floor = tile.floor,
            sortingIndex = tile.sortingIndex,
            tilePosX = tile.tilePos.x,
            tilePosY = tile.tilePos.y,
            tileType = tile.tileType,
            state = (int)(tile.state == ITEMTILE_STATE.HOVERING ? ITEMTILE_STATE.TOUCHABLE : tile.state),
            slottedIndex = slottedIndex,
            is2Up = tile.is2Up,
            isRainbow = tile.isRainbow,
            isEventTile = tile.IsEventTile,
            eventCounter = tile.IsEventTile ? tile.eventTileExtend.counter : 0,
            linkKey = tile.sortingIndex,
            linkUnlinked = false,
            randomRevealed = false,
            iceCounter = 0,
        };

        if (tile.TryGetComponent(out LinkTileVariant linkTile))
        {
            data.linkUnlinked = linkTile.unLinked || linkTile.linkedTile == null;
            data.linkKey = linkTile.linkedTile != null ? Mathf.Min(tile.sortingIndex, linkTile.linkedTile.sortingIndex) : tile.sortingIndex;
        }
        if (tile.TryGetComponent(out RandomTileVariant randomTile))
            data.randomRevealed = randomTile.isRevealed;
        if (tile.TryGetComponent(out IceTileVariant iceTile))
            data.iceCounter = iceTile.counter;

        return data;
    }

    int GetPrefabType(ItemTile tile)
    {
        if (tile.TryGetComponent(out RandomTileVariant _))
            return 1;
        if (tile.TryGetComponent(out LinkTileVariant _))
            return 2;
        if (tile.TryGetComponent(out IceTileVariant _))
            return 3;
        if (tile.TryGetComponent(out GoldTileVariant _))
            return 4;
        return 0;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveProgress();
    }

    void OnApplicationQuit()
    {
        SaveProgress();
    }
}

public enum TILE_APPEAR_ANIM
{
    POP,
    DROP,
    SPIN,
    SLIDE
}
