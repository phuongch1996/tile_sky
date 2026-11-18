using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class TileSkyTestAdsAabBuild
{
    const string Request = "Temp/TileSkyTestAdsAabBuild.request";

    [InitializeOnLoadMethod]
    static void Watch() => EditorApplication.update += Check;

    static void Check()
    {
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        Build();
    }

    [MenuItem("Tools/Tile Sky/Build Test Ads AAB")]
    public static void Build()
    {
        Directory.CreateDirectory("Temp/TestAdsAab");
        var platform = NamedBuildTarget.Android;
        string defines = PlayerSettings.GetScriptingDefineSymbols(platform);
        bool bundle = EditorUserBuildSettings.buildAppBundle;
        bool customSigning = PlayerSettings.Android.useCustomKeystore;
        try
        {
            // Build first; the output is signed with the upload key separately.
            EditorUserBuildSettings.buildAppBundle = true;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.SetScriptingDefineSymbols(platform, string.Join(";",
                defines.Split(';').Where(symbol => symbol != "TILESKY_NO_ADS")));
            TileSkyAppIconSettings.Apply();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = "Temp/TestAdsAab/TileSky-TestAds.aab",
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            File.WriteAllText("Temp/TestAdsAab/build-result.txt", report.summary.result +
                "\nErrors: " + report.summary.totalErrors + "\nSize: " + report.summary.totalSize +
                "\nUpload signing still required.");
            if (report.summary.result != BuildResult.Succeeded)
                Debug.LogError("Tile Sky test-ads AAB build failed.");
        }
        catch (Exception error)
        {
            File.WriteAllText("Temp/TestAdsAab/build-result.txt", error.ToString());
            Debug.LogException(error);
        }
        finally
        {
            EditorUserBuildSettings.buildAppBundle = bundle;
            PlayerSettings.Android.useCustomKeystore = customSigning;
            PlayerSettings.SetScriptingDefineSymbols(platform, defines);
        }
    }
}
