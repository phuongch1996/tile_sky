#nullable enable
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.Leaderboards.Authoring.Client.Models;

namespace Unity.Services.Leaderboards.Editor.Authoring.AdminApi
{
    [JsonConverter(typeof(LeaderboardConfig1PatchConverter))]
    class LeaderboardConfig1Patch : LeaderboardConfig1
    {
        public LeaderboardConfig1Patch(
            string name,
            SortOrderOptions sortOrder,
            UpdateTypeOptions updateType,
            LeaderboardConfig1ResetConfig? resetConfig,
            LeaderboardConfig1TieringConfig? tieringConfig
        ) : base(name, sortOrder, updateType, resetConfig, tieringConfig)
        {
        }
    }

    class LeaderboardConfig1PatchConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            // simplification that's good enough for our usage for now
            return typeof(LeaderboardConfig1Patch) == objectType;
        }

        // override necessary for the following use case:
        // 1. a configuration has already been deployed and has for example a ResetConfig that has been setup
        // 2. a user wants to update the configuration and remove the ResetConfig
        // 3. the user will send a PATCH request with a ResetConfig that is not set
        // 4. the endpoint will remove the reset config from the configuration
        //
        // Problem: currently a reset configuration or a tiering configuration that is not set or null will be interpreted
        // by the backend as keys that should not be affected. The backend expect empty objects for properties that are
        // supposed to be removed.
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            JObject jsonObject = new JObject();
            var properties = value !.GetType().GetProperties();
            var clearableValues = new[] { nameof(LeaderboardConfig1Patch.TieringConfig), nameof(LeaderboardConfig1Patch.ResetConfig) };

            foreach (var property in properties)
            {
                object? propertyValue = property.GetValue(value);

                // create empty objects instead of null/undefined values for optional object properties
                if (propertyValue == null && clearableValues.Contains(property.Name))
                {
                    jsonObject.Add(property.Name, new JObject());
                }
                else if (propertyValue == null)
                {
                    jsonObject.Add(property.Name, JValue.CreateNull());
                }
                else
                {
                    jsonObject.Add(property.Name, JToken.FromObject(propertyValue !, serializer));
                }
            }

            jsonObject.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Should only be used for serialization.");
        }
    }
}
