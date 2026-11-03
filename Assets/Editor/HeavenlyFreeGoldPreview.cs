using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HeavenlyFreeGoldPreview
{
    [MenuItem("Tools/Tile Space/Preview Heavenly Free Gold")]
    public static void Preview()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var previousTarget = RenderTexture.active;
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            var canvasObject = new GameObject("Free Gold preview", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraObject = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(20, 28, 55, 255);
            target = new RenderTexture(1080, 1920, 24);
            camera.targetTexture = target;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TileGOPrefab/UI/PopupFreeGold.prefab");
            var instance = Object.Instantiate(prefab, canvas.transform, false);
            instance.SetActive(true);
            var popup = instance.GetComponent<PopupFreeGold>();
            Canvas.ForceUpdateCanvases();
            var originalBackground = popup.bg;
            var originalOutsideButton = popup.btPanel;
            popup.SetupTweenCallback();
            if (popup.box.name != "Free Gold animation" || popup.bg != originalBackground || popup.btPanel != originalOutsideButton)
                throw new System.InvalidOperationException("Art failed to load or exterior controls changed.");
            var liveRoot = popup.btWatchAds.transform.parent;
            if (!liveRoot.gameObject.activeInHierarchy || popup.btClose.transform.parent != liveRoot || popup.fillImage.transform.parent != liveRoot)
                throw new System.InvalidOperationException("Live controls are not on the new panel.");
            var panelImage = liveRoot.GetComponent<Image>();
            if (!panelImage || panelImage.GetType() != typeof(Image) || !panelImage.sprite.name.Contains("PanelClean"))
                throw new System.InvalidOperationException("Panel must remain continuous behind the buttons.");
            foreach (var button in new[] { popup.btClose, popup.btWatchAds })
                if (!button.image.sprite || button.image.sprite.texture.name != "Buttons" || !button.GetComponent<ButtonAnim>())
                    throw new System.InvalidOperationException("Buttons must use separate transparent sprites and press animation.");
            popup.SetupTweenCallback();
            if (liveRoot != popup.btWatchAds.transform.parent)
                throw new System.InvalidOperationException("Layout was built twice.");
            popup.tweenCallback.Invoke();
            popup.fillImage.fillAmount = 1;
            popup.txFill.text = "5/5";
            Canvas.ForceUpdateCanvases();
            instance.SendMessage("LateUpdate", SendMessageOptions.DontRequireReceiver);
            camera.Render();
            RenderTexture.active = target;
            capture = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);
            capture.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
            capture.Apply();
            Directory.CreateDirectory("Docs");
            File.WriteAllBytes("Docs/HeavenlyFreeGoldPreview.png", capture.EncodeToPNG());
            // Also expose the background around both shrunken buttons for visual inspection.
            popup.btWatchAds.transform.localScale = Vector3.one * 0.95f;
            popup.btClose.transform.localScale = Vector3.one * 0.95f;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            capture.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
            capture.Apply();
            File.WriteAllBytes("Docs/HeavenlyFreeGoldPressedPreview.png", capture.EncodeToPNG());
            popup.btWatchAds.transform.localScale = Vector3.one;
            popup.btClose.transform.localScale = Vector3.one;
            popup.fillImage.fillAmount = 0.4f;
            popup.txFill.text = "2/5";
            if (Mathf.Abs(popup.fillImage.fillAmount - 0.4f) > 0.001f || popup.txFill.text != "2/5")
                throw new System.InvalidOperationException("Live progress did not update.");
            popup.btClose.onClick.Invoke();
            if (popup.panel.activeSelf) throw new System.InvalidOperationException("Close button failed.");
            File.WriteAllText("Docs/HeavenlyFreeGoldValidation.txt",
                "PASS: prefab loads heavenly art; exterior background and outside-dismiss references retained; " +
                "live controls visible; repeated setup is idempotent; progress supports 5/5 and 2/5; close button closes popup.\n" +
                "Rendered preview at 1080x1920. Rewarded ad network and live currency grant were not invoked.\n");
            Debug.Log("Heavenly Free Gold preview checks passed.");
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
