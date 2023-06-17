using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SoundInstanceEditor : MonoBehaviour
{
    [SerializeField] private EventReference eventReference;
    private EventInstance eventInstance;
    private string EventName { get; set; }
    public Vector2 Level { get; set; }
    private float Tempo { get; set; }
    private float Pitch { get; set; }
    private float Volume { get; set; }
    private SoundManagerPresetData[] presets;
    private int selectedPresetIndex;
    private int previouslySelectedPresetIndex = -1;
    private FMODParameterDescription[] parameterDescriptions;
    private bool showGUI;
    private bool showAdditionalParameters = true;
    private string playbackState;

    public void LoadPresets()
    {
        presets = Resources.LoadAll<SoundManagerPresetData>("SM_Presets");
    }

    void Start()
    {
        eventInstance = FMODSoundManager.instance.CreateEventInstance(eventReference);
        RetrieveEventInstanceInformation();
        FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(gameObject.transform);
        eventInstance.set3DAttributes(attributes);
        LoadPresets();
        Play();
    }

    void Update()
    {
        SetVolume();
        SetPitch();
        UpdateAdditionalParameters();
    }

    private void RetrieveEventInstanceInformation()
    {
        // Get the event description
        EventDescription eventDescription;
        eventInstance.getDescription(out eventDescription);

        RetrieveEventInstanceName();
        RetrieveEventInstanceParameters(eventDescription);
    }

    private void RetrieveEventInstanceName()
    {
        EventName = eventReference.Path.Substring("event:/".Length);
    }

    private void RetrieveEventInstanceParameters(EventDescription eventDescription)
    {
        int parameterCountAll;
        eventDescription.getParameterDescriptionCount(out parameterCountAll);

        List<FMODParameterDescription> descriptions = new List<FMODParameterDescription>();

        for (int i = 0; i < parameterCountAll; i++)
        {
            // Get the parameter description
            PARAMETER_DESCRIPTION parameterDescription;
            eventDescription.getParameterDescriptionByIndex(i, out parameterDescription);

            if((parameterDescription.flags & PARAMETER_FLAGS.READONLY) == 0) {
                FMODParameterDescription description = ScriptableObject.CreateInstance<FMODParameterDescription>();;
                description.SetParameterDescriptionFromStruct(eventDescription, parameterDescription);

                descriptions.Add(description);
            }
        }

        parameterDescriptions = descriptions.ToArray();
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

        foreach(FMODParameterDescription parameterDescription in parameterDescriptions)
        {
            eventInstance.setParameterByName(parameterDescription.ParameterName, parameterDescription.CurrentValue);
        }

    }

    public void DrawInspectorGUI()
    {
        
        showGUI = EditorGUILayout.Foldout(showGUI, eventReference.Path.Substring("event:/".Length));

        if (showGUI)
        {
            // BEGIN DEFAULT SLIDERS
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            
            PLAYBACK_STATE playbackState;
            eventInstance.getPlaybackState(out playbackState);
            EditorGUILayout.LabelField("Playback State", playbackState.ToString());

            selectedPresetIndex = EditorGUILayout.Popup("Presets", selectedPresetIndex, GetPresetNames());

            // EditorGUILayout.ObjectField(eventReference, typeof(EventReference));

            Tempo = EditorGUILayout.Slider("Tempo", Tempo, 0.1f, 2f);
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
                for (int i = 0; i < parameterDescriptions.Length; i++)
                {
                    FMODParameterDescription attributeValue = parameterDescriptions[i];
                    if((attributeValue.ParameterType & PARAMETER_FLAGS.LABELED) != 0){
                        attributeValue.CurrentValue = EditorGUILayout.Popup(attributeValue.ParameterName, (int) attributeValue.CurrentValue, attributeValue.Labels);
                    } else {
                        attributeValue.CurrentValue = EditorGUILayout.Slider(attributeValue.ParameterName, attributeValue.CurrentValue, attributeValue.Minimum, attributeValue.Maximum);
                    }
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
        }

        LoadPresets();
        ApplyPreset();
    }

    private void ApplyPreset()
    {
        if(previouslySelectedPresetIndex != selectedPresetIndex){
            previouslySelectedPresetIndex = selectedPresetIndex;

            if(selectedPresetIndex >= 0 && selectedPresetIndex < presets.Length)
            {
                SoundManagerPresetData presetData = presets[selectedPresetIndex];

                Tempo = presetData.tempo;
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
public class SoundManagerEditor : Editor
{
    SoundInstanceEditor soundManager;

    private void OnEnable()
    {
        soundManager = (SoundInstanceEditor)target;
        soundManager.LoadPresets();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        soundManager.DrawInspectorGUI();
    }
}
