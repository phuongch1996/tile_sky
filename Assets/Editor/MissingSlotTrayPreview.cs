using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class MissingSlotTrayPreview
{
    const string Request = "Temp/MissingSlotTrayPreview.request";
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
        catch (Exception error) { File.WriteAllText("Temp/MissingSlotTrayPreview.error.txt", error.ToString()); }
    }

    [MenuItem("Tools/Tile Space/Preview Cut Slot Tray")]
    public static void Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewPreviewScene();
        var previous = RenderTexture.active;
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            var root = new GameObject("Cut tray preview");
            SceneManager.MoveGameObjectToScene(root, scene);
            var cameraGO = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            var camera = cameraGO.GetComponent<Camera>();
            camera.scene = scene;
            camera.orthographic = true;
            camera.orthographicSize = 1.45f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.14f, 0.24f);
            var frame = new GameObject("Tray frame", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            frame.transform.SetParent(root.transform, false);
            frame.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/TileSky/SkyTray.png").OfType<Sprite>().First();
            frame.drawMode = SpriteDrawMode.Sliced;
            frame.size = new Vector2(8.1f, 1.7f);
            var slots = new List<Transform>();
            for (int i = 0; i < 7; i++)
            {
                var slot = new GameObject("Slot " + i).transform;
                slot.SetParent(root.transform, false);
                slot.localPosition = new Vector3(-3.197467f + i * (6.383741f / 6f), 0.080863f, 0);
                slots.Add(slot);
            }
            var effect = MissingSlotTrayEffect.Create(frame, slots, 6);
            target = new RenderTexture(1400, 450, 24);
            camera.targetTexture = target;
            Directory.CreateDirectory("Docs");
            for (int state = 0; state < 2; state++)
            {
                if (state == 1) effect.ApplyFinalLayout();
                camera.Render();
                RenderTexture.active = target;
                capture = new Texture2D(1400, 450, TextureFormat.RGBA32, false);
                capture.ReadPixels(new Rect(0, 0, 1400, 450), 0, 0);
                capture.Apply();
                File.WriteAllBytes(state == 0 ? "Docs/TrayBeforeCut.png" : "Docs/TrayAfterCut.png", capture.EncodeToPNG());
                Object.DestroyImmediate(capture);
                capture = null;
            }
            if (slots[6].gameObject.activeSelf || !effect.IsComplete ||
                Mathf.Abs(slots.Take(6).Average(slot => slot.localPosition.x)) > 0.001f)
                throw new InvalidOperationException("Six live slots must be centered and the seventh inactive.");
            var parts = effect.GetComponentsInChildren<SpriteRenderer>().Where(renderer => renderer.enabled &&
                renderer.gameObject.activeInHierarchy).ToArray();
            if (parts.Length != 2) throw new InvalidOperationException("Only the six-well body and right cap should remain.");
            float wellPitch = Mathf.Abs(slots[1].position.x - slots[0].position.x);
            if (parts[1].bounds.size.x <= wellPitch || parts[0].sprite.rect.x != frame.sprite.rect.x ||
                parts[1].sprite.rect.xMax != frame.sprite.rect.xMax)
                throw new InvalidOperationException("Keep both original outer corners and the whole final well; never splice a narrow curved cap.");
            Bounds bounds = parts[0].bounds;
            bounds.Encapsulate(parts[1].bounds);
            if (Mathf.Abs(bounds.center.x) > 0.001f ||
                Mathf.Abs(parts[0].bounds.max.x - parts[1].bounds.min.x) > 0.001f)
                throw new InvalidOperationException("The shortened tray must be centered and the cut edge closed without a gap.");
            Vector3 firstPosition = slots[0].localPosition;
            effect.ApplyFinalLayout();
            if (slots[0].localPosition != firstPosition)
                throw new InvalidOperationException("Applying the layout twice must not shift the tray again.");
            File.WriteAllText("Temp/MissingSlotTrayPreview.done",
                "PASS: six active slots centered, seventh disabled, shortened tray centered, cap flush, repeated layout stable. Rendered before/after in Unity Editor.");
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
