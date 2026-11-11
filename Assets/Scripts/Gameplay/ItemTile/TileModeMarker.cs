using TMPro;
using UnityEngine;

public enum TileModeKind { None, Poison, Bomb, Winning, Coin }

// Added to the existing pooled tile at runtime; no duplicate tile prefab is needed.
public sealed class TileModeMarker : MonoBehaviour
{
    public TileModeKind kind;
    public int bombMoves;
    public bool bombResolved;
    ItemTile tile;
    TextMeshPro label;

    public void Configure(ItemTile owner, TileModeKind value, int moves = 0, bool resolved = false)
    {
        tile = owner;
        kind = value;
        bombMoves = moves;
        bombResolved = resolved;
        Refresh();
    }

    public void ResetMarker()
    {
        kind = TileModeKind.None;
        bombMoves = 0;
        bombResolved = false;
        if (label != null) label.gameObject.SetActive(false);
    }

    void LateUpdate() => Refresh();

    void Refresh()
    {
        if (kind == TileModeKind.None || kind == TileModeKind.Coin || kind == TileModeKind.Winning || tile == null || tile.item == null)
        {
            if (label != null) label.gameObject.SetActive(false);
            return;
        }
        if (label == null)
        {
            var go = new GameObject("Mode badge", typeof(TextMeshPro));
            go.transform.SetParent(tile.sprites, false);
            go.transform.localPosition = new Vector3(0, -0.28f, -0.02f);
            label = go.GetComponent<TextMeshPro>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 2.6f;
            label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.sizeDelta = new Vector2(0.85f, 0.3f);
            label.raycastTarget = false;
        }
        label.gameObject.SetActive(true);
        label.text = kind switch
        {
            TileModeKind.Poison => "POISON",
            TileModeKind.Bomb => bombResolved ? "BOMB OFF" : "BOMB " + bombMoves,
            _ => ""
        };
        label.color = kind switch
        {
            TileModeKind.Poison => new Color(0.4f, 1f, 0.3f),
            TileModeKind.Bomb => new Color(1f, 0.4f, 0.3f),
            _ => new Color(1f, 0.85f, 0.2f)
        };
        label.renderer.sortingLayerID = tile.item.sortingLayerID;
        label.renderer.sortingOrder = tile.item.sortingOrder + 2;
    }
}
