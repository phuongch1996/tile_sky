using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PopupBattlePass : Popup
{
    public BattlePassCard bpCardTemplate;
    public Transform bpCardParent;
    public List<BattlePassCard> bpCardList;
    public Image bpFill;
    //public Slider bpSlider;
    public Button btClose;
    public Slider progressSlider;
    public TextMeshProUGUI txProgress;
    public TextMeshProUGUI txProgressGoal;
    public TextMeshProUGUI txTimer;
    public CodelessIAPButton iapButtonBattlePass;
    public TextMeshProUGUI txBattlePassPrice;
    bool isInit;
    private void Awake0()
    {
        iapButtonBattlePass.productId = IAPManager.tileevo_battlepass;
        iapButtonBattlePass.button.onClick.AddListener(() => IAPManager.instance.OnIAPClick());
        txBattlePassPrice.text = GameConfig.GetLocalizedPrice(IAPManager.tileevo_battlepass);
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            for (int i = 0; i < bpCardList.Count; i++)
                SetupButton(bpCardList[i], i);
        });
    }
    public override void Open()
    {
        btClose.onClick.RemoveAllListeners();
        foreach (var card in bpCardList)
        {
            card.btFreeClaim.onClick.RemoveAllListeners();
            card.btAdsClaim.onClick.RemoveAllListeners();
            card.btVipClaim.onClick.RemoveAllListeners();
        }
        if (!isInit)
            SetupBPCards();
        BPProgress();
        base.Open();
    }
    //int tfmIndex;
    void Tfm(int tfmIndex)
    {
        bpCardParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 320 * tfmIndex - bpCardParent.parent.GetComponent<RectTransform>().rect.height + 520);
    }
    private void Update()
    {
        UpdateTimerText();
    }
    private void UpdateTimerText()
    {
        TimeSpan ts = BattlePassManager.instance.TimeLeftBattlePass();
        if (ts.TotalDays >= 1)
            txTimer.text = $"{Mathf.Floor((float)ts.TotalDays)}d {ts:%h}h";
        else
            txTimer.text = $"{ts:%h}h {ts:%m}m";
    }
    void SetupBPCards()
    {
        Awake0();
        bpCardList = new();
        for (int i = 0; i < BattlePassManager.battlePass.Count; i++)
        {
            BattlePassCard card = i == 0 ? bpCardTemplate : Instantiate(bpCardTemplate, bpCardParent);
            card.SetupCard(BattlePassManager.battlePass[i]);
            bpCardList.Add(card);
        }
        isInit = true;
    }
    public void BPProgress()
    {
        iapButtonBattlePass.gameObject.SetActive(!BattlePassManager.IsBattlePassVIP());
        var bpData = BattlePassManager.instance.bpData;
        int winCount = BattlePassManager.GetBPWinCount();
        var (index, winFill) = BattlePassManager.GetBPWinFill(winCount);
        int tfmIndex = index;
        Debug.Log($"index{index} winFill{winFill}");
        bpFill.fillAmount = (index - 1 + winFill) / (bpCardList.Count - 1);
        //bpSlider.minValue = 0;
        //bpSlider.maxValue = bpCardList.Count - 1;
        //bpSlider.value = index - 1 + winFill;
        int fillMax = BattlePassManager.battlePass[index < BattlePassManager.battlePass.Count ? index : BattlePassManager.battlePass.Count - 1].winAmount;
        txProgress.text = $"{winCount}/{fillMax}";
        txProgressGoal.text = $"{fillMax}";
        progressSlider.minValue = 0;
        progressSlider.maxValue = fillMax;
        progressSlider.value = winCount;
        for (int i = 0; i < bpCardList.Count; i++)
        {
            if (BattlePassManager.battlePass[i].winAmount <= winCount)
            {
                bpCardList[i].notAvail.SetActive(false);
                if (bpData.freeClaimed.Contains(i))
                    bpCardList[i].SetupFree(2);
                else
                    bpCardList[i].SetupFree(1);

                if (bpData.adsClaimed.Contains(i))
                    bpCardList[i].SetupAds(2);
                else
                    bpCardList[i].SetupAds(1);

                if (bpData.vipClaimed.Contains(i))
                    bpCardList[i].SetupVip(2);
                else
                    bpCardList[i].SetupVip(1);
            }
            else
            {
                bpCardList[i].notAvail.SetActive(true);
                bpCardList[i].SetupFree(0);
                bpCardList[i].SetupAds(0);
                bpCardList[i].SetupVip(0);
            }
            bpCardList[i].numCurrentGO.SetActive(index > 0 ? (i == index - 1) : i == 0);
        }
        Tfm(tfmIndex);
    }
    void SetupButton(BattlePassCard bpCard, int index)
    {
        bpCard.btFreeClaim.onClick.AddListener(() =>
        {
            BattlePassManager.instance.bpData.ClaimFree(index);
            BattlePassManager.instance.UpdateData();
            ClaimPrize(bpCard.freePrizeGO, BattlePassManager.battlePass[index].bpPrizes[0]);
        });
        bpCard.btAdsClaim.onClick.AddListener(() =>
        {
            adsIndex = index;
            GameConfig.ShowRewardedAd(8);
        });
        bpCard.btVipClaim.onClick.AddListener(() =>
        {
            BattlePassManager.instance.bpData.ClaimVip(index);
            BattlePassManager.instance.UpdateData();
            ClaimPrize(bpCard.vipPrizeGO, BattlePassManager.battlePass[index].bpPrizes[2]);
        });
    }
    int adsIndex;
    public void AdsClaim()
    {
        BattlePassManager.instance.bpData.ClaimAds(adsIndex);
        BattlePassManager.instance.UpdateData();
        ClaimPrize(bpCardList[adsIndex].adsPrizeGO, BattlePassManager.battlePass[adsIndex].bpPrizes[1]);
    }
    public void ClaimPrize(GameObject prizeGO, BPPrizes bpPrizes)
    {
        if (bpPrizes.prizeTypes.Contains(4))
            GameConfig.AddGold(bpPrizes.prizeAmount);
        else
            GameConfig.AddPowerUps(bpPrizes.prizeAmount, bpPrizes.prizeTypes);
        BPProgress();
        var go = Instantiate(prizeGO, prizeGO.transform.parent);
        //go.transform.position = prizeGO.transform.position;
        go.transform.DOBlendableLocalMoveBy(new Vector3(0, 100), 1).OnStart(() =>
        {
            MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
            foreach (var graphic in graphics)
            {
                graphic.DOFade(0, 1 - Time.deltaTime);
            }
        }).OnComplete(() => { Destroy(go); });
    }
}
