using System;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Unity.Services.Leaderboards.Editor.Authoring.Model
{
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.services.leaderboards@2.1/manual/Authoring/leaderboard_configuration.html")]
    [ScriptedImporter(1, LeaderboardAssetsExtensions.configExtension)]
    class LeaderboardConfigAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (ctx.assetPath == null)
            {
                throw new FileLoadException("Impossible to load the asset.");
            }

            var asset = LeaderboardAuthoringServices.Instance.GetService<ObservableLeaderboardConfigAssets>()
                .GetOrCreateInstance(ctx.assetPath);
            ctx.AddObjectToAsset("MainAsset", asset);
            ctx.SetMainObject(asset);
        }

        public void OnValidate()
        {
            hideFlags = HideFlags.HideInInspector;
        }
    }
}
