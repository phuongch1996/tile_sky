using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class PopupPiggyBank : Popup
{
    public Image piggyProgressFill;
    public TextMeshProUGUI txPiggy;
    public RectTransform rectTfmMax1;
    public TextMeshProUGUI txMax1Piggy;
    public RectTransform rectTfmMax2;
    public TextMeshProUGUI txMax2Piggy;
    public Button btBuyPiggyBank;
    public CodelessIAPButton iapButtonPiggyBank;
    public TextMeshProUGUI txDesc;
    public TextMeshProUGUI txTimer;
    public Button btClose;
    public Button btPanel;
    bool isInit;
    private void Awake0()
    {
        if (isInit) return;
        btBuyPiggyBank.onClick.RemoveAllListeners();
        btBuyPiggyBank.onClick.AddListener(() =>
        {
            if (iapButtonPiggyBank.enabled)
                IAPManager.instance.OnIAPClick();
            else
                ToastScript.instance.Toast(2);
        });
        iapButtonPiggyBank.productId = IAPManager.tilespace_piggybank;
        iapButtonPiggyBank.GetComponentInChildren<TextMeshProUGUI>().text = GameConfig.GetLocalizedPrice(IAPManager.tilespace_piggybank);
        txMax1Piggy.text = $"{GameConfig.piggyMax1}";
        txMax2Piggy.text = $"{GameConfig.piggyMax2}";
        float max2 = 718;
        float max1 = (float)GameConfig.piggyMax1 / GameConfig.piggyMax2 * max2;
        rectTfmMax1.anchoredPosition = new Vector2(max1, 0);
        rectTfmMax2.anchoredPosition = new Vector2(max2, 0);
        isInit = true;
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            btPanel.onClick.AddListener(() => Close());
        });
    }
    public void SetupPiggyBank()
    {
        int piggyBank = GameConfig.GetPiggyBank();
        txDesc.text = piggyBank >= GameConfig.piggyMax2 ?
            "Piggy bank is Full! Hurry up and buy it at a great price" : "Save more golds by beating levels to buy at a great price";
        btBuyPiggyBank.interactable = piggyBank >= GameConfig.piggyMax1;
        iapButtonPiggyBank.enabled = piggyBank >= GameConfig.piggyMax1;
        piggyProgressFill.fillAmount = (float)piggyBank / GameConfig.piggyMax2;
        txPiggy.text = $"{piggyBank}/{GameConfig.piggyMax2}";
    }
    public override void Open()
    {
        Awake0();
        btClose.onClick.RemoveAllListeners();
        btPanel.onClick.RemoveAllListeners();
        SetupPiggyBank();
        base.Open();
    }
    //public override void Close()
    //{
    //    base.Close();
    //}
    private void Update()
    {
        var (isPiggyBank, timeLeft) = TimeManager.instance.TimeEndPiggyBank();
        if (isPiggyBank)
        {
            txTimer.text = "reset in\n" + $"{Mathf.Floor((float)timeLeft.TotalHours):00}:{timeLeft:mm}:{timeLeft:ss}";
        }
        else
        {
            Close();
        }
    }
}
