using System.Collections.Generic;
using UnityEngine;

public class BrokenAnimScript : MonoBehaviour
{
    public PoolObject[] brokenTiles;
    [SerializeField] string matchEffectSortingLayer = "Hovering";
    [SerializeField] int matchEffectSortingOrder = 5000;
    [SerializeField] float matchEffectYOffset = 0.25f;

    public void BrokenTile(int tileColor, Vector3 pos)
    {
        PoolObject breakPrefab = GetBreakPrefab(tileColor);
        if (breakPrefab == null)
            return;

        var brokenAnim = PoolManager.instance.GetObject<PoolObject>(breakPrefab, transform, 1);
        if (brokenAnim == null)
            return;

        brokenAnim.transform.position = pos + Vector3.up * matchEffectYOffset;
        brokenAnim.transform.rotation = Quaternion.identity;
        BringToTopLayer(brokenAnim);
    }

    public void BrokenTileForMatch(ItemTile tile, Vector3 pos)
    {
        BrokenTile(GetBreakIndex(tile), pos);
    }

    public void BrokenTilesForMatch(List<ItemTile> tiles)
    {
        if (tiles == null)
            return;

        foreach (ItemTile tile in tiles)
        {
            if (tile == null)
                continue;

            BrokenTileForMatch(tile, tile.transform.position);
        }
    }

    int GetBreakIndex(ItemTile tile)
    {
        if (brokenTiles == null || brokenTiles.Length == 0 || tile == null)
            return 0;

        int fallbackIndex = brokenTiles.Length - 1;
        if (tile.TryGetComponent(out GoldTileVariant goldTile) && goldTile.tileType == -1)
            return Mathf.Min(3, fallbackIndex);

        if (tile.isRainbow || tile.IsEventTile || tile.tileType < 0)
            return fallbackIndex;

        return Mathf.Abs(tile.tileType) % brokenTiles.Length;
    }

    PoolObject GetBreakPrefab(int tileColor)
    {
        if (brokenTiles == null || brokenTiles.Length == 0)
            return null;

        int index = Mathf.Clamp(tileColor, 0, brokenTiles.Length - 1);
        if (brokenTiles[index] != null)
            return brokenTiles[index];

        for (int i = 0; i < brokenTiles.Length; i++)
        {
            if (brokenTiles[i] != null)
                return brokenTiles[i];
        }

        return null;
    }

    void BringToTopLayer(PoolObject brokenAnim)
    {
        if (brokenAnim == null)
            return;

        foreach (Renderer renderer in brokenAnim.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sortingLayerName = matchEffectSortingLayer;
            renderer.sortingOrder = matchEffectSortingOrder;
        }
    }
}
