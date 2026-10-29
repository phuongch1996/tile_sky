using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassCard : MonoBehaviour
{
    //public Sprite[] goldSprites;
    public GameObject notAvail;
    [Header("free")]
    public GameObject freeAvailGO;
    public GameObject freePrizeGO;
    public Button btFreeClaim;
    public GameObject[] freePrizes;
    public Image freeGoldImg;
    public TextMeshProUGUI txFreeAmount;
    public GameObject freeClaimedGO;
    [Header("ads")]
    public GameObject adsAvailGO;
    public GameObject adsPrizeGO;
    public Button btAdsClaim;
    public GameObject adsGO;
    public GameObject[] adsPrizes;
    public Image adsGoldImg;
    public TextMeshProUGUI txAdsAmount;
    public GameObject adsClaimedGO;
    [Header("vip")]
    public GameObject vipAvailGO;
    public GameObject vipPrizeGO;
    public Button btVipClaim;
    public GameObject lockGO;
    public GameObject[] vipPrizes;
    public Image vipGoldImg;
    public TextMeshProUGUI txVipAmount;
    public GameObject vipClaimedGO;
    [Header("num")]
    public GameObject numCurrentGO;
    public TextMeshProUGUI txNum;

    public void SetupCard(BattlePass battlePass)
    {
        //gold
        foreach (var b in freePrizes)
            b.SetActive(false);
        foreach (var b in adsPrizes)
            b.SetActive(false);
        foreach (var b in vipPrizes)
            b.SetActive(false);
        SetupCard(battlePass.bpPrizes[0], freePrizes, freeGoldImg, txFreeAmount);
        SetupCard(battlePass.bpPrizes[1], adsPrizes, adsGoldImg, txAdsAmount);
        SetupCard(battlePass.bpPrizes[2], vipPrizes, vipGoldImg, txVipAmount);
        txNum.text = battlePass.winAmount.ToString();
    }
    void SetupCard(BPPrizes bpPrize, GameObject[] prizes, Image goldImg, TextMeshProUGUI txAmount)
    {
        foreach (var i in bpPrize.prizeTypes)
        {
            int normalizedPrize = GameConfig.NormalizePowerUpPrizeType(i);
            if (normalizedPrize < 0 || normalizedPrize >= prizes.Length)
                continue;
            prizes[normalizedPrize].SetActive(true);
        }
        goldImg.sprite = UnityAuthentication.instance.GetGoldSprite(bpPrize.prizeAmount);
        txAmount.text = $"{bpPrize.prizeAmount}";
    }
    public void SetupFree(int state) //unavail, avail, claimed
    {
        switch (state)
        {
            case 0:
                freeAvailGO.SetActive(false);
                btFreeClaim.gameObject.SetActive(false);
                freeClaimedGO.SetActive(false);
                break;
            case 1:
                freeAvailGO.SetActive(true);
                btFreeClaim.gameObject.SetActive(true);
                freeClaimedGO.SetActive(false);
                break;
            case 2:
                freeAvailGO.SetActive(false);
                btFreeClaim.gameObject.SetActive(false);
                freeClaimedGO.SetActive(true);
                break;
        }
    }
    public void SetupAds(int state)
    {
        switch (state)
        {
            case 0:
                adsAvailGO.SetActive(false);
                btAdsClaim.gameObject.SetActive(false);
                adsGO.SetActive(false);
                adsClaimedGO.SetActive(false);
                break;
            case 1:
                adsAvailGO.SetActive(true);
                btAdsClaim.gameObject.SetActive(true);
                adsGO.SetActive(true);
                adsClaimedGO.SetActive(false);
                break;
            case 2:
                adsAvailGO.SetActive(false);
                btAdsClaim.gameObject.SetActive(false);
                adsGO.SetActive(false);
                adsClaimedGO.SetActive(true);
                break;
        }
    }
    public void SetupVip(int state)
    {
        switch (state)
        {
            case 0:
                vipAvailGO.SetActive(false);
                btVipClaim.gameObject.SetActive(false);
                lockGO.SetActive(false);
                vipClaimedGO.SetActive(false);
                break;
            case 1:
                vipAvailGO.SetActive(true);
                if (BattlePassManager.IsBattlePassVIP())
                {
                    btVipClaim.gameObject.SetActive(true);
                    lockGO.SetActive(false);
                }
                else
                {
                    btVipClaim.gameObject.SetActive(false);
                    lockGO.SetActive(true);
                }
                vipClaimedGO.SetActive(false);
                break;
            case 2:
                vipAvailGO.SetActive(false);
                btVipClaim.gameObject.SetActive(false);
                lockGO.SetActive(false);
                vipClaimedGO.SetActive(true);
                break;
        }
    }
}
