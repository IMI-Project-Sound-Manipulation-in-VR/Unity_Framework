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
    public int previousPropertyPreset;
    public int selectedPropertyPreset;

    // Properties
    private List<SoundInstanceEditorAudioProperty> audioProperties;
    public List<SoundInstanceEditorAudioProperty> AudioProperties { get { return audioProperties; } set { audioProperties = value; } }
    private bool[] audioPropertyFoldouts;
    public bool[] AudioPropertyFoldouts { get { return audioPropertyFoldouts; } set { audioPropertyFoldouts = value; } }

    // Public

    // Public Virtual
    public virtual void SetAudioPropertyValue(SoundInstanceEditorAudioProperty property, int index, float value){}
    public virtual void UpdateInstanceReference() { }
    public virtual void SetupAudioInstance() { }
    public virtual void UpdatePropertyTemplates() { }
    public virtual void UpdatePropertyPresets() { }
    public virtual void AddNewAudioProperty() { }
}
