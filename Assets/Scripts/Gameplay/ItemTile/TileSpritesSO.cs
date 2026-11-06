using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TileSprites", menuName = "TileSprites", order = 0)]
public class TileSpritesSO : ScriptableObject
{
    public List<Sprite> itemSprites;
    public List<Sprite> itemSprites2;
    public Sprite rainbowSprite;
    public Sprite goldSprite;
    public Sprite bgSprite;
    public Sprite fogSprite;
    public List<ColorTileSprites> itemColorSprites;

    List<Sprite> GetCurrentSprites()
    {
        return Common.config_tile_set == 1 ? itemSprites2 : itemSprites;
    }

    public List<Sprite> GetUsingSprites(int count)
    {
        var sprites = new List<Sprite>(GetCurrentSprites());
        return GameConfig.GetRandomCount(count, sprites.ToArray());
    }

    public List<Sprite> GetAllSprites()
    {
        return new List<Sprite>(GetCurrentSprites());
    }
}

[Serializable]
public class ColorTileSprites
{
    public List<Sprite> itemSprites;
}
