using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SoundInstanceEditorObject
{
    // General Info
    private string instanceName;
    public string InstanceName { get { return instanceName; } set { instanceName = value; } }
    private SoundInstanceEditorType editorType;
    public SoundInstanceEditorType EditorType { get { return editorType; } set { editorType = value; } }

    // private PropertyInfo[] reflectionAudioProperties;

    // Templates
    private SoundInstanceEditorAudioPropertyTemplate[] propertyTemplates;
    public SoundInstanceEditorAudioPropertyTemplate[] PropertyTemplates { get { return propertyTemplates; } set { propertyTemplates = value; } }
    public int previousPropertyTemplateIndex;
    public int selectedPropertyTemplateIndex;

    // Presets
    // TODO: convert to properties
    public SoundInstanceEditorAudioPropertyPreset[] propertyPresets;
    public int previousPropertyPresetIndex;
    public int selectedPropertyPresetIndex;

    // Properties
    private List<SoundInstanceEditorAudioProperty> audioProperties;
    public List<SoundInstanceEditorAudioProperty> AudioProperties { get { return audioProperties; } set { audioProperties = value; } }
    private bool[] audioPropertyFoldouts;
    public bool[] AudioPropertyFoldouts { get { return audioPropertyFoldouts; } set { audioPropertyFoldouts = value; } }

    // Public

    public bool ComparePresetWithAudioProperties()
    {
        SoundInstanceEditorAudioProperty[] presetArray = propertyPresets[selectedPropertyPresetIndex].propertiesArray;
        if(presetArray.Length != audioProperties.Count) { return false; }
        for(int i = 0; i < presetArray.Length; i++)
        {
            if(presetArray[i].propertyName != audioProperties[i].propertyName) { return false; }
        }
        
        return true;
    }

    // Public Virtual
    public virtual void SetAudioPropertyValue(SoundInstanceEditorAudioProperty property, int index, float value){}
    public virtual void SetupAudioReference() { }
    public virtual void SetAudioInstance() { }
    public virtual void DisableAudioInstance() { }
    public virtual void LoadPropertyTemplates() { }
    public virtual void LoadPropertyPresets() { }
    public virtual void AddNewAudioProperty() { }
    public virtual void RemoveAudioProperty(int index) { }
    public virtual void SetAudioProperties() { }
    public virtual void SetAudioPropertiesFromPreset() { }
}
