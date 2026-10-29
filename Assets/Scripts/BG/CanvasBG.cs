using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasBG : MonoBehaviour
{
    public List<Image> images;
    public List<Image> bgImages;
    public Image bg;
    public BGSpritesSO bgSpritesSO;
    public bool isRandom;
    public Action OnBGChange;
    private void Awake()
    {
        GameConfig.SetBgTypeDefault();
        SetBG();
        //ChangeBG();
    }
    public void SetBG()
    {
        foreach (Image bg in bgImages)
        {
            if (isRandom)
                bg.sprite = bgSpritesSO.GetRandomBGSprite();
            else
                bg.sprite = bgSpritesSO.GetBGSprite(GameConfig.GetCurrentBackground());
            OnRectTransformDimensionsChange();
        }
    }
    private void OnEnable()
    {
        OnBGChange += OnRectTransformDimensionsChange;
    }
    private void OnDisable()
    {
        OnBGChange -= OnRectTransformDimensionsChange;
    }
    private void OnRectTransformDimensionsChange()
    {
        var rect = GetComponent<RectTransform>().rect;
        foreach (Image img in images)
        {
            if (img)
            {
                var bgRect = img.sprite.rect;
                if ((rect.width / rect.height) < (bgRect.width / bgRect.height))
                    img.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width * 2, rect.height);
                else
                    img.GetComponent<RectTransform>().sizeDelta = new Vector2(rect.width, rect.width * 2);
            }
        }
    }
}
