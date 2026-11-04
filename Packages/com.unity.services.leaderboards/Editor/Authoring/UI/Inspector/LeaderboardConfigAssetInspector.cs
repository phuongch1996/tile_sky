using System;
using System.IO;
using System.Linq;
using Unity.Services.Leaderboards.Editor.Authoring.Commands;
using Unity.Services.Leaderboards.Editor.Authoring.Deployment;
using Unity.Services.Leaderboards.Editor.Authoring.Model;
using Unity.Services.Leaderboards.Editor.Authoring.Shared.Analytics;
using Unity.Services.Leaderboards.Editor.Authoring.Shared.UI.DeploymentConfigInspectorFooter;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [CustomEditor(typeof(LeaderboardConfigAsset))]
    class LeaderboardConfigAssetInspector : UnityEditor.Editor
    {
        const string k_Uxml = "Packages/com.unity.services.leaderboards/Editor/Authoring/UI/Assets/LeaderboardConfigAssetInspector.uxml";
        const string ServiceName = "leaderboards";

        VisualElement m_RootElement;
        Button m_ApplyButton;
        Button m_RevertButton;

        LeaderboardConfigAsset m_TargetAsset;
        LeaderboardInspectorConfig m_LeaderboardInspectorConfig;
        InspectorElement m_LeaderboardConfigInspector;
        SerializedObject m_SerializedObjectOriginal;
        SerializedObject m_SerializedObjectCurrent;

        public override VisualElement CreateInspectorGUI()
        {
            m_TargetAsset = target as LeaderboardConfigAsset;

            m_RootElement = new VisualElement();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_Uxml);
            visualTree.CloneTree(m_RootElement);

            #if !UNITY_2022_1_OR_NEWER
            //2021 does not support nested inspector elements properly
            var applyRevert = m_RootElement.Q<VisualElement>("ContainerApplyRevert");
            m_RootElement.Remove(applyRevert);
            var txtEle = new TextField() { multiline = true, isReadOnly = true };
            m_RootElement.Insert(0, txtEle);
            txtEle.value = ReadResourceBody(targets[0]);
            #else
            CreateInspector();
            BuildLeaderboardInspectorItem();
            InitializeSerializedObjects();
            InitializeApplyRevertButtons();
            #endif

            AddDeploymentFooter(m_RootElement);

            return m_RootElement;
        }

#if !UNITY_2022_1_OR_NEWER
        static string ReadResourceBody(UnityEngine.Object resource)
        {
            var path = AssetDatabase.GetAssetPath(resource);
            var lines = File.ReadLines(path).Take(75).ToList();
            if (lines.Count == 75)
            {
                lines.Add("...");
            }
            return string.Join(Environment.NewLine, lines);
        }

#endif

        void CreateInspector()
        {
            var container = m_RootElement.Q("ContainerConfig");
            m_LeaderboardConfigInspector = new InspectorElement();
            container.Add(m_LeaderboardConfigInspector);
        }

        void BuildLeaderboardInspectorItem()
        {
            var configTarget = target as LeaderboardConfigAsset;
            m_LeaderboardInspectorConfig = CreateInstance<LeaderboardInspectorConfig>();
            m_LeaderboardInspectorConfig.Initialize(configTarget?.Model);
        }

        void InitializeSerializedObjects()
        {
            m_SerializedObjectOriginal = new SerializedObject(m_LeaderboardInspectorConfig);
            m_SerializedObjectCurrent = new SerializedObject(m_LeaderboardInspectorConfig);

            m_LeaderboardConfigInspector.Unbind();
            m_LeaderboardConfigInspector.Bind(m_SerializedObjectCurrent);
            m_LeaderboardConfigInspector.TrackSerializedObjectValue(m_SerializedObjectCurrent, SerializedObjectValueChanged);
        }

        void SerializedObjectValueChanged(SerializedObject obj)
        {
            var areObjectsEqual = AreSerializedObjectsEqual(m_SerializedObjectOriginal, m_SerializedObjectCurrent);
            UpdateApplyRevertButtons(!areObjectsEqual);
#if UNITY_2022_1_OR_NEWER
            hasUnsavedChanges = !areObjectsEqual;
#endif
        }

        void InitializeApplyRevertButtons()
        {
            m_ApplyButton = m_RootElement.Q<Button>("ApplyButton");
            m_ApplyButton.clicked += SaveChanges;
            m_RevertButton = m_RootElement.Q<Button>("RevertButton");
            m_RevertButton.clicked += DiscardChanges;

            UpdateApplyRevertButtons(false);
        }

        void UpdateApplyRevertButtons(bool toggle)
        {
            m_RevertButton.SetEnabled(toggle);
            m_ApplyButton.SetEnabled(toggle);
        }

        void SaveAssetChanges()
        {
            m_TargetAsset.Model.CopyFrom((LeaderboardInspectorConfig)m_SerializedObjectCurrent.targetObject);
            m_TargetAsset.Model.SaveToDisk();

            InitializeSerializedObjects();
        }

#if UNITY_2022_1_OR_NEWER
        public override void SaveChanges()
        {
            SaveAssetChanges();
            base.SaveChanges();
            UpdateApplyRevertButtons(false);
        }

        public override void DiscardChanges()
        {
            RevertAssetChanges();
            base.DiscardChanges();
            UpdateApplyRevertButtons(false);
        }
#else
        public void SaveChanges()
        {
            SaveAssetChanges();
            UpdateApplyRevertButtons(false);
        }

        public void DiscardChanges()
        {
            RevertAssetChanges();
            UpdateApplyRevertButtons(false);
        }
#endif

        void RevertAssetChanges()
        {
            BuildLeaderboardInspectorItem();
            InitializeSerializedObjects();
            UpdateApplyRevertButtons(false);
        }

        void AddDeploymentFooter(VisualElement container)
        {
            var deploymentConfigInspectorFooter = container.Q<DeploymentConfigInspectorFooter>();
            deploymentConfigInspectorFooter.BindGUI(
                AssetDatabase.GetAssetPath(target),
                LeaderboardAuthoringServices.Instance.GetService<ICommonAnalytics>(),
                ServiceName);

            if (m_TargetAsset == null)
                return;

            // cannot null-coalesce, because its a unity object
            if (m_TargetAsset.Model?.Id != null)
            {
                deploymentConfigInspectorFooter.DashboardLinkUrlGetter = () => LeaderboardAuthoringServices.Instance
                    .GetService<ILeaderboardsDashboardUrlResolver>()
                    .Leaderboard(m_TargetAsset.Model?.Id);
            }

            var resetCommand = LeaderboardAuthoringServices.Instance
                .GetService<ResetLeaderboardCommand>();
            deploymentConfigInspectorFooter.BindCommand(resetCommand, m_TargetAsset.Model, new ICommonAnalytics.CommonEventPayload()
            {
                action = "clicked_reset_leaderboard_inspector_btn",
                context = ServiceName
            });
        }

        static bool AreSerializedObjectsEqual(SerializedObject obj1, SerializedObject obj2)
        {
            if (obj1 == null || obj2 == null)
                return false;

            var iterator1 = obj1.GetIterator();
            var iterator2 = obj2.GetIterator();

            while (iterator1.NextVisible(true) && iterator2.NextVisible(true))
            {
                if (iterator1.propertyType != iterator2.propertyType || iterator1.name != iterator2.name)
                    return false;

                if (!SerializedProperty.DataEquals(iterator1, iterator2))
                    return false;
            }

            return true;
        }
    }
}
