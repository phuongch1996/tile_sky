using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideUIButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] bool hideAds = true;
    [SerializeField] List<GameObject> uiToHide = new();
    bool isHidden;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Toggle()
    {
        isHidden = !isHidden;

        if (isHidden && hideAds)
        {
            GameConfig.HideBannerAd();
            GameConfig.HideMrec();
        }

        foreach (GameObject ui in uiToHide)
        {
            if (ui != null)
            {
                if (ui == gameObject || transform.IsChildOf(ui.transform))
                    continue;

                ui.SetActive(!isHidden);
            }
        }
    }
}
