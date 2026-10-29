using TMPro;
using UnityEngine;

public class PopupAdBreak : Popup
{
    float timer = 3;
    public TextMeshProUGUI txTimer;
    public bool isAdBreak;
    public override void Open()
    {
        isAdBreak = true;
        timer = 3;
        txTimer.text = timer.ToString("0.0");
        base.Open();
    }
    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 0;
            GameConfig.ShowInterstitialAd(true);
        }
        txTimer.text = timer.ToString("0.0");
    }
    public override void Close()
    {
        isAdBreak = false;
        //if (GameScript.instance)
        //    GameScript.instance.timer = 0;
        base.Close();
    }
}
