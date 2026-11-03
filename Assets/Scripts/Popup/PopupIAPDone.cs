using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupIAPDone : Popup
{
    public GameObject failGO;
    public GameObject successGO;
    public Image goldImg;
    public TextMeshProUGUI txGoldAm;
    public GameObject boostersGO;
    public TextMeshProUGUI txBoostersAm;
    public GameObject removeAdsGO;
    public TextMeshProUGUI txPackName;

    public Button btPanel;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btPanel.onClick.AddListener(() => Close());
        });
    }
    void Setup(string iapid, int goldAm = 0, int boostersAm = 0, bool isRemoveAds = false)
    {
        txGoldAm.text = $"x{goldAm}";
        goldImg.gameObject.SetActive(goldAm > 0);
        goldImg.sprite = UnityAuthentication.instance.GetGoldSprite(goldAm);
        txBoostersAm.text = $"x{boostersAm}";
        boostersGO.SetActive(boostersAm > 0);
        removeAdsGO.SetActive(isRemoveAds);
        txPackName.text = iapid switch
        {
            IAPManager.tilespace_removeads => "remove ads",
            IAPManager.tilespace_firstpack => "first pack",
            IAPManager.tilespace_megapack => "mega pack",
            IAPManager.tilespace_ultipack => "ultimate pack",
            IAPManager.tilespace_gold1 => "gold pack",
            IAPManager.tilespace_gold2 => "gold pack",
            IAPManager.tilespace_gold3 => "gold pack",
            IAPManager.tilespace_piggybank => "piggy bank",
            _ => ""
        };
    }
    public void DoneIAP(bool successful, string iapid = "")
    {
        //successGO.SetActive(false);
        //failGO.SetActive(false);
        if (successful)
        {
            successGO.SetActive(true);
            failGO.SetActive(false);
            if (string.Equals(iapid, IAPManager.tilespace_piggybank, System.StringComparison.Ordinal))
            {
                Setup(IAPManager.tilespace_piggybank, IAPManager.piggyBank);
            }
            else if (string.Equals(iapid, IAPManager.tilespace_battlepass, System.StringComparison.Ordinal))
            {
                return;
            }
            else
            {
                IAPValues iap = IAPManager.GetIAPValues(iapid);
                Setup(iapid, iap.goldAm, iap.boostersAm, iap.removeAds);
            }
        }
        else
        {
            successGO.SetActive(false);
            failGO.SetActive(true);
        }
        Open();
    }
    public override void Open()
    {
        btPanel.onClick.RemoveAllListeners();
        base.Open();
    }
}
