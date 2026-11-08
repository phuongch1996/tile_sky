using System;
using System.IO;
using System.Reflection;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class HeavenlyLosePreview
{
    const string Request = "Temp/HeavenlyLosePreview.request";

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
        catch (Exception error)
        {
            File.WriteAllText("Temp/HeavenlyLosePreview.error.txt", error.ToString());
            Debug.LogException(error);
        }
    }

    [MenuItem("Tools/Tile Space/Preview Revive and Lose")]
    public static void Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewPreviewScene();
        var previousTarget = RenderTexture.active;
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            var canvasGO = new GameObject("Lose preview", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraGO = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            var camera = cameraGO.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.23f, 0.36f);
            camera.transform.position = new Vector3(0, 0, -10);
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var popup = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/TileGOPrefab/UI/Game/PopupLose.prefab"), canvas.transform, false).GetComponent<PopupLose>();
            popup.panel.SetActive(true);
            foreach (var safeArea in popup.GetComponentsInChildren<SafeAreaView>(true))
            {
                safeArea.enabled = false;
                var rect = (RectTransform)safeArea.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            typeof(PopupLose).GetMethod("EnsureHeavenlyLose", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(popup, null);
            if (popup.btReviveAds.image.sprite == null || popup.btReviveGold.image.sprite == null || popup.btRestart.image.sprite == null)
                throw new InvalidOperationException("Buttons must each own their visible sprite.");
            var eventGO = new GameObject("Preview events", typeof(EventSystem));
            SceneManager.MoveGameObjectToScene(eventGO, scene);
            var pointer = new PointerEventData(eventGO.GetComponent<EventSystem>());
            foreach (var button in new[] { popup.btReviveAds, popup.btReviveGold, popup.btRestart })
            {
                popup.lose1.SetActive(button != popup.btRestart);
                popup.lose2.SetActive(button == popup.btRestart);
                var anim = button.GetComponent<ButtonAnim>();
                if (!anim) throw new InvalidOperationException("Button press animation is missing.");
                Vector3 resting = button.transform.localScale;
                ((IPointerDownHandler)anim).OnPointerDown(pointer);
                DOTween.Complete(button.transform, true);
                if (Vector3.Distance(button.transform.localScale, resting * 0.95f) > 0.001f)
                    throw new InvalidOperationException("Button sprite did not scale on press.");
                ((IPointerExitHandler)anim).OnPointerExit(pointer);
                DOTween.Complete(button.transform, true);
                if (Vector3.Distance(button.transform.localScale, resting) > 0.001f)
                    throw new InvalidOperationException("Button scale did not restore.");
            }
            Directory.CreateDirectory("Docs");
            foreach (var size in new[] { new Vector2Int(1080, 1920), new Vector2Int(1080, 2340) })
            {
                target = new RenderTexture(size.x, size.y, 24);
                camera.targetTexture = target;
                Canvas.ForceUpdateCanvases();
                typeof(PopupLose).GetMethod("FitHeavenlyLose", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(popup, null);
                foreach (string state in new[] { "Revive1", "Revive3", "Revive5", "Lose" })
                {
                    bool revive = state != "Lose";
                    popup.lose1.SetActive(revive);
                    popup.lose2.SetActive(!revive);
                    int number = revive ? state[state.Length - 1] - '0' : 1;
                    popup.imgCounterFill.fillAmount = (number - 1) / 4f;
                    typeof(PopupLose).GetMethod("UpdateReviveCountdown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(popup, null);
                    if (revive && popup.txTimeCounter.text != number.ToString())
                        throw new InvalidOperationException("Count must increase from 1 to 5 as the ring fills.");
                    Canvas.ForceUpdateCanvases();
                    camera.Render();
                    RenderTexture.active = target;
                    capture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                    capture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                    capture.Apply();
                    File.WriteAllBytes("Docs/Heavenly" + state + "-" + size.x + "x" + size.y + ".png", capture.EncodeToPNG());
                    Object.DestroyImmediate(capture);
                    capture = null;
                }
                camera.targetTexture = null;
                RenderTexture.active = previousTarget;
                Object.DestroyImmediate(target);
                target = null;
            }
            File.WriteAllText("Docs/HeavenlyLoseValidation.txt",
                "PASS: independent button sprites scale to 95% on press and restore on exit. " +
                "Rendered Revive at 1, 3, 5 and Lose at 1080x1920 and 1080x2340. No game/service calls.\n");
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
