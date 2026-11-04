using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Leaderboards.Editor.Shared.Assets;
using UnityEngine;
using Unity.Services.Leaderboards.Authoring.Core.Serialization;


namespace Unity.Services.Leaderboards.Editor.Authoring.Model
{
    /// <summary>
    /// This class serves to track creation and deletion of assets of the
    /// associated service type
    /// </summary>
    sealed class ObservableLeaderboardConfigAssets : ObservableCollection<IDeploymentItem>, IDisposable
    {
        readonly ILeaderboardsSerializer m_Serializer;
        readonly ObservableAssets<LeaderboardConfigAsset> m_LeaderboardsAssets;

        public ObservableLeaderboardConfigAssets(ILeaderboardsSerializer serializer)
        {
            m_Serializer = serializer;
            m_LeaderboardsAssets = new ObservableAssets<LeaderboardConfigAsset>(new[] {LeaderboardAssetsExtensions.configExtension} );

            foreach (var asset in m_LeaderboardsAssets)
            {
                OnNewAsset(asset);
            }
            m_LeaderboardsAssets.CollectionChanged += LeaderboardsAssetsOnCollectionChanged;
        }

        public void Dispose()
        {
            m_LeaderboardsAssets.CollectionChanged -= LeaderboardsAssetsOnCollectionChanged;
        }

        void OnNewAsset(LeaderboardConfigAsset asset)
        {
            PopulateModel(asset.Model);
            Add(asset.Model);
        }

        void PopulateModel(EditorLeaderboardConfig model)
        {
            try
            {
                model.ClearOwnedStates();
                m_Serializer.DeserializeAndPopulate(model);
            }
            catch (LeaderboardsDeserializeException e)
            {
                model.States.Add(new AssetState(e.ErrorMessage, e.Details, SeverityLevel.Error));
            }
        }

        void LeaderboardsAssetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems.Cast<LeaderboardConfigAsset>())
                {
                    Remove(oldItem.Model);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems.Cast<LeaderboardConfigAsset>())
                {
                    OnNewAsset(newItem);
                }
            }
        }

        // this method takes an asset being reloaded by the editor and recreate a scriptable object after having
        // transferred the data from the previous one
        LeaderboardConfigAsset RegenAsset(LeaderboardConfigAsset asset)
        {
            // creating an asset is mandatory because the previous one is already partially destroyed at this point
            var newAsset = ScriptableObject.CreateInstance<LeaderboardConfigAsset>();
            // we transfer the previous model instance to the new asset
            newAsset.m_LeaderboardConfig = asset.Model;
            asset = newAsset;
            // we update the deserialization states
            PopulateModel(asset.Model);
            // we don't modify this[] because the instance of IDeploymentItem hasn't changed.
            return asset;
        }

        public LeaderboardConfigAsset GetOrCreateInstance(string ctxAssetPath)
        {
            foreach (var a in m_LeaderboardsAssets)
            {
                if (ctxAssetPath == a.Path)
                {
                    return a == null ? RegenAsset(a) : a;
                }
            }
            var asset = ScriptableObject.CreateInstance<LeaderboardConfigAsset>();
            asset.Path = ctxAssetPath;
            return asset;
        }
    }
}
