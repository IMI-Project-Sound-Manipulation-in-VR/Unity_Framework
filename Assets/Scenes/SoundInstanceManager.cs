using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoundInstanceManager : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditorUnity[] soundInstancesUnity;

    [SerializeField]
    private SoundInstanceEditorFmod[] soundInstancesFmod;

    private bool showInstanceEditorsUnity;
    private bool showInstanceEditorsFmod;

    private bool[] soundInstanceEditorFoldoutsUnity;
    private bool[] soundInstanceEditorFoldoutsFmod;

    [SerializeField]
    private MonoBehaviour script;
    private Type scriptType;
    private float managerLevel;
    private bool managerLevelActive;
    private PropertyInfo managerLevelProperty;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateManagerLevel();
    }

    private void UpdateManagerLevel()
    {
        for(int i = 0; i < soundInstancesUnity.Length; i++)
        {
            SoundInstanceEditorUnity soundInstance = soundInstancesUnity[i];
            soundInstance.UpdateManagerLevel(managerLevelActive, managerLevel);
        }

        // for(int i = 0; i < soundInstancesFmod.Length; i++)
        // {
        //     SoundInstanceEditorFmod soundInstance = soundInstancesFmod[i];
        //     soundInstance.UpdateManagerLevel(managerLevelActive, managerLevel);
        // }
    }

    public void DrawInspectorGUI()
    {
        GUILayout.BeginHorizontal();

        string managerLevelString = "Manager Level";
        if(managerLevelProperty != null) { 
            managerLevelString += " (controlled by script)"; 
            managerLevel = Convert.ToSingle(managerLevelProperty.GetValue(script));
        }

        EditorGUI.BeginDisabledGroup(managerLevelActive == false || managerLevelProperty != null);
        managerLevel = EditorGUILayout.Slider(managerLevelString, managerLevel, 0, 1);
        EditorGUI.EndDisabledGroup();

        managerLevelActive = EditorGUILayout.Toggle(managerLevelActive);

        GUILayout.EndHorizontal();

        
        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;

        
        // Unity Sound Instances
        showInstanceEditorsUnity = EditorGUILayout.Foldout(showInstanceEditorsUnity, "Unity Sound Instances");
        if (showInstanceEditorsUnity) {
            EditorGUI.indentLevel++;
            for(int i = 0; i < soundInstancesUnity.Length; i++)
            {
                SoundInstanceEditorUnity soundInstance = soundInstancesUnity[i];
                soundInstanceEditorFoldoutsUnity[i] = EditorGUILayout.Foldout(soundInstanceEditorFoldoutsUnity[i], soundInstance.name);
                if(soundInstanceEditorFoldoutsUnity[i])
                {
                    soundInstance.DrawInspectorGUI();
                }
            }
            EditorGUI.indentLevel--;
        }
        
        // FMOD Sound Instances
        showInstanceEditorsFmod = EditorGUILayout.Foldout(showInstanceEditorsFmod, "Fmod Sound Instances");
        if (showInstanceEditorsFmod) {
            EditorGUI.indentLevel++;
            for(int i = 0; i < soundInstancesFmod.Length; i++)
            {
                SoundInstanceEditorFmod soundInstance = soundInstancesFmod[i];
                soundInstanceEditorFoldoutsFmod[i] = EditorGUILayout.Foldout(soundInstanceEditorFoldoutsFmod[i], soundInstance.name);
                if(soundInstanceEditorFoldoutsFmod[i])
                {
                    soundInstance.DrawInspectorGUI();
                }
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }

    public void SetManagerLevel(bool active, float level)
    {
        this.managerLevelActive = active;
        this.managerLevel = level;
    }

    public void UpdateSoundInstances()
    {
        soundInstancesUnity = UnityEngine.Object.FindObjectsOfType<SoundInstanceEditorUnity>();
        soundInstanceEditorFoldoutsUnity = new bool[soundInstancesUnity.Length];

        soundInstancesFmod = UnityEngine.Object.FindObjectsOfType<SoundInstanceEditorFmod>();
        soundInstanceEditorFoldoutsFmod = new bool[soundInstancesFmod.Length];
    }

    public void LoadAudioSourcePropertiesResources()
    {
        for(int i = 0; i < soundInstancesUnity.Length; i++)
        {
            soundInstancesUnity[i].LoadAudioSourcePropertiesResources();
        }

        for(int i = 0; i < soundInstancesFmod.Length; i++)
        {
            // soundInstancesFmod[i].LoadAudioSourcePropertiesResources();
        }
    }
}

[CustomEditor(typeof(SoundInstanceManager))]
public class SoundInstanceManagerEditor : UnityEditor.Editor
{
    SoundInstanceManager SoundInstanceManager;
    
    private void OnEnable()
    {
        SoundInstanceManager = (SoundInstanceManager)target;
        SoundInstanceManager.UpdateSoundInstances();
        SoundInstanceManager.LoadAudioSourcePropertiesResources();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SoundInstanceManager.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
