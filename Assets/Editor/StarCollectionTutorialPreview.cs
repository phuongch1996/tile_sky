using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class StarCollectionTutorialPreview
{
    const string Request = "Temp/StarCollectionTutorialPreview.request";

    [InitializeOnLoadMethod]
    static void CheckRequest()
    {
        if (File.Exists(Request)) EditorApplication.delayCall += RunRequest;
    }

    static void RunRequest()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunRequest;
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Request);
        try { Preview(); }
        catch (Exception error) { File.WriteAllText("Temp/StarCollectionTutorialPreview.error.txt", error.ToString()); }
    }

    [MenuItem("Tools/Tile Space/Preview Star Collection Tutorial")]
    public static void Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewPreviewScene();
        var previous = RenderTexture.active;
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            var canvasGO = new GameObject("Star collection preview", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraGO = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            var camera = cameraGO.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.18f, 0.3f);
            camera.transform.position = new Vector3(0, 0, -10);
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var popup = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/TileGOPrefab/UI/Event/PopupCollectionEventTutorial.prefab"), canvas.transform, false)
                .GetComponent<PopupEventTutorial>();
            foreach (var safeArea in popup.GetComponentsInChildren<SafeAreaView>(true))
            {
                safeArea.enabled = false;
                var rect = (RectTransform)safeArea.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            popup.EnsureStarCollectionTutorial();
            Directory.CreateDirectory("Docs");
            foreach (var size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340) })
            {
                target = new RenderTexture(size.x, size.y, 24);
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                popup.SendMessage("FitStarCollectionTutorial");
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                capture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                capture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                capture.Apply();
                File.WriteAllBytes("Docs/StarCollectionTutorial-" + size.x + "x" + size.y + ".png", capture.EncodeToPNG());
                Object.DestroyImmediate(capture);
                capture = null;
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(target);
                target = null;
            }
            File.WriteAllText("Temp/StarCollectionTutorialPreview.done", "Rendered both portrait sizes without game/service calls.");
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
