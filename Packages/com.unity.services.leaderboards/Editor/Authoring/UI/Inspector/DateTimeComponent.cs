using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Services.Leaderboards.Editor.Authoring.UI.Inspector
{
#if UNITY_2023_3_OR_NEWER
    [UxmlElement]
#endif
    partial class DateTimeComponent : VisualElement
    {
        enum DropdownIdentifier
        {
            DropdownYear,
            DropdownMonth,
            DropdownDay,
            DropdownHour,
            DropdownMinute,
            DropdownSecond
        }

        internal DropdownField YearsDropdown;
        internal DropdownField MonthsDropdown;
        internal DropdownField DaysDropdown;
        internal DropdownField HoursDropdown;
        internal DropdownField MinutesDropdown;
        internal DropdownField SecondsDropdown;

        internal DateTime DateTime;
        bool m_IsAttached;

        public event Action<DateTime> OnValueChanged;

        public DateTimeComponent()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.services.leaderboards/Editor/Authoring/UI/Assets/DateTime_Template.uxml");
            asset.CloneTree(this);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        public DateTimeComponent(DateTime dateTime) : this()
        {
            DateTime = dateTime;
        }

        void OnAttachToPanel(AttachToPanelEvent e)
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            m_IsAttached = true;
            SetupDropdowns();
            SetDateTime(DateTime);
        }

        void SetupDropdowns()
        {
            YearsDropdown = SetupGenericDropdown(DropdownIdentifier.DropdownYear, 2025, 2050);
            MonthsDropdown = SetupGenericDropdown(DropdownIdentifier.DropdownMonth, 1, 12);
            HoursDropdown = SetupGenericDropdown(DropdownIdentifier.DropdownHour, 0, 23);
            MinutesDropdown = SetupGenericDropdown(DropdownIdentifier.DropdownMinute, 0, 59);
            SecondsDropdown = SetupGenericDropdown(DropdownIdentifier.DropdownSecond, 0, 59);
            SetupDaysDropdown();
        }

        DropdownField SetupGenericDropdown(DropdownIdentifier identifier, int minValue, int maxValue)
        {
            var dropdownField = this.Q<DropdownField>(identifier.ToString());
            dropdownField.RegisterCallback<ChangeEvent<string>>(c => OnFieldValueChanged(c , identifier));
            for (var i = minValue; i <= maxValue; ++i)
            {
                dropdownField.choices.Add(i.ToString());
            }

            return dropdownField;
        }

        void SetupDaysDropdown()
        {
            DaysDropdown = this.Q<DropdownField>("DropdownDay");
            DaysDropdown.RegisterCallback<ChangeEvent<string>>(c => OnFieldValueChanged(c, DropdownIdentifier.DropdownDay));
            SetupDaysChoices(DateTime.Year, DateTime.Month);
        }

        void SetupDaysChoices(int year, int month)
        {
            var dayMustChange = true;
            DaysDropdown.choices.Clear();
            for (var i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                if (DateTime.Day == i)
                {
                    dayMustChange = false;
                }

                DaysDropdown.choices.Add(i.ToString());
            }

            if (dayMustChange)
            {
                DaysDropdown.SetValueWithoutNotify("1");
                DateTime = new DateTime(
                    year,
                    month,
                    1,
                    DateTime.Hour,
                    DateTime.Minute,
                    DateTime.Second);
            }
        }

        void OnFieldValueChanged(ChangeEvent<string> evt, DropdownIdentifier identifier)
        {
            var year = DateTime.Year;
            var month = DateTime.Month;
            var day = DateTime.Day;
            var hour = DateTime.Hour;
            var minute = DateTime.Minute;
            var second = DateTime.Second;

            switch (identifier)
            {
                case DropdownIdentifier.DropdownYear:
                    int.TryParse(evt.newValue, out year);
                    SetupDaysChoices(year, month);
                    day = DateTime.Day;
                    break;
                case DropdownIdentifier.DropdownMonth:
                    int.TryParse(evt.newValue, out month);
                    SetupDaysChoices(year, month);
                    day = DateTime.Day;
                    break;
                case DropdownIdentifier.DropdownDay:
                    int.TryParse(evt.newValue, out day);
                    break;
                case DropdownIdentifier.DropdownHour:
                    int.TryParse(evt.newValue, out hour);
                    break;
                case DropdownIdentifier.DropdownMinute:
                    int.TryParse(evt.newValue, out minute);
                    break;
                case DropdownIdentifier.DropdownSecond:
                    int.TryParse(evt.newValue, out second);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(identifier), identifier, null);
            }

            DateTime = new DateTime(year, month, day, hour, minute, second);
            OnValueChanged?.Invoke(DateTime);
        }

        void SetDateTime(DateTime dateTime)
        {
            if (!m_IsAttached)
            {
                return;
            }

            DateTime = dateTime;
            YearsDropdown.SetValueWithoutNotify(dateTime.Year.ToString());
            MonthsDropdown.SetValueWithoutNotify(dateTime.Month.ToString());
            DaysDropdown.SetValueWithoutNotify(dateTime.Day.ToString());
            HoursDropdown.SetValueWithoutNotify(dateTime.Hour.ToString());
            MinutesDropdown.SetValueWithoutNotify(dateTime.Minute.ToString());
            SecondsDropdown.SetValueWithoutNotify(dateTime.Second.ToString());
        }

#if !UNITY_2023_3_OR_NEWER
        new class UxmlFactory : UxmlFactory<DateTimeComponent> {}
#endif
    }
}
