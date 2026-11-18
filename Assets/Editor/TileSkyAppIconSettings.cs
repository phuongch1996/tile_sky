using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// APK and AAB share these PlayerSettings. Reapply before building so an open
// Editor cannot retain the previous launcher textures after a source update.
public sealed class TileSkyAppIconSettings : IPreprocessBuildWithReport
{
    const string IconPath = "Assets/Art/TileSky/AppIcon/TileSkyStackedTiles512.png";
    const string TransparentPath = "Assets/Art/TileSky/AppIcon/AdaptiveTransparent.png";
    public int callbackOrder => -1000;

    [InitializeOnLoadMethod]
    static void ScheduleApply() => EditorApplication.update += ApplyWhenReady;

    static void ApplyWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= ApplyWhenReady;
        Apply();
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android) Apply();
    }

    [MenuItem("Tools/Tile Sky/Apply App Icon")]
    public static void Apply()
    {
        var artwork = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        var transparent = AssetDatabase.LoadAssetAtPath<Texture2D>(TransparentPath);
        if (!artwork || !transparent)
            throw new BuildFailedException("Tile Sky launcher icon textures are missing.");

        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { artwork }, IconKind.Application);
        // Unity 6.6 removed the separate Legacy/Round API. The default icon
        // covers older launchers; Android-specific slots are now Adaptive only.
        // The complete, opaque illustration is the background. The clear
        // foreground preserves it without compositing a second copy over it.
        SetKind(AndroidPlatformIconKind.Adaptive, artwork, transparent);
        AssetDatabase.SaveAssets();
        Validate();
    }

    static void SetKind(PlatformIconKind kind, params Texture2D[] layers)
    {
        var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
        foreach (var icon in icons) icon.SetTextures(layers);
        PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
    }

    public static void Validate()
    {
        var artwork = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        var transparent = AssetDatabase.LoadAssetAtPath<Texture2D>(TransparentPath);
        if (!artwork || artwork.width != 512 || artwork.height != 512)
            throw new BuildFailedException("Tile Sky app icon must be 512x512.");
        int count = 0;
        foreach (var kind in new[] { AndroidPlatformIconKind.Adaptive })
        {
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            if (icons.Length == 0) throw new BuildFailedException("Missing Android icon slots: " + kind);
            foreach (var icon in icons)
            {
                var textures = icon.GetTextures();
                bool adaptive = kind == AndroidPlatformIconKind.Adaptive;
                if (textures.Length < (adaptive ? 2 : 1) || textures[0] != artwork ||
                    (adaptive && textures[1] != transparent))
                    throw new BuildFailedException("Incorrect Tile Sky icon: " + kind + " " + icon.width);
                count++;
            }
        }
        Directory.CreateDirectory("Temp");
        File.WriteAllText("Temp/TileSkyAppIcon-validation.txt",
            "PASS: 512x512 artwork; " + count + " Android Adaptive slots verified; default icon assigned.\n" + IconPath);
    }
}
