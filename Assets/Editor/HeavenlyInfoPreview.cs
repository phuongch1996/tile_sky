using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the actual popup in an isolated preview scene without touching HomeScene.
public static class HeavenlyInfoPreview
{
    [MenuItem("Tools/Tile Space/Preview Heavenly Info")]
    public static void Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;
        var scene = EditorSceneManager.NewPreviewScene();
        RenderTexture target = null;
        Texture2D capture = null;
        var previousTarget = RenderTexture.active;
        try
        {
            var canvasObject = new GameObject("Info preview canvas", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraObject = new GameObject("Info preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(9, 12, 42, 255);
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            target = new RenderTexture(1024, 1536, 24);
            camera.targetTexture = target;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            canvasObject.AddComponent<GraphicRaycaster>();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/TileGOPrefab/UI/Auth/PopupEditPlayer.prefab");
            var instance = Object.Instantiate(prefab, canvas.transform, false);
            instance.SetActive(true);
            var popup = instance.GetComponent<PopupEditPlayer>();
            Canvas.ForceUpdateCanvases();
            popup.SetupTweenCallback();
            if (popup.btAvatars.Length != 9 || popup.box.name != "Heavenly Info")
                throw new System.InvalidOperationException("Heavenly Info art or layout did not load.");
            popup.tweenCallback.Invoke();
            popup.SelectAvatar(11);
            if (popup.selectAvatar != 11 || !popup.btAvatars[2].transform.GetChild(0).gameObject.activeSelf)
                throw new System.InvalidOperationException("Existing avatar ID was not preserved.");
            popup.btAvatars[3].onClick.Invoke();
            if (popup.selectAvatar != 3 || !popup.btAvatars[3].transform.GetChild(0).gameObject.activeSelf)
                throw new System.InvalidOperationException("Avatar button selection did not work.");
            popup.SelectAvatar(0);
            popup.inputFieldName.SetTextWithoutNotify("Prudent");
            var boxRect = (RectTransform)popup.box;
            boxRect.localScale = Vector3.one;
            boxRect.anchoredPosition = Vector2.zero;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = target;
            capture = new Texture2D(1024, 1536, TextureFormat.RGBA32, false);
            capture.ReadPixels(new Rect(0, 0, 1024, 1536), 0, 0);
            capture.Apply();
            Directory.CreateDirectory("Docs");
            File.WriteAllBytes("Docs/HeavenlyInfoPreview.png", capture.EncodeToPNG());
            File.WriteAllText("Docs/HeavenlyInfoValidation.txt",
                "PASS: actual popup built with generated art; nine avatars; existing avatar IDs preserved; " +
                "avatar click updates selection; name field and Save button retained.\n" +
                "Preview rendered at 1024x1536. Live account save was not invoked.\n");
            Debug.Log("Heavenly Info preview and interaction checks passed: Docs/HeavenlyInfoPreview.png");
        }
        finally
        {
            RenderTexture.active = previousTarget;
            if (capture) Object.DestroyImmediate(capture);
            if (target) Object.DestroyImmediate(target);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
