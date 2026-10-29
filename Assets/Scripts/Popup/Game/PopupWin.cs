using DG.Tweening;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupWin : Popup
{
    public Button btNext;
    public Button btHome;
    public TextMeshProUGUI txHomeRewardMultiplier;
    //public SkeletonGraphic[] stars;
    //int starsGain;
    int coinsGain;
    const int MinHomeRewardMultiplier = 10;
    const int MaxHomeRewardMultiplier = 15;
    int homeRewardMultiplier;
    bool homeRewardClaimed;
    public bool openWin = false;
    bool levelProgress;
    public TextMeshProUGUI txCoinsGain;
    [Space]
    public GameObject win1GO;
    public GameObject progressGO;
    public GameObject firstSegment;
    public GameObject secondSegment;
    public List<Image> progressSegments;
    public TextMeshProUGUI txProgress;
    public GameObject piggyGO;
    public Image piggyProgressFill;
    public TextMeshProUGUI txPiggy;
    [Space]
    bool streakWin;
    public GameObject streakGO;
    public Slider streakProgressSlider;
    public TextMeshProUGUI txStreak;
    public GameObject[] streakPrizeGOS;
    public Image streakGoldImg;
    public TextMeshProUGUI txStreakPrizeAm;
    public Image imgStreakPrizeGold;
    [Space]
    bool goldenTile;
    public GameObject goldenTileGO;
    public Slider goldenTileProgressSlider;
    public TextMeshProUGUI txGoldenTile;
    public GameObject[] goldenTilePrizeGOS;
    public Image goldenTileGoldImg;
    public TextMeshProUGUI txGoldenTilePrizeAm;
    public Image imgGoldenTilePrizeGold;
    [Space]
    bool combo;
    public GameObject comboGO;
    public Slider comboProgressSlider;
    public TextMeshProUGUI txCombo;
    public GameObject[] comboPrizeGOS;
    public Image comboGoldImg;
    public TextMeshProUGUI txComboPrizeAm;
    public Image imgComboPrizeGold;
    //[Space]
    //public Sprite[] goldSprites;
    [Space]
    public GameObject win2GO;
    public TextMeshProUGUI txNext;
    [SerializeField] private SkeletonGraphic skeletonAnimation;
    [SerializeField] private string openAnim = "open";
    [SerializeField] private string runAnim = "run";

    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            //btNext.onClick.AddListener(() => Next());
            //btHome.onClick.AddListener(() => Home());
            Win1();
            //foreach (var spine in stars)
            //    spine.AnimationState.SetAnimation(0, "2", true);
        });
    }

    public void PlayOpenThenRun()
    {
        if (skeletonAnimation == null)
            return;

        skeletonAnimation.AnimationState.ClearTracks();
        skeletonAnimation.AnimationState.SetAnimation(0, openAnim, false);
        skeletonAnimation.AnimationState.AddAnimation(0, runAnim, true, 0f);
    }

    public override void Open()
    {
        PlayOpenThenRun();
        FirebaseManager.instance.LogLevelComplete(GameConfig.level, GameConfig.GetActionDeltaTime(GameConfig.startLevelKey));

        if (BattlePassManager.instance.isBattlePass)
            BattlePassManager.AddBPWinCount();

        if (GameController.instance.isGoldTile)
            goldenTile = EventManager.AddGoldTileCount(EventTileExtend.eventCounter);

        if (GameController.instance.isComboEvent)
            combo = EventManager.AddComboCount(GameTextScript.comboCount);

        streakWin = EventManager.instance.StreakWin();

        levelProgress = GameConfig.level >= GameConfig.GetCurrentLevel();
        if (levelProgress)
            GameConfig.SetCurrentLevel(GameConfig.level + 1);

        UnityAuthentication.instance.WinAddScore();

        openWin = true;

        int coinsPerStar = LevelManager.instance.GetLevelDifficulty(GameConfig.level) switch
        {
            0 => 3,
            1 => 5,
            2 => 7,
            _ => 3
        };
        if (GameConfig.gameMode != GAME_MODE.tile_gold)
            coinsGain = 3 * coinsPerStar;
        else
            coinsGain = GoldTileVariant.goldCount;

        homeRewardClaimed = false;
        homeRewardMultiplier = Random.Range(MinHomeRewardMultiplier, MaxHomeRewardMultiplier + 1);
        if (txHomeRewardMultiplier != null)
            txHomeRewardMultiplier.text = $"x{homeRewardMultiplier}";
        txCoinsGain.text = $"{coinsGain}";
        AudioManager.instance.PlaySFXOneShot(AudioManager.SFX.WIN);

        btHome.onClick.RemoveAllListeners();
        btNext.onClick.RemoveAllListeners();
        win1GO.SetActive(false);
        win2GO.SetActive(false);
        txNext.text = "Level " + (GameConfig.level + 1);

        base.Open();
    }
    void Win1()
    {
        GameScript.instance.TopBarUpdateGolds(txCoinsGain.transform, coinsGain, GameConfig.gameMode != GAME_MODE.tile_gold ? 1 : 10);
        bool progressFull = false;
        progressGO.SetActive(levelProgress);
        var (isPiggyBank, timeLeft) = TimeManager.instance.TimeEndPiggyBank();
        if (isPiggyBank)
        {
            piggyGO.SetActive(GameConfig.level >= Common.config_unlock_piggybank && GameConfig.piggyAdd > 0);
        }
        else
        {
            piggyGO.SetActive(false);
            GameConfig.SetPiggyBankLevelsUnlock();
        }
        streakGO.SetActive(streakWin);
        goldenTileGO.SetActive(goldenTile);
        comboGO.SetActive(combo);
        var bgLevelRange = GameConfig.GetBGLevelRange(GameConfig.level);
        var bonusLevelRange = GameConfig.GetBonusLevelRange(GameConfig.level);
        if (progressGO.activeSelf)
        {
            int progress = GameConfig.level - bonusLevelRange.start;
            int count = bonusLevelRange.end - bonusLevelRange.start + 1;
            for (int i = 2; i < count; i++)
            {
                GameObject seg = Instantiate(secondSegment, secondSegment.transform.parent);
                progressSegments.Add(seg.transform.GetChild(0).GetComponent<Image>());
            }
            for (int i = 0; i < progressSegments.Count; i++)
            {
                if (i < progress)
                    progressSegments[i].gameObject.SetActive(true);
                else if (i == progress)
                {
                    progressSegments[i].gameObject.SetActive(true);
                    progressSegments[i].fillAmount = 0;
                }
                else
                    progressSegments[i].gameObject.SetActive(false);
            }
            //DOTween.To(() => progressSegments[progress].fillAmount, x => progressSegments[progress].fillAmount = x, 1, 0.5f).SetDelay(0.5f);
            progressSegments[progress].DOFillAmount(1, 0.5f).SetDelay(0.5f);
            int levelLeft = count - progress - 1;
            progressFull = levelLeft == 0;
            txProgress.text = $"Only {levelLeft} Level{(levelLeft > 1 ? "s" : "")} Left";
        }
        if (piggyGO.activeSelf)
        {
            txPiggy.text = $"{GameConfig.GetPiggyBank()}/{GameConfig.piggyMax2}";
            GameConfig.AddPiggyBank(GameConfig.piggyAdd);
            piggyProgressFill.DOFillAmount((float)GameConfig.GetPiggyBank() / GameConfig.piggyMax2, 0.5f).SetDelay(0.5f).OnStart(() => txPiggy.text = $"{GameConfig.GetPiggyBank()}/{GameConfig.piggyMax2}");
            piggyGO.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f);
        }
        if (goldenTileGO.activeSelf)
        {
            goldenTileGO.transform.DOBlendableLocalMoveBy(new Vector3(0, 500), 0.25f).SetInverted(true);
            int eventPrizeClaimed = EventManager.GetEventPrizeClaimed();
            var eventPrizes = EventManager.eventCollectPrizes;
            var prize = eventPrizes[eventPrizeClaimed];
            foreach (var go in goldenTilePrizeGOS)
                go.SetActive(false);
            foreach (var type in prize.prizeType)
                goldenTilePrizeGOS[type].SetActive(true);
            goldenTileGoldImg.sprite = UnityAuthentication.instance.GetGoldSprite(prize.prizeAmount);
            txGoldenTilePrizeAm.text = $"{prize.prizeAmount}";
            int goldTileCount = EventManager.GetGoldTileCount();
            goldenTileProgressSlider.minValue = eventPrizeClaimed <= 0 ? 0 : eventPrizes[eventPrizeClaimed - 1].collect;
            goldenTileProgressSlider.maxValue = eventPrizeClaimed < eventPrizes.Count ? eventPrizes[eventPrizeClaimed].collect : 1;
            goldenTileProgressSlider.value = goldTileCount - EventTileExtend.eventCounter;
            txGoldenTile.text = $"{goldenTileProgressSlider.value - goldenTileProgressSlider.minValue}/{goldenTileProgressSlider.maxValue - goldenTileProgressSlider.minValue}";
            goldenTileProgressSlider.DOValue(goldTileCount, 0.5f).SetDelay(0.5f).OnStart(() =>
            {
                txGoldenTile.text = $"{goldTileCount - goldenTileProgressSlider.minValue}/{goldenTileProgressSlider.maxValue - goldenTileProgressSlider.minValue}";
            });
            goldenTileGO.transform.DOBlendableLocalMoveBy(new Vector3(0, 500), 0.25f).SetDelay(5);
        }
        if (streakGO.activeSelf)
        {
            streakGO.transform.DOBlendableLocalMoveBy(new Vector3(0, 500), 0.25f).SetInverted(true);
            int eventPrizeClaimed = EventManager.GetEventPrizeClaimed();
            var eventPrizes = EventManager.eventStreakPrizes;
            var prize = eventPrizes[eventPrizeClaimed];
            foreach (var go in streakPrizeGOS)
                go.SetActive(false);
            foreach (var type in prize.prizeType)
                streakPrizeGOS[type].SetActive(true);
            streakGoldImg.sprite = UnityAuthentication.instance.GetGoldSprite(prize.prizeAmount);
            txStreakPrizeAm.text = $"{prize.prizeAmount}";
            int currentStreak = EventManager.GetCurrentStreak();
            int collect = prize.collect + EventManager.GetStreakAdditive();
            streakProgressSlider.minValue = 0;
            streakProgressSlider.maxValue = collect;
            streakProgressSlider.value = currentStreak - 1;
            txStreak.text = $"win streak {currentStreak - 1}/{collect}";
            streakProgressSlider.DOValue(currentStreak, 0.5f).SetDelay(0.5f).OnStart(() =>
            {
                txStreak.text = $"win streak {currentStreak}/{collect}";
            });
            streakGO.transform.DOBlendableLocalMoveBy(new Vector3(0, 500), 0.25f).SetDelay(5);
        }
        if (comboGO)
        {
            comboGO.transform.DOBlendableLocalMoveBy(new Vector3(0, 500), 0.25f).SetInverted(true);
            int eventPrizeClaimed = EventManager.GetEventPrizeClaimed();
            var eventPrizes = EventManager.eventComboPrizes;
            var prize = eventPrizes[eventPrizeClaimed];
            foreach (var go in comboPrizeGOS)
                go.SetActive(false);
            foreach (var type in prize.prizeType)
                comboPrizeGOS[type].SetActive(true);
            comboGoldImg.sprite = UnityAuthentication.instance.GetGoldSprite(prize.prizeAmount);
            txComboPrizeAm.text = $"{prize.prizeAmount}";
            int comboCount = EventManager.GetComboCount();
            comboProgressSlider.minValue = eventPrizeClaimed <= 0 ? 0 : eventPrizes[eventPrizeClaimed - 1].collect;
            comboProgressSlider.maxValue = eventPrizeClaimed < eventPrizes.Count ? eventPrizes[eventPrizeClaimed].collect : 1;
            comboProgressSlider.value = comboCount - GameTextScript.comboCount;
            txCombo.text = $"{comboProgressSlider.value - comboProgressSlider.minValue}/{comboProgressSlider.maxValue - comboProgressSlider.minValue}";
            comboProgressSlider.DOValue(comboCount, 0.5f).SetDelay(0.5f).OnStart(() =>
            {
                txCombo.text = $"{comboCount - comboProgressSlider.minValue}/{comboProgressSlider.maxValue - comboProgressSlider.minValue}";
            });
            comboGO.transform.DOBlendableLocalMoveBy(new Vector3(0, 500), 0.25f).SetDelay(5);
        }
        MaskableGraphic[] maskgraphics = win1GO.GetComponentsInChildren<MaskableGraphic>(false);
        foreach (var graphic in maskgraphics)
        {
            float a = graphic.color.a;
            graphic.DOFade(0, 0);
            graphic.DOFade(a, 0.5f);
        }
        win1GO.transform.DOScale(1, 1.5f).OnStart(() =>
        {
            win1GO.SetActive(true);
            PlayOpenThenRun();
        }).OnComplete(() =>
        {
            txCoinsGain.gameObject.SetActive(false);
            foreach (var graph in piggyGO.GetComponentsInChildren<MaskableGraphic>(false))
                graph.DOFade(0, 0.5f);
            if (progressFull)
            {
                GameScript.instance.OpenBonus(() =>
                {
                    if (GameConfig.level == bgLevelRange.end && (bgLevelRange.rangeIndex + 1) < GameScript.instance.canvasBG.bgSpritesSO.GetBGCount())
                    {
                        GameScript.instance.OnPreviewMap(bgLevelRange.rangeIndex + 1, () => Next());
                    }
                    Invoke(nameof(Win2), 1f);
                });
                return;
            }
            else if (levelProgress && GameConfig.level == bgLevelRange.end && (bgLevelRange.rangeIndex + 1) < GameScript.instance.canvasBG.bgSpritesSO.GetBGCount())
            {
                GameScript.instance.OnPreviewMap(bgLevelRange.rangeIndex + 1, () => Next());
            }
            else
            {
                if (GameConfig.InterEndLevel())
                {
                    GameConfig.ShowInterstitialAd();
                }
            }
            float waitTime = (goldenTileGO.activeSelf || streakGO.activeSelf || comboGO.activeSelf) ? 1f : 0.5f;
            Invoke(nameof(Win2), waitTime);
        });
    }
    void Win2()
    {
        if (GameConfig.GetCurrentLevel() == GameConfig.unlockDailyLogin && GameConfig.IsDailyLogin() && GameConfig.IsDailyLoginAvailable())//daily login
            GameScript.instance.OpenUnlock(0);
        else if (GameConfig.GetCurrentLevel() == GameConfig.unlockFreeGold)//freegold
            GameScript.instance.OpenUnlock(1);
        else if (GameConfig.GetCurrentLevel() == Common.config_unlock_piggybank && TimeManager.instance.TimeEndPiggyBank().isPiggyBank)//piggybank
            GameScript.instance.OpenUnlock(2);
        else if (GameConfig.GetCurrentLevel() == Common.config_unlock_rank && UnityAuthentication.IsLeaderboard())//rank
            GameScript.instance.OpenUnlock(3);
        //else if (GameConfig.GetCurrentLevel() == Common.config_unlock_battlepass && BattlePassManager.instance.isBattlePass)//battlepass
        //    GameScript.instance.OpenUnlock(4);
        else if (goldenTileProgressSlider.value >= goldenTileProgressSlider.maxValue ||
            streakProgressSlider.value >= streakProgressSlider.maxValue ||
            comboProgressSlider.value >= comboProgressSlider.maxValue)
            GameScript.instance.OnOpenEvent();
        //else if (GameConfig.level % 10 == 1 && GameConfig.level > 10)
        else if (GameConfig.GetCurrentLevel() > 10 && GameConfig.GetCurrentLevel() % 10 == 2)
        {
            GameScript.instance.rateUsPopup.WinOpen();
        }
        MaskableGraphic[] maskgraphics = win2GO.GetComponentsInChildren<MaskableGraphic>(false);
        foreach (var graphic in maskgraphics)
        {
            float a = graphic.color.a;
            graphic.DOFade(0, 0);
            graphic.DOFade(a, 0.5f);
        }
        //win2GO.transform.DOScale(1, 0).OnComplete(() => win2GO.SetActive(true));
        win2GO.transform.DOScale(1, 0.5f).OnStart(() => win2GO.SetActive(true))
            .OnComplete(() =>
            {
                btHome.onClick.AddListener(() => OnHomeRewardAdsClick());
                btNext.onClick.AddListener(() => Next());
            });
    }
    void Next()
    {
        GameScript.instance.Next();
    }
    void Home()
    {
        GameScript.instance.Home();
    }
    void OnHomeRewardAdsClick()
    {
        if (homeRewardClaimed)
        {
            Home();
            return;
        }

        if (!GameConfig.IsRewardedAdAvailable())
        {
            ToastScript.instance.Toast(1);
            return;
        }

        btHome.interactable = false;
        GameConfig.ShowRewardedAd(9);
    }
    public void ClaimHomeRewardAds()
    {
        if (homeRewardClaimed)
            return;

        homeRewardClaimed = true;
        int bonusCoins = coinsGain * (homeRewardMultiplier - 1);
        txCoinsGain.gameObject.SetActive(true);
        txCoinsGain.text = $"{bonusCoins}";

        if (bonusCoins <= 0)
        {
            Home();
            return;
        }

        int coinFlyValue = GameConfig.gameMode != GAME_MODE.tile_gold ? Mathf.Max(1, bonusCoins / 20) : 10;
        GameScript.instance.TopBarUpdateGolds(txCoinsGain.transform, bonusCoins, coinFlyValue, () =>
        {
            txCoinsGain.gameObject.SetActive(false);
            Home();
        });
    }
}
