using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Reflection;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [CustomEditor(typeof(LeaderboardInspectorConfig))]
    class LeaderboardInspectorConfigEditor : UnityEditor.Editor
    {
        const string k_FoldoutPrefPrefix = "LeaderboardInspectorConfig_Foldout_";

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var advancedFields = new List<FieldInfo>();
            var baseFields = new List<FieldInfo>();

            foreach (var field in fields)
            {
                if (field.Name == "LeaderboardId" ||
                    field.Name == "BucketSize" ||
                    field.Name == "ResetConfig" ||
                    field.Name == "TieringConfig")
                {
                    advancedFields.Add(field);
                }
                else
                {
                    baseFields.Add(field);
                }
            }

            foreach (var field in baseFields)
            {
                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    root.Add(new PropertyField(property));
                }
            }

            var foldoutPrefKey = k_FoldoutPrefPrefix +  "AdvancedSettings";
            var isExpanded = EditorPrefs.GetBool(foldoutPrefKey, false);

            var foldout = new Foldout();
            foldout.text = "Advanced Settings";
            foldout.value = isExpanded;
            foldout.style.marginTop = 5;
            foldout.style.marginBottom = 5;

            foreach (var field in advancedFields)
            {
                var property = serializedObject.FindProperty(field.Name);
                if (property != null)
                {
                    var propertyField = new PropertyField(property);
                    propertyField.style.marginLeft = 15;
                    foldout.Add(propertyField);
                }
            }

            foldout.RegisterValueChangedCallback(evt => {
                if (evt.target == foldout)
                {
                    EditorPrefs.SetBool(foldoutPrefKey, evt.newValue);
                }
            });

            root.Add(foldout);

            return root;
        }
    }
}
