using System.Collections;
using UnityEditor;
using Extensions.UnityExtensions.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class SoundInstanceManager : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditor[] soundInstances;

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

            foreach(SoundInstanceEditor soundInstance in soundInstances){
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
        soundInstances = Object.FindObjectsOfType<SoundInstanceEditor>();
    }
}

[CustomEditor(typeof(SoundInstanceManager))]
public class SoundInstanceManagerEditor : UnityEditor.Editor
{
    SoundInstanceManager soundInstanceManager;
    
    private void OnEnable()
    {
        soundInstanceManager = (SoundInstanceManager)target;
        soundInstanceManager.UpdateSoundInstances();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        soundInstanceManager.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
