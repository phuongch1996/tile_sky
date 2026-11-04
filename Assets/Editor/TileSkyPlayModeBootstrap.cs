using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// HomeScene and GameScene use persistent managers created by the first build scene.
// Keep direct Play from either scene on the same initialization path as the player.
[InitializeOnLoad]
public static class TileSkyPlayModeBootstrap
{
    const string HomePath = "Assets/BaseProject/Scenes/HomeScene.unity";
    const string GamePath = "Assets/BaseProject/Scenes/GameScene.unity";
    const string BootstrapPath = "Assets/Samples/iOS 14 Advertising Support/1.0.1/01 Context Screen/Context Screen Sample.unity";
    const string StartGameKey = "TileSky.PlayMode.StartGame";
    static double deadline;

    static TileSkyPlayModeBootstrap()
    {
        EditorApplication.delayCall += ConfigureStartScene;
        EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnActiveSceneChanged(Scene previous, Scene next) => ConfigureStartScene();

    internal static void ConfigureStartScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        var activePath = SceneManager.GetActiveScene().path;
        var currentStart = EditorSceneManager.playModeStartScene;
        var currentPath = AssetDatabase.GetAssetPath(currentStart);
        if (currentStart != null && currentPath != BootstrapPath)
            return;

        EditorSceneManager.playModeStartScene = activePath == HomePath || activePath == GamePath
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(BootstrapPath)
            : null;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            SessionState.SetBool(StartGameKey, SceneManager.GetActiveScene().path == GamePath
                && AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) == BootstrapPath);
        }
        else if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(StartGameKey, false))
        {
            deadline = EditorApplication.timeSinceStartup + 60;
            EditorApplication.update += StartRequestedGame;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.update -= StartRequestedGame;
            SessionState.SetBool(StartGameKey, false);
            if (state == PlayModeStateChange.EnteredEditMode)
                ConfigureStartScene();
        }
    }

    static void StartRequestedGame()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup > deadline)
        {
            EditorApplication.update -= StartRequestedGame;
            return;
        }

        if (!GameConfig.isGameLoaded)
            return;

        // First-time players are sent directly to level 1 by LoadingCamera.
        if (SceneManager.GetActiveScene().path == GamePath)
        {
            EditorApplication.update -= StartRequestedGame;
            return;
        }

        if (GameConfig.GetCurrentLevel() <= 1 || HomeScript.instance == null
            || SceneManager.GetActiveScene().path != HomePath)
            return;

        EditorApplication.update -= StartRequestedGame;
        HomeScript.instance.Play();
    }
}
