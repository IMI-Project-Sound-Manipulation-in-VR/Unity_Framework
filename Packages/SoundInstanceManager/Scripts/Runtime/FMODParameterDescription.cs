using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;

public class FMODParameterDescription : ScriptableObject
{
    public string ParameterName { get; set; }
    public FMOD.Studio.PARAMETER_FLAGS ParameterType { get; set; }
    public string[] Labels { get; set; }
    public float Minimum { get; set; }
    public float Maximum { get; set; }
    public float DefaultValue{ get; set; }
    public float CurrentValue { get; set; }

    public void SetParameterDescriptionFromStruct(EventDescription eventDescription, PARAMETER_DESCRIPTION parameterDescription){
        ParameterType = parameterDescription.flags;
        if((ParameterType & PARAMETER_FLAGS.LABELED) != 0) {
            Labels = new string[(int) parameterDescription.maximum + 1];
            for(int i = 0; i <= (int) parameterDescription.maximum; i++)
            {
                string label;
                eventDescription.getParameterLabelByID(parameterDescription.id, i, out label);
                Labels[i] = label;
            }
        }

        ParameterName = parameterDescription.name;
        Minimum = parameterDescription.minimum;
        Maximum = parameterDescription.maximum;
        DefaultValue = parameterDescription.defaultvalue;
        CurrentValue = parameterDescription.defaultvalue;
    }
}
