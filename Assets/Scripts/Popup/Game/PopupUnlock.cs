using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUnlock : Popup
{
    public Button btGoToFeature;
    public Button btNextLevel;
    public GameObject[] features;
    readonly string[] featureDesc = new string[]
    {
        "Log in daily to earn special rewards.",//dailyreward
        //"Log in daily to receive free prizes.",
        "Watch ads to get free coins.",//freegold
        "Collect coins in the piggy bank and get a big reward when it's full.",//piggybank
        //"Log in for 7 days to earn special rewards.",
        //"Join events to earn exciting rewards.",
        //"A new challenge every day with rewards."
        "Win the match, score points, climb to the top.",//rank
        "Unlock rewards upon clearing the level.",//battlepass
    };
    public TextMeshProUGUI txDesc;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btGoToFeature.onClick.AddListener(() => GoToFeature());
            btNextLevel.onClick.AddListener(() => Next());
        });
    }
    int feature;
    public void OpenUnlock(int feature)
    {
        this.feature = feature;
        foreach (var go in features)
            go.SetActive(false);
        features[feature].SetActive(true);
        txDesc.text = featureDesc[feature];
        Open();
    }
    public override void Open()
    {
        btGoToFeature.onClick.RemoveAllListeners();
        btNextLevel.onClick.RemoveAllListeners();
        base.Open();
    }
    public override void Close()
    {
        base.Close();
    }
    void GoToFeature()
    {
        switch (feature)
        {
            case 0:
                GameScript.instance.OnDailyLogin();
                break;
            case 1:
                GameScript.instance.OnOpenFreeGold();
                break;
            case 2:
                GameScript.instance.OnOpenPiggyBank();
                break;
            case 3:
                GameScript.instance.OnOpenRank();
                break;
            case 4:
                GameScript.instance.OnOpenBattlePass();
                break;
            default:
                break;
        }
        //Close();
    }
    void Next()
    {
        GameScript.instance.Next();
    }
}
