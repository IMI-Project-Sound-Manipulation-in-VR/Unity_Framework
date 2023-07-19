using System.Collections;
using UnityEditor;
using Extensions.UnityExtensions.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class SoundInstanceManagerFmod : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditorFmod[] soundInstances;

    private float stressLevel = 0.5f;
    private bool showSoundInstances = true;
    
    public void DrawInspectorGUI()
    {
        stressLevel = EditorGUILayout.Slider("Stress Level", stressLevel, 0.0f, 1f);
        showSoundInstances = EditorGUILayout.Foldout(showSoundInstances, "Sound Instances");

        if (showSoundInstances)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            foreach(SoundInstanceEditorFmod soundInstance in soundInstances){
                soundInstance.DrawInspectorGUI();
            }

            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }
    }

    private void Update()
    {
        foreach (var soundInstance in soundInstances)
        {
            // Debug.Log(soundInstance.level.x);
            if (stressLevel >= soundInstance.Level.x && stressLevel <= soundInstance.Level.y)
            {
                soundInstance.Play();
            }
            else
            {
                soundInstance.Stop();
            }
        }
    }

    public void UpdateSoundInstances()
    {
        soundInstances = Object.FindObjectsOfType<SoundInstanceEditorFmod>();
    }
}

[CustomEditor(typeof(SoundInstanceManagerFmod))]
public class SoundInstanceManagerEditor : UnityEditor.Editor
{
    SoundInstanceManagerFmod SoundInstanceManagerFmod;
    
    private void OnEnable()
    {
        SoundInstanceManagerFmod = (SoundInstanceManagerFmod)target;
        SoundInstanceManagerFmod.UpdateSoundInstances();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SoundInstanceManagerFmod.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
