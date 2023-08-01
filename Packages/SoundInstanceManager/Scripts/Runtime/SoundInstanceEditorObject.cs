using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SoundInstanceEditorObject
{
    // General Info
    public string InstanceName { get; set; }
    public SoundInstanceEditorType EditorType { get; set; }

    // Templates
    public SoundInstanceEditorAudioPropertyTemplate[] PropertyTemplates { get; set; }
    public int SelectedPropertyTemplateIndex { get; set; }

    // Presets
    public SoundInstanceEditorAudioPropertyPreset[] PropertyPresets { get; set; }
    public int PreviousPropertyPresetIndex { get; set; }
    public int SelectedPropertyPresetIndex { get; set; }

    // Properties
    public List<SoundInstanceEditorAudioProperty> AudioProperties { get; set; }

    public bool ComparePresetWithAudioProperties()
    {
        SoundInstanceEditorAudioProperty[] presetArray = PropertyPresets[SelectedPropertyPresetIndex].propertiesArray;
        if(presetArray.Length != AudioProperties.Count) { return false; }
        for(int i = 0; i < presetArray.Length; i++)
        {
            if(!presetArray[i].Equals(AudioProperties[i])) { return false; }
        }
        
        return true;
    }

    // Public Virtual
    public virtual void SetAudioPropertyValue(SoundInstanceEditorAudioProperty property, int index, float value){}
    public virtual void SetupAudioReference() { }
    public virtual void SetAudioInstance() { }
    public virtual void DisableAudioInstance() { }
    public virtual void LoadPropertyPresets() {
        this.PropertyPresets = Resources.LoadAll<SoundInstanceEditorAudioPropertyPreset>("Audio Property Presets");
    }
    public virtual void LoadPropertyTemplates() { 
        this.PropertyTemplates = Resources.LoadAll<SoundInstanceEditorAudioPropertyTemplate>("Audio Property Templates");
    }
    public virtual void AddNewAudioProperty() { }
    public virtual void RemoveAudioProperty(int index) { }
    public virtual void SetAudioProperties() { }
    public virtual void SetAudioPropertiesFromPreset() { }
    public virtual void SetInstanceName() {}
}
