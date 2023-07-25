using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundInstanceEditorObjectFmod : SoundInstanceEditorObject
{
    private EventReference eventReference;
    public EventReference EventReference { get {return eventReference; } set { eventReference = value; } }
    private EventReference previousEventReference;
    public EventReference PreviousEventReference { get {return previousEventReference; } set { previousEventReference = value; } }
    private EventInstance eventInstance;
    private EditorEventRef eventAsset;
    private string playbackState;
    private GameObject gameObject;
    public SoundInstanceEditorObjectFmod(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }

    // Overrides
    public override void UpdateInstanceReference(){
        // checks if event reference has changed
        if(!eventReference.Equals(previousEventReference)){
            // if so retrieve event information from fmod event reference
            previousEventReference = eventReference;
            RetrieveEventInformation();

            SetInstanceName(RetrieveEventName());
            SetAudioProperties();

            SetupAudioInstance();
        }
    }

    public override void AddNewAudioProperty()
    {
        base.AddNewAudioProperty();
        // TODO:

    }

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


        // loads property presets. presets are simply a handy collection of templates
        // for example: a stress "preset" that includes two "templates" pitch and volume
        // audioSourcePropertiesPresets = Resources.LoadAll<SoundInstanceEditorUnityPropertyPreset>("SoundInstanceEditorUnityParameterPresets");
        
        // // generate a string array, which includes the names of each template included in the preset
        // // this is primarily used in the presets dropdown in the inspector, in which you can select different presets
        // List<string> options = new List<string>();
        // options.Add("No preset");
        // options.AddRange(System.Array.ConvertAll(audioSourcePropertiesPresets, obj => obj.name));
        // audioSourcePropertiesPresetsStrings = options.ToArray();
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
                    // TODO:
                    break;
                }
            }
        }
    }

    public override void SetupAudioInstance()
    {
        // TODO: when event instance already exist, destroy it first
        // or else there will be multiple sounds playing
        if(FMODSoundManager.instance != null)
        {
            eventInstance = FMODSoundManager.instance.CreateEventInstance(eventReference);
            // RetrieveEventInstanceInformation();
            // sets 3d attributes of sound (needed for spatialize)
            FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(gameObject.transform);
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

    private void RetrieveEventInformation()
    {
        eventAsset = EventManager.EventFromPath(eventReference.Path);
    }

    private void SetAudioProperties() {
        List<SoundInstanceEditorAudioProperty> propertiesFromParameters = RetrieveAudioPropertiesFromFMODParameters();
        // TODO: get properties from selections
        this.AudioProperties = propertiesFromParameters;
        this.AudioPropertyFoldouts = new bool[this.AudioProperties.Count];
    }

    private void SetInstanceName(string instanceName)
    {
        this.InstanceName = eventAsset.Path.Substring("event:/".Length);
    }

    private string RetrieveEventName()
    {
        return eventAsset != null ? eventAsset.Path.Substring("event:/".Length) : "";
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
}
