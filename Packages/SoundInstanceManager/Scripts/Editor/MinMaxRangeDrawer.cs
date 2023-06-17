using Extensions.UnityExtensions.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minMaxSlider = (MinMaxRangeAttribute)attribute;
            
            position = EditorGUI.PrefixLabel(position, label);
            
            float minValue = 0;
            float maxValue = 0;
            
            switch (minMaxSlider.option)
            {
                case AttributeParameterOption.NameAndValue:
                    minValue = property.serializedObject.FindProperty(minMaxSlider.minValueName).floatValue;
                    break;
                case AttributeParameterOption.ValueAndName:
                    maxValue = property.serializedObject.FindProperty(minMaxSlider.maxValueName).floatValue;
                    break;
                case AttributeParameterOption.NameOnly:
                    minValue = property.serializedObject.FindProperty(minMaxSlider.minValueName).floatValue;
                    maxValue = property.serializedObject.FindProperty(minMaxSlider.maxValueName).floatValue;
                    break;
                case AttributeParameterOption.ValueOnly:
                    minValue = minMaxSlider.minValue;
                    maxValue = minMaxSlider.maxValue;
                    break;
                default:
                    minValue = minMaxSlider.minValue;
                    maxValue = minMaxSlider.maxValue;
                    break;
            }

            var handleWidthRectLeft = new Rect(position.x, position.y, position.width * 0.15f, position.height);
            var handleWidthRectRight= new Rect(position.x + position.width * 0.85f, position.y, position.width * 0.15f, position.height);
            var sliderRect = new Rect(position.x + position.width * 0.15f, position.y, position.width * 0.7f, position.height);

            EditorGUI.BeginChangeCheck();
            
            var sliderValue = property.vector2Value;
            
            EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
            EditorGUI.FloatField(handleWidthRectLeft, sliderValue.x);
            sliderValue.x = Mathf.Clamp(sliderValue.x, minValue, Mathf.Min(maxValue, sliderValue.y));
            EditorGUI.FloatField(handleWidthRectRight, sliderValue.y);
            sliderValue.y = Mathf.Clamp(sliderValue.y, Mathf.Max(minValue, sliderValue.x), maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = sliderValue;
            }
        }
    }
}