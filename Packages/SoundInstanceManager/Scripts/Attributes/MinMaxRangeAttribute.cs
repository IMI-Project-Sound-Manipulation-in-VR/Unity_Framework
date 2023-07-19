using System;
using UnityEngine;

namespace Extensions.UnityExtensions.Attributes
{
    [Serializable]
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public readonly float maxValue;
        public readonly string maxValueName;
        public readonly float minValue;

        public readonly string minValueName;

        public readonly AttributeParameterOption option;

        public MinMaxRangeAttribute(float minValue, float maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            option = AttributeParameterOption.ValueOnly;
        }

        public MinMaxRangeAttribute(float minValue, string maxValueName)
        {
            this.minValue = minValue;
            this.maxValueName = maxValueName;
            option = AttributeParameterOption.ValueAndName;
        }

        public MinMaxRangeAttribute(string minValueName, float maxValue)
        {
            this.minValueName = minValueName;
            this.maxValue = maxValue;
            option = AttributeParameterOption.NameAndValue;
        }

        public MinMaxRangeAttribute(string minValueName, string maxValueName)
        {
            this.minValueName = minValueName;
            this.maxValueName = maxValueName;
            option = AttributeParameterOption.NameOnly;
        }
    }
}