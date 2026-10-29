using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Levels", menuName = "Levels", order = 2)]
public class LevelsSO : ScriptableObject
{
    public TextAsset levelCSV;
    public List<Leveljson> levels;
    public List<TextAsset> challengeLevelCSVList;
    public List<ChallengeLevels> challengeLevels;
    public int LevelCount()
    {
        return levels.Count;
    }
#if UNITY_EDITOR
    [ContextMenu("LevelSetup")]
    public void LevelSetup()
    {
        //Debug.Log($"levelCSV\t{levelCSV.text}");
        levels = GetLevelFromTextAsset(levelCSV);
        challengeLevels = new();
        foreach (var textAsset in challengeLevelCSVList)
            challengeLevels.Add(new ChallengeLevels(GetLevelFromTextAsset(textAsset)));
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    List<Leveljson> GetLevelFromTextAsset(TextAsset textAsset)
    {
        List<Leveljson> levelsList = new();
        string csvText = textAsset.text;
        string[] datas = csvText.Split("\n");
        //Debug.Log(datas.Length);
        for (int i = 0; i < datas.Length; i++)
        {
            if (!string.IsNullOrEmpty(datas[i]))
            {
                datas[i] = datas[i].Trim().Replace("\"\"", "\"").TrimStart('\"').TrimEnd('\"');
                //Debug.Log(datas[i]);
                levelsList.Add(JsonConvert.DeserializeObject<Leveljson>(datas[i]));
            }
        }
        return levelsList;
    }
    public Leveljson GetLevel(int level)
    {
        if (level > levels.Count)
            return GetLevel(level - 1000);
        return levels[level - 1];
    }
    public Leveljson GetChallengeLevel(int[] date)
    {
        int dayOfYear = new DateTime(date[0], date[1], date[2]).DayOfYear;
        return GetChallengeLevel(dayOfYear);
    }
    public Leveljson GetChallengeLevel(int dayOfYear)
    {
        var levels = challengeLevels[dayOfYear % challengeLevels.Count];
        Debug.Log($"{dayOfYear % challengeLevels.Count} {dayOfYear / challengeLevels.Count}");
        return levels.levels[(dayOfYear / challengeLevels.Count) % levels.levels.Count];
    }
}
[Serializable]
public class ChallengeLevels
{
    public List<Leveljson> levels;
    public ChallengeLevels(List<Leveljson> levels)
    {
        this.levels = levels;
    }
}
