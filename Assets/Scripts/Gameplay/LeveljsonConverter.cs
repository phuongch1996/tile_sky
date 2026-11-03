using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// The reference tool exports arrays; the main game still runs one mode per level.
public sealed class LeveljsonConverter : JsonConverter<Leveljson>
{
    public override bool CanWrite => false;

    public override Leveljson ReadJson(JsonReader reader, Type objectType, Leveljson existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var json = JObject.Load(reader);
        int mode = ReadSingle(json["mode"], "mode");
        int parameter = ReadSingle(json["param"], "param");
        if (!NewModeRules.IsReadableMode(mode))
            throw new JsonSerializationException("Unsupported mode " + mode +
                ". Readable: legacy 0-8, new 9-14. Conveyor, 3D and combined modes are not enabled.");
        var data = json["data"]?.ToObject<List<string>>(serializer);
        if (data == null || data.Count == 0 || data.Exists(line => line == null))
            throw new JsonSerializationException("Level data must contain non-null board layers.");
        var level = new Leveljson(data, json.Value<int?>("itemTypeCount") ?? 2, mode, parameter);
        level.tileCount = json.Value<int?>("tileCount") ?? level.tileCount;
        level.bombMoves = NewModeRules.BombBudget(json.Value<int?>("bombMoves") ?? 5);
        return level;
    }

    static int ReadSingle(JToken token, string field)
    {
        if (token == null || token.Type == JTokenType.Null) return 0;
        if (token is JArray array)
        {
            if (array.Count != 1)
                throw new JsonSerializationException(field +
                    " must contain exactly one value; combined modes require a separate integration.");
            token = array[0];
        }
        if (token.Type != JTokenType.Integer)
            throw new JsonSerializationException(field + " must be an integer.");
        return token.Value<int>();
    }

    public override void WriteJson(JsonWriter writer, Leveljson value, JsonSerializer serializer) =>
        throw new NotSupportedException();
}
