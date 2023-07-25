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

    // Unity
    [SerializeField]
    private AudioSource audioSourceReference;

    // FMOD
    [SerializeField]
    private EventReference fmodEventReference;

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

    // Start is called before the first frame update
    void Start()
    {
        LoadReflectionScript();
        SetupAudioInstance();
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
            if(editorType == SoundInstanceEditorType.Fmod)
            {
                SoundInstanceEditorObjectFmod fmodObject = (SoundInstanceEditorObjectFmod) soundInstanceEditorObject;
                fmodObject.EventReference = fmodEventReference;
                fmodObject.UpdateInstanceReference();
            }
            else if(editorType == SoundInstanceEditorType.Unity)
            {
                Debug.Log("bruh");
                SoundInstanceEditorObjectUnity unityObject = (SoundInstanceEditorObjectUnity) soundInstanceEditorObject;
                unityObject.AudioSource = audioSourceReference;
                unityObject.UpdateInstanceReference();
            }

            GUILayout.BeginVertical(GUI.skin.window);

            GUILayout.Label("Sound Instance Editor: " + soundInstanceEditorObject.InstanceName);
            
            //////////// Editor Level
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
            editorLevel = EditorGUILayout.Slider(editorLevelString, editorLevel, 0, 1);
            EditorGUI.EndDisabledGroup();

            editorLevelActive = EditorGUILayout.Toggle(editorLevelActive);

            GUILayout.EndHorizontal();

            //////////// PRESETS
            // popup or dropdown list for selecting presets
            string[] audioSourcePropertiesPresetsStrings = System.Array.ConvertAll(soundInstanceEditorObject.propertyPresets, obj => obj.name);
            soundInstanceEditorObject.selectedPropertyTemplateIndex = EditorGUILayout.Popup("Presets", soundInstanceEditorObject.selectedPropertyTemplateIndex, audioSourcePropertiesPresetsStrings);

            //////////// AUDIO PROPERTIES
            EditorGUI.indentLevel++;
            if(soundInstanceEditorObject.AudioProperties != null)
            {
                for (int i = 0; i < soundInstanceEditorObject.AudioProperties.Count; i++)
                {
                    SoundInstanceEditorAudioProperty property = soundInstanceEditorObject.AudioProperties[i];

                    // get the name of the property
                    string foldoutName =  char.ToUpper(property.propertyName[0]) + property.propertyName.Substring(1);

                    // display a foldout for current property
                    GUILayout.BeginVertical(GUI.skin.box);
                    soundInstanceEditorObject.AudioPropertyFoldouts[i] = EditorGUILayout.Foldout(soundInstanceEditorObject.AudioPropertyFoldouts[i], foldoutName);
                    if(soundInstanceEditorObject.AudioPropertyFoldouts[i]){
                        property.propertyEvaluationType = (SoundInstanceEditorAudioPropertyEvaluationType) EditorGUILayout.EnumPopup("Property Type", property.propertyEvaluationType);

                        // disables the input value, if the value is controlled by script, editor level or manager level
                        // EditorGUI.BeginDisabledGroup(reflectionScriptProperties[i] != null || editorLevelActive || managerLevelActive);
                        EditorGUI.BeginDisabledGroup(editorLevelActive || managerLevelActive);
                        // display the input value as slider
                        property.inputValue = EditorGUILayout.Slider("Input Value", property.inputValue, 0, 1);
                        EditorGUI.EndDisabledGroup();


                        switch(property.propertyEvaluationType)
                        {
                            case SoundInstanceEditorAudioPropertyEvaluationType.Curve:
                                Rect curveRect = EditorGUILayout.GetControlRect();
                                property.curve = EditorGUI.CurveField(curveRect, "Curve", property.curve);

                                GUI.enabled = false;
                                property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                                GUI.enabled = true;

                                break;
                            case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:

                                GUI.enabled = false;
                                property.outputValue = EditorGUILayout.IntSlider("Output Value", (int) property.outputValue, (int) property.minValue, (int) property.maxValue);
                                // Display the corresponding string label for the selected slider value
                                EditorGUILayout.LabelField("Selected Label: ", property.labels[(int) property.outputValue]);
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

                                GUI.enabled = false;
                                bool b = property.outputValue != 0.0f;
                                property.outputValue = EditorGUILayout.Toggle("Active: ", b) ? 1.0f : 0.0f;
                                GUI.enabled = true;

                                break;
                            case SoundInstanceEditorAudioPropertyEvaluationType.Linear:
                                property.minValue = EditorGUILayout.FloatField("Min Value", property.minValue);
                                property.maxValue = EditorGUILayout.FloatField("Max Value", property.maxValue);

                                GUI.enabled = false;
                                property.outputValue = EditorGUILayout.Slider("Output Value: ", property.outputValue, property.minValue, property.maxValue);
                                GUI.enabled = true;

                                break;
                        }
                    }

                    GUILayout.EndVertical();
                }
            }
            EditorGUI.indentLevel--;



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

            GUILayout.EndVertical();
        }
    }

    // Private
    private void SetupAudioInstance()
    {
        if(soundInstanceEditorObject != null)
        {
            soundInstanceEditorObject.SetupAudioInstance();
        }
    }

    private void LoadReflectionScript(){
        reflectionScriptType = reflectionScript ? reflectionScript.GetType() : null;
    }

    private void UpdateOnEditorTypeChanged()
    {
        if(previousEditorType != editorType)
        {
            SetupEditorObject();
            previousEditorType = editorType;
        }
    }

    // Private

    private void UpdatePropertyTemplates()
    {
        if(soundInstanceEditorObject == null) return;
        soundInstanceEditorObject.UpdatePropertyTemplates();
    }

    private void UpdatePropertyPresets()
    {
        if(soundInstanceEditorObject == null) return;
        soundInstanceEditorObject.UpdatePropertyPresets();
    }

    private void UpdateMethods()
    {
        UpdateOnEditorTypeChanged();
        UpdateAudioProperties();
        UpdatePropertyTemplates();
        UpdatePropertyPresets();
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
                    property.outputValue = property.minValue + (property.maxValue - property.minValue) * inputValue;
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Level:
                    property.outputValue = inputValue >= property.level.x && inputValue <= property.level.y ? 1 : 0;
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
                case SoundInstanceEditorAudioPropertyEvaluationType.Labeled:
                    property.outputValue = Mathf.RoundToInt(Mathf.Lerp(property.minValue, property.maxValue, property.inputValue));
                    property.minValue = property.defaultMinValue;
                    property.maxValue = property.defaultMaxValue;
                    break;
            }

            soundInstanceEditorObject.SetAudioPropertyValue(property, index, property.outputValue);
        }
    }

    private void SetupEditorObject()
    {
        if(soundInstanceEditorObject == null) {
            switch (editorType)
            {
                case SoundInstanceEditorType.Unity:
                    // TODO: Create Unity Object;
                    soundInstanceEditorObject = new SoundInstanceEditorObjectUnity(this);
                    break;
                case SoundInstanceEditorType.Fmod:
                    // TODO: Create Fmod Object;
                    soundInstanceEditorObject = new SoundInstanceEditorObjectFmod(this.gameObject);
                    break;
            }
        }
    }

    [CustomEditor(typeof(SoundInstanceEditor))]
    public class SoundInstanceEditorInspector : UnityEditor.Editor
    {
        SoundInstanceEditor SoundInstanceEditor;
        private SerializedProperty eventReferenceProperty;
        private SerializedProperty audioSourceProperty;
        
        private void OnEnable()
        {
            SoundInstanceEditor = (SoundInstanceEditor)target;
            eventReferenceProperty = serializedObject.FindProperty("fmodEventReference");
            audioSourceProperty = serializedObject.FindProperty("audioSourceReference");
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
            else if(SoundInstanceEditor.editorType == SoundInstanceEditorType.Fmod)
            {
                EditorGUILayout.PropertyField(audioSourceProperty);
            }

            SoundInstanceEditor.DrawInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
