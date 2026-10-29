using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyObject : MonoBehaviour
{
    public Button btDaily;
    public TextMeshProUGUI txDay;
    [Header("Day Label")]
    public Image imgDay;
    public Sprite daySprite;
    public Sprite todayDaySprite;
    public GameObject prizeGO;
    public Image goldImg;
    public TextMeshProUGUI txGoldAm;
    public GameObject boosterGO;
    public TextMeshProUGUI txBoosterAm;
    [Header("State Visuals")]
    public GameObject todayGO;
    public GameObject claimedGO;
    public GameObject imageGO;
    public GameObject coverGO;

    Sprite cachedDaySprite;
    bool cachedDefaultDaySprite;

    public void InitPrize(DailyPrize dailyPrize)
    {
        goldImg.gameObject.SetActive(dailyPrize.goldAm > 0);
        goldImg.sprite = UnityAuthentication.instance.GetGoldSprite(dailyPrize.goldAm);
        boosterGO.SetActive(dailyPrize.boosterAm > 0);
        txGoldAm.text = dailyPrize.goldAm.ToString();
        txBoosterAm.text = dailyPrize.boosterAm.ToString();
    }
    public void Init(int state, string txDay) //0:unavail 1:today 2:claimed
    {
        this.txDay.text = txDay;
        SetDaySprite(state == 1);
        switch (state)
        {
            case 0:
                coverGO.SetActive(true);
                todayGO.SetActive(false);
                SetActiveIfAssigned(claimedGO, false);
                SetActiveIfAssigned(imageGO, false);
                btDaily.interactable = false;
                this.txDay.gameObject.SetActive(true);
                break;
            case 1:
                coverGO.SetActive(true);
                todayGO.SetActive(true);
                SetActiveIfAssigned(claimedGO, false);
                SetActiveIfAssigned(imageGO, false);
                btDaily.interactable = true;
                this.txDay.gameObject.SetActive(true);
                break;
            case 2:
                coverGO.SetActive(false);
                todayGO.SetActive(false);
                SetActiveIfAssigned(claimedGO, true);
                SetActiveIfAssigned(imageGO, true);
                btDaily.interactable = false;
                this.txDay.gameObject.SetActive(true);
                break;
        }
    }

    void SetActiveIfAssigned(GameObject go, bool active)
    {
        if (go)
            go.SetActive(active);
    }

    void SetDaySprite(bool today)
    {
        if (!imgDay)
            return;

        if (!cachedDefaultDaySprite)
        {
            cachedDaySprite = imgDay.sprite;
            cachedDefaultDaySprite = true;
        }

        Sprite normalSprite = daySprite ? daySprite : cachedDaySprite;
        Sprite targetSprite = today && todayDaySprite ? todayDaySprite : normalSprite;
        if (targetSprite)
            imgDay.sprite = targetSprite;
    }
}
