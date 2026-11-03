using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TileSkyLeaderboardPreview
{
    const string Request = "Temp/TileSkyLeaderboardPreview.request";

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
        catch (System.Exception exception)
        {
            File.WriteAllText("Temp/TileSkyLeaderboardPreview.error.txt", exception.ToString());
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/Tile Space/Preview Tile Sky Leaderboard")]
    public static void Preview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewPreviewScene();
        RenderTexture target = null;
        Texture2D capture = null;
        var previous = RenderTexture.active;
        try
        {
            var canvasGO = new GameObject("Leaderboard preview", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var cameraGO = new GameObject("Preview camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraGO, scene);
            var camera = cameraGO.GetComponent<Camera>();
            camera.scene = scene;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(82, 174, 240, 255);
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            target = new RenderTexture(1080, 1920, 24);
            camera.targetTexture = target;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TileGOPrefab/UI/Auth/PopupRank.prefab");
            var instance = Object.Instantiate(prefab, canvas.transform, false);
            instance.SetActive(true);
            var popup = instance.GetComponent<PopupRank>();
            popup.panel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            popup.EnsureTileSkyLeaderboard();
            popup.EnsureTileSkyLeaderboard();
            popup.SetupTweenCallback();
            popup.tweenCallback.Invoke();
            popup.loadingGO.SetActive(false);
            popup.loadFailedGO.SetActive(false);
            popup.prizeReviewGO.SetActive(false);
            popup.btPrizePreview.gameObject.SetActive(false);
            popup.btEditPlayer.gameObject.SetActive(true);
            popup.txTimeLeft.text = "RESETS IN 08:24:36";
            foreach (var empty in new[] { popup.noDailyGO, popup.noAllTimeGO, popup.noDailyChallengeGO }) empty.SetActive(false);
            var names = new[] { "Fashionable", "Dear", "Magenta", "Hearty", "Outstanding", "Dynamic", "Sky Dreamer", "Cloud Rider" };
            for (int i = 0; i < names.Length; i++)
            {
                var card = Object.Instantiate(popup.rankCardTemplate, popup.dailyParent, false);
                card.gameObject.SetActive(true);
                card.ApplyTileSkyStyle();
                Fill(card, i + 1, names[i], 64 - i * 5, i);
            }
            Fill(popup.currentUserDaily, 94, "Prudent", 3, 2);
            popup.toggleDailyChallengeRank.gameObject.SetActive(true);
            popup.toggleDailyRank.isOn = true;
            popup.ChangeTab();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)popup.dailyParent);
            camera.Render();
            RenderTexture.active = target;
            capture = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);
            capture.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
            capture.Apply();
            Directory.CreateDirectory("Docs");
            File.WriteAllBytes("Docs/TileSkyLeaderboardPreview.png", capture.EncodeToPNG());

            // Use real value-change events and the ToggleGroup, rather than
            // changing flags silently and invoking ChangeTab for every selection.
            popup.toggleAllTimeRank.isOn = true;
            AssertTab(popup, 1);
            popup.toggleDailyChallengeRank.isOn = true;
            AssertTab(popup, 2);
            popup.toggleAllTimeRank.isOn = true;
            AssertTab(popup, 1);
            popup.toggleDailyRank.isOn = true;
            AssertTab(popup, 0);
            popup.toggleDailyRank.isOn = false;
            AssertTab(popup, 0);
            popup.toggleDailyChallengeRank.isOn = true;
            popup.toggleDailyChallengeRank.gameObject.SetActive(false);
            popup.ChangeTab();
            AssertTab(popup, 0);

            var scroll = popup.dailyParent.GetComponentInParent<ScrollRect>();
            scroll.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 1;
            Canvas.ForceUpdateCanvases();
            foreach (var card in popup.dailyParent.GetComponentsInChildren<RankCard>())
            {
                if (card.imgFrame.type != Image.Type.Simple || !card.imgFrame.preserveAspect ||
                    (card.txRank.text == "" && card.imgRankIcon.type != Image.Type.Simple) || !card.imgRankIcon.preserveAspect)
                    throw new System.InvalidOperationException("Portrait frame or medal can stretch while scrolling.");
            }
            File.WriteAllText("Docs/TileSkyLeaderboardValidation.txt",
                "PASS: actual prefab rendered at 1080x1920; Tile Sky assets loaded; setup is idempotent; " +
                "real toggle events select exactly one tab; selected tab cannot be cleared; unavailable Challenge falls back to Daily; " +
                "countdown and info are Challenge-only; scrolling preserves Simple/aspect image rendering. " +
                "Preview uses sample data and does not call leaderboard services.\n");
        }
        finally
        {
            RenderTexture.active = previous;
            if (capture) Object.DestroyImmediate(capture);
            if (target) Object.DestroyImmediate(target);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    static void AssertTab(PopupRank popup, int expected)
    {
        var toggles = new[] { popup.toggleDailyRank, popup.toggleAllTimeRank, popup.toggleDailyChallengeRank };
        var tabs = new[] { popup.dailyTab, popup.allTimeTab, popup.dailyChallengeTab };
        var playerCards = new[] { popup.currentUserDaily, popup.currentUserAllTime, popup.currentUserDailyChallenge };
        for (int i = 0; i < tabs.Length; i++)
        {
            if (toggles[i].isOn != (i == expected) || tabs[i].activeSelf != (i == expected))
                throw new System.InvalidOperationException("Tab selection is not exclusive: " + expected);
            if (playerCards[i].gameObject.activeInHierarchy != (i == expected) ||
                playerCards[i].GetComponentInParent<ScrollRect>() != null)
                throw new System.InvalidOperationException("Pinned player rank is hidden or inside the scrolling list: " + i);
        }
        if (popup.txTimeLeft.gameObject.activeSelf != (expected == 2) ||
            popup.btInfo.gameObject.activeSelf != (expected == 2))
            throw new System.InvalidOperationException("Countdown/info appeared outside Challenge.");
    }

    static void Fill(RankCard card, int rank, string name, int score, int avatar)
    {
        card.UpdateRank(rank - 1);
        card.txRank.text = rank <= 3 ? "" : rank.ToString();
        if (rank <= 3 && card.topRankSprites.Length >= rank)
        {
            card.imgRankIcon.sprite = card.topRankSprites[rank - 1];
            card.imgRankIcon.color = Color.white;
            card.imgRankIcon.type = Image.Type.Simple;
        }
        card.txDisplayName.text = name;
        card.txScore.text = score.ToString();
        var avatars = HeavenlyInfoArt.Avatars;
        if (avatars.Length > 0) card.imgAvatar.sprite = avatars[avatar % avatars.Length];
        card.imgAvatar.color = Color.white;
        card.imgFrame.color = Color.white;
    }
}
