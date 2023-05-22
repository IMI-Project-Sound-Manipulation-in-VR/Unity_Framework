using Extensions.UnityExtensions.Attributes;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(MinMaxRangeIntAttribute))]
    public class MinMaxRangeIntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var minMaxSlider = (MinMaxRangeIntAttribute)attribute;

            position = EditorGUI.PrefixLabel(position, label);

            var minValue = 0;
            var maxValue = 0;

            switch (minMaxSlider.option)
            {
                case AttributeParameterOption.NameAndValue:
                    minValue = property.serializedObject.FindProperty(minMaxSlider.minValueName).intValue;
                    break;
                case AttributeParameterOption.ValueAndName:
                    maxValue = property.serializedObject.FindProperty(minMaxSlider.maxValueName).intValue;
                    break;
                case AttributeParameterOption.NameOnly:
                    minValue = property.serializedObject.FindProperty(minMaxSlider.minValueName).intValue;
                    maxValue = property.serializedObject.FindProperty(minMaxSlider.maxValueName).intValue;
                    break;
                case AttributeParameterOption.ValueOnly:
                    minValue = minMaxSlider.minValue;
                    maxValue = minMaxSlider.maxValue;
                    break;
                case AttributeParameterOption.Array:
                    minValue = 0;
                    var temp =  property.serializedObject.FindProperty(minMaxSlider.maxValueName);
                    maxValue = temp.arraySize - 1;
                    break;
                default:
                    minValue = minMaxSlider.minValue;
                    maxValue = minMaxSlider.maxValue;
                    break;
            }

            var handleWidthRectLeft = new Rect(position.x, position.y, position.width * 0.15f, position.height);
            var handleWidthRectRight = new Rect(position.x + position.width * 0.85f, position.y, position.width * 0.15f,
                position.height);
            var sliderRect = new Rect(position.x + position.width * 0.15f, position.y, position.width * 0.7f,
                position.height);

            EditorGUI.BeginChangeCheck();

            var sliderIntValue = property.vector2IntValue;
            var sliderValue = new Vector2(sliderIntValue.x, sliderIntValue.y);
            
            EditorGUI.MinMaxSlider(sliderRect, ref sliderValue.x, ref sliderValue.y, minValue, maxValue);
            EditorGUI.IntField(handleWidthRectLeft, sliderIntValue.x);
            EditorGUI.IntField(handleWidthRectRight, sliderIntValue.y);
            
            sliderValue.x = Mathf.Clamp(sliderValue.x, minValue, Mathf.Min(maxValue, sliderValue.y));
            sliderValue.y = Mathf.Clamp(sliderValue.y, Mathf.Max(minValue, sliderValue.x), maxValue);
            
            if (EditorGUI.EndChangeCheck())
            {
                property.vector2IntValue = new Vector2Int(Mathf.RoundToInt(sliderValue.x), Mathf.RoundToInt(sliderValue.y));
            }
        }
    }
}