using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToastScript : MonoBehaviour
{
    public static ToastScript instance;
    private void Awake()
    {
        instance = this;
        textBox.DOFade(0, 0);
        textTMP.DOFade(0, 0);
    }
    public Image textBox;
    public TextMeshProUGUI textTMP;
    readonly string[] msg = new string[]
    {
        "Not enough coins!",
        "Ads not available!",
        "Not enough coins in piggy bank!",
        "Available in {0}",
        "No available move!",
        "Unlock at level {0}",
        "Thank you so much\nfor your rating!",
    };
    public void Toast(int index, params string[] parameters)
    {
        textBox.DOKill();
        textTMP.DOKill();
        textBox.DOFade(0, 0);
        textTMP.DOFade(0, 0);
        textTMP.text = string.Format(msg[index], parameters);
        textBox.DOFade(.75f, 0.25f);
        textTMP.DOFade(1, 0.25f);
        textBox.DOFade(0, 0.25f).SetDelay(1);
        textTMP.DOFade(0, 0.25f).SetDelay(1);
    }
}
