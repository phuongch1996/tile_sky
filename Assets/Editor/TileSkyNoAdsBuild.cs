using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class TileSkyNoAdsBuild
{
    const string Request = "Temp/TileSkyNoAdsBuild.request";
    [InitializeOnLoadMethod]
    static void Watch() { EditorApplication.update += Check; }
    static void Check()
    {
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        Build();
    }
    [MenuItem("Tools/Tile Sky/Build No Ads APK")]
    public static void Build()
    {
        Directory.CreateDirectory("Builds/Android");
        bool bundle = EditorUserBuildSettings.buildAppBundle;
        bool customSigning = PlayerSettings.Android.useCustomKeystore;
        try
        {
            EditorUserBuildSettings.buildAppBundle = false;
            PlayerSettings.Android.useCustomKeystore = false;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Builds/Android/TileSky-NoAds.apk",
                target = BuildTarget.Android,
                extraScriptingDefines = new[] { "TILESKY_NO_ADS" },
                options = BuildOptions.None
            });
            File.WriteAllText("Builds/Android/NoAds-build-result.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nSize: " + report.summary.totalSize);
            if (report.summary.result != BuildResult.Succeeded) Debug.LogError("Tile Sky no-ads APK build failed.");
        }
        catch (Exception e) { File.WriteAllText("Builds/Android/NoAds-build-result.txt", e.ToString()); Debug.LogException(e); }
        finally { EditorUserBuildSettings.buildAppBundle = bundle; PlayerSettings.Android.useCustomKeystore = customSigning; }
    }
}
