using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;

public class SoundInstanceEditorObjectUnity : SoundInstanceEditorObject
{
    PropertyInfo[] reflectionAudioPropertyInfos;
    private SoundInstanceEditor editor;

    public SoundInstanceEditorObjectUnity(SoundInstanceEditor editor)
    {
        this.editor = editor;
        this.EditorType = SoundInstanceEditorType.Unity;

        LoadPropertyPresets();
        LoadPropertyTemplates();

        SetupAudioReference();
    }

    // Overrides

    public override void SetAudioPropertiesFromPreset()
    {
        SoundInstanceEditorAudioPropertyPreset propertyPreset = this.PropertyPresets[this.SelectedPropertyPresetIndex];
        if(propertyPreset.propertiesArray.Length == 0) {
            this.AudioProperties = new List<SoundInstanceEditorAudioProperty>();
        } else {
            this.AudioProperties = RetrieveAudioPropertiesFromPreset();
        }
        SetAudioProperties();
    }

    private List<SoundInstanceEditorAudioProperty> RetrieveAudioPropertiesFromTemplates()
    {
        SoundInstanceEditorAudioPropertyTemplate[] filteredTemplates = this.PropertyTemplates.Where(obj => obj.propertyData.propertyType == SoundInstanceEditorAudioPropertyType.FmodAudioProperty).ToArray();
        List<SoundInstanceEditorAudioProperty> audioProperties = new List<SoundInstanceEditorAudioProperty>();
        for(int i = 0; i < filteredTemplates.Length; i++)
        {
            SoundInstanceEditorAudioProperty newAudioProperty = new SoundInstanceEditorAudioProperty();
            newAudioProperty.SetAudioPropertyFromPropertyTemplate(filteredTemplates[i]);
            audioProperties.Add(newAudioProperty);
        }
        return audioProperties;
    }

    private List<SoundInstanceEditorAudioProperty> RetrieveAudioPropertiesFromPreset()
    {
        SoundInstanceEditorAudioProperty[] propertiesArray = this.PropertyPresets[this.SelectedPropertyPresetIndex].propertiesArray;
        List<SoundInstanceEditorAudioProperty> audioProperties = new List<SoundInstanceEditorAudioProperty>();
        for(int i = 0; i < propertiesArray.Length; i++)
        {
            SoundInstanceEditorAudioProperty newAudioProperty = new SoundInstanceEditorAudioProperty();
            newAudioProperty.SetAudioPropertyFromAudioProperty(propertiesArray[i]);
            audioProperties.Add(newAudioProperty);
        }
        return audioProperties;
    }

    public override void SetupAudioReference()
    {
        SetAudioInstance();
        SetInstanceName();
        SetAudioPropertiesFromPreset();
    }

    public override void SetAudioProperties()
    {
        base.SetAudioProperties();

        if(this.AudioProperties == null) { this.AudioProperties = new List<SoundInstanceEditorAudioProperty>(); }
        
        FindCorrespondingAudioPropertyInfo();

    }

    public override void AddNewAudioProperty()
    {
        base.AddNewAudioProperty();

        SoundInstanceEditorAudioPropertyTemplate template = this.PropertyTemplates[this.SelectedPropertyTemplateIndex];
        SoundInstanceEditorAudioProperty newAudioProperty = new SoundInstanceEditorAudioProperty();
        newAudioProperty.SetAudioPropertyFromPropertyTemplate(template);
        this.AudioProperties.Add(newAudioProperty);

        SetAudioProperties();
    }

    public override void RemoveAudioProperty(int index)
    {
        base.RemoveAudioProperty(index);

        this.AudioProperties.RemoveAt(index);

        SetAudioProperties();
    }

    public override void LoadPropertyTemplates()
    {
        base.LoadPropertyTemplates();

        // loads property templates, which are responsible for manipulating properties of a audio source object with additional options
        // the templates setup default values, such as default input value, max and min values and a curve object.
        this.PropertyTemplates = this.PropertyTemplates.Where(obj => obj.propertyData.propertyType == SoundInstanceEditorAudioPropertyType.UnityAudioProperty).ToArray();
    }

    public override void SetAudioPropertyValue(SoundInstanceEditorAudioProperty property, int index, float value)
    {
        // if the audio source has such a field, update it with output value
        if(reflectionAudioPropertyInfos == null) { return; }
        if(reflectionAudioPropertyInfos[index] != null && editor.AudioSourceReference != null) {
            if(reflectionAudioPropertyInfos[index].PropertyType == typeof(bool))
            {
                bool b = property.outputValue != 0.0f;
                reflectionAudioPropertyInfos[index].SetValue(editor.AudioSourceReference, b);
            } else {
                reflectionAudioPropertyInfos[index].SetValue(editor.AudioSourceReference, value);
            }
        }
    }

    public override void SetAudioInstance()
    {
        DisableAudioInstance();

        AudioSource newAudioSource = editor.gameObject.AddComponent<AudioSource>();
        newAudioSource.enabled = true;
        newAudioSource.clip = editor.AudioClipReference;
        newAudioSource.loop = true;
        // newAudioSource.hideFlags = HideFlags.HideInInspector;

        editor.AudioSourceReference = newAudioSource;
        if(Application.isPlaying)
        {
            editor.AudioSourceReference.Play();
        }
    }

    public override void DisableAudioInstance()
    {
        base.DisableAudioInstance();

        foreach(AudioSource audioSource in editor.gameObject.GetComponents<AudioSource>())
        {
            GameObject.DestroyImmediate(audioSource);
        }
    }

    public override void SetInstanceName()
    {
        this.InstanceName = editor.AudioSourceReference.clip.name;
    }

    private void FindCorrespondingAudioPropertyInfo()
    {   
        if(this.AudioProperties != null){
            // generates a array of property infos for...
            
            // the properties of the audio source. these will hold the properties that will be controlled by this script
            reflectionAudioPropertyInfos = new PropertyInfo[this.AudioProperties.Count];

            // the properties of the script. these will hold the properties, from which this script can retrieves values automatically
            editor.ReflectionScriptProperties = new PropertyInfo[this.AudioProperties.Count];

            // for each property available in the preset
            for (int i = 0; i < this.AudioProperties.Count; i++) {
                SoundInstanceEditorAudioProperty property = this.AudioProperties[i];

                // check if the audio source has a properties with the same name (ideally it should always yield the correct result. if not, maybe the name is wrong)
                PropertyInfo f = editor.AudioSourceReference != null ? editor.AudioSourceReference.GetType().GetProperty(property.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;
                
                // check if the script has a field with the same property name
                // this is optional. if the property doesnt exists, the external script will not controll the property
                // and so only manual mainpulation through the inspector will work
                PropertyInfo p = editor.reflectionScriptType != null ? editor.reflectionScriptType.GetProperty(property.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;

                // assign the property infos if they exist
                if(f != null) reflectionAudioPropertyInfos[i] = f;
                if(p != null) editor.ReflectionScriptProperties[i] = p;
            }
        }

        // editorLevelProperty = scriptType != null ? scriptType.GetProperty("editorLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;
    }

}
