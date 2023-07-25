using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMODUnity;

public enum SoundInstanceEditorAudioPropertyEvaluationType
{
    Curve,
    Linear,
    Level,
    Labeled
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
    public SoundInstanceEditorAudioPropertyType propertyType;
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
}


