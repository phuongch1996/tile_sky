using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "BGSprites", menuName = "BGSprites", order = 1)]
public class BGSpritesSO : ScriptableObject
{
    public BG[] bgSprites;
    public int? randomIndex;
    private void OnValidate()
    {
        foreach (var bg in bgSprites)
        {
            if (bg.bg != null && string.IsNullOrWhiteSpace(bg.title))
                bg.title = bg.bg.name;
        }
    }
    public Sprite GetBGSprite(int index)
    {
        return bgSprites[index % bgSprites.Length].bg;
    }
    public string GetBGTitle(int index)
    {
        return bgSprites[index % bgSprites.Length].title;
    }
    public int GetBGCount()
    {
        return bgSprites.Length;
    }
    public Sprite GetRandomBGSprite()
    {
        if (!randomIndex.HasValue)
            randomIndex = Random.Range(0, bgSprites.Length);
        return bgSprites[randomIndex.Value].bg;
    }
}
[Serializable]
public class BG
{
    public Sprite bg;
    public string title;
}
