using FMODUnity;
using UnityEngine;

public class FMODParameterDescription : ScriptableObject
{
    public EditorParamRef editorParamRef;
    public bool Locked;
    public string ParameterName { get; set; }
    public FMODUnity.ParameterType ParameterType { get; set; }
    public string[] Labels { get; set; }
    public float Minimum { get; set; }
    public float Maximum { get; set; }
    public float DefaultValue{ get; set; }
    public float CurrentValue { get; set; }

    public void SetParameterDescriptionFromStruct(EditorParamRef reference){

        editorParamRef = reference;

        ParameterName = reference.Name;
        ParameterType = reference.Type;
        Labels = reference.Labels;
        Minimum = reference.Min;
        Maximum = reference.Max;
        DefaultValue = reference.Default;
        CurrentValue = reference.Default;
        Locked = false;
    }
}
