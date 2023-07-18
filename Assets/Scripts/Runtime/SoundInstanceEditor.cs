using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

public class SoundInstanceEditor : MonoBehaviour
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
    private SoundManagerPresetData[] presets;
    private int selectedPresetIndex;
    private int previouslySelectedPresetIndex = -1;
    private List<FMODParameterDescription> parameterDescriptions;
    private bool showGUI;
    private bool showAdditionalParameters = true;
    private string playbackState;

    public void LoadPresets()
    {
        presets = Resources.LoadAll<SoundManagerPresetData>("InstanceEditorPresets");
    }

    void Start()
    {
        scriptType = script != null ? script.GetType() : null;
        eventInstance = FMODSoundManager.instance.CreateEventInstance(eventReference);
        // RetrieveEventInstanceInformation();
        FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(gameObject.transform);
        eventInstance.set3DAttributes(attributes);
        LoadPresets();
        Play();
    }

    void Update()
    {
        SetVolume();
        SetPitch();
        UpdateReflectionParameters();
        UpdateAdditionalParameters();
    }

    public void UpdateEventReference(){
        if(!eventReference.Equals(previousEventReference)){
            previousEventReference = eventReference;
            RetrieveEventInformation();
        }
    }

    public void RetrieveEventInformation()
    {
        EditorEventRef eventAsset = EventManager.EventFromPath(eventReference.Path);

        RetrieveEventName(eventAsset);
        RetrieveEventParameters(eventAsset);
    }

    private void RetrieveEventName(EditorEventRef eventAsset)
    {
        EventName = eventAsset != null ? eventAsset.Path.Substring("event:/".Length) : "";
    }

    private void RetrieveEventParameters(EditorEventRef eventAsset)
    {
        if(eventAsset != null)
        {
            List<FMODParameterDescription> temp = new List<FMODParameterDescription>();
            List<EditorParamRef> localParameters = eventAsset.LocalParameters;
            foreach(EditorParamRef editorParamRef in localParameters){
                FMODParameterDescription paramDescription = ScriptableObject.CreateInstance<FMODParameterDescription>();
                paramDescription.SetParameterDescriptionFromStruct(editorParamRef);
                temp.Add(paramDescription);
            }
            parameterDescriptions = temp;
        }
    }

    private void UpdateReflectionParameters(){
        if(scriptType != null){
            foreach(FMODParameterDescription paramDescription in parameterDescriptions)
            {
                FieldInfo field = scriptType.GetField(paramDescription.ParameterName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(field == null) continue;
                if(!field.FieldType.IsEnum) {
                    paramDescription.CurrentValue = (float) field.GetValue(script);
                    paramDescription.Locked = true;
                }
                else
                {
                    object value = field.GetValue(script);
                    MyEnum enumValue = (MyEnum)value;

                    paramDescription.CurrentValue = ((int)enumValue);
                    paramDescription.Locked = true;
                }
            }
        }
    }

    private void RetrieveEventInstanceName()
    {
        EventName = eventReference.Path.Substring("event:/".Length);
    }

    public void SetVolume()
    {
        eventInstance.setVolume(Volume);
    }

    public void SetPitch()
    {
        eventInstance.setPitch(Pitch);
    }

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

    public void UpdateAdditionalParameters(){

        if(parameterDescriptions != null){
            foreach(FMODParameterDescription parameterDescription in parameterDescriptions)
            {
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
                
                PLAYBACK_STATE playbackState;
                eventInstance.getPlaybackState(out playbackState);
                EditorGUILayout.LabelField("Playback State", playbackState.ToString());

                selectedPresetIndex = EditorGUILayout.Popup("Presets", selectedPresetIndex, GetPresetNames());

                // EditorGUILayout.ObjectField(eventReference, typeof(EventReference));

                // Tempo = EditorGUILayout.Slider("Tempo", Tempo, 0.1f, 2f);
                Pitch = EditorGUILayout.Slider("Pitch", Pitch, 0.5f, 2f);
                Volume = EditorGUILayout.Slider("Volume", Volume, 0f, 1f);

                Vector2 lvl = Level;
                float labelWidth = 40;
                // float sliderWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 120f;

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

                if(parameterDescriptions != null && showAdditionalParameters) 
                {   
                    EditorGUI.indentLevel++;

                    // Display new attribute sliders
                    foreach(FMODParameterDescription paramDescription in parameterDescriptions)
                    {
                        EditorGUI.BeginDisabledGroup(paramDescription.Locked);
                        if( paramDescription.ParameterType == FMODUnity.ParameterType.Labeled ){
                            paramDescription.CurrentValue = EditorGUILayout.Popup(paramDescription.ParameterName, (int) paramDescription.CurrentValue, paramDescription.Labels);
                        } else {
                            // Create the slider
                            paramDescription.CurrentValue = EditorGUILayout.Slider(paramDescription.ParameterName, paramDescription.CurrentValue, paramDescription.Minimum, paramDescription.Maximum);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel--;
                }

                GUILayout.EndVertical();
            }
        }

        LoadPresets();
        ApplyPreset();
    }

    private void ApplyPreset()
    {
        if (presets == null || presets.Length == 0){
            Pitch = 1;
            Volume = 1;
            Level = new Vector2(0, 1);
        }
        
        if(previouslySelectedPresetIndex != selectedPresetIndex){
            previouslySelectedPresetIndex = selectedPresetIndex;

            if(selectedPresetIndex >= 0 && selectedPresetIndex < presets.Length)
            {
                SoundManagerPresetData presetData = presets[selectedPresetIndex];

                // Tempo = presetData.tempo;
                Pitch = presetData.pitch;
                Volume = presetData.volume;
                Level = presetData.level;
            }
        }
    }

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

[CustomEditor(typeof(SoundInstanceEditor))]
public class SoundManagerEditor : UnityEditor.Editor
{
    SoundInstanceEditor soundManager;

    private void OnEnable()
    {
        soundManager = (SoundInstanceEditor)target;
        soundManager.LoadPresets();
        soundManager.RetrieveEventInformation();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        soundManager.UpdateEventReference();

        soundManager.DrawInspectorGUI();
    }
}
