using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GameplayProgressReset
{
    const string RequestPath = "Temp/ResetPlayerData.request";
    const string ResultPath = "Temp/ResetPlayerData.result";
    const string PendingKey = "TileSpace.ResetPlayerData.Pending";

    static GameplayProgressReset()
    {
        EditorApplication.delayCall += ProcessRequest;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                ProcessRequest();
        };
    }

    [MenuItem("Tools/Tile Space/Reset All Player Data (New Player)")]
    public static void ResetPlayerData()
    {
        // Stop first so OnApplicationQuit cannot save the old board again.
        SessionState.SetBool(PendingKey, true);
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }
        ClearNow();
    }

    static void ProcessRequest()
    {
        if (File.Exists(RequestPath) || SessionState.GetBool(PendingKey, false))
            ResetPlayerData();
    }

    static void ClearNow()
    {
        // All local player data (level, board, tutorials, currency, boosters,
        // events, profile and settings) uses this project's PlayerPrefs.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        if (GameplayProgressSave.HasSave || GameConfig.GetCurrentLevel() != 1 || GameConfig.GetGold() != 0)
        {
            Debug.LogError("Failed to reset player data to a new player.");
            return;
        }
        SessionState.SetBool(PendingKey, false);
        if (File.Exists(RequestPath))
        {
            File.Delete(RequestPath);
            File.WriteAllText(ResultPath, "All local player data reset; level=1, gold=0, savedBoard=false: " + System.DateTime.UtcNow.ToString("o"));
        }
        Debug.Log("All local player data reset. Press Play to start as a new player at level 1 with tutorials and default rewards.");
    }
}
