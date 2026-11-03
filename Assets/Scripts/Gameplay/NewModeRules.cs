using System;
using System.Collections.Generic;

// Pure rules shared by runtime code and the standalone regression checks.
public static class NewModeRules
{
    public const int WinningTileType = -3; // -1 and -2 already belong to existing modes/events.
    public const int CoinTileType = -4;

    public static bool IsExtendedMode(int mode) => mode >= 9 && mode <= 14;
    // Legacy CSVs include 8, which intentionally keeps its old normal-mode fallback.
    public static bool IsReadableMode(int mode) => (mode >= 0 && mode <= 8) || IsExtendedMode(mode);
    public static int LockedSlots(int parameter) => Math.Max(1, Math.Min(4, parameter <= 0 ? 1 : parameter));
    public static int BombBudget(int value) => value > 0 ? value : 5;
    public static int TargetRounds(int parameter) => parameter > 0 ? parameter : 3;

    public static int CoinTileCount(int parameter, int availableTriples)
    {
        int requested = parameter > 0 ? Math.Max(1, parameter / 3) : 3;
        return Math.Min(requested, Math.Max(0, availableTriples)) * 3;
    }

    public static bool IsSpecialType(int mode, int type) =>
        (mode == 13 && type == WinningTileType) || (mode == 14 && type == CoinTileType);

    public static bool CoversTray(float dx, float dy) => Math.Abs(dx) < 1.5f && Math.Abs(dy) < 1.5f;

    public static int BombAfterSelection(int remaining, bool exposed, bool selected) =>
        remaining > 0 && exposed && !selected ? remaining - 1 : remaining;

    public static List<int[]> CompleteTripleIndices(IList<int> types)
    {
        var result = new List<int[]>();
        var partial = new Dictionary<int, List<int>>();
        for (int i = 0; i < types.Count; i++)
        {
            int type = types[i];
            if (type < 0) continue;
            if (!partial.TryGetValue(type, out var indices))
                partial[type] = indices = new List<int>();
            indices.Add(i);
            if (indices.Count != 3) continue;
            result.Add(indices.ToArray());
            indices.Clear();
        }
        return result;
    }
}
