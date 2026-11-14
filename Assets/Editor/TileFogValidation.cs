using System;
using System.IO;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class TileFogValidation
{
    const string Request = "Temp/TileFogValidation.request";

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
        try { Validate(); }
        catch (Exception error) { File.WriteAllText("Temp/TileFogValidation.error.txt", error.ToString()); }
    }

    [MenuItem("Tools/Tile Space/Validate Fog Cover After Shuffle")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewPreviewScene();
        ItemTile tile = null;
        try
        {
            tile = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Game/ItemTile.prefab"), scene)).GetComponent<ItemTile>();
            for (int i = 0; i < 5; i++)
            {
                tile.state = ITEMTILE_STATE.HIDDEN;
                tile.isFogAnim = true;
                tile.shadow.gameObject.SetActive(true);
                tile.shadow.color = new Color(1, 1, 1, 0);
                tile.shadow.transform.localPosition = Vector3.right;
                tile.shadow.DOFade(0, 0.5f);
                tile.shadow.transform.DOLocalMoveX(2, 0.5f);
                tile.RefreshFogCover();
                if (!tile.shadow.gameObject.activeSelf || tile.shadow.color.a != 1f ||
                    tile.shadow.transform.localPosition != Vector3.zero || tile.isFogAnim ||
                    DOTween.IsTweening(tile.shadow) || DOTween.IsTweening(tile.shadow.transform))
                    throw new InvalidOperationException("A hidden tile lost its opaque, stationary fog cover.");

                tile.state = ITEMTILE_STATE.TOUCHABLE;
                tile.RefreshFogCover();
                if (tile.shadow.gameObject.activeSelf)
                    throw new InvalidOperationException("Fog must not cover an exposed tile after shuffle.");

                tile.state = ITEMTILE_STATE.SLOTTED;
                tile.RefreshFogCover();
                if (tile.shadow.gameObject.activeSelf)
                    throw new InvalidOperationException("Fog must not cover a tile already in the tray.");
            }
            File.WriteAllText("Temp/TileFogValidation.done",
                "PASS: five repeated fog resets restore hidden tiles, cancel old fades/movement and keep exposed/slotted tiles clear. Real Unity objects; no game/service calls.");
        }
        finally
        {
            if (tile)
            {
                tile.shadow.DOKill();
                tile.shadow.transform.DOKill();
            }
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
