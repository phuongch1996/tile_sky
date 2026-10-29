using Beebyte.Obfuscator;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //public TextAsset levelCSV;
    public LevelsSO levelsSO;
    public TileSpritesSO tileSpritesSO;
    public static LevelManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
        //levelsSO.LevelSetup();
    }
    //private void Start()
    //{
    //    levelsSO.LevelSetup();
    //}
    //public List<Leveljson> levels;
    //void LevelSetup()
    //{
    //    levels = new();
    //    string csvText = levelCSV.text;
    //    string[] datas = csvText.Split("\n");
    //    //Debug.Log(datas.Length);
    //    for (int i = 0; i < datas.Length; i++)
    //    {
    //        if (!string.IsNullOrEmpty(datas[i]))
    //        {
    //            datas[i] = datas[i].Trim().Replace("\"\"", "\"").TrimStart('\"').TrimEnd('\"');
    //            //Debug.Log(datas[i]);
    //            levels.Add(JsonConvert.DeserializeObject<Leveljson>(datas[i]));
    //        }
    //    }
    //}
    //public Leveljson GetLevel(int level)
    //{
    //    return levelsSO.GetLevel(level);
    //}
    public int GetLevelDifficulty(int level)
    {
        if (level % 10 == 4)
            return 1;
        else if (level % 10 == 9)
            return 2;
        else
            return 0;
    }
    public static GAME_MODE GetGameMode(int mode)
    {
        return mode switch
        {
            1 => GAME_MODE.tile_random,
            2 => GAME_MODE.tile_double,
            3 => GAME_MODE.tile_freeze,
            4 => GAME_MODE.tile_gold,
            5 => GAME_MODE.tile_2up,
            6 => GAME_MODE.tile_rainbow,
            7 => GAME_MODE.tile_fog,
            //8 => GAME_MODE.tile_color,
            _ => GAME_MODE.normal
        };
    }
    public static int GetGameModeIndex(GAME_MODE gameMode)
    {
        return gameMode switch
        {
            GAME_MODE.tile_random => 1,
            GAME_MODE.tile_double => 2,
            GAME_MODE.tile_freeze => 3,
            GAME_MODE.tile_gold => 4,
            GAME_MODE.tile_2up => 5,
            GAME_MODE.tile_rainbow => 6,
            GAME_MODE.tile_fog => 7,
            //GAME_MODE.tile_color => 8,
            GAME_MODE.normal => 0,
            _ => -1
        };
    }
}

[Skip]
[Serializable]
public class Leveljson
{
    public List<string> data;
    public int tileCount;
    public int itemTypeCount;
    public int mode;
    public int param;
    public Leveljson(List<string> data, int itemTypeCount, int mode, int param)
    {
        this.data = data;
        this.tileCount = 0;
        foreach (string d in data)
            foreach (char c in d)
                if (c == '1')
                    this.tileCount++;
        this.itemTypeCount = itemTypeCount;
        this.mode = mode;
        if (mode != 0)
            this.param = param;
    }
}
