using UnityEngine;

namespace Extensions.UnityExtensions.Attributes
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly object comparedField1;
        public readonly object comparedField2;

        public readonly string comparedFieldName1;
        public readonly string comparedFieldName2;

        public readonly AttributeParameterOption option;

        public ShowIfAttribute(object comparedField1, object comparedField2)
        {
            this.comparedField1 = comparedField1;
            this.comparedField2 = comparedField2;
            option = AttributeParameterOption.ValueOnly;
        }

        public ShowIfAttribute(object comparedField, string comparedFieldName)
        {
            comparedFieldName1 = comparedFieldName;
            comparedField1 = comparedField;
            option = AttributeParameterOption.ValueAndName;
        }

        public ShowIfAttribute(string comparedFieldName, object comparedField)
        {
            comparedFieldName1 = comparedFieldName;
            comparedField1 = comparedField;
            option = AttributeParameterOption.NameAndValue;
        }

        public ShowIfAttribute(string comparedFieldName1, string comparedFieldName2)
        {
            this.comparedFieldName1 = comparedFieldName1;
            this.comparedFieldName2 = comparedFieldName2;
            option = AttributeParameterOption.NameOnly;
        }
    }

    public enum AttributeParameterOption
    {
        ValueOnly,
        NameAndValue,
        ValueAndName,
        NameOnly,
        Array
    }
}