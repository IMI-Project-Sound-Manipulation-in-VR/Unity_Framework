using System;
using Extensions.UnityExtensions.Attributes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfEqualDrawer : PropertyDrawer
    {
        private float _propertyHeight;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _propertyHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var showIfAttribute = (ShowIfAttribute)attribute;
            
            var value1 = showIfAttribute.comparedField1;
            var value2 = showIfAttribute.comparedField2;
            
            if (showIfAttribute.option is AttributeParameterOption.NameAndValue or AttributeParameterOption.ValueAndName)
            {
                value2 = property.serializedObject.FindProperty(RemovePartFromEnd(property.propertyPath) + showIfAttribute.comparedFieldName1).GetUnderlyingValue();
            }
            if (showIfAttribute.option == AttributeParameterOption.NameOnly)
            {
                value1 = property.serializedObject.FindProperty(RemovePartFromEnd(property.propertyPath) + showIfAttribute.comparedFieldName1).GetUnderlyingValue();
                value2 = property.serializedObject.FindProperty(RemovePartFromEnd(property.propertyPath) + showIfAttribute.comparedFieldName1).GetUnderlyingValue();
            }
            _propertyHeight = base.GetPropertyHeight(property, label);
            
            if (value1.Equals(value2))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                _propertyHeight = 0;
            }
        }
        
        private string RemovePartFromEnd(string input)
        {
            return input[..(input.LastIndexOf(".", StringComparison.Ordinal) + 1)];
        }
    }
}