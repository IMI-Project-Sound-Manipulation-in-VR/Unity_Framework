using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoundInstanceManager : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditor[] soundInstancesUnity;

    [SerializeField]
    private SoundInstanceEditor[] soundInstancesFmod;

    private bool showInstanceEditorsUnity;
    private bool showInstanceEditorsFmod;

    private bool[] soundInstanceEditorFoldoutsUnity;
    private bool[] soundInstanceEditorFoldoutsFmod;

    [SerializeField]
    private MonoBehaviour script;
    private Type scriptType;
    private float managerLevel;
    private bool managerLevelActive;
    private bool managerLevelScriptActive;
    private PropertyInfo managerLevelProperty;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying)
        {
            UpdateSoundInstanceMethods();
        }
    }

    public void UpdateSoundInstanceMethods()
    {
        for(int i = 0; i < soundInstancesUnity.Length; i++)
        {
            SoundInstanceEditor soundInstance = soundInstancesUnity[i];
            soundInstance.UpdateManagerLevel(managerLevelActive, managerLevel);
            soundInstance.UpdateMethods();
        }

        for(int i = 0; i < soundInstancesFmod.Length; i++)
        {
            SoundInstanceEditor soundInstance = soundInstancesFmod[i];
            soundInstance.UpdateManagerLevel(managerLevelActive, managerLevel);
            soundInstance.UpdateMethods();
        }
    }

    public void DrawInspectorGUI()
    {
        string managerLevelString = "Manager Level";
        if(managerLevelProperty != null) { 
            managerLevelString += " (controlled by script)"; 
            managerLevel = Convert.ToSingle(managerLevelProperty.GetValue(script));
        }

        managerLevelScriptActive = EditorGUILayout.Toggle("Enable Script Control", managerLevelScriptActive);

        EditorGUI.BeginDisabledGroup(managerLevelProperty != null || managerLevelScriptActive);
        managerLevelActive = EditorGUILayout.Toggle("Enable Manager Level", managerLevelActive);
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(managerLevelActive == false || managerLevelProperty != null || managerLevelScriptActive);
        managerLevel = EditorGUILayout.Slider(managerLevelString, managerLevel, 0, 1);
        EditorGUI.EndDisabledGroup();
        

        GUILayout.BeginVertical(GUI.skin.box);
        EditorGUI.indentLevel++;
        
        // Unity Sound Instances
        showInstanceEditorsUnity = EditorGUILayout.Foldout(showInstanceEditorsUnity, "Unity Sound Instances: (" + soundInstancesUnity.Length + ")");
        if (showInstanceEditorsUnity) {
            EditorGUI.indentLevel++;
            for(int i = 0; i < soundInstancesUnity.Length; i++)
            {
                SoundInstanceEditor soundInstance = soundInstancesUnity[i];
                if(soundInstance.SoundInstanceEditorObject != null)
                {
                    soundInstanceEditorFoldoutsUnity[i] = EditorGUILayout.Foldout(soundInstanceEditorFoldoutsUnity[i], soundInstance.SoundInstanceEditorObject.InstanceName);
                    if(soundInstanceEditorFoldoutsUnity[i])
                    {
                        soundInstance.DrawInspectorGUI();
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
        
        // FMOD Sound Instances
        showInstanceEditorsFmod = EditorGUILayout.Foldout(showInstanceEditorsFmod, "Fmod Sound Instances: (" + soundInstancesFmod.Length + ")");
        if (showInstanceEditorsFmod) {
            EditorGUI.indentLevel++;
            for(int i = 0; i < soundInstancesFmod.Length; i++)
            {
                SoundInstanceEditor soundInstance = soundInstancesFmod[i];
                if(soundInstance.SoundInstanceEditorObject != null)
                {
                    soundInstanceEditorFoldoutsFmod[i] = EditorGUILayout.Foldout(soundInstanceEditorFoldoutsFmod[i], soundInstance.SoundInstanceEditorObject.InstanceName);
                    if(soundInstanceEditorFoldoutsFmod[i])
                    {
                        soundInstance.DrawInspectorGUI();
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }

    public void SetManagerLevel(bool active, float level)
    {
        if (managerLevelScriptActive) {
            this.managerLevelActive = active;
            this.managerLevel = level;
        }
    }

    public void UpdateSoundInstances()
    {
        SoundInstanceEditor[] soundInstances = UnityEngine.Object.FindObjectsOfType<SoundInstanceEditor>();
        soundInstancesUnity = soundInstances.Where(obj => obj.editorType == SoundInstanceEditorType.Unity).ToArray();
        soundInstanceEditorFoldoutsUnity = new bool[soundInstancesUnity.Length];

        soundInstancesFmod = soundInstances.Where(obj => obj.editorType == SoundInstanceEditorType.Fmod).ToArray();
        soundInstanceEditorFoldoutsFmod = new bool[soundInstancesFmod.Length];
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
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!Application.isPlaying)
        {
            SoundInstanceManager.UpdateSoundInstanceMethods();
        }

        SoundInstanceManager.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
