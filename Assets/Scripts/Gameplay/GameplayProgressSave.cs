using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameplayProgressSave
{
    const string SaveKey = "gameplay_progress_save";
    const int Version = 1;

    public static bool HasSave => PlayerPrefs.HasKey(SaveKey);

    public static GameplayProgressData Load()
    {
        if (!HasSave)
            return null;

        try
        {
            return JsonUtility.FromJson<GameplayProgressData>(PlayerPrefs.GetString(SaveKey));
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to load gameplay progress: {ex}");
            Clear();
            return null;
        }
    }

    public static bool TryLoad(out GameplayProgressData data)
    {
        data = Load();
        return data != null && data.version == Version && data.tiles != null && data.tiles.Count > 0;
    }

    public static void Save(GameplayProgressData data)
    {
        if (data == null || data.tiles == null || data.tiles.Count == 0)
            return;

        data.version = Version;
        data.savedAt = DateTime.UtcNow.ToString("o");
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}

[Serializable]
public class GameplayProgressData
{
    public int version;
    public string savedAt;
    public int level;
    public int gameMode;
    public int itemTypeCount;
    public int parameter;
    public bool isGoldTile;
    public bool isComboEvent;
    public bool isExtraSlot;
    public bool clearAnimPlay;
    public int comboCount;
    public int eventCounter;
    public int eventTotal;
    public List<int> itemSpriteIndexes = new();
    public List<int> undoSortingIndexes = new();
    public List<GameplayTileProgressData> tiles = new();
}

[Serializable]
public class GameplayTileProgressData
{
    public int prefabType;
    public int floor;
    public int sortingIndex;
    public float tilePosX;
    public float tilePosY;
    public int tileType;
    public int state;
    public int slottedIndex;
    public bool is2Up;
    public bool isRainbow;
    public bool isEventTile;
    public int eventCounter;
    public int linkKey;
    public bool linkUnlinked;
    public bool randomRevealed;
    public int iceCounter;
}
