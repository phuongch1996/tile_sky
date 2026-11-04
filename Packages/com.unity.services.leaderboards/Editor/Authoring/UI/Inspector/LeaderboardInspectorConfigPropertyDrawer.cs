using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
    [CustomPropertyDrawer(typeof(NullableSerializableResetConfig), useForChildren: true)]
    class ResetConfigPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            LeaderboardPropertyDrawerUtils.CreateNullableGUI(property, container, "Use Reset", "Configuration");
            return container;
        }
    }

    [CustomPropertyDrawer(typeof(SerializableDateTime), useForChildren: true)]
    class SerializableDateTimePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var ticksType = property.FindPropertyRelative("Ticks");
            var dateTime = new DateTime(ticksType.longValue);
            var dateTimeComponent = new DateTimeComponent(dateTime);
            container.Add(dateTimeComponent);
            dateTimeComponent.OnValueChanged += d => OnDateTimeValueChanged(d, property);

            return container;
        }

        void OnDateTimeValueChanged(DateTime dateTime, SerializedProperty property)
        {
            var ticksType = property.FindPropertyRelative("Ticks");
            ticksType.longValue = dateTime.Ticks;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(NullableSerializableTieringConfig), useForChildren: true)]
    class TieringConfigPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            LeaderboardPropertyDrawerUtils.CreateNullableGUI(property, container, "Use Tiering", "Configuration");
            return container;
        }
    }

    [CustomPropertyDrawer(typeof(NullableDouble), useForChildren: true)]
    class NullableDoublePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            LeaderboardPropertyDrawerUtils.CreateNullableGUI(property, container, "Use Cutoff", "Cutoff value");
            return container;
        }
    }

    [CustomPropertyDrawer(typeof(NullableBucketSize), useForChildren: true)]
    class NullableBucketSizePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            LeaderboardPropertyDrawerUtils.CreateNullableGUI(property, container, "Use Buckets", "Max players per bucket");
            return container;
        }
    }

    static class LeaderboardPropertyDrawerUtils
    {
        public static void CreateNullableGUI(SerializedProperty property, VisualElement container, string usePropertyName, string valueName)
        {
            var hasValueProp = property.FindPropertyRelative("HasValue");
            var hasValueField = new PropertyField(hasValueProp);
            hasValueField.name = "HasValue";
            hasValueField.label = usePropertyName;
            container.Add(hasValueField);

            var valueProp = property.FindPropertyRelative("Value");
            var valueField = new PropertyField(valueProp);
            valueField.name = "Value";
            valueField.label = valueName;
            container.Add(valueField);

            hasValueField.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                valueField.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            });
        }
    }

}
