using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BattlePassLayoutPreview
{
    const string Request = "Temp/BattlePassLayoutPreview.request";

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
        catch (System.Exception error)
        {
            File.WriteAllText("Temp/BattlePassLayoutPreview.error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/Tile Space/Preview Battle Pass Layout")]
    public static void Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewPreviewScene();
        var oldAuth = UnityAuthentication.instance;
        var previousTarget = RenderTexture.active;
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            var canvasGO = new GameObject("Battle Pass preview", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraGO = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            var camera = cameraGO.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.blue;
            camera.transform.position = new Vector3(0, 0, -10);
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var popup = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/TileGOPrefab/UI/Event/PopupBattlePass.prefab"), canvas.transform, false).GetComponent<PopupBattlePass>();
            popup.gameObject.SetActive(true);
            popup.panel.SetActive(true);
            // Keep the preview independent of the Editor Game view's safe-area dimensions.
            foreach (var safeArea in popup.GetComponentsInChildren<SafeAreaView>(true))
            {
                safeArea.enabled = false;
                var safeRect = (RectTransform)safeArea.transform;
                safeRect.anchorMin = Vector2.zero;
                safeRect.anchorMax = Vector2.one;
                safeRect.offsetMin = safeRect.offsetMax = Vector2.zero;
            }
            var topBar = Object.FindObjectsByType<TopBarScript>(FindObjectsInactive.Include)
                .FirstOrDefault(bar => bar.goldsCounter);
            if (!topBar)
            {
                var barGO = new GameObject("Sample top bar", typeof(TopBarScript));
                SceneManager.MoveGameObjectToScene(barGO, scene);
                topBar = barGO.GetComponent<TopBarScript>();
                var gold = new GameObject("Sample gold counter", typeof(RectTransform), typeof(TextMeshProUGUI));
                gold.transform.SetParent(canvas.transform, false);
                var goldRect = gold.GetComponent<RectTransform>();
                goldRect.anchorMin = goldRect.anchorMax = new Vector2(0.8f, 0.95f);
                goldRect.sizeDelta = new Vector2(250, 80);
                var goldText = gold.GetComponent<TextMeshProUGUI>();
                goldText.font = popup.txTimer.font;
                goldText.text = "1200";
                topBar.goldsCounter = gold;
            }
            // Render the source counter in the preview canvas too, so a taller
            // target does not mix coordinates from the Editor's separate Game view.
            var counterSource = Object.Instantiate(topBar.goldsCounter, canvas.transform, false);
            var sourceRect = (RectTransform)counterSource.transform;
            sourceRect.anchorMin = sourceRect.anchorMax = Vector2.one;
            sourceRect.pivot = Vector2.one;
            sourceRect.anchoredPosition = new Vector2(-24, -36);
            sourceRect.localScale = Vector3.one;
            var sampleTopBarGO = new GameObject("Preview counter source", typeof(TopBarScript));
            SceneManager.MoveGameObjectToScene(sampleTopBarGO, scene);
            topBar = sampleTopBarGO.GetComponent<TopBarScript>();
            topBar.goldsCounter = counterSource;
            counterSource.SetActive(false);
            var authGO = new GameObject("Preview art provider", typeof(UnityAuthentication));
            SceneManager.MoveGameObjectToScene(authGO, scene);
            var auth = authGO.GetComponent<UnityAuthentication>();
            var coin = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/HomeTileFlex/Icon-Gold.png");
            auth.goldSprites = new[] { coin, coin, coin };
            UnityAuthentication.instance = auth;
            popup.EnsureHeavenlyHeader(topBar);
            popup.EnsureHeavenlyHeader(topBar);
            popup.progressSlider.maxValue = 100;
            popup.progressSlider.value = 25;
            popup.txProgress.text = "25/100";
            popup.txProgressGoal.text = "100";
            popup.txTimer.text = "ENDS IN 6d 05h 24m";
            popup.txBattlePassPrice.text = "Unavailable";
            popup.bpFill.fillAmount = 0.15f;
            for (int i = 0; i < 8; i++)
            {
                var card = i == 0 ? popup.bpCardTemplate : Object.Instantiate(popup.bpCardTemplate, popup.bpCardParent);
                card.SetupCard(BattlePassManager.battlePass[i]);
                card.SetupFree(i < 2 ? 1 : 0);
                card.SetupAds(i < 2 ? 1 : 0);
                card.SetupVip(0);
            }
            Directory.CreateDirectory("Docs");
            foreach (var size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340), new Vector2Int(1440, 1920) })
            {
                target = new RenderTexture(size.x, size.y, 24);
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                popup.SendMessage("LateUpdate");
                Canvas.ForceUpdateCanvases();
                popup.SendMessage("LateUpdate");
                var scroll = popup.bpCardParent.GetComponentInParent<ScrollRect>();
                var frame = scroll.transform.parent as RectTransform;
                var timerBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(frame, popup.txTimer.transform);
                var listBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(frame, scroll.viewport);
                if (timerBounds.min.y <= listBounds.max.y || scroll.viewport.rect.height <= 0 ||
                    !frame.rect.Contains((Vector2)timerBounds.min) || !frame.rect.Contains((Vector2)timerBounds.max))
                    throw new System.InvalidOperationException("Timer/list/frame overlap at " + size);
                if (popup.bg.type != Image.Type.Simple || popup.bg.rectTransform.anchorMin != Vector2.zero ||
                    popup.bg.rectTransform.anchorMax != Vector2.one)
                    throw new System.InvalidOperationException("Full-screen background still stretches a baked UI frame.");
                camera.Render();
                RenderTexture.active = target;
                capture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                capture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                capture.Apply();
                File.WriteAllBytes("Docs/BattlePassLayout-" + size.x + "x" + size.y + ".png", capture.EncodeToPNG());
                Object.DestroyImmediate(capture);
                capture = null;
                camera.targetTexture = null;
                RenderTexture.active = previousTarget;
                Object.DestroyImmediate(target);
                target = null;
            }
            File.WriteAllText("Docs/BattlePassLayoutValidation.txt",
                "PASS: actual prefab rendered at 1080x1920, 1080x2340 and 1440x1920. " +
                "Timer stays inside the frame and above the viewport; list has positive height; background is full-screen. " +
                "Preview uses sample progress/time and makes no service requests.\n");
        }
        finally
        {
            UnityAuthentication.instance = oldAuth;
            RenderTexture.active = previousTarget;
            if (capture) Object.DestroyImmediate(capture);
            if (target) Object.DestroyImmediate(target);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
