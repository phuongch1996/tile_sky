using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventCard : MonoBehaviour
{
    public GameObject linkGO;
    public GameObject linkFillGO;
    public TextMeshProUGUI txIndex;
    public GameObject currentGO;
    public GameObject lockGO;
    public GameObject claimableGO;
    public GameObject claimedGO;
    public GameObject prizesGO;
    public GameObject[] prizes;
    public Image goldImg;
    public TextMeshProUGUI txPrizeAm;
    public void InitPrize(int index, bool link, int[] prizesIndex, int amount)
    {
        foreach (var p in prizes)
            p.SetActive(false);
        foreach (int prize in prizesIndex)
        {
            int normalizedPrize = GameConfig.NormalizePowerUpPrizeType(prize);
            if (normalizedPrize < 0 || normalizedPrize >= prizes.Length)
                continue;
            prizes[normalizedPrize].SetActive(true);
        }
        goldImg.sprite = UnityAuthentication.instance.GetGoldSprite(amount);
        txPrizeAm.text = amount.ToString();
        txIndex.text = index.ToString();
        linkGO.SetActive(link);
    }
    public void Init(int state, bool linkFill = false) //0:unavail 1:current 2:claimable 3:claimed
    {
        linkFillGO.SetActive(linkFill);
        switch (state)
        {
            case 0:
                lockGO.SetActive(true);
                currentGO.SetActive(false);
                claimableGO.SetActive(false);
                claimedGO.SetActive(false);
                break;
            case 1:
                lockGO.SetActive(false);
                currentGO.SetActive(true);
                claimableGO.SetActive(false);
                claimedGO.SetActive(false);
                break;
            case 2:
                lockGO.SetActive(false);
                currentGO.SetActive(true);
                claimableGO.SetActive(true);
                claimedGO.SetActive(false);
                break;
            case 3:
                lockGO.SetActive(false);
                currentGO.SetActive(false);
                claimableGO.SetActive(false);
                claimedGO.SetActive(true);
                break;
        }
    }
}
