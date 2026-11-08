using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class HeavenlyWinPreview
{
    const string Request = "Temp/HeavenlyWinPreview.request";

    [InitializeOnLoadMethod]
    static void Initialize() => EditorApplication.update += Check;

    static void Check()
    {
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        try { Preview(); }
        catch (Exception error) { File.WriteAllText("Temp/HeavenlyWinPreview.error.txt", error.ToString()); }
    }

    [MenuItem("Tools/TileSpace/Preview Heavenly Win")]
    public static void Preview()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        RenderTexture previous = RenderTexture.active;
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            var canvasGO = new GameObject("Win preview", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraGO = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            var camera = cameraGO.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.22f, 0.34f);
            camera.transform.position = new Vector3(0, 0, -10);
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var popup = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/TileGOPrefab/UI/Game/PopupWin.prefab"), canvasGO.transform).GetComponent<PopupWin>();
            popup.panel.SetActive(true);
            foreach (var safeArea in popup.GetComponentsInChildren<SafeAreaView>(true))
            {
                safeArea.enabled = false;
                var rect = (RectTransform)safeArea.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            typeof(PopupWin).GetMethod("EnsureHeavenlyWin", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(popup, null);
            bool hasHeader = false;
            foreach (var image in popup.GetComponentsInChildren<Image>(true))
                hasHeader |= image.name == "Victory header" && image.sprite != null;
            if (!hasHeader) throw new InvalidOperationException("Import the standalone Victory header before rendering.");
            popup.win1GO.SetActive(false);
            popup.win2GO.SetActive(true);
            popup.progressGO.SetActive(true);
            popup.txNext.text = "LEVEL 7";
            popup.txProgress.text = "1/4";
            popup.txCoinsGain.gameObject.SetActive(true);
            popup.txCoinsGain.text = "9";
            var icon = (Image)typeof(PopupWin).GetField("adRewardIcon", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(popup);
            var boosters = (Sprite[])typeof(PopupWin).GetField("adRewardBoosterIcons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(popup);
            icon.sprite = boosters[1];
            popup.txAdReward.text = "x1";
            Directory.CreateDirectory("Docs");
            foreach (var size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340) })
            {
                target = new RenderTexture(size.x, size.y, 24);
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                typeof(PopupWin).GetMethod("FitHeavenlyWin", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(popup, null);
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                capture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                capture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                capture.Apply();
                File.WriteAllBytes("Docs/HeavenlyWin-LevelGold-" + size.x + "x" + size.y + ".png", capture.EncodeToPNG());
                Object.DestroyImmediate(capture);
                capture = null;
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(target);
                target = null;
            }
            File.WriteAllText("Temp/HeavenlyWinPreview.done", "Rendered both portrait sizes without game or ad service calls.");
        }
        finally
        {
            RenderTexture.active = previous;
            if (capture) Object.DestroyImmediate(capture);
            if (target) Object.DestroyImmediate(target);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
