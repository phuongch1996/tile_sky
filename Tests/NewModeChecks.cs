using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Standalone checks against the real game assembly; not imported into Assets or a player build.
internal static class NewModeChecks
{
    static int assertions;

    static int Main(string[] args)
    {
        string root = Path.GetFullPath(args[0]);
        var directories = File.ReadAllLines(Path.Combine(root, "Temp/mode-integration/reference-dirs.txt"));
        AppDomain.CurrentDomain.AssemblyResolve += (_, request) =>
        {
            string name = new AssemblyName(request.Name).Name + ".dll";
            foreach (string directory in directories)
            {
                string candidate = Path.Combine(directory, name);
                if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
            }
            return null;
        };
        try
        {
            Run(root);
            Console.WriteLine("PASS: " + assertions + " assertions. No Unity Play Mode was run.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }

    static void Reject(string json)
    {
        bool rejected = false;
        try { JsonConvert.DeserializeObject<Leveljson>(json); }
        catch (JsonSerializationException) { rejected = true; }
        Check(rejected, "Unsupported or ambiguous level should be rejected");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Run(string root)
    {
        Check(NewModeRules.TargetRounds(0) == 3 && NewModeRules.TargetRounds(9) == 3,
            "Target mode must finish after its three hatching stages");
        Check(new[] { 0, 1, 2, 3, -1 }.Select(NewModeRules.TargetStage)
                .SequenceEqual(new[] { 0, 1, 2, 2, 0 }),
            "Target art must advance egg/shell/carrot and clamp saved stage values");
        for (int id = 0; id <= 7; id++)
        {
            Check((int)LevelManager.GetGameMode(id) == id, "Legacy enum changed: " + id);
            Check(LevelManager.GetGameModeIndex((GAME_MODE)id) == id, "Legacy reverse mapping changed");
        }
        Check((int)GAME_MODE.tile_color == 8, "Reserved enum value changed");
        Check(LevelManager.GetGameMode(8) == GAME_MODE.normal, "Legacy ID 8 fallback changed");
        for (int id = 9; id <= 14; id++)
        {
            Check((int)LevelManager.GetGameMode(id) == id, "New mapping missing");
            Check(LevelManager.GetGameModeIndex((GAME_MODE)id) == id, "New reverse mapping missing");
            var scalar = JsonConvert.DeserializeObject<Leveljson>(
                "{\"data\":[\"111111\"],\"itemTypeCount\":2,\"mode\":" + id + ",\"param\":3}");
            var array = JsonConvert.DeserializeObject<Leveljson>(
                "{\"data\":[\"111111\"],\"itemTypeCount\":2,\"mode\":[" + id + "],\"param\":[3]}");
            Check(scalar.mode == id && array.mode == id && scalar.param == array.param,
                "Single-array/scalar schema differs");
            Check(scalar.tileCount == 6 && scalar.bombMoves == 5, "Level defaults wrong");
            Check(JsonConvert.DeserializeObject<Leveljson>(JsonConvert.SerializeObject(scalar)).mode == id,
                "Level JSON roundtrip failed");
        }
        var customBomb = JsonConvert.DeserializeObject<Leveljson>(
            "{\"data\":[\"111\"],\"mode\":11,\"param\":1,\"bombMoves\":8}");
        Check(customBomb.bombMoves == 8, "Custom bomb countdown lost");
        Reject("{\"data\":[\"111\"],\"mode\":[10,11],\"param\":[1,1]}");
        Reject("{\"data\":[\"111\"],\"mode\":15}");
        Reject("{\"data\":[\"111\"],\"mode\":100}");
        Reject("{\"data\":[null],\"mode\":10}");
        Reject("{\"data\":[\"111\"],\"mode\":\"11\"}");

        int legacyLevels = 0;
        var activeLevels = new List<Leveljson>();
        foreach (string path in Directory.GetFiles(Path.Combine(root, "Assets/Scripts/Gameplay"), "*.csv"))
        {
            foreach (string line in File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                string json = line.Trim().Replace("\"\"", "\"").Trim('"');
                var expected = JObject.Parse(json);
                var actual = JsonConvert.DeserializeObject<Leveljson>(json);
                if (Path.GetFileName(path) == "configlv_1200_1.csv") activeLevels.Add(actual);
                Check(actual.mode == expected.Value<int>("mode"), "Legacy mode changed in " + path);
                Check(actual.data.SequenceEqual(expected["data"].ToObject<List<string>>()), "Legacy board changed");
                Check(actual.itemTypeCount == expected.Value<int>("itemTypeCount"), "Legacy item count changed");
                Check(actual.param == (actual.mode == 0 ? 0 : expected.Value<int>("param")), "Legacy parameter changed");
                legacyLevels++;
            }
        }
        Console.WriteLine("Legacy CSV rows verified: " + legacyLevels);
        AuditActiveLevels(root, activeLevels);

        Check(NewModeRules.LockedSlots(0) == 1 && NewModeRules.LockedSlots(100) == 4,
            "Locked-slot safety limits wrong");
        Check(NewModeRules.BombBudget(0) == 5 && NewModeRules.BombBudget(8) == 8, "Bomb budget wrong");
        Check(NewModeRules.BombAfterSelection(5, true, false) == 4, "Exposed bomb did not tick");
        Check(NewModeRules.BombAfterSelection(5, false, false) == 5, "Hidden bomb ticked");
        Check(NewModeRules.BombAfterSelection(5, true, true) == 5, "Selected bomb ticked");
        Check(NewModeRules.BombAfterSelection(0, true, false) == 0, "Bomb went below zero");
        Check(NewModeRules.CoversTray(0, 0) && NewModeRules.CoversTray(1.49f, -1.49f), "Tray overlap missed");
        Check(!NewModeRules.CoversTray(1.5f, 0) && !NewModeRules.CoversTray(0, -2), "Distant tile covers tray");
        Check(!NewModeRules.IsSpecialType(0, -3) && !NewModeRules.IsSpecialType(13, -2),
            "Existing negative tile types collide with new objectives");
        Check(NewModeRules.IsSpecialType(13, -3) && NewModeRules.IsSpecialType(14, -4), "Special tile rules wrong");

        Check(NewModeRules.CoinTileCount(90, 14) == 39, "Coin targets consumed every ordinary triple");
        Check(NewModeRules.CoinTileCount(120, 18) == 51, "Oversized coin target was not bounded");
        Check(NewModeRules.CoinTileCount(0, 14) == 9, "Default coin target changed");
        Check(NewModeRules.CoinTileCount(9, 1) == 0 && NewModeRules.CoinTileCount(9, 0) == 0,
            "Tiny boards must use the ordinary clear-board fallback");
        foreach (var coinLevel in activeLevels.Where(level => level.mode == 14))
        {
            int coins = NewModeRules.CoinTileCount(coinLevel.param, coinLevel.tileCount / 3);
            Check(coinLevel.param == 0 || coinLevel.param == 9, "Legacy coin parameter was not migrated");
            Check(coins == 9 && coins < coinLevel.tileCount, "Coin level does not retain ordinary tiles");
        }

        var random = new Random(3721);
        for (int run = 0; run < 250; run++)
        {
            var types = Enumerable.Range(0, 12).SelectMany(type => Enumerable.Repeat(type, 3 * random.Next(1, 5)))
                .OrderBy(_ => random.Next()).ToList();
            var groups = NewModeRules.CompleteTripleIndices(types);
            int count = NewModeRules.CoinTileCount(random.Next(0, 40), groups.Count);
            var chosen = groups.Take(count / 3).SelectMany(group => group).ToHashSet();
            Check(chosen.Count == count && count % 3 == 0, "Coin target count/indices wrong");
            Check(groups.All(group => group.Length == 3 && group.Select(index => types[index]).Distinct().Count() == 1),
                "Target group mixes types");
            Check(types.Where((_, index) => !chosen.Contains(index)).GroupBy(type => type).All(group => group.Count() % 3 == 0),
                "Reserving objectives stranded ordinary tiles");
        }

        var oldSave = JsonConvert.DeserializeObject<GameplayProgressData>(
            "{\"version\":1,\"level\":20,\"gameMode\":3,\"tiles\":[{\"prefabType\":3,\"iceCounter\":2,\"tileType\":1}]}");
        Check(oldSave.version == 1 && oldSave.modeProgress == null && oldSave.tiles[0].iceCounter == 2,
            "Legacy save extension is not optional");
        var newSave = new GameplayProgressData
        {
            version = 1, gameMode = 12,
            modeProgress = new ModeProgressData { winningRounds = 2, roundTarget = 3,
                trays = new List<ModeTrayProgress> { new ModeTrayProgress { floor = 2, x = 1, y = -1, cleared = true } } },
            tiles = new List<GameplayTileProgressData> { new GameplayTileProgressData
                { modeKind = (int)TileModeKind.Bomb, bombMoves = 2, bombResolved = false, tileType = 1 } }
        };
        var restored = JsonConvert.DeserializeObject<GameplayProgressData>(JsonConvert.SerializeObject(newSave));
        Check(restored.modeProgress.trays[0].cleared && restored.modeProgress.winningRounds == 2 &&
            restored.tiles[0].bombMoves == 2, "New mode save payload did not roundtrip");
    }

    static void AuditActiveLevels(string root, List<Leveljson> levels)
    {
        string scene = File.ReadAllText(Path.Combine(root, "Assets/BaseProject/Scenes/GameScene.unity"));
        string floors = Regex.Match(scene, @"(?ms)^  floorTransformList:\r?\n(.*?)(?=^  \w)").Groups[1].Value;
        int floorCount = Regex.Matches(floors, @"fileID: \d+").Count;
        string asset = File.ReadAllText(Path.Combine(root, "Assets/Scripts/Gameplay/Levels.asset"));
        string section = Regex.Match(asset, @"(?ms)^  levels:[^\n]*\n(.*?)(?=^  challengeLevelCSVList:)").Groups[1].Value;
        var entries = Regex.Matches(section, @"(?ms)^  - data:[^\n]*\n(.*?)(?=^  - data:|\z)");
        Check(floorCount > 0 && entries.Count == levels.Count, "Active CSV/asset count or floor references invalid");
        for (int index = 0; index < levels.Count; index++)
        {
            var level = levels[index];
            string context = "Level " + (index + 1);
            Check(level.data.Count <= floorCount, context + " exceeds the scene's floor capacity");
            int size = 2;
            do { size += 2; } while (size * (size - 2) < level.data[0].Length);
            int tiles = 0;
            for (int floor = 0; floor < level.data.Count; floor++)
            {
                int capacity = (size - 2 - floor % 2) * (size - floor % 2);
                string layer = level.data[floor];
                Check(Regex.IsMatch(layer, "^[01]+$"), context + " contains invalid cells");
                for (int cell = 0; cell < layer.Length; cell++)
                    if (layer[cell] == '1')
                    {
                        Check(cell < capacity, context + " has a tile outside the decoded board");
                        tiles++;
                    }
            }
            Check(tiles == level.tileCount && tiles >= 3 && tiles % 3 == 0, context + " has an invalid tile count");
            string entry = entries[index].Groups[1].Value;
            int Field(string name) => int.Parse(Regex.Match(entry, @"(?m)^    " + name + @": (\d+)").Groups[1].Value);
            Check(Field("tileCount") == level.tileCount && Field("itemTypeCount") == level.itemTypeCount &&
                Field("mode") == level.mode && Field("param") == level.param, context + " differs between CSV and Levels.asset");
            var layers = Regex.Matches(entry, @"(?m)^    - ([01]+)").Cast<Match>().Select(match => match.Groups[1].Value);
            Check(level.data.SequenceEqual(layers), context + " asset board does not match CSV");
        }
        Console.WriteLine("Active levels audited: " + levels.Count + "; CSV/asset data and decoded boards match scene capacity.");
    }
}
