using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SoundInstanceEditorUnityProperty
{
    public string propertyName;
    public SoundInstanceEditorUnityPropertyType propertyType;
    public AnimationCurve curve;
    public Vector2 level;
    public float inputValue = 0f;
    public float outputValue = 0f;
    public float minValue = 0f;
    public float maxValue = 1f;

    public void SetupFields(SoundInstanceEditorUnityPropertyScriptableObject propertyCurve)
    {
        this.propertyName = propertyCurve.propertyName;
        this.propertyType = propertyCurve.propertyType;
        this.curve = propertyCurve.curve;
        this.level = propertyCurve.level;
        this.inputValue = propertyCurve.inputValue;
        this.outputValue = propertyCurve.outputValue;
        this.minValue = propertyCurve.minValue;
        this.maxValue = propertyCurve.maxValue;
    }
}