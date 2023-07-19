using System;
using UnityEngine;

namespace Extensions.UnityExtensions.Attributes
{
    [Serializable]
    public class MinMaxRangeIntAttribute : PropertyAttribute
    {
        public readonly int maxValue;
        public readonly string maxValueName;
        public readonly int minValue;

        public readonly string minValueName;

        public readonly AttributeParameterOption option;

        public MinMaxRangeIntAttribute(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            option = AttributeParameterOption.ValueOnly;
        }

        public MinMaxRangeIntAttribute(int minValue, string maxValueName)
        {
            this.minValue = minValue;
            this.maxValueName = maxValueName;
            option = AttributeParameterOption.ValueAndName;
        }

        public MinMaxRangeIntAttribute(string minValueName, int maxValue)
        {
            this.minValueName = minValueName;
            this.maxValue = maxValue;
            option = AttributeParameterOption.NameAndValue;
        }

        public MinMaxRangeIntAttribute(string minValueName, string maxValueName)
        {
            this.minValueName = minValueName;
            this.maxValueName = maxValueName;
            option = AttributeParameterOption.NameOnly;
        }

        public MinMaxRangeIntAttribute(string arrayName)
        {
            maxValueName = arrayName;
            option = AttributeParameterOption.Array;
        }
    }
}