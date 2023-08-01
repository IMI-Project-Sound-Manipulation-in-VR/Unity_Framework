using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FMODUnity;
using UnityEditor;
using UnityEngine;

public enum SoundInstanceEditorType
{
    Unity,
    Fmod
}

public class SoundInstanceEditor : MonoBehaviour
{
    // Editor Type
    [SerializeField]
    private SoundInstanceEditorType editorType;
    public SoundInstanceEditorType EditorType
    {
        get { return editorType; }
        set {
            if(value != editorType)
            {
                editorType = value;
                SetupEditorObject();
            }
            editorType = value;
        }
    }

    // Sound Instance Editor Object
    private SoundInstanceEditorObject soundInstanceEditorObject;
    public SoundInstanceEditorObject SoundInstanceEditorObject => soundInstanceEditorObject;

    // Unity
    // Audio Clip Reference
    [SerializeField]
    private AudioClip audioClipReference;
    public AudioClip AudioClipReference 
    {
        get { return audioClipReference; }
        set 
        {
            if(value != audioClipReference)
            {
                audioClipReference = value;
                if(editorType != SoundInstanceEditorType.Unity) { return; }
                if(audioClipReference != null)
                {
                    SetupEditorObject();
                } else {
                    ResetSoundInstanceEditor();
                }
            }
            audioClipReference = value;
        }
    }

    public AudioSource AudioSourceReference { get; set; }

    // FMOD Event reference
    [SerializeField]
    private EventReference fmodEventReference;
    private Guid previousGuid;
    public EventReference FmodEventReference
    {
        get { return fmodEventReference; }
        set 
        {
            if(value.Guid != previousGuid)
            {
                fmodEventReference = value;
                previousGuid = value.Guid;
                if(EditorType != SoundInstanceEditorType.Fmod) { return; }
                SetupEditorObject();
            }
            fmodEventReference = value;
            previousGuid = value.Guid;
        }
    }

    // Manager Level
    private float managerLevel;
    private bool managerLevelActive;
    public bool ShowInManager { get; set; }

    // Editor Level
    private float editorLevel;
    private bool editorLevelActive;
    private PropertyInfo editorLevelProperty;

    // Reflection external script
    [SerializeField] private MonoBehaviour reflectionScript;
    public Type reflectionScriptType;
    public PropertyInfo[] ReflectionScriptProperties { get; set; }
    private bool reflectionScriptActive;

    // Properties
    private bool addProperty;
    private bool addPreset;
    private string presetName;

    // Start is called before the first frame update
    void Start()
    {
        LoadReflectionScript();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateInspectorAndOnRunning();
    }

    // Public
    public void SetManagerLevel(bool active, float value)
    {
        this.managerLevelActive = active;
        this.managerLevel = value;
    }

    public void SetEditorLevel(bool active, float value)
    {
        this.editorLevelActive = active;
        this.managerLevel = value;
    }

    // Inspector GUI Methods
    public void DrawInspectorGUIDefaultInfo()
    {
        EditorType = (SoundInstanceEditorType) EditorGUILayout.EnumPopup("Editor Type", editorType);
    }

    public void DrawInspectorGUI()
    {
        if(soundInstanceEditorObject != null)
        {
            GUILayout.BeginVertical(GUI.skin.window);

            GUILayout.Label("Sound Instance Editor: " + soundInstanceEditorObject.InstanceName);
            
            DrawInspectorGUIEditorLevel();

            DrawInspectorGUIPresets();

            EditorGUI.indentLevel++;

            DrawInspectorGUISoundProperties();
            
            EditorGUI.indentLevel--;

            DrawInspectorGUIAddingAudioProperties();

            DrawInspectorGUISaveAudioPropertiesAsExistingPreset();

            DrawInspectorGUISaveAudioPropertiesAsNewPreset();
            
            GUILayout.EndVertical();
        }
    }

    private void DrawInspectorGUISaveAudioPropertiesAsNewPreset()
    {
        if(editorType == SoundInstanceEditorType.Unity)
        {
            if(addPreset){
                    EditorGUILayout.BeginHorizontal();

                    // if this button is pressed, addproperty will be set to false
                    // which will hide the current display
                    addPreset = !GUILayout.Button("Go back", GUILayout.Width(100));

                    Rect curveRect = EditorGUILayout.GetControlRect();
                    presetName = EditorGUI.TextField(curveRect, presetName);

                    if (GUILayout.Button("Add new preset", GUILayout.Width(100)))
                    {
                        string directoryPath = "Assets" + "/" + "Scenes" + "/" + "Resources" + "/" + "Audio Property Presets";

                        // Check if the directory exists
                        if (!Directory.Exists(directoryPath))
                        {
                            // If the directory doesn't exist, create it
                            Directory.CreateDirectory(directoryPath);
                        }

                        if(!string.IsNullOrEmpty(presetName))
                        {
                            string assetPath = directoryPath + "/" + presetName + ".asset";

                            SoundInstanceEditorAudioPropertyPreset newPreset = ScriptableObject.CreateInstance<SoundInstanceEditorAudioPropertyPreset>();
                            newPreset.propertiesArray = soundInstanceEditorObject.AudioProperties.ToArray();
                        
                            AssetDatabase.CreateAsset(newPreset, assetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();


                            soundInstanceEditorObject.LoadPropertyPresets();
                            soundInstanceEditorObject.SelectedPropertyPresetIndex = System.Array.FindIndex(soundInstanceEditorObject.PropertyPresets, p => p.name == presetName);
                            soundInstanceEditorObject.SetAudioPropertiesFromPreset();

                            addPreset = false;
                        }
                        
                    }

                    EditorGUILayout.EndHorizontal();
                } else {
                    addPreset = GUILayout.Button("Save configuration as new preset");
                }
        }
    }

    private void DrawInspectorGUISaveAudioPropertiesAsExistingPreset()
    {
        if(editorType == SoundInstanceEditorType.Unity)
        {
            if(!soundInstanceEditorObject.ComparePresetWithAudioProperties())
            {
                if (GUILayout.Button("Save changes to preset"))
                {
                    SoundInstanceEditorAudioPropertyPreset currentPreset = soundInstanceEditorObject.PropertyPresets[soundInstanceEditorObject.SelectedPropertyPresetIndex];
                    SoundInstanceEditorAudioProperty[] audioPropertiesArray = soundInstanceEditorObject.AudioProperties.ToArray();
                    currentPreset.UpdatePropertiesArray(audioPropertiesArray);
                    soundInstanceEditorObject.SetAudioPropertiesFromPreset();
                }
            }
    }
    }

    private void DrawInspectorGUIAddingAudioProperties()
    {
        if(editorType == SoundInstanceEditorType.Unity)
        {
            if(addProperty){
                EditorGUILayout.BeginHorizontal();

                // will display the names of all available property templates
                string[] propertyNames = System.Array.ConvertAll(soundInstanceEditorObject.PropertyTemplates , obj => obj.propertyData.propertyName);
                
                // if this button is pressed, addproperty will be set to false
                // which will hide the current display
                addProperty = !GUILayout.Button("Go back", GUILayout.Width(100));

                // a popup that will select the index of the selected template indexx
                soundInstanceEditorObject.SelectedPropertyTemplateIndex = EditorGUILayout.Popup(soundInstanceEditorObject.SelectedPropertyTemplateIndex, propertyNames);

                // if the add button is clicked
                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    //add the property template to the preset
                    soundInstanceEditorObject.AddNewAudioProperty();

                    // set "addProperty" to false, to hide current display after adding a new template
                    addProperty = false;
                }

                EditorGUILayout.EndHorizontal();
            }
            else 
            {
                // a button to display the menu to add a new template to the property preset
                addProperty = GUILayout.Button("Add new property");
            }
        }
    }

    private void DrawInspectorGUISoundProperties()
    {   
        // check if list of audio properties have been initialized
        if(soundInstanceEditorObject.AudioProperties != null)
        {
            // iterate over all audio properties
            for (int i = 0; i < soundInstanceEditorObject.AudioProperties.Count; i++)
            {
                // take current property from list
                SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[i];

                // get the name of the property
                string foldoutName =  char.ToUpper(property.propertyName[0]) + property.propertyName.Substring(1);
                if(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None) { foldoutName += "(" + property.propertyControlType.ToString() + ")"; }

                // display a foldout for current property
                // will control whether the property will be show in the inspector or not
                GUILayout.BeginVertical(GUI.skin.box);
                property.showProperty = EditorGUILayout.Foldout(property.showProperty, foldoutName);
                // if the property can be displayed
                if(property.showProperty){
                    // TODO: Make space between property type controlls and input/output
                    // display a enum popup for the property evaluation type
                    property.propertyEvaluationType = (SoundInstanceEditorAudioPropertyEvaluationType) EditorGUILayout.EnumPopup("Property Type", property.propertyEvaluationType);

                    // switch through the property evaluation types
                    // the property evaluation types, will display different controlls of how a input value should be evaluated
                    switch(property.propertyEvaluationType)
                    {
                        // the curve controll will evaluate a input value
                        // according to the animation curve field
                        case SoundInstanceEditorAudioPropertyEvaluationType.Curve:
                            // will display the animation curve in the inspector
                            Rect curveRect = EditorGUILayout.GetControlRect();
                            property.curve = EditorGUI.CurveField(curveRect, "Curve", property.curve);

                            // will display the input slider, that can be controlled by the user 
                            // will be disabled if a control type has been set
                            // NONE indicates no external control over the property
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // displays the output value on a slider, simply for visualization
                            GUI.enabled = false;
                            property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                            GUI.enabled = true;

                            break;
                        // the labeled evaluation type will evaluate a input value to a set of lables
                        // for example, given two labels "a" and "b", a input value between 0.0-0.5 will evaluate "a" and 0.5-1.0 will evaluate "b"
                        case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:

                            // will display the input slider, that can be controlled by the user 
                            // will be disabled if a control type has been set
                            // NONE indicates no external control over the property
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // displays the output value on a slider and as a separate label, simply for visualization
                            GUI.enabled = false;
                            property.outputValue = EditorGUILayout.IntSlider("Output Value", (int) property.outputValue, (int) property.minValue, (int) property.maxValue);
                            // Display the corresponding string label for the selected slider value
                            if(property.labels.Length == 0)
                            {
                                EditorGUILayout.LabelField("Property has no labels");
                            } else {
                                EditorGUILayout.LabelField("Selected Label: ", property.labels[(int) property.outputValue]);
                            }
                            GUI.enabled = true;

                            break;
                        // the evaluation type level, evaluates normalized value to a boolean value, or rather a float value of 0.0f or 1.0f
                        // if the input value is between the levels vector2 x and y values, the output will be 1.0f, else 0.0f
                        case SoundInstanceEditorAudioPropertyEvaluationType.Level:

                            // will display a min max slider which lets the user control the vectors2 x and y levels
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Level");
                            GUILayout.Label(property.level.x.ToString("F2"), GUILayout.Width(30));
                            EditorGUILayout.MinMaxSlider(ref property.level.x, ref property.level.y, 0.0f, 1.0f);
                            GUILayout.Label(property.level.y.ToString("F2"), GUILayout.Width(30));
                            GUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            
                            // will display the input slider, that can be controlled by the user 
                            // will be disabled if a control type has been set
                            // NONE indicates no external control over the property
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // displays the output value as a toggle, simply for visualization
                            GUI.enabled = false;
                            bool b = property.outputValue != 0.0f;
                            property.outputValue = EditorGUILayout.Toggle("Active: ", b) ? 1.0f : 0.0f;
                            GUI.enabled = true;

                            break;
                        // the evaluation type linear, will linearly evaluate a input value between the set
                        // min and max values. for example given a min and max value of -1f and 1f
                        // a input value of 0.5f would equal a output value of 0.0f
                        case SoundInstanceEditorAudioPropertyEvaluationType.Linear:
                            // displays a float field for user controlled min and max values
                            property.minValue = EditorGUILayout.FloatField("Min Value", property.minValue);
                            property.maxValue = EditorGUILayout.FloatField("Max Value", property.maxValue);

                            // will display the input slider, that can be controlled by the user 
                            // will be disabled if a control type has been set
                            // NONE indicates no external control over the property
                            EditorGUI.BeginDisabledGroup(property.propertyControlType != SoundInstanceEditorAudioPropertyControlType.None);
                            property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                            EditorGUI.EndDisabledGroup();

                            // displays the output value on a slider, simply for visualization
                            GUI.enabled = false;
                            property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                            GUI.enabled = true;

                            break;
                    }
                    
                    // field for removing current property
                    if(property.propertyType != SoundInstanceEditorAudioPropertyType.FmodParameter)
                    {
                        if (GUILayout.Button("Remove Property")) {
                            soundInstanceEditorObject.RemoveAudioProperty(i);
                        }
                    }
                }

                GUILayout.EndVertical();
            }
        }
    }

    private void DrawInspectorGUIPresets()
    {
        // presets are currently only present for unity sound
        if(editorType == SoundInstanceEditorType.Unity)
            {
                // popup or dropdown list for selecting presets
                string[] audioSourcePropertiesPresetsStrings = System.Array.ConvertAll(soundInstanceEditorObject.PropertyPresets, obj => obj.name);
                soundInstanceEditorObject.SelectedPropertyPresetIndex = EditorGUILayout.Popup("Presets", soundInstanceEditorObject.SelectedPropertyPresetIndex, audioSourcePropertiesPresetsStrings);
                if(soundInstanceEditorObject.SelectedPropertyPresetIndex != soundInstanceEditorObject.PreviousPropertyPresetIndex)
                {
                    soundInstanceEditorObject.PreviousPropertyPresetIndex = soundInstanceEditorObject.SelectedPropertyPresetIndex;
                    soundInstanceEditorObject.SetAudioPropertiesFromPreset();
                }
            }
    }

    private void DrawInspectorGUIEditorLevel()
    {
        GUILayout.BeginHorizontal();

        string editorLevelString = "Editor Level";
        // check if manager level is active or the editor level is controlled by a script property
        // will then add a postfix to the editor level string, just for information
        if (editorLevelProperty != null && !managerLevelActive)
        {
            editorLevelString += " (controlled by script)";
        }
        else if (managerLevelActive)
        {
            editorLevelString += " (controlled by manager level)";
        }

        // disable the slider if manager level is active or if the editor level is disabled
        EditorGUI.BeginDisabledGroup(editorLevelActive == false || editorLevelProperty != null || managerLevelActive == true);
        
        // if the manager level is active, the editor level value will be replaced by the manager level value
        if(managerLevelActive) { 
            editorLevel = EditorGUILayout.Slider(editorLevelString, managerLevel, 0, 1);
        } else { 
            editorLevel = EditorGUILayout.Slider(editorLevelString, editorLevel, 0, 1);
        }
        EditorGUI.EndDisabledGroup();

        // toggle to enable editor level
        editorLevelActive = EditorGUILayout.Toggle(editorLevelActive);

        GUILayout.EndHorizontal();
    }

    // Private

    private void ResetSoundInstanceEditor()
    {
        if(soundInstanceEditorObject != null)
        {
            soundInstanceEditorObject.DisableAudioInstance();
            soundInstanceEditorObject = null;
        }
        addProperty = false;
    }

    private void SetAudioInstance()
    {
        if(soundInstanceEditorObject != null)
        {
            soundInstanceEditorObject.SetAudioInstance();
        }
    }

    private void LoadReflectionScript(){
        reflectionScriptType = reflectionScript ? reflectionScript.GetType() : null;
    }

    // Private

    private void LoadPropertyTemplates()
    {
        if(soundInstanceEditorObject == null) return;
        soundInstanceEditorObject.LoadPropertyTemplates();
    }

    private void LoadPropertyPresets()
    {
        if(soundInstanceEditorObject == null) return;
        soundInstanceEditorObject.LoadPropertyPresets();
    }

    public void UpdateInspectorAndOnRunning()
    {
        CheckReflectionScriptActive();
        UpdateAudioPropertyValues();
    }

    public void UpdateInspectorOnly()
    {
        LoadPropertyTemplates();
        LoadPropertyPresets();
    }

    private void CheckReflectionScriptActive()
    {
        if(reflectionScript != null)
        {
            if(reflectionScript.gameObject.activeInHierarchy != reflectionScriptActive)
            {
                reflectionScriptActive = reflectionScript.gameObject.activeInHierarchy;
                if(soundInstanceEditorObject != null) { 
                    soundInstanceEditorObject.SetAudioProperties();
                }
            }
        }
    }

    private void UpdateAudioPropertyValues()
    {
        if(soundInstanceEditorObject == null) return;
        if(soundInstanceEditorObject.AudioProperties == null) return;
        
        for (int index = 0; index < soundInstanceEditorObject.AudioProperties.Count; index++)
        {
            SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[index];
            PropertyInfo reflectionAudioProperty = ReflectionScriptProperties[index];
            // TODO: catch if property doesnt have matching audio property

            PropertyInfo reflectionScriptProperty = ReflectionScriptProperties != null ? ReflectionScriptProperties[index] : null;
            
            float inputValue = 0;

            inputValue = property.inputValue;
            property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.None;

            if(editorLevelActive) {
                inputValue = editorLevel; property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.Editor;
            }
            if(managerLevelActive) { 
                inputValue = managerLevel; property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.Manager; 
            }
            if(reflectionScriptProperty != null) { 
                inputValue = Convert.ToSingle(reflectionScriptProperty.GetValue(reflectionScript));
                property.propertyControlType = SoundInstanceEditorAudioPropertyControlType.Script;
            }

            switch (property.propertyEvaluationType)
            {
                case SoundInstanceEditorAudioPropertyEvaluationType.Curve:
                    property.inputValue = inputValue;
                    property.outputValue = property.curve.Evaluate(inputValue);
                    // get the min and max value of the curve
                    // it evaluates the value, not the time, as the time should always be 0-1
                    if(property.curve.length != 0)
                    {
                        property.minValue = property.curve.keys.Min(key => key.value);
                        property.maxValue = property.curve.keys.Max(key => key.value);
                    } else {
                        property.minValue = 0;
                        property.maxValue = 0;
                    }
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Linear:
                    property.inputValue = inputValue;
                    property.outputValue = property.minValue + (property.maxValue - property.minValue) * inputValue;
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Level:
                    property.inputValue = inputValue;
                    property.outputValue = inputValue >= property.level.x && inputValue <= property.level.y ? 1 : 0;
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:
                    property.inputValue = inputValue;
                    property.outputValue = Mathf.RoundToInt(Mathf.Lerp(property.minValue, property.maxValue, inputValue));
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
            }

            soundInstanceEditorObject.SetAudioPropertyValue(property, index, property.outputValue);
        }
    }

    private void SetupEditorObject()
    {
        ResetSoundInstanceEditor();
        switch (editorType)
        {
            case SoundInstanceEditorType.Unity:
                if(audioClipReference != null)
                {
                    soundInstanceEditorObject = new SoundInstanceEditorObjectUnity(this);
                }
                break;
            case SoundInstanceEditorType.Fmod:
                if(!fmodEventReference.Guid.IsNull){
                    soundInstanceEditorObject = new SoundInstanceEditorObjectFmod(this);
                }
                break;
        }
    }

    public void InitializeEditorObject()
    {
        ResetSoundInstanceEditor();
        switch (editorType)
        {
            case SoundInstanceEditorType.Unity:
                if(audioClipReference != null && soundInstanceEditorObject == null)
                {
                    soundInstanceEditorObject = new SoundInstanceEditorObjectUnity(this);
                }
                break;
            case SoundInstanceEditorType.Fmod:
                if(!fmodEventReference.Guid.IsNull && soundInstanceEditorObject == null){
                    soundInstanceEditorObject = new SoundInstanceEditorObjectFmod(this);
                }
                break;
        }
    }

    [CustomEditor(typeof(SoundInstanceEditor))]
    public class SoundInstanceEditorInspector : UnityEditor.Editor
    {
        SoundInstanceEditor SoundInstanceEditor;
        private SerializedProperty eventReferenceProperty;
        private SerializedProperty audioClipProperty;
        private SerializedProperty reflectionScriptProperty;
        
        private void OnEnable()
        {
            SoundInstanceEditor = (SoundInstanceEditor)target;
            eventReferenceProperty = serializedObject.FindProperty("fmodEventReference");
            audioClipProperty = serializedObject.FindProperty("audioClipReference");
            reflectionScriptProperty = serializedObject.FindProperty("reflectionScript");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!Application.isPlaying)
            {
                SoundInstanceEditor.UpdateInspectorAndOnRunning();
                SoundInstanceEditor.UpdateInspectorOnly();
            }

            SoundInstanceEditor.DrawInspectorGUIDefaultInfo();

            DrawSerializedProperties();

            SoundInstanceEditor.DrawInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

        
        private void DrawSerializedProperties()
        {   
            // Displays, depending on whether FMOD or Unity has been selected as editor type,
            // either a property field for a FMOD event or Unity Audio Clip
            // it will set the reference to the appropriate property, which in turn will trigger the property setter
            EditorGUILayout.Space();
            if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Fmod)
            { 
                EditorGUILayout.PropertyField(eventReferenceProperty);
                EventReference eventReference = (EventReference) eventReferenceProperty.GetEventReference();
                SoundInstanceEditor.FmodEventReference = eventReference;
            }
            else if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Unity)
            {
                EditorGUILayout.PropertyField(audioClipProperty);
                SoundInstanceEditor.AudioClipReference = (AudioClip) audioClipProperty.objectReferenceValue;
            }

            if(!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(reflectionScriptProperty);
            }
        }
    }
}
