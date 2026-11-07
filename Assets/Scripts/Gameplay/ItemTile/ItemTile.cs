using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemTile : PoolObject, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    const float SlottedScale = 1.1f * 0.95f;
    // Inner well dimensions measured from SkyTray's imported rect, in tray units.
    static readonly Vector2 SlottedCellSize = new Vector2(1.01f, 1.02f);
    const float SlottedCellFill = 0.92f;
    const float SlotFlyDuration = 0.34f;
    const int RocketFlameTextureWidth = 192;
    const int RocketFlameTextureHeight = 96;
    const float RocketFlameBaseScale = 1.85f;
    const float RocketFlameNozzleOffset = 0.2f;
    static Material tileTrailMaterial;
    static Sprite rocketFlameSprite;

    public Transform sprites;
    public SpriteRenderer bg;
    public SpriteRenderer item;
    public SpriteRenderer shadow;
    public Collider2D itemCollider;
    public SpriteRenderer glow;
    public GameObject holdAnim;
    [SerializeField] private TrailRenderer tileTrail;

    int collisionCount;
    Sequence tileAppearSequence;
    Transform slotFlameRoot;
    SpriteRenderer slotFlameOuter;
    SpriteRenderer slotFlameCore;
    SpriteRenderer slotFlameGlow;
    Sequence slotFlamePulse;
    bool slotFlameActive;
    Vector3 lastSlotFlameWorldPosition;
    Vector3 lastSlotFlameDirection = Vector3.down;

    public ITEMTILE_STATE state;
    public int tileType;
    public int floor;
    public Vector2 tilePos;
    public int sortingIndex;
    public bool isHolding;
    public int _slotted = -1;
    public int Slotted
    {
        get
        {
            if (GameController.instance.slottedItems.Contains(this))
            {
                int index = GameController.instance.slottedItems.IndexOf(this);
                if (_slotted != index)
                {
                    _slotted = index;
                    OnSlotChanged(_slotted);
                }
            }
            else
                _slotted = -1;
            return _slotted;
        }
    }
    Action<int> OnSlotChanged;

    protected virtual void LateUpdate()
    {
        UpdateSlotRocketFlame();
    }

    public override void Active(float? timeActive = null)
    {
        state = ITEMTILE_STATE.START;
        OnSlotChanged += SlottedMove;
        StopTileTrail(true);
        StopTileAppearAnimation();
        base.Active(timeActive);
    }
    public override void Inactive()
    {
        OnSlotChanged -= SlottedMove;
        itemCollider.enabled = false;
        StopTileTrail(true);
        StopTileAppearAnimation();
        base.Inactive();
        DefaultValue();
    }

    public EventTileExtend eventTileExtend;
    public bool IsEventTile => eventTileExtend != null;
    public bool is2Up;
    public bool isRainbow;
    public bool isFogAnim;
    public virtual void DefaultValue()
    {
        if (TryGetComponent(out TileModeMarker modeMarker))
            modeMarker.ResetMarker();
        state = ITEMTILE_STATE.START;
        tileType = 0;
        floor = 0;
        tilePos = Vector2.zero;
        sortingIndex = 0;
        isHolding = false;
        _slotted = -1;
        is2Up = false;
        isRainbow = false;
        isFogAnim = false;
        eventTileExtend = null;
        shadow.sprite = bg.sprite;
        shadow.DOKill();
        shadow.color = new Color(0, 0, 0, 0.5f);
        shadow.transform.DOKill();
        shadow.transform.localPosition = Vector3.zero;
        StopTileTrail(true);
        StopTileAppearAnimation();
        //shadow.gameObject.SetActive(true);
        //sprites.DOKill();
        sprites.localScale = Vector3.one;
        //transform.DOKill();
        transform.localScale = Vector3.one;
        if (shadow.gameObject.TryGetComponent<Animator>(out var animator))
            Destroy(animator);
    }
    public virtual void SetSortingLayer(string layer)
    {
        //Debug.Log("tileLayer");
        glow.sortingLayerName = layer;
        bg.sortingLayerName = layer;
        item.sortingLayerName = layer;
        shadow.sortingLayerName = layer;
        if (tileTrail != null)
            tileTrail.sortingLayerName = layer;
        if (IsEventTile)
        {
            foreach (var renderer in eventTileExtend.goldPieces)
                renderer.sortingLayerName = layer;
            eventTileExtend.glow.sortingLayerName = layer;
        }
    }
    public virtual void SetSortingOrder(int order)
    {
        //Debug.Log("tileOrder");
        glow.sortingOrder = order;
        bg.sortingOrder = order;
        item.sortingOrder = order;
        shadow.sortingOrder = order;
        if (tileTrail != null)
            tileTrail.sortingOrder = order - 1;
        if (IsEventTile)
        {
            foreach (var renderer in eventTileExtend.goldPieces)
                renderer.sortingOrder = order;
            eventTileExtend.glow.sortingOrder = order;
        }
    }
    public RuntimeAnimatorController fogAnimController;
    public virtual void InitItemTile(int floor, int sortingIndex, Vector2 tilePos)
    {
        CancelInvoke();
        hovering?.Kill();
        hovering = null;
        upDown?.Kill();
        upDown = null;
        gameObject.transform.DOKill();
        sprites.transform.DOKill();
        shadow.DOKill();
        shadow.transform.DOKill();
        isHolding = false;
        isFogAnim = false;
        _slotted = -1;
        itemCollider.enabled = false;
        if (holdAnim)
            holdAnim.SetActive(false);
        glow.gameObject.SetActive(false);
        sprites.transform.localScale = Vector3.one;
        sprites.transform.localPosition = Vector3.zero;
        sprites.transform.localRotation = Quaternion.identity;
        gameObject.transform.localRotation = Quaternion.identity;
        shadow.transform.localPosition = Vector3.zero;
        StopTileTrail(true);
        this.floor = floor;
        this.sortingIndex = sortingIndex;
        this.tilePos = tilePos;
        if (GameConfig.gameMode == GAME_MODE.tile_fog)
        {
            shadow.sprite = LevelManager.instance.tileSpritesSO.fogSprite;
            shadow.color = Color.white;
            if (!shadow.gameObject.TryGetComponent<Animator>(out _))
                shadow.gameObject.AddComponent<Animator>().runtimeAnimatorController = fogAnimController;
            shadow.gameObject.SetActive(true);
        }
        else
        {
            shadow.DOFade(GameConfig.level > Common.config_tile_shadow ? 0 : 0.5f, 0);
            shadow.gameObject.SetActive(false);
        }
        SetSortingLayer("Floor" + floor);
        SetSortingOrder(sortingIndex);
        gameObject.transform.SetParent(GameController.instance.floorTransformList[floor - 1]);
        transform.localScale = Vector3.zero;
        glow.gameObject.SetActive(true);
        transform.localPosition = new Vector3(tilePos.x, tilePos.y, sortingIndex / -10f);
        itemCollider.enabled = false;
        PlayTileAppearAnimation(GameController.instance.GetTileAppearDelay(sortingIndex));
    }
    //public void InitSlot(int floor, int sortingIndex, Vector2 tilePos, int slot)
    //{
    //    itemCollider.enabled = false;
    //    this.floor = floor;
    //    this.sortingIndex = sortingIndex;
    //    this.tilePos = tilePos;
    //    SetSortingLayer("Slotted");
    //    SetSortingOrder(slot);
    //    shadow.gameObject.SetActive(false);
    //    state = ITEMTILE_STATE.SLOTTED;
    //    gameObject.transform.SetParent(GameController.instance.slotTransformList[slot]);
    //    gameObject.transform.DOKill();
    //    gameObject.transform.localScale = Vector3.one;
    //    gameObject.transform.localPosition = Vector3.zero;
    //}
    public virtual void SetType(int type)
    {
        if (IsEventTile)
            return;
        isRainbow = false;
        if (GameController.instance != null && GameController.instance.TrySetNewModeTileType(this, type))
            return;
        if (GameConfig.gameMode == GAME_MODE.tile_rainbow && type == -1)
        {
            SetRainbow();
            return;
        }
        tileType = type;
        var usingSprite = GameController.instance.itemSpritesUsing[type];
        item.sprite = usingSprite;
        //wid 8.5 hei 8
        if (usingSprite.rect.width > usingSprite.rect.height)
            item.size = new Vector2(0.825f, usingSprite.rect.height / usingSprite.rect.width * 0.825f);
        else
            item.size = new Vector2(usingSprite.rect.width / usingSprite.rect.height * 0.825f, 0.825f);
        item.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
    }
    public void SetRainbow()
    {
        tileType = -1;
        isRainbow = true;
        var usingSprite = LevelManager.instance.tileSpritesSO.rainbowSprite;
        const float rainbowSize = 0.85f;
        item.sprite = usingSprite;
        if (usingSprite.rect.width > usingSprite.rect.height)
            item.size = new Vector2(rainbowSize, usingSprite.rect.height / usingSprite.rect.width * rainbowSize);
        else
            item.size = new Vector2(usingSprite.rect.width / usingSprite.rect.height * rainbowSize, rainbowSize);
        item.transform.localPosition = new Vector3(0.02f, 0.05f, 0.05f);
    }
    public void SetEventTile()
    {
        //eventTileExtend = Instantiate(GameController.instance.eventTileExtend, sprites);
        eventTileExtend = PoolManager.instance.GetObject<EventTileExtend>(GameController.instance.eventExtendPrefab, sprites);
        eventTileExtend.transform.localPosition = Vector3.zero;
        eventTileExtend.transform.localScale = Vector3.one;
        eventTileExtend.itemTile = this;
        tileType = -2;
        var usingSprite = eventTileExtend.ungoldItemSprite;
        item.sprite = usingSprite;
        if (usingSprite.rect.width > usingSprite.rect.height)
            item.size = new Vector2(0.825f, usingSprite.rect.height / usingSprite.rect.width * 0.825f);
        else
            item.size = new Vector2(usingSprite.rect.width / usingSprite.rect.height * 0.825f, 0.825f);
        item.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
        eventTileExtend.SetCounter();
        eventTileExtend.ResetGoldPiecesVisual();
        //return;
    }
    public virtual void SetState()
    {
        itemCollider.enabled = true;
        collisionCount = 0;
        ToggleTouchable(true);
    }
    public virtual void ToggleTouchable(bool touchable)
    {
        //Debug.Log(touchable);
        if (touchable)
        {
            state = ITEMTILE_STATE.TOUCHABLE;
            if (GameController.instance.gameState != GAME_STATE.START && GameController.instance.gameState != GAME_STATE.SHUFFLE)
            {
                if (GameConfig.gameMode == GAME_MODE.tile_fog)
                {
                    isFogAnim = true;
                    shadow.transform.DOBlendableLocalMoveBy(new Vector3(GameConfig.GetRandom(-1, 1), 0), 0.5f);
                    shadow.DOFade(0, 0.5f).OnComplete(() =>
                    {
                        shadow.gameObject.SetActive(false);
                        shadow.transform.localPosition = Vector3.zero;
                        isFogAnim = false;
                    });
                }
                else
                    shadow.gameObject.SetActive(false);
            }
        }
        else
        {
            state = ITEMTILE_STATE.HIDDEN;
            if (GameController.instance.gameState != GAME_STATE.START && GameController.instance.gameState != GAME_STATE.SHUFFLE)
                shadow.gameObject.SetActive(true);
        }
    }
    //hidden -> touchable
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (state == ITEMTILE_STATE.START)
            return;
        if (collision.CompareTag("ItemTile"))
        {
            if (collision.GetComponent<ItemTile>().floor > this.floor)
                collisionCount--;
            if (collisionCount <= 0)
            {
                ToggleTouchable(true);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == ITEMTILE_STATE.START)
            return;
        if (collision.CompareTag("ItemTile"))
        {
            if (collision.GetComponent<ItemTile>().floor > this.floor)
                collisionCount++;
            if (collisionCount > 0)
            {
                ToggleTouchable(false);
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Mouse Down: " + gameObject.name);
        if (isFogAnim)
            return;
        MouseDown();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Mouse Up: " + gameObject.name);
        if (isFogAnim)
            return;
        MouseUp();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("Mouse Click: " + gameObject.name);
        if (isFogAnim)
            return;
        MouseUpAsButton();
    }
    public virtual void MouseDown()
    {
        if (!GameController.instance.UIClickable || !GameController.instance.IsSlotAvailable)
            return;
        GameController.instance.PlayTileTapEffect(transform.position);
        if (state == ITEMTILE_STATE.TOUCHABLE)
        {
            isHolding = true;
            SetSortingLayer("Hovering");
            state = ITEMTILE_STATE.HOVERING;
            sprites.transform.DOKill();
            sprites.transform.DOScale(Vector3.one * 1.75f, 0.25f).SetEase(Ease.OutBack);
            Invoke(nameof(Hovering), 0.25f);
        }
    }
    Sequence hovering;
    Sequence upDown;
    public void Hovering()
    {
        if (state != ITEMTILE_STATE.HOVERING)
            return;
        int rnd = GameConfig.GetRandom(-1, 1);

        hovering = DOTween.Sequence().Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, -10 * rnd), 0.75f))
            .Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, 10 * rnd), 1.5f))
            .Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.75f)).SetLoops(-1, LoopType.Restart);

        upDown = DOTween.Sequence().Append(sprites.transform.DOBlendableMoveBy(new Vector3(0, 1, 0), 1.5f).SetEase(Ease.InOutSine))
            .Append(sprites.transform.DOBlendableMoveBy(new Vector3(0, -1, 0), 1.5f).SetEase(Ease.InOutSine)).SetLoops(-1, LoopType.Restart);
    }
    public virtual void MouseUp()
    {
        if (GameController.instance.gameState != GAME_STATE.PLAYING || !GameController.instance.IsSlotAvailable)
            return;
        if (state == ITEMTILE_STATE.HOVERING)
        {
            isHolding = false;
            holdAnim.SetActive(false);
            SetSortingLayer("Floor" + floor);
            state = ITEMTILE_STATE.TOUCHABLE;
            hovering?.Kill();
            upDown?.Kill();
            sprites.transform.DOKill();
            sprites.transform.DOLocalRotate(Vector3.zero, 0);
            sprites.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
            sprites.transform.DOLocalMove(Vector3.zero, 0);
        }
    }
    public virtual void MouseUpAsButton()
    {
        if (!GameController.instance.UIClickable || !GameController.instance.IsSlotAvailable)
            return;
        if (state == ITEMTILE_STATE.HIDDEN)
        {
            int rnd = GameConfig.GetRandom(-1, 1);
            DOTween.Sequence().Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, -10 * rnd), 0.125f))
                   .Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, 10 * rnd), 0.25f))
                   .Append(sprites.transform.DOLocalRotate(new Vector3(0, 0, 0), 0.125f));
            return;
        }
        if (state == ITEMTILE_STATE.TOUCHABLE || state == ITEMTILE_STATE.HOVERING)
        {
            InvokeUpAsButton();
        }
    }
    public void InvokeUpAsButton()
    {
        GameScript.instance.EndItemsAnim();
        state = ITEMTILE_STATE.SLOTTED;
        Invoke(nameof(UpAsButton), Time.deltaTime);
    }
    public void UpAsButton()
    {
        Debug.Log("move tile");
        //state = ITEMTILE_STATE.MOVING;
        if (GameConfig.gameMode == GAME_MODE.tile_freeze)
        {
            if (IceTileVariant.iceBreak)
            {
                //SoundManager.instance.PlaySFXOneShot(SoundManager.SFX.ICE_BREAK);
                IceTileVariant.iceBreak = false;
                AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.TAP);
            }
            else
                AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.TAP);
        }
        else
        {
            if (TryGetComponent(out LinkTileVariant linked))
            {
                if (linked.unLinked)
                    AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.TAP);
                else if (LinkTileVariant.linkTilesList.IndexOf(linked) % 2 == 0)
                    //SoundManager.instance.PlaySFXOneShot(SoundManager.SFX.STICKY_SPLIT);
                    AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.TAP);
            }
            else
                AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.TAP);
        }

        if (GameConfig.vib)
            GameConfig.VibratePop();
        //AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.TAP);
        hovering?.Kill();
        upDown?.Kill();
        MoveTileToSlot();

        //if (Config.gameMode == GAME_MODE.tile_snail)
        //    StartCoroutine(SnailScript.instance.Snail());
        //else 
        if (GameConfig.gameMode == GAME_MODE.tile_freeze)
            StartCoroutine(GameController.instance.CheckFullFreeze());
    }

    public virtual void MoveTileToSlot(bool delay = false)
    {
        // All collection paths (tap, Hint and automatic moves) reveal the same star.
        if (IsEventTile && eventTileExtend.TryCollect())
        {
            GameScript.instance.GoldTileUpdate(eventTileExtend);
            eventTileExtend.goldParticle.Play(true);
        }
        if (GameConfig.gameMode == GAME_MODE.tile_freeze)
            IceTileVariant.UnfreezeNeighbours(this);

        itemCollider.enabled = false;
        StopTileTrail(true);
        StopTileAppearAnimation();
        if (GameController.instance.slottedItems.Exists(x => !GameController.instance.match3Items.Contains(x)
            && x.tileType == this.tileType))
        {
            int index = GameController.instance.slottedItems.FindLastIndex(x => x.tileType == this.tileType) + 1;
            GameController.instance.slottedItems.Insert(index, this);
        }
        else
        {
            GameController.instance.slottedItems.Add(this);
        }
        GameController.instance.undoList.Add(this);
        foreach (var tile in GameController.instance.slottedItems)
        {
            _ = tile.Slotted;
        }
        (bool isMatch3, List<ItemTile> matching) = GameController.instance.Check3Match();
        if (isMatch3)
        {
            if (matching.Exists(x => x.is2Up))
                GameController.instance.gameState = GAME_STATE.UP2;
            if (matching.Exists(x => x.isRainbow))
                GameController.instance.gameState = GAME_STATE.RAINBOW;
        }
        bool isLose = GameController.instance.match3Items.Count == 0 && !GameController.instance.IsSlotAvailable;
        if (isLose)
            GameController.instance.gameState = GAME_STATE.LOSE;
        GameController.instance.Danger();
        GameController.instance.IdleAnimKill();
        state = ITEMTILE_STATE.SLOTTED;
        shadow.gameObject.SetActive(false);
        GameController.instance.OnNewModeTileCollected(this, delay);
        SetSortingLayer("Slotted");
        SetSortingOrder(_slotted);
        if (delay)
            glow.gameObject.SetActive(true);
        else
            GameController.instance.LikeAnim(transform.position);
        Transform slotTransform = GameController.instance.GetSlotTransform(_slotted);
        if (slotTransform == null)
            return;

        gameObject.transform.SetParent(slotTransform, true);
        Vector3 slotScale = GetSlottedScale(slotTransform);
        Vector3 slotPosition = GetSlottedPosition(slotScale);
        transform.localRotation = Quaternion.identity;
        Debug.Log(transform.parent);
        sprites.transform.DOKill();
        sprites.transform.DOLocalMove(Vector3.zero, 0);
        sprites.transform.DOLocalRotate(Vector3.zero, 0);
        sprites.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        gameObject.transform.DOScale(delay ? slotScale * (1.75f / SlottedScale) : slotScale, 0.35f);
        //gameObject.transform.DOMove(MovePos(PlayTestController.instance.slotTransformList[_slotted].transform.position), 0.25f)
        Vector3 halfLocalPos = gameObject.transform.localPosition / 2;
        Vector3 curveLocation = halfLocalPos + (halfLocalPos.x < 0 ? new Vector3(halfLocalPos.y, -halfLocalPos.x) : new Vector3(-halfLocalPos.y, halfLocalPos.x)) / 4;
        StopTileTrail(true);
        gameObject.transform.DOLocalPath(new Vector3[] { curveLocation, slotPosition }, SlotFlyDuration, PathType.CatmullRom).SetDelay(delay ? 0.35f : 0)
            .OnStart(() =>
            {
                gameObject.transform.DOScale(slotScale, SlotFlyDuration);
                //GameScript.instance.PowerUpsInteractable();
            })
            .OnComplete(() =>
            {
                glow.gameObject.SetActive(false);
                GameController.instance.BounceTray();
                GameController.instance.OnNewModeTileArrived();
                Debug.Log($"match3 {isMatch3}");
                Debug.Log($"isLose {isLose} {GameController.instance.gameState}");
                if (isMatch3)
                {
                    //if (Config.GetTutorial() == 0 && Config.level == 1)
                    //{
                    //    LevelPopup.instance.tutorialPowerPopup.ExitTutorialPower();
                    //    Config.SetTutorial(1);
                    //}
                    GameController.instance.Remove3Match(this);
                }
                else
                {
                    if (GameController.instance.gameState == GAME_STATE.LOSE)
                    {
                        GameController.instance.ClearSavedProgress();
                        GameScript.instance.Lose();
                        //GameUI.instance.OnLose();
                    }
                    else
                    {
                        GameController.instance.SaveProgress();
                    }
                }
                GameScript.instance.PowerUpsInteractable();
            });
    }
    Vector3 GetSlottedScale(Transform slot)
    {
        // Fit the background inside the painted well, preserving its proportions.
        Vector3 scale = slot.localScale;
        float size = Mathf.Min(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) * SlottedScale;
        Vector3 backgroundSize = Vector3.Scale(bg.localBounds.size, bg.transform.localScale);
        if (backgroundSize.x > 0f && backgroundSize.y > 0f)
            size = Mathf.Min(size, SlottedCellSize.x * SlottedCellFill / backgroundSize.x,
                SlottedCellSize.y * SlottedCellFill / backgroundSize.y);
        return new Vector3(size / Mathf.Max(Mathf.Abs(scale.x), 0.0001f),
            size / Mathf.Max(Mathf.Abs(scale.y), 0.0001f),
            size / Mathf.Max(Mathf.Abs(scale.z), 0.0001f));
    }

    Vector3 GetSlottedPosition(Vector3 scale)
    {
        // Slot anchors follow the painted wells; disabled legacy slot sprites have other pivots.
        Vector3 tileCenter = sprites.InverseTransformPoint(bg.transform.TransformPoint(bg.localBounds.center));
        Vector3 position = -Vector3.Scale(tileCenter, scale);
        position.z = 0f;
        return position;
    }

    void SlottedMove(int i)
    {
        if (state == ITEMTILE_STATE.SLOTTED)
        {
            SetSortingOrder(i);
            gameObject.transform.DOKill();
            StopTileTrail(true);
            sprites.transform.DOKill();
            sprites.transform.DOLocalMove(Vector3.zero, 0);
            sprites.transform.DOLocalRotate(Vector3.zero, 0);
            sprites.transform.localScale = Vector3.one;
            Transform slotTransform = GameController.instance.GetSlotTransform(i);
            if (slotTransform == null)
                return;

            gameObject.transform.SetParent(slotTransform, true);
            transform.localRotation = Quaternion.identity;
            Vector3 slotScale = GetSlottedScale(slotTransform);
            gameObject.transform.DOScale(slotScale, 0.05f);
            gameObject.transform.DOLocalMove(GetSlottedPosition(slotScale), 0.2f).OnComplete(() =>
            {
                if (GameController.instance.match3Items.Contains(this))
                    GameController.instance.Remove3Match(this);
                if (is2Up)
                    glow.gameObject.SetActive(false);
            });
        }
    }

    public void ForceSnapToSlot(int i)
    {
        state = ITEMTILE_STATE.SLOTTED;
        _slotted = i;
        itemCollider.enabled = false;
        shadow.gameObject.SetActive(false);
        StopTileTrail(true);
        gameObject.transform.DOKill();
        sprites.transform.DOKill();
        sprites.transform.localPosition = Vector3.zero;
        sprites.transform.localRotation = Quaternion.identity;
        sprites.transform.localScale = Vector3.one;
        Transform slotTransform = GameController.instance.GetSlotTransform(i);
        if (slotTransform == null)
            return;

        gameObject.transform.SetParent(slotTransform, false);
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = GetSlottedScale(slotTransform);
        gameObject.transform.localPosition = GetSlottedPosition(transform.localScale);
        SetSortingLayer("Slotted");
        SetSortingOrder(i);
        if (!is2Up)
            glow.gameObject.SetActive(false);
    }

    public virtual void UndoMove()
    {
        if (state == ITEMTILE_STATE.SLOTTED)
        {
            if (IsEventTile)
                eventTileExtend.UndoCollection();
            GameController.instance.gameState = GAME_STATE.UNDO;
            gameObject.transform.SetParent(GameController.instance.floorTransformList[floor - 1]);
            StopTileTrail(true);
            gameObject.transform.DOScale(Vector3.one * 1.25f, 0.35f);
            Debug.Log(sortingIndex);
            glow.gameObject.SetActive(true);
            gameObject.transform.DOLocalMove(new Vector3(tilePos.x, tilePos.y, sortingIndex / -10f), 0.25f).SetDelay(0.35f)
                .OnStart(() =>
                {
                    GameController.instance.slottedItems.Remove(this);
                    GameController.instance.undoList.Remove(this);
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
                    if (GameConfig.gameMode == GAME_MODE.tile_freeze)
                        StartCoroutine(GameController.instance.CheckFullFreeze());
                    GameScript.instance.PowerUpsInteractable();
                    GameController.instance.gameState = GAME_STATE.PLAYING;
                GameController.instance.SaveProgress();
                });
        }
    }

    public virtual void PlayTileAppearAnimation(float delay)
    {
        if (state == ITEMTILE_STATE.SLOTTED || state == ITEMTILE_STATE.DONE)
            return;

        StopTileAppearAnimation();

        Vector3 finalPosition = transform.localPosition;
        Vector3 finalScale = Vector3.one;
        Quaternion finalRotation = Quaternion.identity;

        transform.localPosition = finalPosition;
        transform.localScale = finalScale;
        transform.localRotation = finalRotation;
        glow.gameObject.SetActive(true);

        Sequence currentSequence = DOTween.Sequence();
        tileAppearSequence = currentSequence;

        switch (GameController.instance.TileAppearAnim)
        {
            case TILE_APPEAR_ANIM.DROP:
                SetupDropAnimation(currentSequence, finalPosition, finalScale);
                break;
            case TILE_APPEAR_ANIM.SPIN:
                SetupSpinAnimation(currentSequence, finalPosition, finalScale);
                break;
            case TILE_APPEAR_ANIM.SLIDE:
                SetupSlideAnimation(currentSequence, finalPosition, finalScale);
                break;
            default:
                SetupPopAnimation(currentSequence, finalPosition, finalScale);
                break;
        }

        currentSequence
            .SetDelay(delay)
            .OnComplete(() =>
            {
                if (tileAppearSequence != currentSequence)
                    return;

                tileAppearSequence = null;

                if (state == ITEMTILE_STATE.SLOTTED || state == ITEMTILE_STATE.DONE)
                    return;

                transform.localPosition = finalPosition;
                transform.localScale = finalScale;
                transform.localRotation = finalRotation;
                glow.gameObject.SetActive(false);
            });
    }

    public void StopTileAppearAnimation()
    {
        if (tileAppearSequence != null)
        {
            tileAppearSequence.Kill(false);
            tileAppearSequence = null;
        }

        transform.DOKill(false);
    }

    void SetupPopAnimation(Sequence sequence, Vector3 finalPosition, Vector3 finalScale)
    {
        transform.localScale = Vector3.zero;
        sequence.Append(transform.DOScale(finalScale, AppearDuration(0.5f)).SetEase(Ease.OutBack));
    }

    void SetupDropAnimation(Sequence sequence, Vector3 finalPosition, Vector3 finalScale)
    {
        transform.localPosition = finalPosition + Vector3.up * 1.2f;
        transform.localScale = finalScale * 0.8f;

        sequence.Append(transform.DOLocalMove(finalPosition, AppearDuration(0.45f)).SetEase(Ease.OutBounce));
        sequence.Join(transform.DOScale(finalScale, AppearDuration(0.3f)).SetEase(Ease.OutBack));
    }

    void SetupSpinAnimation(Sequence sequence, Vector3 finalPosition, Vector3 finalScale)
    {
        transform.localScale = finalScale * 0.1f;
        transform.localRotation = Quaternion.Euler(0f, 0f, -120f);

        sequence.Append(transform.DOScale(finalScale, AppearDuration(0.45f)).SetEase(Ease.OutBack));
        sequence.Join(transform.DOLocalRotate(Vector3.zero, AppearDuration(0.45f), RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
    }

    void SetupSlideAnimation(Sequence sequence, Vector3 finalPosition, Vector3 finalScale)
    {
        float direction = finalPosition.x < 0f ? -1f : 1f;

        transform.localPosition = finalPosition + Vector3.right * direction * 1.5f;
        transform.localScale = finalScale * 0.75f;
        transform.localRotation = Quaternion.Euler(0f, 0f, direction * -12f);

        sequence.Append(transform.DOLocalMove(finalPosition, AppearDuration(0.4f)).SetEase(Ease.OutBack));
        sequence.Join(transform.DOScale(finalScale, AppearDuration(0.4f)).SetEase(Ease.OutBack));
        sequence.Join(transform.DOLocalRotate(Vector3.zero, AppearDuration(0.35f)).SetEase(Ease.OutQuad));
    }

    float AppearDuration(float duration)
    {
        return duration * GameController.instance.TileAppearDurationMultiplier;
    }

    void StopTileTrail(bool clearNow = false)
    {
        if (tileTrail != null)
        {
            tileTrail.emitting = false;
            if (clearNow)
                tileTrail.Clear();
        }

        StopSlotRocketFlame(clearNow);
    }

    TrailRenderer EnsureTileTrail()
    {
        if (tileTrail == null)
        {
            GameObject trailObject = new("Tile Slot Trail");
            trailObject.transform.SetParent(transform, false);
            trailObject.transform.localPosition = Vector3.zero;
            tileTrail = trailObject.AddComponent<TrailRenderer>();
        }

        tileTrail.time = 0.36f;
        tileTrail.widthMultiplier = 1.05f;
        tileTrail.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0.82f),
            new Keyframe(0.22f, 0.62f),
            new Keyframe(1f, 0f));
        tileTrail.colorGradient = TileGoTrailGradient();
        tileTrail.numCornerVertices = 8;
        tileTrail.numCapVertices = 6;
        tileTrail.alignment = LineAlignment.View;
        tileTrail.textureMode = LineTextureMode.Stretch;
        tileTrail.minVertexDistance = 0.2f;
        tileTrail.autodestruct = false;
        tileTrail.emitting = false;
        tileTrail.sortingLayerName = bg != null ? bg.sortingLayerName : "Slotted";
        tileTrail.sortingOrder = bg != null ? bg.sortingOrder - 1 : -1;

        if (tileTrail.sharedMaterial == null)
            tileTrail.sharedMaterial = TileTrailMaterial();

        return tileTrail;
    }

    static Gradient TileGoTrailGradient()
    {
        Gradient gradient = new();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.74f, 0.22f), 0f),
                new GradientColorKey(new Color(0.32f, 0.18f, 0.12f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.82f, 0f),
                new GradientAlphaKey(0.5f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });

        return gradient;
    }

    void StartSlotRocketFlame()
    {
        EnsureSlotRocketFlame();
        if (slotFlameRoot == null)
            return;

        slotFlameActive = true;
        lastSlotFlameWorldPosition = transform.position;
        lastSlotFlameDirection = Vector3.down;

        slotFlameRoot.gameObject.SetActive(true);
        slotFlameRoot.position = transform.position;
        slotFlameRoot.localScale = Vector3.one;
        slotFlameGlow.color = new Color(1f, 0.16f, 0.01f, 0.86f);
        slotFlameOuter.color = new Color(1f, 0.3f, 0f, 1f);
        slotFlameCore.color = new Color(1f, 0.98f, 0.52f, 1f);

        slotFlamePulse?.Kill();
        slotFlamePulse = DOTween.Sequence()
            .Append(slotFlameGlow.transform.DOScale(new Vector3(1.8f, 1.45f, 1f), 0.045f).SetEase(Ease.OutSine))
            .Join(slotFlameOuter.transform.DOScale(new Vector3(1.62f, 1.24f, 1f), 0.045f).SetEase(Ease.OutSine))
            .Join(slotFlameCore.transform.DOScale(new Vector3(1.18f, 0.78f, 1f), 0.045f).SetEase(Ease.OutSine))
            .Append(slotFlameGlow.transform.DOScale(new Vector3(1.38f, 1.78f, 1f), 0.055f).SetEase(Ease.InOutSine))
            .Join(slotFlameOuter.transform.DOScale(new Vector3(1.22f, 1.56f, 1f), 0.055f).SetEase(Ease.InOutSine))
            .Join(slotFlameCore.transform.DOScale(new Vector3(0.9f, 1.02f, 1f), 0.055f).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo)
            .SetTarget(slotFlameRoot);
    }

    void StopSlotRocketFlame(bool clearNow)
    {
        slotFlameActive = false;
        slotFlamePulse?.Kill();
        slotFlamePulse = null;

        if (slotFlameRoot == null)
            return;

        if (clearNow)
        {
            slotFlameRoot.gameObject.SetActive(false);
            return;
        }

        DOTween.Sequence()
            .Join(slotFlameGlow.DOFade(0f, 0.14f))
            .Join(slotFlameOuter.DOFade(0f, 0.12f))
            .Join(slotFlameCore.DOFade(0f, 0.1f))
            .OnComplete(() => slotFlameRoot.gameObject.SetActive(false))
            .SetTarget(slotFlameRoot);
    }

    void UpdateSlotRocketFlame()
    {
        if (!slotFlameActive || slotFlameRoot == null)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastSlotFlameWorldPosition;
        if (movement.sqrMagnitude > 0.000001f)
            lastSlotFlameDirection = movement.normalized;

        float angle = Mathf.Atan2(lastSlotFlameDirection.y, lastSlotFlameDirection.x) * Mathf.Rad2Deg;
        float speedPulse = Mathf.Clamp(movement.magnitude * 120f, 0.92f, 1.45f);
        slotFlameRoot.position = currentPosition - lastSlotFlameDirection * RocketFlameNozzleOffset;
        slotFlameRoot.rotation = Quaternion.Euler(0f, 0f, angle);
        slotFlameRoot.localScale = Vector3.one * RocketFlameBaseScale * speedPulse;
        lastSlotFlameWorldPosition = currentPosition;
    }

    void EnsureSlotRocketFlame()
    {
        if (slotFlameRoot != null)
        {
            UpdateSlotRocketFlameSorting();
            return;
        }

        EnsureRocketFlameSprite();
        if (rocketFlameSprite == null)
            return;

        GameObject flameObject = new("Tile Rocket Flame Fx");
        slotFlameRoot = flameObject.transform;

        GameObject glowObject = new("Flame Glow");
        glowObject.transform.SetParent(slotFlameRoot, false);
        glowObject.transform.localScale = new Vector3(1.72f, 1.52f, 1f);
        slotFlameGlow = glowObject.AddComponent<SpriteRenderer>();
        slotFlameGlow.sprite = rocketFlameSprite;

        GameObject outerObject = new("Outer Flame");
        outerObject.transform.SetParent(slotFlameRoot, false);
        outerObject.transform.localScale = new Vector3(1.48f, 1.32f, 1f);
        slotFlameOuter = outerObject.AddComponent<SpriteRenderer>();
        slotFlameOuter.sprite = rocketFlameSprite;

        GameObject coreObject = new("White Hot Core");
        coreObject.transform.SetParent(slotFlameRoot, false);
        coreObject.transform.localPosition = new Vector3(0.02f, 0f, -0.01f);
        coreObject.transform.localScale = new Vector3(1.05f, 0.82f, 1f);
        slotFlameCore = coreObject.AddComponent<SpriteRenderer>();
        slotFlameCore.sprite = rocketFlameSprite;

        UpdateSlotRocketFlameSorting();
        slotFlameRoot.gameObject.SetActive(false);
    }

    void UpdateSlotRocketFlameSorting()
    {
        if (slotFlameGlow == null || slotFlameOuter == null || slotFlameCore == null)
            return;

        string sortingLayerName = bg != null ? bg.sortingLayerName : "Slotted";
        int sortingOrder = bg != null ? bg.sortingOrder + 1 : 1;
        slotFlameGlow.sortingLayerName = sortingLayerName;
        slotFlameGlow.sortingOrder = sortingOrder;
        slotFlameOuter.sortingLayerName = sortingLayerName;
        slotFlameOuter.sortingOrder = sortingOrder + 1;
        slotFlameCore.sortingLayerName = sortingLayerName;
        slotFlameCore.sortingOrder = sortingOrder + 2;
    }

    static void EnsureRocketFlameSprite()
    {
        if (rocketFlameSprite != null)
            return;

        Texture2D texture = new(RocketFlameTextureWidth, RocketFlameTextureHeight, TextureFormat.RGBA32, false)
        {
            name = "Runtime Tile Rocket Flame Texture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Vector2 nozzle = new(RocketFlameTextureWidth * 0.92f, RocketFlameTextureHeight * 0.5f);
        for (int y = 0; y < RocketFlameTextureHeight; y++)
        {
            for (int x = 0; x < RocketFlameTextureWidth; x++)
            {
                float back = Mathf.Clamp01((nozzle.x - x) / (RocketFlameTextureWidth * 0.88f));
                float halfWidth = Mathf.Lerp(RocketFlameTextureHeight * 0.12f, RocketFlameTextureHeight * 0.43f, Mathf.Sin(back * Mathf.PI));
                float vertical = Mathf.Abs(y - nozzle.y);
                float edge = 1f - Mathf.SmoothStep(halfWidth * 0.72f, halfWidth, vertical);
                float taper = Mathf.SmoothStep(0f, 0.08f, back) * (1f - Mathf.SmoothStep(0.92f, 1f, back));
                float noise = Mathf.PerlinNoise(x * 0.085f, y * 0.12f);
                float lick = Mathf.Sin(back * 26f + noise * 3.2f) * 0.1f + noise * 0.12f;
                float alpha = Mathf.Clamp01((edge + lick) * taper);

                Color hot = new(1f, 0.96f, 0.46f, 1f);
                Color orange = new(1f, 0.42f, 0.03f, 1f);
                Color red = new(0.72f, 0.04f, 0.01f, 1f);
                Color smoke = new(0.14f, 0.06f, 0.04f, 1f);
                Color color = Color.Lerp(hot, orange, Mathf.SmoothStep(0.08f, 0.36f, back));
                color = Color.Lerp(color, red, Mathf.SmoothStep(0.34f, 0.72f, back));
                color = Color.Lerp(color, smoke, Mathf.SmoothStep(0.74f, 1f, back));
                color.a = Mathf.Clamp01(alpha * Mathf.Lerp(1.75f, 0.75f, back));

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        rocketFlameSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, RocketFlameTextureWidth, RocketFlameTextureHeight),
            new Vector2(0.92f, 0.5f),
            96f,
            0,
            SpriteMeshType.FullRect);
    }

    static Material TileTrailMaterial()
    {
        if (tileTrailMaterial != null)
            return tileTrailMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
            return null;

        tileTrailMaterial = new Material(shader)
        {
            name = "TileGo Slot Trail Runtime Material"
        };
        if (tileTrailMaterial.HasProperty("_Color"))
            tileTrailMaterial.SetColor("_Color", Color.white);
        return tileTrailMaterial;
    }
}
