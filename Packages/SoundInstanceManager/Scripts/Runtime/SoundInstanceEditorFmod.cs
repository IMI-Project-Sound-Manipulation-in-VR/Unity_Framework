using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class SoundInstanceEditorFmod : MonoBehaviour
{
    [SerializeField] private MonoBehaviour script;
    private Type scriptType; 
    private EventReference previousEventReference;
    [SerializeField] private EventReference eventReference;
    private EventInstance eventInstance;
    private string EventName { get; set; }
    public Vector2 Level { get; set; }
    // private float Tempo { get; set; }
    private float Pitch { get; set; }
    private float Volume { get; set; }
    private SoundInstanceEditorFmodPresetData[] presets;
    private int selectedPresetIndex;
    private int previouslySelectedPresetIndex = -1;
    private List<FMODParameterDescription> parameterDescriptions;
    private PropertyInfo[] propertyInfos;
    private bool showGUI;
    private bool showAdditionalParameters = true;
    private string playbackState;

    public void LoadPresets()
    {
        presets = Resources.LoadAll<SoundInstanceEditorFmodPresetData>("InstanceEditorPresets");
    }

    void Start()
    {
        // saves script type of monobehaviour
        scriptType = script != null ? script.GetType() : null;
        // creates and set eventinstance of fmod event reference
        eventInstance = FMODSoundManager.instance.CreateEventInstance(eventReference);
        // RetrieveEventInstanceInformation();
        // sets 3d attributes of sound (needed for spatialize)
        FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(gameObject.transform);
        eventInstance.set3DAttributes(attributes);
        // Loads presets of basic audio attributes
        LoadPresets();
        // plays event instance on start up
        Play();
    }

    void Update()
    {
        // sets volume and pitch
        SetVolume();
        SetPitch();
        // updates parameters from reflection properties, if they exist
        UpdateReflectionParameters();
        // updates additional parameters aka fmod parameters
        UpdateAdditionalParameters();
        // checks if event reference has changed
        UpdateEventReference();
    }

    public void UpdateEventReference(){
        // checks if event reference has changed
        if(!eventReference.Equals(previousEventReference)){
            // if so retrieve event information from fmod event reference
            previousEventReference = eventReference;
            RetrieveEventInformation();
        }
    }

    public void RetrieveEventInformation()
    {
        EditorEventRef eventAsset = EventManager.EventFromPath(eventReference.Path);

        // retrieves infos, such as event name and all fmod parameters
        RetrieveEventName(eventAsset);
        RetrieveEventParameters(eventAsset);
        // sets up list of property infos through reflection
        SetupReflectionProperties();
    }

    private void RetrieveEventName(EditorEventRef eventAsset)
    {
        // retrieves path name, without the "event://" prefix
        EventName = eventAsset != null ? eventAsset.Path.Substring("event:/".Length) : "";
    }

    private void RetrieveEventParameters(EditorEventRef eventAsset)
    {
        // retrieves all fmod event parameters
        if(eventAsset != null)
        {
            // resets parameter descritpion list
            parameterDescriptions = new List<FMODParameterDescription>();
            // retrieves local parameters of fmod event
            List<EditorParamRef> localParameters = eventAsset.LocalParameters;
            // iterates through each parameter and copies data from fmod parameter
            foreach(EditorParamRef editorParamRef in localParameters){
                FMODParameterDescription paramDescription = ScriptableObject.CreateInstance<FMODParameterDescription>();
                paramDescription.SetParameterDescriptionFromStruct(editorParamRef);
                parameterDescriptions.Add(paramDescription);
            }
        }
    }

    private void SetupReflectionProperties(){
        // checks if they fmod parameter have a matching property in the script
        propertyInfos = new PropertyInfo[parameterDescriptions == null ? 0 : parameterDescriptions.Count];
        if(scriptType != null)
        {   
            // iterates over the parameters
            for(int i = 0; i < parameterDescriptions.Count; i++){
                // checks for property inside script with the same name
                PropertyInfo propertyInfo = scriptType.GetProperty(parameterDescriptions[i].ParameterName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                // adds it to the list of property infos
                if(propertyInfo != null) propertyInfos[i] = propertyInfo;;
            }
        }
    }

    private void UpdateReflectionParameters(){
        if(scriptType != null){
            // iterates of the property infos and updates the parameter descriptions
            for(int i = 0; i < propertyInfos.Length; i++)
            {
                // checks if reflection property exists, not every parameter may have one
                PropertyInfo property = propertyInfos[i];
                if(property == null) continue;
                // check if property is of type enum or not
                if(!property.PropertyType.IsEnum) {
                    // type single / float, get appropriate value from property
                    parameterDescriptions[i].CurrentValue = (float) property.GetValue(script);
                    parameterDescriptions[i].Locked = true;
                }
                else
                {
                    // type enum, get appropriate value from property
                    object value = property.GetValue(script);
                    MyEnum enumValue = (MyEnum)value;

                    parameterDescriptions[i].CurrentValue = ((int)enumValue);
                    parameterDescriptions[i].Locked = true;
                }
            }
        }
    }

    // updates volume of event instance
    public void SetVolume()
    {
        eventInstance.setVolume(Volume);
    }

    // updates pitch of event instance
    public void SetPitch()
    {
        eventInstance.setPitch(Pitch);
    }

    // starts / plays event instance
    public void Play() 
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

    // pauses event instance
    public void Pause()
    {
        PLAYBACK_STATE currentPlaybackState;
        eventInstance.getPlaybackState(out currentPlaybackState);

        if (currentPlaybackState == PLAYBACK_STATE.PLAYING)
        {
            eventInstance.setPaused(true);
            
            PLAYBACK_STATE pbState;
            eventInstance.getPlaybackState(out pbState);
            playbackState = pbState.ToString();
        }
    }

    // stops event instance
    public void Stop()
    {
        PLAYBACK_STATE currentPlaybackState;
        eventInstance.getPlaybackState(out currentPlaybackState);

        if (currentPlaybackState != PLAYBACK_STATE.STOPPED && currentPlaybackState != PLAYBACK_STATE.STOPPING)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            PLAYBACK_STATE pbState;
            eventInstance.getPlaybackState(out pbState);
            playbackState = pbState.ToString();
        }
    }

    // updates the fmod parameters
    public void UpdateAdditionalParameters(){

        if(parameterDescriptions != null){
            // iterates over each instance of parameter descriptions
            foreach(FMODParameterDescription parameterDescription in parameterDescriptions)
            {
                // sets the fmod parameter by name and retrieves current value from parameter description
                eventInstance.setParameterByName(parameterDescription.ParameterName, parameterDescription.CurrentValue);
            }
        }
    }

    public void DrawInspectorGUI()
    {
        if (!eventReference.IsNull)
        {
            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            showGUI = EditorGUILayout.Foldout(showGUI, eventReference.Path.Length > 0 ? eventReference.Path.Substring("event:/".Length) : "");
            
            if(showGUI)
            {
                // BEGIN DEFAULT SLIDERS
                GUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.indentLevel++;
                
                // displays playback state
                PLAYBACK_STATE playbackState;
                eventInstance.getPlaybackState(out playbackState);
                EditorGUILayout.LabelField("Playback State", playbackState.ToString());

                // creates popup for presets
                selectedPresetIndex = EditorGUILayout.Popup("Presets", selectedPresetIndex, GetPresetNames());

                // EditorGUILayout.ObjectField(eventReference, typeof(EventReference));

                // sets up sliders for default audio properties
                // Tempo = EditorGUILayout.Slider("Tempo", Tempo, 0.1f, 2f);
                Pitch = EditorGUILayout.Slider("Pitch", Pitch, 0.5f, 2f);
                Volume = EditorGUILayout.Slider("Volume", Volume, 0f, 1f);

                Vector2 lvl = Level;
                float labelWidth = 40;
                // float sliderWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 120f;

                // min max slider is f'ed, so have to adds labels to each end manually
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Level");
                GUILayout.Label(lvl.x.ToString("F2"), GUILayout.Width(labelWidth));
                EditorGUILayout.MinMaxSlider(ref lvl.x, ref lvl.y, 0.0f, 1.0f);
                Level = new Vector2(lvl.x, lvl.y);
                GUILayout.Label(lvl.y.ToString("F2"), GUILayout.Width(labelWidth));
                GUILayout.EndHorizontal();
                EditorGUILayout.Space();

                EditorGUI.indentLevel--;

                GUILayout.EndVertical();

                // ADDITIONAL PARAMETER SLIDERS (CURRENTLY FOR FMOD)
                GUILayout.BeginVertical(GUI.skin.box);
                showAdditionalParameters = EditorGUILayout.Foldout(showAdditionalParameters, "Show Additional Parameters");

                // shows additional parameters 
                if(parameterDescriptions != null && showAdditionalParameters) 
                {   
                    EditorGUI.indentLevel++;

                    // Display new attribute sliders
                    foreach(FMODParameterDescription paramDescription in parameterDescriptions)
                    {
                        EditorGUI.BeginDisabledGroup(paramDescription.Locked);
                        string parameterName = paramDescription.ParameterName;
                        if(paramDescription.Locked) parameterName += " (controlled by script property)";
                        if( paramDescription.ParameterType == FMODUnity.ParameterType.Labeled ){
                            paramDescription.CurrentValue = EditorGUILayout.Popup(parameterName, (int) paramDescription.CurrentValue, paramDescription.Labels);
                        } else {
                            // Create the slider
                            paramDescription.CurrentValue = EditorGUILayout.Slider(parameterName, paramDescription.CurrentValue, paramDescription.Minimum, paramDescription.Maximum);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel--;
                }

                GUILayout.EndVertical();
            }
        }

        // load list of presets
        LoadPresets();
        // apply presets (if it has changed)
        ApplyPreset();
    }

    private void ApplyPreset()
    {
        // sets default values of there are no presets available
        if (presets == null || presets.Length == 0){
            Pitch = 1;
            Volume = 1;
            Level = new Vector2(0, 1);
        }
        
        // change preset values, if the presets change
        if(previouslySelectedPresetIndex != selectedPresetIndex){
            previouslySelectedPresetIndex = selectedPresetIndex;

            if(selectedPresetIndex >= 0 && selectedPresetIndex < presets.Length)
            {
                SoundInstanceEditorFmodPresetData presetData = presets[selectedPresetIndex];

                // Tempo = presetData.tempo;
                Pitch = presetData.pitch;
                Volume = presetData.volume;
                Level = presetData.level;
            }
        }
    }

    // just a method to get the preset names as a string array, needed for the dropdown
    private string[] GetPresetNames()
    {
        if(presets == null || presets.Length == 0)
        {
            return new string[] { "No presets available "};
        }

        string[] presetNames = new string[presets.Length];
        for (int i = 0; i < presets.Length; i++)
        {
            presetNames[i] = presets[i].presetName;
        }
        return presetNames;
    }

}

[CustomEditor(typeof(SoundInstanceEditorFmod))]
public class SoundManagerEditor : UnityEditor.Editor
{
    SoundInstanceEditorFmod soundManager;

    private void OnEnable()
    {
        soundManager = (SoundInstanceEditorFmod)target;
        soundManager.LoadPresets();
        soundManager.RetrieveEventInformation();
        soundManager.UpdateEventReference();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        soundManager.UpdateEventReference();

        soundManager.DrawInspectorGUI();
    }
}
