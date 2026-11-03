using DG.Tweening;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupChallengeReward : Popup
{
    const string descFormat = "You placed <color=red>{0}</color> on <color=yellow>{1}";
    public Button btConfirm;
    public Image goldImage;
    public GameObject boosterGO;
    public GameObject[] boosterPrizes;
    public TextMeshProUGUI txGoldAm;
    public TextMeshProUGUI txBoosterAm;
    public TextMeshProUGUI txDesc;
    public GameObject prizeGO;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            ClaimPrize();
            btConfirm.onClick.AddListener(() => Close());
        });
    }
    int gold;
    BPPrizes booster;
    public void OpenChallengeReward(DateTime date, int rank, bool test = false)//, int gold, int booster)
    {
        (gold, booster) = UnityAuthentication.GetDailyChallengePrizeFromRank(rank);
        boosterGO.SetActive(booster != null && booster.prizeTypes.Length > 0);
        goldImage.sprite = UnityAuthentication.instance.GetGoldSprite(gold);
        txGoldAm.text = $"{gold}";
        if (booster != null)
        {
            txBoosterAm.text = $"{booster.prizeAmount}";
            for (int i = 0; i < 4; i++)
                boosterPrizes[i].SetActive(i != 3 && booster.prizeTypes.Select(GameConfig.NormalizePowerUpPrizeType).Contains(i));
        }
        txDesc.text = string.Format(descFormat, rank + 1, date.ToString("d"));// $"You placed <color=red>{rank + 1}</color> on <color=yellow>{date:d}";
        if (!test)
            UnityAuthentication.instance.PastLeaderboardPrizeClaim();
        Open();
    }
    public override void Open()
    {
        btConfirm.onClick.RemoveAllListeners();
        base.Open();
    }
    void ClaimPrize()
    {
        var go = Instantiate(prizeGO, prizeGO.transform.parent);
        GameConfig.AddGold(gold);
        if (booster != null)
        {
            GameConfig.AddPowerUps(booster.prizeAmount, booster.prizeTypes);
        }
        //go.transform.localPosition = Vector3.zero;
        go.transform.DOBlendableLocalMoveBy(new Vector3(0, 150), 1).OnStart(() =>
        {
            MaskableGraphic[] graphics = go.GetComponentsInChildren<MaskableGraphic>();
            foreach (var graphic in graphics)
            {
                graphic.DOFade(0, 1 - Time.deltaTime);
            }
        }).OnComplete(() => { Destroy(go); });
    }
}
