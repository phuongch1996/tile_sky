using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.Services.DeploymentApi.Editor;

namespace Unity.Services.Leaderboards.Authoring.Core.Model
{
    [DataContract, Serializable]
    class LeaderboardConfig : ILeaderboardConfig
    {
        DeploymentStatus m_Status;
        float m_Progress;
        string m_Path;
        string m_Name;

        internal const string ConfigType = "Leaderboard";

        public LeaderboardConfig(string name) : this(null, name, SortOrder.Asc, UpdateType.KeepBest)
        {
        }

        public LeaderboardConfig(
            string id,
            string name,
            SortOrder sortOrder = SortOrder.Asc,
            UpdateType updateType = UpdateType.KeepBest)
        {
            Id = id;
            Name = name;
            Path = string.Empty;
            States = new ObservableCollection<AssetState>();
            SortOrder = sortOrder;
            UpdateType = updateType;
        }

        public string Type => ConfigType;
        [DataMember(IsRequired = true)]
        public SortOrder SortOrder { get; set; }
        [DataMember(IsRequired = true)]
        public UpdateType UpdateType { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public int BucketSize { get; set; }
        [DataMember]
        public ResetConfig ResetConfig { get; set; }
        [DataMember]
        public TieringConfig TieringConfig { get; set; }

        /// <inheritdoc cref="ILeaderboardConfig.Name" />
        [DataMember]
        public string Name
        {
            get => m_Name;
            set => SetField(ref m_Name, value);
        }

        /// <inheritdoc cref="IDeploymentItem.Name" />
        string IDeploymentItem.Name
        {
            get => System.IO.Path.GetFileName(Path);
        }

        /// <inheritdoc cref="IDeploymentItem.Path" />
        public string Path
        {
            get => m_Path;
            set
            {
                if (SetField(ref m_Path, value))
                {
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <inheritdoc cref="IDeploymentItem.Progress" />
        public float Progress
        {
            get => m_Progress;
            set => SetField(ref m_Progress, value);
        }

        /// <inheritdoc cref="IDeploymentItem.Status" />
        public DeploymentStatus Status
        {
            get => m_Status;
            set => SetField(ref m_Status, value);
        }

        /// <inheritdoc cref="IDeploymentItem.States" />
        public ObservableCollection<AssetState> States { get; }

        /// <inheritdoc cref="IDeploymentItem.PropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the field and raises an OnPropertyChanged event.
        /// </summary>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="onFieldChanged">The callback.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <typeparam name="T">Type of the parameter.</typeparam>
        protected bool SetField<T>(
            ref T field,
            T value,
            Action<T> onFieldChanged = null,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName !);
            onFieldChanged?.Invoke(field);
            return true;
        }

        public override string ToString()
        {
            if (Path == "Remote")
                return Id;
            return $"'{Path}'";
        }

        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
