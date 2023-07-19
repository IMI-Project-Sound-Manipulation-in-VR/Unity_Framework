using System.Collections;
using System.Collections.Generic;
using Extensions.UnityExtensions.Attributes;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Sound Instance Editor FMOD Preset Data", menuName = "Sound Instance Editor FMOD Preset Data")]
public class SoundInstanceEditorFmodPresetData : ScriptableObject
{
    public string presetName;

    [Range(0.0f, 5.0f)]
    public float tempo = 1.0f;
    [Range(0.0f, 4.0f)]
    public float pitch = 1.0f;
    [SerializeField, Range(0.0f, 1.0f)]
    public float volume = 1.0f;
    [SerializeField, MinMaxRange(0, 1)]
    public Vector2 level;
}

[CustomEditor(typeof(SoundInstanceEditorFmodPresetData))]
public class SoundManagerPresetDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty presetNameProperty = serializedObject.FindProperty("presetName");
        SerializedProperty volumeProperty = serializedObject.FindProperty("volume");
        SerializedProperty pitchProperty = serializedObject.FindProperty("pitch");
        SerializedProperty levelProperty = serializedObject.FindProperty("level");
        SerializedProperty tempoProperty = serializedObject.FindProperty("tempo");

        EditorGUILayout.PropertyField(presetNameProperty);
        EditorGUILayout.PropertyField(volumeProperty);
        EditorGUILayout.PropertyField(pitchProperty);
        EditorGUILayout.PropertyField(levelProperty);
        EditorGUILayout.PropertyField(tempoProperty);

        const float limit = 1.0f;
        Vector2 level = levelProperty.vector2Value;
        level.x = Mathf.Clamp(level.x, -limit, limit);
        level.y = Mathf.Clamp(level.y, -limit, limit);
        levelProperty.vector2Value = level;

        serializedObject.ApplyModifiedProperties();
    }
}
