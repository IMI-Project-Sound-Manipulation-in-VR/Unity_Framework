using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SoundInstanceEditorObjectUnity : SoundInstanceEditorObject
{
    private AudioSource audioSource;
    private AudioSource previousAudioSource;
    PropertyInfo[] reflectionAudioPropertyInfos;
    private SoundInstanceEditor editor;

    public SoundInstanceEditorObjectUnity(SoundInstanceEditor gameObject)
    {
        this.editor = gameObject;
    }

    public override void UpdateInstanceReference()
    {
        if(!audioSource.Equals(previousAudioSource)){
            previousAudioSource = audioSource;
            // TODO: name and shit

        }
    }

    // Overrides
    public override void UpdatePropertyPresets()
    {
        base.UpdatePropertyPresets();

        this.propertyPresets = Resources.LoadAll<SoundInstanceEditorAudioPropertyPreset>("Audio Property Presets");
    }

    public override void UpdatePropertyTemplates()
    {
        base.UpdatePropertyTemplates();

        // loads property templates, which are responsible for manipulating properties of a audio source object with additional options
        // the templates setup default values, such as default input value, max and min values and a curve object.
        this.PropertyTemplates = Resources.LoadAll<SoundInstanceEditorAudioPropertyTemplate>("Audio Property Templates");
    }

    public override void SetAudioPropertyValue(SoundInstanceEditorAudioProperty property, int index, float value)
    {
        // if the audio source has such a field, update it with output value
        if(reflectionAudioPropertyInfos == null) { return; }
        if(reflectionAudioPropertyInfos[index] != null && audioSource != null) {
            if(reflectionAudioPropertyInfos[index].PropertyType == typeof(bool))
            {
                bool b = property.outputValue != 0.0f;
                reflectionAudioPropertyInfos[index].SetValue(audioSource, b);
            } else {
                reflectionAudioPropertyInfos[index].SetValue(audioSource, value);
            }
        }
    }

    public override void SetupAudioInstance()
    {
        LoadReflectionScriptProperties();
    }

    // Public

    // Private
    public void LoadReflectionScriptProperties()
    {   
        if(this.AudioProperties != null){
            // generates a array of property infos for...
            
            // the properties of the audio source. these will hold the properties that will be controlled by this script
            reflectionAudioPropertyInfos = new PropertyInfo[this.AudioProperties.Count];

            // the properties of the script. these will hold the properties, from which this script can retrieves values automatically
            // reflectionScriptProperties = new PropertyInfo[this.AudioProperties.Count];

            // for each property available in the preset
            for (int i = 0; i < this.AudioProperties.Count; i++) {
                SoundInstanceEditorAudioProperty property = this.AudioProperties[i];

                // check if the audio source has a properties with the same name (ideally it should always yield the correct result. if not, maybe the name is wrong)
                PropertyInfo f = audioSource != null ? audioSource.GetType().GetProperty(property.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;
                
                // check if the script has a field with the same property name
                // this is optional. if the property doesnt exists, the external script will not controll the property
                // and so only manual mainpulation through the inspector will work
                // PropertyInfo p = editor.reflectionScriptType != null ? editor.reflectionScriptType.GetProperty(property.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;

                // assign the property infos if they exist
                if(f != null) reflectionAudioPropertyInfos[i] = f;
                // if(p != null) reflectionScriptProperties[i] = p;
            }
        }

        // editorLevelProperty = scriptType != null ? scriptType.GetProperty("editorLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;
    }


}
