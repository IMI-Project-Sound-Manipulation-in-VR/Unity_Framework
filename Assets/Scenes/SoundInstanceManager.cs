using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEditor;

public class SoundInstanceManager : MonoBehaviour
{
    [SerializeField] private EventReference eventReference;
    public EventInstance EventInstance { get; set; }
    public string EventName { get; set; }
    public Vector2 Level { get; set; }
    public float Tempo { get; set; }
    public float Pitch { get; set; }
    public float Volume { get; set; }
    public SoundManagerPresetData[] Presets { get; set; }
    public int SelectedPresetIndex { get; set; }
    public int PreviouslySelectedPresetIndex { get; set; }

    public void LoadPresets()
    {
        Presets = Resources.LoadAll<SoundManagerPresetData>("SM_Presets");
    }

    void Start()
    {
        EventInstance = FMODSoundManager.instance.CreateEventInstance(eventReference);
        // RetrieveEventParameters();
        FMOD.ATTRIBUTES_3D attributes = RuntimeUtils.To3DAttributes(gameObject.transform);
        EventInstance.set3DAttributes(attributes);
        Play();
    }

    void Update()
    {
        SetVolume();
        SetPitch();
    }

    private void RetrieveEventParameters()
    {
        // Get the event description
        EventDescription eventDescription;
        EventInstance.getDescription(out eventDescription);

        int parameterCount;
        eventDescription.getParameterDescriptionCount(out parameterCount);

        for (int i = 0; i < parameterCount; i++)
        {
            // Get the parameter description
            PARAMETER_DESCRIPTION parameterDescription;
            eventDescription.getParameterDescriptionByIndex(i, out parameterDescription);

            // Access the parameter properties
            string parameterName = parameterDescription.name;
        }

        Debug.Log(parameterCount);
    }

    void SetVolume()
    {
        EventInstance.setVolume(Volume);
    }

    void SetPitch()
    {
        EventInstance.setPitch(Pitch);
    }

    void Play() 
    {
        EventInstance.start();
    }

    void Pause()
    {
        EventInstance.setPaused(true);
    }

    void Stop()
    {
        EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

}

[CustomEditor(typeof(SoundInstanceManager))]
public class SoundManagerEditor : Editor
{
    bool showGUI = true;

    private void OnEnable()
    {
        SoundInstanceManager soundManager = (SoundInstanceManager)target;
        soundManager.LoadPresets();
    }

    public override void OnInspectorGUI()
    {

        SoundInstanceManager soundManager = (SoundInstanceManager)target;

        GUILayout.BeginVertical(GUI.skin.box);
        showGUI = EditorGUILayout.Foldout(showGUI, "Show GUI");

        if (showGUI)
        {
            // Begin GUI Layout
            EditorGUI.indentLevel++;
            
            // !!!!!!!!!TODO: replace guilayout sliders with serializedObjects 

            soundManager.SelectedPresetIndex = EditorGUILayout.Popup("Presets", soundManager.SelectedPresetIndex, GetPresetNames(soundManager.Presets));
            soundManager.Tempo = EditorGUILayout.Slider("Tempo", soundManager.Tempo, 0.1f, 2f);
            soundManager.Pitch = EditorGUILayout.Slider("Pitch", soundManager.Pitch, 0.5f, 2f);
            soundManager.Volume = EditorGUILayout.Slider("Volume", soundManager.Volume, 0f, 1f);

            Vector2 level = soundManager.Level;
            float labelWidth = 40;
            // float sliderWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 120f;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level");
            GUILayout.Label(level.x.ToString("F2"), GUILayout.Width(labelWidth));
            EditorGUILayout.MinMaxSlider(ref level.x, ref level.y, -1.0f, 1.0f);
            soundManager.Level = new Vector2(level.x, level.y);
            GUILayout.Label(level.y.ToString("F2"), GUILayout.Width(labelWidth));
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUI.indentLevel--;
        }

        GUILayout.EndVertical();
        
        ApplyPreset(soundManager);

        DrawDefaultInspector();
    }

    private string[] GetPresetNames(SoundManagerPresetData[] presets)
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

    private void ApplyPreset(SoundInstanceManager soundManager)
    {
        if(soundManager.PreviouslySelectedPresetIndex != soundManager.SelectedPresetIndex){
            soundManager.PreviouslySelectedPresetIndex = soundManager.SelectedPresetIndex;

            if(soundManager.SelectedPresetIndex >= 0 && soundManager.SelectedPresetIndex < soundManager.Presets.Length)
            {
                SoundManagerPresetData presetData = soundManager.Presets[soundManager.SelectedPresetIndex];

                soundManager.Tempo = presetData.tempo;
                soundManager.Pitch = presetData.pitch;
                soundManager.Volume = presetData.volume;
                soundManager.Level = presetData.level;
            }
        }
        
    }
}
