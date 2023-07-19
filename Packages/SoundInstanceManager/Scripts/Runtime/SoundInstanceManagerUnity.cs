using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SoundInstanceManagerUnity : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditorUnity[] soundInstances;
    private bool[] soundInstanceEditorFoldouts;
    private bool showSoundInstances = true;
    [SerializeField]
    private MonoBehaviour script;
    private Type scriptType;
    private float managerLevel;
    private bool managerLevelActive;
    private PropertyInfo managerLevelProperty;

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

        showSoundInstances = EditorGUILayout.Foldout(showSoundInstances, "Sound Instances");

        if (showSoundInstances)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            for(int i = 0; i < soundInstances.Length; i++)
            {
                SoundInstanceEditorUnity soundInstance = soundInstances[i];
                UpdateManagerLevelInstance(soundInstance);
                soundInstanceEditorFoldouts[i] = EditorGUILayout.Foldout(soundInstanceEditorFoldouts[i], "Sound Instances");
                if(soundInstanceEditorFoldouts[i])
                {
                    soundInstance.DrawInspectorGUI();
                }
            }

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }
    }

    public void Update()
    {
        UpdateManagerLevel();
    }

    private void UpdateManagerLevel()
    {
        for(int i = 0; i < soundInstances.Length; i++)
        {
            SoundInstanceEditorUnity soundInstance = soundInstances[i];
            UpdateManagerLevelInstance(soundInstance);
        }
    }

    private void UpdateManagerLevelInstance(SoundInstanceEditorUnity instance){
        instance.UpdateManagerLevel(managerLevelActive, managerLevel);
    }

    public void UpdateSoundInstances()
    {
        soundInstances = UnityEngine.Object.FindObjectsOfType<SoundInstanceEditorUnity>();
        soundInstanceEditorFoldouts = new bool[soundInstances.Length];
    }

    public void LoadAudioSourcePropertiesResources()
    {
        for(int i = 0; i < soundInstances.Length; i++)
        {
            soundInstances[i].LoadAudioSourcePropertiesResources();
        }
    }
}

[CustomEditor(typeof(SoundInstanceManagerUnity))]
public class SoundInstanceManagerUnityEditor : UnityEditor.Editor
{
    SoundInstanceManagerUnity SoundInstanceManagerFmod;
    
    private void OnEnable()
    {
        SoundInstanceManagerFmod = (SoundInstanceManagerUnity)target;
        SoundInstanceManagerFmod.UpdateSoundInstances();
        SoundInstanceManagerFmod.LoadAudioSourcePropertiesResources();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SoundInstanceManagerFmod.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
