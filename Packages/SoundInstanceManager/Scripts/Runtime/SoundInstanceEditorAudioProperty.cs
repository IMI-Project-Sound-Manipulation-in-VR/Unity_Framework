using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMODUnity;
using System.Linq;

public enum SoundInstanceEditorAudioPropertyEvaluationType
{
    Curve,
    Linear,
    Level,
    Labeled
}

public enum SoundInstanceEditorAudioPropertyControlType
{
    None,
    Editor,
    Manager,
    Script
}

public enum SoundInstanceEditorAudioPropertyType
{
    FmodParameter,
    FmodAudioProperty,
    UnityAudioProperty
}

[Serializable]
public class SoundInstanceEditorAudioProperty
{
    public string propertyName;
    public SoundInstanceEditorAudioPropertyEvaluationType propertyEvaluationType;
    public SoundInstanceEditorAudioPropertyControlType propertyControlType;
    public SoundInstanceEditorAudioPropertyType propertyType;
    public bool showProperty = true;
    public AnimationCurve curve;
    public Vector2 level;
    public string[] labels;
    public float inputValue = 0f;
    public float outputValue = 0f;
    public float minValue = 0f;
    public float maxValue = 1f;
    public float defaultMinValue = 0f;
    public float defaultMaxValue = 0f;

    public void SetAudioPropertyFromFMODParameter(EditorParamRef reference){
        propertyName = reference.Name;
        propertyType = SoundInstanceEditorAudioPropertyType.FmodParameter;

        showProperty = false;

        curve = new AnimationCurve();
        level = new Vector2();
        labels = new string[0];

        // TODO: check if of type discrete or continuous
        if(reference.Type == FMODUnity.ParameterType.Labeled) { 
            propertyEvaluationType = SoundInstanceEditorAudioPropertyEvaluationType.Labeled;
            labels = reference.Labels;
        }

        else if(reference.Type == FMODUnity.ParameterType.Continuous) { 
            propertyEvaluationType = SoundInstanceEditorAudioPropertyEvaluationType.Linear;
        }
        
        inputValue = reference.Default;
        outputValue = 0f;
        minValue = reference.Min;
        maxValue = reference.Max;
        defaultMinValue = reference.Min;
        defaultMaxValue = reference.Max;
    }

    public void SetAudioPropertyFromPropertyTemplate(SoundInstanceEditorAudioPropertyTemplate template){

        propertyName = template.propertyData.propertyName;
        propertyEvaluationType = template.propertyData.propertyEvaluationType;
        propertyControlType = template.propertyData.propertyControlType;
        propertyType = template.propertyData.propertyType;
        showProperty = template.propertyData.showProperty;
        curve = template.propertyData.curve;
        level = template.propertyData.level;
        labels = template.propertyData.labels;
        inputValue = template.propertyData.inputValue;
        outputValue = template.propertyData.outputValue;
        minValue = template.propertyData.minValue;
        maxValue = template.propertyData.maxValue;
        defaultMinValue = template.propertyData.defaultMinValue;
        defaultMaxValue = template.propertyData.defaultMaxValue;
    }

    public void SetAudioPropertyFromAudioProperty(SoundInstanceEditorAudioProperty audioProperty)
    {
        propertyName = audioProperty.propertyName;
        propertyEvaluationType = audioProperty.propertyEvaluationType;
        propertyControlType = audioProperty.propertyControlType;
        propertyType = audioProperty.propertyType;
        showProperty = audioProperty.showProperty;
        curve = audioProperty.curve;
        level = audioProperty.level;
        labels = audioProperty.labels;
        inputValue = audioProperty.inputValue;
        outputValue = audioProperty.outputValue;
        minValue = audioProperty.minValue;
        maxValue = audioProperty.maxValue;
        defaultMinValue = audioProperty.defaultMinValue;
        defaultMaxValue = audioProperty.defaultMaxValue;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        SoundInstanceEditorAudioProperty other = (SoundInstanceEditorAudioProperty)obj;

        // Compare all the fields here
        if (!propertyName.Equals(other.propertyName))
            return false;

        if (propertyEvaluationType != other.propertyEvaluationType)
            return false;

        if (propertyControlType != other.propertyControlType)
            return false;

        if (propertyType != other.propertyType)
            return false;

        if (showProperty != other.showProperty)
            return false;

        if (!AreAnimationCurvesEqual(curve, other.curve))
            return false;

        if (!level.Equals(other.level))
            return false;

        if (!AreStringArraysEqual(labels, other.labels))
            return false;

        if (inputValue != other.inputValue)
            return false;

        if (minValue != other.minValue)
            return false;

        if (maxValue != other.maxValue)
            return false;

        if (defaultMinValue != other.defaultMinValue)
            return false;

        if (defaultMaxValue != other.defaultMaxValue)
            return false;

        // If all fields match, return true
        return true;
    }

    private bool AreAnimationCurvesEqual(AnimationCurve curve1, AnimationCurve curve2)
    {
        // Compare AnimationCurves using their serialized representation
        // You can use other comparison methods if needed
        return curve1 == curve2 || (curve1 != null && curve1.Equals(curve2));
    }

    private bool AreStringArraysEqual(string[] array1, string[] array2)
    {
        // Compare string arrays
        // You can use other comparison methods if needed
        return array1 == array2 || (array1 != null && array2 != null && array1.SequenceEqual(array2));
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + propertyName.GetHashCode();
        hash = hash * 23 + propertyEvaluationType.GetHashCode();
        hash = hash * 23 + propertyControlType.GetHashCode();
        hash = hash * 23 + propertyType.GetHashCode();
        // Add more fields to the hash calculation as needed
        return hash;
    }

}


