using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundInstanceEditorUnityPropertyType
{
    Curve,
    Level,
    Linear
}

[CreateAssetMenu(fileName = "New Sound Instance Editor Property", menuName = "Sound Instance Editor Property")]
[System.Serializable]
public class SoundInstanceEditorUnityPropertyScriptableObject : ScriptableObject
{
    public string propertyName;
    public SoundInstanceEditorUnityPropertyType propertyType;
    public AnimationCurve curve;
    public Vector2 level;
    public float inputValue = 0f;
    public float outputValue = 0f;
    public float minValue = 0f;
    public float maxValue = 1f;

    public void SetupFields(SoundInstanceEditorUnityPropertyScriptableObject property)
    {
        this.propertyName = property.propertyName;
        this.propertyType = property.propertyType;
        this.curve = property.curve;
        this.level = property.level;
        this.inputValue = property.inputValue;
        this.outputValue = property.outputValue;
        this.minValue = property.minValue;
        this.maxValue = property.maxValue;
    }
}
