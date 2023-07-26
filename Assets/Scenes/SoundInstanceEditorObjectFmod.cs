using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundInstanceEditorObjectFmod : SoundInstanceEditorObject
{
    private EventInstance eventInstance;
    private EditorEventRef eventAsset;
    private string playbackState;
    private SoundInstanceEditor editor;
    public SoundInstanceEditorObjectFmod(SoundInstanceEditor editor)
    {
        this.editor = editor;
        this.EditorType = SoundInstanceEditorType.Fmod;
        
        LoadPropertyPresets();
        LoadPropertyTemplates();

        SetupAudioReference();
    }

    // Overrides
    public override void SetupAudioReference(){
        SetEventInformationFromReference();
        SetInstanceName();
        SetAudioProperties();
        SetAudioInstance();
    }

    public override void SetAudioProperties() {
        List<SoundInstanceEditorAudioProperty> propertiesFromParameters = RetrieveAudioPropertiesFromFMODParameters();
        List<SoundInstanceEditorAudioProperty> propertiesFromTemplates = RetrieveAudioPropertiesFromTemplates();
        // List<SoundInstanceEditorAudioProperty> propertiesFromPropertiesList = RetrieveAudioPropertiesFromPropertiesList();
        // TODO: get properties from selections
        this.AudioProperties = propertiesFromTemplates.Concat(propertiesFromParameters).ToList();
        this.AudioPropertyFoldouts = new bool[this.AudioProperties.Count];
    }

    public override void AddNewAudioProperty()
    {
        base.AddNewAudioProperty();

        SoundInstanceEditorAudioPropertyTemplate template = this.PropertyTemplates[this.selectedPropertyTemplateIndex];
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

    public override void LoadPropertyPresets()
    {
        base.LoadPropertyPresets();

        this.propertyPresets = Resources.LoadAll<SoundInstanceEditorAudioPropertyPreset>("Audio Property Presets");
    }

    public override void LoadPropertyTemplates()
    {
        base.LoadPropertyTemplates();

        this.PropertyTemplates = Resources.LoadAll<SoundInstanceEditorAudioPropertyTemplate>("Audio Property Templates");
    }

    public override void SetAudioPropertyValue(SoundInstanceEditorAudioProperty property, int index, float value)
    {
        if(eventInstance.isValid())
        {
            switch (property.propertyType)
            {
                case SoundInstanceEditorAudioPropertyType.FmodParameter:
                { 
                    eventInstance.setParameterByName(property.propertyName, property.outputValue);
                    break;
                }
                case SoundInstanceEditorAudioPropertyType.FmodAudioProperty:
                {
                    switch (property.propertyName)
                    {
                        case "Volume" :
                        {
                            eventInstance.setVolume(property.outputValue);
                            break;
                        }
                        case "Pitch" :
                        {
                            eventInstance.setPitch(property.outputValue);
                            break;
                        }
                    }
                    
                    break;
                }
            }
        }
    }

    public override void SetAudioInstance()
    {
        if(this.eventInstance.isValid())
        {
            FMODSoundManager.instance.ReleaseEventInstance(this.eventInstance);
        }

        // TODO: when event instance already exist, destroy it first
        // or else there will be multiple sounds playing
        if(FMODSoundManager.instance != null)
        {
            eventInstance = FMODSoundManager.instance.CreateEventInstance(editor.FmodEventReference);
            // RetrieveEventInstanceInformation();
            // sets 3d attributes of sound (needed for spatialize)
            FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(editor.gameObject.transform);
            eventInstance.set3DAttributes(attributes);
        } 
        
        if(Application.isPlaying && FMODSoundManager.instance == null)
        {
            Debug.LogError("FMOD Sound Manager is missing!");
        }

        if(Application.isPlaying)
        {
            Play();
        }
        
    }

    public override void DisableAudioInstance()
    {
        base.DisableAudioInstance();

        if(this.eventInstance.isValid())
        {
            FMODSoundManager.instance.ReleaseEventInstance(this.eventInstance);
        }
    }

    // Public
    
    // Private
    private void Play() 
    {
        PLAYBACK_STATE currentPlaybackState;
        eventInstance.getPlaybackState(out currentPlaybackState);

        if (currentPlaybackState != PLAYBACK_STATE.PLAYING)
        {
            eventInstance.start();

            PLAYBACK_STATE pbState;
            eventInstance.getPlaybackState(out pbState);
            playbackState = pbState.ToString();
        }
    }

    private void SetEventInformationFromReference()
    {
        eventAsset = EventManager.EventFromPath(editor.FmodEventReference.Path);
    }

    private void SetInstanceName()
    {
        this.InstanceName = eventAsset != null ? eventAsset.Path.Substring("event:/".Length) : "";
    }

    private List<SoundInstanceEditorAudioProperty> RetrieveAudioPropertiesFromFMODParameters() {
        // resets properties list
        List<SoundInstanceEditorAudioProperty> audioProperties = new List<SoundInstanceEditorAudioProperty>();

        if(eventAsset != null)
        {
            // retrieves local parameters of fmod event
            List<EditorParamRef> localParameters = eventAsset.LocalParameters;
            // iterates through each parameter and copies data from fmod parameter
            foreach(EditorParamRef editorParamRef in localParameters){
                SoundInstanceEditorAudioProperty property = new SoundInstanceEditorAudioProperty();
                property.SetAudioPropertyFromFMODParameter(editorParamRef);
                audioProperties.Add(property);
            }
        }
        
        return audioProperties;
    }

    private List<SoundInstanceEditorAudioProperty> RetrieveAudioPropertiesFromTemplates()
    {
        SoundInstanceEditorAudioPropertyTemplate[] templates = this.PropertyTemplates;
        return templates.Select(obj => obj.propertyData).Where(obj => obj.propertyType == SoundInstanceEditorAudioPropertyType.FmodAudioProperty).ToList();
    }

    private List<SoundInstanceEditorAudioProperty> RetrieveAudioPropertiesFromPropertiesList()
    {
        List<SoundInstanceEditorAudioProperty> currentAudioPropertiesList = this.AudioProperties;
        if(currentAudioPropertiesList == null) { return new List<SoundInstanceEditorAudioProperty>(); }
        return currentAudioPropertiesList.Where(obj => obj.propertyType == SoundInstanceEditorAudioPropertyType.FmodAudioProperty).ToList();   
    }
}
