using System;
using System.Collections;
using System.Collections.Generic;
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
    public SoundInstanceEditorType editorType;
    private SoundInstanceEditorType previousEditorType;
    private SoundInstanceEditorObject soundInstanceEditorObject;
    public SoundInstanceEditorObject SoundInstanceEditorObject { get { return soundInstanceEditorObject; }}

    // Unity
    [SerializeField]
    private AudioClip audioClipReference;
    public AudioClip AudioClipReference { get { return audioClipReference; }}
    private AudioClip previousAudioClipReference;
    private AudioSource audioSourceReference;
    public AudioSource AudioSourceReference { get { return audioSourceReference; } set { audioSourceReference = value; }}

    // FMOD
    [SerializeField]
    private EventReference fmodEventReference;
    public EventReference FmodEventReference { get { return fmodEventReference; } }
    private EventReference previousFmodEventReference;

    // Manager Level
    private float managerLevel;
    private bool managerLevelActive;

    // Editor Level
    private float editorLevel;
    private bool editorLevelActive;
    private PropertyInfo editorLevelProperty;

    // Reflection external script
    [SerializeField] private MonoBehaviour reflectionScript;
    public Type reflectionScriptType;
    private PropertyInfo[] reflectionScriptProperties;

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
        UpdateMethods();
    }

    // Public
    public void UpdateManagerLevel(bool active, float value)
    {
        this.managerLevelActive = active;
        this.managerLevel = value;
    }

    public void UpdateEditorLevel(bool active, float value)
    {
        this.editorLevelActive = active;
        this.managerLevel = value;
    }

    // Inspector GUI Methods
    public void DrawInspectorGUIDefaultInfo()
    {
        editorType = (SoundInstanceEditorType) EditorGUILayout.EnumPopup("Editor Type", editorType);
    }

    public void DrawInspectorGUI()
    {
        if(soundInstanceEditorObject != null)
        {
            GUILayout.BeginVertical(GUI.skin.window);

            GUILayout.Label("Sound Instance Editor: " + soundInstanceEditorObject.InstanceName);
            
            //////////// EDITOR LEVEL
            GUILayout.BeginHorizontal();

            string editorLevelString = "Editor Level";
            if (editorLevelProperty != null && !managerLevelActive)
            {
                editorLevelString += " (controlled by script)";
            }
            else if (managerLevelActive)
            {
                editorLevelString += " (controlled by manager level)";
            }

            EditorGUI.BeginDisabledGroup(editorLevelActive == false || editorLevelProperty != null || managerLevelActive == true);
            
            if(managerLevelActive) { 
                editorLevel = EditorGUILayout.Slider(editorLevelString, managerLevel, 0, 1);
            } else { 
                editorLevel = EditorGUILayout.Slider(editorLevelString, editorLevel, 0, 1);
            }
            EditorGUI.EndDisabledGroup();

            editorLevelActive = EditorGUILayout.Toggle(editorLevelActive);

            GUILayout.EndHorizontal();

            //////////// PRESETS
            if(editorType == SoundInstanceEditorType.Unity)
            {
                // popup or dropdown list for selecting presets
                string[] audioSourcePropertiesPresetsStrings = System.Array.ConvertAll(soundInstanceEditorObject.propertyPresets, obj => obj.name);
                soundInstanceEditorObject.selectedPropertyPresetIndex = EditorGUILayout.Popup("Presets", soundInstanceEditorObject.selectedPropertyPresetIndex, audioSourcePropertiesPresetsStrings);
                if(soundInstanceEditorObject.selectedPropertyPresetIndex != soundInstanceEditorObject.previousPropertyPresetIndex)
                {
                    soundInstanceEditorObject.previousPropertyPresetIndex = soundInstanceEditorObject.selectedPropertyPresetIndex;
                    soundInstanceEditorObject.SetAudioPropertiesFromPreset();
                }
            }
            

            //////////// AUDIO PROPERTIES
            EditorGUI.indentLevel++;
            if(soundInstanceEditorObject.AudioProperties != null)
            {
                for (int i = 0; i < soundInstanceEditorObject.AudioProperties.Count; i++)
                {
                    SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[i];

                    // get the name of the property
                    string foldoutName =  char.ToUpper(property.propertyName[0]) + property.propertyName.Substring(1);
                    if (editorLevelActive && !managerLevelActive)
                    {
                        foldoutName += " (controlled by script)";
                    }
                    else if (managerLevelActive)
                    {
                        foldoutName += " (controlled by manager level)";
                    }

                    // display a foldout for current property
                    GUILayout.BeginVertical(GUI.skin.box);
                    soundInstanceEditorObject.AudioPropertyFoldouts[i] = EditorGUILayout.Foldout(soundInstanceEditorObject.AudioPropertyFoldouts[i], foldoutName);
                    if(soundInstanceEditorObject.AudioPropertyFoldouts[i]){
                        // TODO: Make space between property type controlls and input/output
                        property.propertyEvaluationType = (SoundInstanceEditorAudioPropertyEvaluationType) EditorGUILayout.EnumPopup("Property Type", property.propertyEvaluationType);

                        switch(property.propertyEvaluationType)
                        {
                            case SoundInstanceEditorAudioPropertyEvaluationType.Curve:
                                Rect curveRect = EditorGUILayout.GetControlRect();
                                property.curve = EditorGUI.CurveField(curveRect, "Curve", property.curve);

                                EditorGUI.BeginDisabledGroup(editorLevelActive || managerLevelActive);
                                property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                                EditorGUI.EndDisabledGroup();

                                GUI.enabled = false;
                                property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                                GUI.enabled = true;

                                break;
                            case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:

                                EditorGUI.BeginDisabledGroup(editorLevelActive || managerLevelActive);
                                property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                                EditorGUI.EndDisabledGroup();

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
                            case SoundInstanceEditorAudioPropertyEvaluationType.Level:
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Level");
                                GUILayout.Label(property.level.x.ToString("F2"), GUILayout.Width(30));
                                EditorGUILayout.MinMaxSlider(ref property.level.x, ref property.level.y, 0.0f, 1.0f);
                                GUILayout.Label(property.level.y.ToString("F2"), GUILayout.Width(30));
                                GUILayout.EndHorizontal();
                                EditorGUILayout.Space();

                                EditorGUI.BeginDisabledGroup(editorLevelActive || managerLevelActive);
                                property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                                EditorGUI.EndDisabledGroup();

                                GUI.enabled = false;
                                bool b = property.outputValue != 0.0f;
                                property.outputValue = EditorGUILayout.Toggle("Active: ", b) ? 1.0f : 0.0f;
                                GUI.enabled = true;

                                break;
                            case SoundInstanceEditorAudioPropertyEvaluationType.Linear:
                                property.minValue = EditorGUILayout.FloatField("Min Value", property.minValue);
                                property.maxValue = EditorGUILayout.FloatField("Max Value", property.maxValue);

                                EditorGUI.BeginDisabledGroup(editorLevelActive || managerLevelActive);
                                property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                                EditorGUI.EndDisabledGroup();

                                GUI.enabled = false;
                                property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                                GUI.enabled = true;

                                break;
                        }
                        
                        ///////// REMOVING AUDIO PROPERTIES
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
            EditorGUI.indentLevel--;

            if(editorType == SoundInstanceEditorType.Unity)
            {
                //////////// ADDING AUDIO PROPERTIES
                if(addProperty){
                    EditorGUILayout.BeginHorizontal();

                    // will display the names of all available property templates
                    string[] propertyNames = System.Array.ConvertAll(soundInstanceEditorObject.PropertyTemplates , obj => obj.propertyData.propertyName);
                    
                    // if this button is pressed, addproperty will be set to false
                    // which will hide the current display
                    addProperty = !GUILayout.Button("Go back", GUILayout.Width(100));

                    // a popup that will select the index of the selected template indexx
                    soundInstanceEditorObject.selectedPropertyTemplateIndex = EditorGUILayout.Popup(soundInstanceEditorObject.selectedPropertyTemplateIndex, propertyNames);

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

                //////////// SAVING PROPERTY CONFIGURATION AS PRESET
                GUILayout.Space(10);

                if(!soundInstanceEditorObject.ComparePresetWithAudioProperties())
                {
                    if (GUILayout.Button("Save configuration to current preset"))
                    {
                        SoundInstanceEditorAudioPropertyPreset currentPreset = soundInstanceEditorObject.propertyPresets[soundInstanceEditorObject.selectedPropertyPresetIndex];
                        SoundInstanceEditorAudioProperty[] audioPropertiesArray = soundInstanceEditorObject.AudioProperties.ToArray();
                        currentPreset.UpdatePropertiesArray(audioPropertiesArray);
                        soundInstanceEditorObject.SetAudioPropertiesFromPreset();
                    }
                }

                /////////////// SAVE PROPERTY CONFIGURATION AS NEW PRESET
                if(addPreset){
                    EditorGUILayout.BeginHorizontal();

                    // if this button is pressed, addproperty will be set to false
                    // which will hide the current display
                    addPreset = !GUILayout.Button("Go back", GUILayout.Width(100));

                    Rect curveRect = EditorGUILayout.GetControlRect();
                    presetName = EditorGUI.TextField(curveRect, presetName);

                    if (GUILayout.Button("Add new preset", GUILayout.Width(100)))
                    {
                        // TODO: doesnt seem to work, when its a package
                        string assetPath = "Packages" + "/" + "Sound Instance Manager" + "/" + "Resources" + "/" + "Audio Property Presets" + "/" + presetName + ".asset";
                        SoundInstanceEditorAudioPropertyPreset newPreset = ScriptableObject.CreateInstance<SoundInstanceEditorAudioPropertyPreset>();
                        newPreset.propertiesArray = soundInstanceEditorObject.AudioProperties.ToArray();

                        if(!string.IsNullOrEmpty(assetPath))
                        {
                            AssetDatabase.CreateAsset(newPreset, assetPath);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }

                        soundInstanceEditorObject.LoadPropertyPresets();
                        soundInstanceEditorObject.selectedPropertyPresetIndex = System.Array.FindIndex(soundInstanceEditorObject.propertyPresets, p => p.name == presetName);
                        soundInstanceEditorObject.SetAudioPropertiesFromPreset();

                        addPreset = false;
                    }

                    EditorGUILayout.EndHorizontal();
                } else {
                    addPreset = GUILayout.Button("Save configuration as new preset");
                }
            }
            
            GUILayout.EndVertical();
        }
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

    private void UpdateOnEditorTypeChanged()
    {
        if(previousEditorType != editorType)
        {
            previousEditorType = editorType;
            SetupEditorObject();
        }
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

    public void UpdateMethods()
    {
        UpdateOnEditorTypeChanged();
        CheckAudioClipChange();
        CheckEventReferenceChange();
        UpdateAudioProperties();
        LoadPropertyTemplates();
        LoadPropertyPresets();
    }

    private void CheckAudioClipChange()
    {
        if(editorType != SoundInstanceEditorType.Unity) { return; }
        if(audioClipReference != null)
        {
            if(!audioClipReference.Equals(previousAudioClipReference))
            {
                previousAudioClipReference = audioClipReference;
                SetupEditorObject();
            }
        } else {
            previousAudioClipReference = audioClipReference;
            ResetSoundInstanceEditor();
        }
    }

    private void CheckEventReferenceChange()
    {
        if(editorType != SoundInstanceEditorType.Fmod) { return; }
        if(!fmodEventReference.Guid.Equals(previousFmodEventReference.Guid))
        {
            previousFmodEventReference = fmodEventReference;
            
            SetupEditorObject();
        }
    }

    private void UpdateAudioProperties()
    {
        if(soundInstanceEditorObject == null) return;
        if(soundInstanceEditorObject.AudioProperties == null) return;
        
        for (int index = 0; index < soundInstanceEditorObject.AudioProperties.Count; index++)
        {
            SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[index];
            // PropertyInfo reflectionAudioProperty = reflectionScriptProperties[i];
            // TODO: catch if property doesnt have matching audio property

            PropertyInfo reflectionScriptProperty = reflectionScriptProperties != null ? reflectionScriptProperties[index] : null;
            
            float inputValue = 0;

            inputValue = reflectionScriptProperty != null ? Convert.ToSingle(reflectionScriptProperty.GetValue(reflectionScript)) : property.inputValue;
            if(editorLevelActive) { inputValue = editorLevel;}
            if(managerLevelActive) { inputValue = managerLevel;}

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

    [CustomEditor(typeof(SoundInstanceEditor))]
    public class SoundInstanceEditorInspector : UnityEditor.Editor
    {
        SoundInstanceEditor SoundInstanceEditor;
        private SerializedProperty eventReferenceProperty;
        private SerializedProperty audioClipProperty;
        
        private void OnEnable()
        {
            SoundInstanceEditor = (SoundInstanceEditor)target;
            eventReferenceProperty = serializedObject.FindProperty("fmodEventReference");
            audioClipProperty = serializedObject.FindProperty("audioClipReference");

            // SoundInstanceEditor.SetupEditorObject();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SoundInstanceEditor.UpdateMethods();

            SoundInstanceEditor.DrawInspectorGUIDefaultInfo();

            EditorGUILayout.Space();
            if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Fmod)
            { 
                EditorGUILayout.PropertyField(eventReferenceProperty);
            }
            else if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Unity)
            {
                EditorGUILayout.PropertyField(audioClipProperty);
            }

            SoundInstanceEditor.DrawInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
