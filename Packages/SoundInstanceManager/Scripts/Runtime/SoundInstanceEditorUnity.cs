using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

public class SoundInstanceEditorUnity : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] private MonoBehaviour script;
    [SerializeField] private string editorName;
    private SoundInstanceEditorUnityPropertyPreset[] audioSourcePropertiesPresets;
    private string[] audioSourcePropertiesPresetsStrings;
    private int previousPropertyPresetIndex = 0;
    private int selectedPropertyPresetIndex = 0;
    private Type scriptType;
    private SoundInstanceEditorUnityPropertyScriptableObject[] audioSourcePropertyTemplates;
    private int selectedSourcePropertyTemplateIndex = -1;
    private List<SoundInstanceEditorUnityProperty> audioSourceProperties;
    private PropertyInfo[] reflectionAudioSourceProperties;
    private PropertyInfo[] reflectionScriptProperties;

    // Editor Level
    private float editorLevel;
    private bool editorLevelActive;
    private PropertyInfo editorLevelProperty;

    // Manager Level
    private bool managerLevelActive;
    private float managerLevel;
    
    private bool[] audioSourcePropertyFoldouts;
    private bool addProperty;

    // Start is called before the first frame update
    void Start()
    {
        scriptType = script != null ? script.GetType() : null;
        LoadReflectionScriptProperties();
        // audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        UpdatePropertyCurves();
    }

    private void UpdatePropertyCurves(){
        if(audioSourceProperties == null || reflectionScriptProperties == null || reflectionAudioSourceProperties == null) return;
        for (int i = 0; i < audioSourceProperties.Count; i++)
        {
            SoundInstanceEditorUnityProperty property = audioSourceProperties[i];
            PropertyInfo reflectionScriptProperty = reflectionScriptProperties[i];
            PropertyInfo reflectionAudioSourceProperty = reflectionAudioSourceProperties[i];

            float inputValue;
            if(editorLevelActive)
            {
                inputValue = editorLevelProperty != null ? Convert.ToSingle(editorLevelProperty.GetValue(script)) : editorLevel;
            } else {
                inputValue = reflectionScriptProperty != null ? Convert.ToSingle(reflectionScriptProperty.GetValue(script)) : property.inputValue;
            }

            float outputValue = 0;

            // evaluation of input through curve
            if(property.propertyType == SoundInstanceEditorUnityPropertyType.Curve)
            {
                outputValue = property.curve.Evaluate(inputValue);
            }
            
            // evaluation of linear output
            if(property.propertyType == SoundInstanceEditorUnityPropertyType.Linear)
            {
                outputValue = property.minValue + (property.maxValue - property.minValue) * inputValue;
            }

            // evaluation of level, returns 0 or 1 (bool) if input is between the level
            if(property.propertyType == SoundInstanceEditorUnityPropertyType.Level)
            {
                outputValue = inputValue >= property.level.x && inputValue <= property.level.y ? 1 : 0;
            }

            // if the audio source has such a field, update it with output value
            if(reflectionAudioSourceProperty != null && audioSource != null){
                if(reflectionAudioSourceProperty.PropertyType == typeof(bool))
                {
                    bool b = property.outputValue != 0.0f;
                    reflectionAudioSourceProperty.SetValue(audioSource, b);
                } else {
                    reflectionAudioSourceProperty.SetValue(audioSource, outputValue);
                }
            }
        }
    }

    public void LoadAudioSourcePropertiesResources()
    {
        // loads property templates, which are responsible for manipulating properties of a audio source object with additional options
        // the templates setup default values, such as default input value, max and min values and a curve object.
        audioSourcePropertyTemplates = Resources.LoadAll<SoundInstanceEditorUnityPropertyScriptableObject>("SoundInstanceEditorUnityParameter");
        // loads property presets. presets are simply a handy collection of templates
        // for example: a stress "preset" that includes two "templates" pitch and volume
        audioSourcePropertiesPresets = Resources.LoadAll<SoundInstanceEditorUnityPropertyPreset>("SoundInstanceEditorUnityParameterPresets");
        
        // generate a string array, which includes the names of each template included in the preset
        // this is primarily used in the presets dropdown in the inspector, in which you can select different presets
        List<string> options = new List<string>();
        options.Add("No preset");
        options.AddRange(System.Array.ConvertAll(audioSourcePropertiesPresets, obj => obj.name));
        audioSourcePropertiesPresetsStrings = options.ToArray();
    }

    public void UpdateManagerLevel(bool active, float value)
    {
        this.managerLevelActive = active;
        this.managerLevel = value;
    }

    private void AddAudioSourcePropertyFromDropdown() {
        // is responsible for adding a property template to the list of audi source properties
        // first get the data from the template
        SoundInstanceEditorUnityPropertyScriptableObject selectedTemplate = audioSourcePropertyTemplates[selectedSourcePropertyTemplateIndex];
        // create a new instance for the property
        SoundInstanceEditorUnityProperty newAudioSourceProperty = new SoundInstanceEditorUnityProperty();
        // copy all data from the template to the new property and setup various other things
        newAudioSourceProperty.SetupFields(selectedTemplate);
        // if there are no audio source properties, generate a completly new list and add this property as its first entry
        if(audioSourceProperties == null) { audioSourceProperties = new List<SoundInstanceEditorUnityProperty>(); }
        // adds property to list of properties
        audioSourceProperties.Add(newAudioSourceProperty);
        // re initializes foldouts
        audioSourcePropertyFoldouts = new bool[audioSourceProperties.Count];

        // adds new property automatically to property preset
        SoundInstanceEditorUnityPropertyPreset preset = audioSourcePropertiesPresets[selectedPropertyPresetIndex - 1];
        preset.AddPropertyArrayToList(audioSourceProperties.ToArray());
    }

    private void RemoveAudioSourceProperty(int index) {
        audioSourceProperties.RemoveAt(index);
        audioSourcePropertyFoldouts = new bool[audioSourceProperties.Count];
        SoundInstanceEditorUnityPropertyPreset preset = audioSourcePropertiesPresets[selectedPropertyPresetIndex - 1];
        preset.AddPropertyArrayToList(audioSourceProperties.ToArray());
    }

    private void LoadAudioSourcePropertiesFromPreset() {
        // if the properties preset doesnt include any properties
        if(audioSourcePropertiesPresets.Length == 0 || selectedPropertyPresetIndex == 0) {
            // simply overwrite the previous audio source properties with a empty list 
            audioSourceProperties = new List<SoundInstanceEditorUnityProperty>();
            // and delete the bool array, which is responsible for displaying and hiding the individual
            audioSourcePropertyFoldouts = new bool[0]; 
        } else {
            // in any other case, overwrite the audio source properties list
            audioSourceProperties = new List<SoundInstanceEditorUnityProperty>();
            // add all sound properties from the preset to the audio source properties
            SoundInstanceEditorUnityProperty[] properties = audioSourcePropertiesPresets[selectedPropertyPresetIndex - 1].propertiesList;
            audioSourceProperties.AddRange(properties);
            // generate a new boolean array for the foldouts, which are responsible for showing and hiding the 
            audioSourcePropertyFoldouts = new bool[properties.Length];
        }
    }

    public void LoadReflectionScriptProperties()
    {   
        if(audioSourceProperties != null){
            // generates a array of property infos for...
            
            // the properties of the audio source. these will hold the properties that will be controlled by this script
            reflectionAudioSourceProperties = new PropertyInfo[audioSourceProperties.Count];

            // the properties of the script. these will hold the properties, from which this script can retrieves values automatically
            reflectionScriptProperties = new PropertyInfo[audioSourceProperties.Count];

            // for each property available in the preset
            for (int i = 0; i < audioSourceProperties.Count; i++) {
                SoundInstanceEditorUnityProperty property = audioSourceProperties[i];

                // check if the audio source has a properties with the same name (ideally it should always yield the correct result. if not, maybe the name is wrong)
                PropertyInfo f = audioSource != null ? audioSource.GetType().GetProperty(property.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;
                
                // check if the script has a field with the same property name
                // this is optional. if the property doesnt exists, the external script will not controll the property
                // and so only manual mainpulation through the inspector will work
                PropertyInfo p = scriptType != null ? scriptType.GetProperty(property.propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;

                // assign the property infos if they exist
                if(f != null) reflectionAudioSourceProperties[i] = f;
                if(p != null) reflectionScriptProperties[i] = p;
            }
        }

        editorLevelProperty = scriptType != null ? scriptType.GetProperty("editorLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) : null;
    }

    public void DrawInspectorGUI() {

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUI.skin.window);
        
        GUILayout.Label("Sound Instance Editor: " + editorName);

        GUILayout.BeginHorizontal();

        string editorLevelString = "Editor Level";
        if(editorLevelProperty != null && managerLevelActive == false) { 
            editorLevelString += " (controlled by script)"; 
            editorLevel = Convert.ToSingle(editorLevelProperty.GetValue(script));
        }

        if(managerLevelActive == true)
        {
            editorLevelString += " (controlled by manager level)"; 
            editorLevel = managerLevel;
        }

        EditorGUI.BeginDisabledGroup(editorLevelActive == false || editorLevelProperty != null || managerLevelActive == true);
        editorLevel = EditorGUILayout.Slider(editorLevelString, editorLevel, 0, 1);
        EditorGUI.EndDisabledGroup();

        editorLevelActive = EditorGUILayout.Toggle(editorLevelActive);

        GUILayout.EndHorizontal();

        // popup or dropdown list for selecting presets
        selectedPropertyPresetIndex = EditorGUILayout.Popup("Presets", selectedPropertyPresetIndex, audioSourcePropertiesPresetsStrings);
        if(audioSourcePropertiesPresetsStrings.Length == 0) { selectedPropertyPresetIndex = -1; }

        EditorGUI.indentLevel++;

        // responsible for detecting change between presets
        if(previousPropertyPresetIndex != selectedPropertyPresetIndex)
        {
            // if a change in the presets is detected, re-initialze all audio source properties
            LoadAudioSourcePropertiesFromPreset();
            // then initialize all reflection properties. the properties include the audio source properties as well as properties found in the target script
            // which should automatically control the audio source
            LoadReflectionScriptProperties();

            previousPropertyPresetIndex = selectedPropertyPresetIndex;
        }

        if(audioSourceProperties != null)
        {
            // go through each property of the preset
            for (int i = 0; i < audioSourceProperties.Count; i++)
            {
                // load the property and all its data
                SoundInstanceEditorUnityProperty property = audioSourceProperties[i];
                // load the script property
                // if it exists, the script will use the value that the script will provide for this property
                PropertyInfo reflectionScriptProperty = reflectionScriptProperties != null ? reflectionScriptProperties[i] : null;

                // get the name of the property
                string foldoutName =  char.ToUpper(property.propertyName[0]) + property.propertyName.Substring(1);
                if(reflectionScriptProperty != null && editorLevelActive == false){
                    // at a suffix to indicate that the value is already in controll by a external script
                    // in this case, youll not be a able to directly controll this value anymore
                    foldoutName += " (controlled by property in script)";
                }

                if(editorLevelActive == true && managerLevelActive == false){
                    foldoutName += " (controlled by editor level value)";
                }

                if(managerLevelActive == true)
                {
                    foldoutName += " (controlled by manager level value)";
                }
                
                // display a foldout for current property
                GUILayout.BeginVertical(GUI.skin.box);
                audioSourcePropertyFoldouts[i] = EditorGUILayout.Foldout(audioSourcePropertyFoldouts[i], foldoutName);

                EditorGUI.indentLevel++;

                if(audioSourcePropertyFoldouts[i]){

                    property.propertyType = (SoundInstanceEditorUnityPropertyType)EditorGUILayout.EnumPopup("Property Type", property.propertyType);

                    if(property.propertyType == SoundInstanceEditorUnityPropertyType.Curve) 
                    {
                        // displays the curve of the property
                        Rect curveRect = EditorGUILayout.GetControlRect();
                        property.curve = EditorGUI.CurveField(curveRect, "Curve", property.curve);
                    }

                    if(property.propertyType == SoundInstanceEditorUnityPropertyType.Linear)
                    {
                        property.minValue = EditorGUILayout.FloatField("Min Value", property.minValue);
                        property.maxValue = EditorGUILayout.FloatField("Max Value", property.maxValue);
                    }

                    if(property.propertyType == SoundInstanceEditorUnityPropertyType.Level)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Level");
                        GUILayout.Label(property.level.x.ToString("F2"), GUILayout.Width(30));
                        EditorGUILayout.MinMaxSlider(ref property.level.x, ref property.level.y, 0.0f, 1.0f);
                        GUILayout.Label(property.level.y.ToString("F2"), GUILayout.Width(30));
                        GUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                    }

                    // disables the input value, if the value is controlled by script, editor level or manager level
                    EditorGUI.BeginDisabledGroup(reflectionScriptProperty != null || editorLevelActive || managerLevelActive);

                    // take the input value and evaluate output value through the present curve
                    float inputValue;
                    if(editorLevelActive)
                    {
                        inputValue = editorLevelProperty != null ? Convert.ToSingle(editorLevelProperty.GetValue(script)) : editorLevel;
                    } 
                    else if(managerLevelActive)
                    {
                        inputValue = editorLevel;
                    }
                    else {
                        inputValue = reflectionScriptProperty != null ? Convert.ToSingle(reflectionScriptProperty.GetValue(script)) : property.inputValue;
                    }

                    // display the input value as slider
                    property.inputValue = EditorGUILayout.Slider("Input Value", inputValue, 0, 1);

                    float outputValue = 0;
                    float minValue = 0;
                    float maxValue = 1;

                    // evaluation of input through curve
                    if(property.propertyType == SoundInstanceEditorUnityPropertyType.Curve)
                    {
                        outputValue = property.curve.Evaluate(inputValue);
                        // get the min and max value of the curve
                        // it evaluates the value, not the time, as the time should always be 0-1
                        minValue = property.curve.keys.Min(key => key.value);
                        maxValue = property.curve.keys.Max(key => key.value);
                    }
                    
                    // evaluation of linear output
                    if(property.propertyType == SoundInstanceEditorUnityPropertyType.Linear)
                    {
                        outputValue = property.minValue + (property.maxValue - property.minValue) * inputValue;
                        minValue = property.minValue;
                        maxValue = property.maxValue;
                    }

                    // evaluation of level, returns 0 or 1 (bool) if input is between the level
                    if(property.propertyType == SoundInstanceEditorUnityPropertyType.Level)
                    {
                        outputValue = inputValue >= property.level.x && inputValue <= property.level.y ? 1 : 0;
                        minValue = 0;
                        maxValue = 1;
                    }

                    // display the output value, also as slider. you can effectively see the change of the curve
                    // will always be disabled
                    GUI.enabled = false;
                    property.outputValue = EditorGUILayout.Slider("Output Value", outputValue, minValue, maxValue);
                    GUI.enabled = true;

                    if (GUILayout.Button("Remove Property")) {
                        RemoveAudioSourceProperty(i);
                        break;
                    }

                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }
        }

        // responsible for showing and hiding elements when adding a new property
        // if add property is true, it will display a menu where you can selected and add a new property
        // from a selection of property templates
        if(addProperty)
        {
            EditorGUILayout.BeginHorizontal();

            // will display the names of all available property templates
            string[] propertyNames = System.Array.ConvertAll(audioSourcePropertyTemplates, obj => obj.propertyName);

            // if this button is pressed, addproperty will be set to false
            // which will hide the current display
            addProperty = !GUILayout.Button("Go back", GUILayout.Width(100));

            // a popup that will select the index of the selected template indexx
            selectedSourcePropertyTemplateIndex = EditorGUILayout.Popup(selectedSourcePropertyTemplateIndex, propertyNames);

            // if the add button is clicked
            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                // add the property template to the preset
                AddAudioSourcePropertyFromDropdown();

                // reload all reflection properties, since weve added a new property 
                LoadReflectionScriptProperties();

                // set "addProperty" to false, to hide current display after adding a new template
                addProperty = false;
                // AddSelectedOption();
            }

            EditorGUILayout.EndHorizontal();
        } else {
            // a button to display the menu to add a new template to the property preset
            addProperty = GUILayout.Button("Add new property");
        }
        
        // a button to save current property list as a new preset
        // this only works when no preset has been selected
        if(selectedPropertyPresetIndex == -1 || selectedPropertyPresetIndex == 0)
        {
            // button to save current properties as preset
            if (GUILayout.Button("Save configuration as preset"))
            {   
                // will display a popup in which the user is able to save the preset
                string filePath = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "NewScriptableObject", "asset", "Save ScriptableObject");
                // create a instance of the property preset
                SoundInstanceEditorUnityPropertyPreset preset = ScriptableObject.CreateInstance<SoundInstanceEditorUnityPropertyPreset>();
                // add all current properties to the preset
                preset.AddPropertyArrayToList(audioSourceProperties.ToArray());

                // responsible for saving the preset as a scriptable object asset
                // which can then be used to load a configuration of properties
                if (!string.IsNullOrEmpty(filePath))
                {
                    AssetDatabase.CreateAsset(preset, filePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        
        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
        
    }
}

[CustomEditor(typeof(SoundInstanceEditorUnity))]
public class SoundInstanceEditorUnityInspector : UnityEditor.Editor
{
    SoundInstanceEditorUnity SoundInstanceEditorFmod;

    private void OnEnable()
    {
        SoundInstanceEditorFmod = (SoundInstanceEditorUnity)target;
        SoundInstanceEditorFmod.LoadAudioSourcePropertiesResources();
        // SoundInstanceEditorFmod.InitializeAudioSourceProperties();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SoundInstanceEditorFmod.DrawInspectorGUI();
    }
}
