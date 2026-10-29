using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupStreakEvent : Popup
{
    public EventCard eventCardTemplate;
    public Transform eventCardParent;
    public List<EventCard> eventCards;
    public Button btClose;
    public Button btInfo;
    public Slider progressSlider;
    public TextMeshProUGUI txProgress;
    public TextMeshProUGUI txCurrentStreak;
    public TextMeshProUGUI txHighestStreak;
    public TextMeshProUGUI txTimer;
    public Button btCollect;
    public Button btCollectAds;
    public GameObject prizeGO;
    public GameObject[] prizes;
    public Image goldImg;
    public TextMeshProUGUI txPrizeAm;
    bool isInit;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            btInfo.onClick.AddListener(() => Info());
            btCollect.onClick.AddListener(() => ClaimPrize());
            btCollectAds.onClick.AddListener(() => GameConfig.ShowRewardedAd(3));
        });
    }
    public override void Open()
    {
        btClose.onClick.RemoveAllListeners();
        btInfo.onClick.RemoveAllListeners();
        btCollect.onClick.RemoveAllListeners();
        btCollectAds.onClick.RemoveAllListeners();
        if (!isInit)
            SetupEventCards();
        EventProgress();
        //Invoke(nameof(Tfm), Time.deltaTime);
        base.Open();
    }
    void Tfm()
    {
        int eventPrizeClaimed = EventManager.GetEventPrizeClaimed();
        eventCardParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -eventPrizeClaimed * 250);
    }
    //public override void Close()
    //{
    //    base.Close();
    //}
    void Info()
    {
        if (HomeScript.instance)
            HomeScript.instance.OpenEventTutorial();
        if (GameScript.instance)
            GameScript.instance.OpenEventTutorial();
    }
    private void Update()
    {
        UpdateTimerText();
    }
    void UpdateTimerText()
    {
        TimeSpan ts = EventManager.instance.GetEventTimeLeft();
        if (ts.TotalDays >= 1)
            txTimer.text = $"{Mathf.Floor((float)ts.TotalDays)}d {ts:%h}h";
        else
            txTimer.text = $"{ts:%h}h {ts:%m}m";
    }
    public void SetupEventCards()
    {
        for (int i = 0; i < EventManager.eventStreakPrizes.Count; i++)
        {
            EventPrize prize = EventManager.eventStreakPrizes[i];
            EventCard card = i == 0 ? eventCardTemplate : Instantiate(eventCardTemplate, eventCardParent);
            card.InitPrize(prize.collect + EventManager.GetStreakAdditive(), i < EventManager.eventStreakPrizes.Count - 1, prize.prizeType, prize.prizeAmount);
            eventCards.Add(card);
        }
        isInit = true;
    }
    public void EventProgress()
    {
        prizeGO.SetActive(false);
        //btCollect.interactable = false;
        //btCollectAds.interactable = false;
        btCollect.gameObject.SetActive(false);
        btCollectAds.gameObject.SetActive(false);
        var eventPrizes = EventManager.eventStreakPrizes;
        int eventPrizeClaimed = EventManager.GetEventPrizeClaimed();
        int currentStreak = EventManager.GetCurrentStreak();
        txCurrentStreak.text = $"Current Streak: {currentStreak}";
        int highestStreak = EventManager.GetHighestStreak();
        int rewardProgress = Mathf.Max(currentStreak, highestStreak);
        txHighestStreak.text = $"Highest Streak: {highestStreak}";
        progressSlider.minValue = 0;
        if (eventPrizeClaimed < eventPrizes.Count)
        {
            progressSlider.maxValue = eventPrizes[eventPrizeClaimed].collect + EventManager.GetStreakAdditive();
            progressSlider.value = rewardProgress;
            txProgress.text = $"win streak {rewardProgress}/{progressSlider.maxValue}";
            prizeGO.SetActive(true);
            foreach (var p in prizes)
                p.SetActive(false);
            foreach (int prize in eventPrizes[eventPrizeClaimed].prizeType)
            {
                int normalizedPrize = GameConfig.NormalizePowerUpPrizeType(prize);
                if (normalizedPrize >= 0 && normalizedPrize < prizes.Length)
                    prizes[normalizedPrize].SetActive(true);
            }
            int amount = eventPrizes[eventPrizeClaimed].prizeAmount;
            goldImg.sprite = UnityAuthentication.instance.GetGoldSprite(amount);
            txPrizeAm.text = $"{amount}";
        }
        else
        {
            progressSlider.maxValue = Mathf.Max(1, currentStreak);
            progressSlider.value = currentStreak;
            txProgress.text = $"{currentStreak}";
            prizeGO.SetActive(false);
        }

        for (int i = 0; i < eventCards.Count; i++)
        {
            int state;
            if (i < eventPrizeClaimed)
            {
                state = 3;
            }
            else if (i == eventPrizeClaimed)
            {
                if (rewardProgress >= progressSlider.maxValue)
                {
                    //btCollect.interactable = true;
                    //btCollectAds.interactable = true;
                    btCollect.gameObject.SetActive(true);
                    btCollectAds.gameObject.SetActive(true);
                    state = 2;
                }
                else
                {
                    state = 1;
                }
            }
            else
            {
                state = 0;
            }
            bool linkFill = state == 3;
            eventCards[i].Init(state, linkFill);
        }
        Tfm();
    }
    public void ClaimPrize(bool ads = false)
    {
        int eventPrizeClaimed = EventManager.GetEventPrizeClaimed();
        EventManager.ClaimCurrentPrize();
        EventPrize prize = EventManager.eventStreakPrizes[eventPrizeClaimed];

        if (prize.prizeType.Contains(4))
            GameConfig.AddGold(prize.prizeAmount * (ads ? 3 : 1));
        else
            GameConfig.AddPowerUps(prize.prizeAmount * (ads ? 3 : 1), prize.prizeType);

        GameObject go = Instantiate(prizeGO, transform);
        go.transform.position = prizeGO.transform.position;
        if (ads)
        {
            var txAmount = go.GetComponentInChildren<TextMeshProUGUI>();
            txAmount.text = (int.Parse(txAmount.text) * 3).ToString();
        }
        go.transform.DOBlendableLocalMoveBy(new Vector3(0, 100), 1).OnStart(() =>
        {
            MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
            foreach (var graphic in graphics)
            {
                graphic.DOFade(0, 1 - Time.deltaTime);
            }
        }).OnComplete(() => { Destroy(go); });
        EventProgress();
    }
}
