using UnityEditor;
using UnityEngine;

public class SoundInstanceManagerUnity : MonoBehaviour
{
    [SerializeField]
    private SoundInstanceEditorUnitySound[] soundInstances;
    private bool[] soundInstanceEditorFoldouts;

    private bool showSoundInstances = true;

    public void DrawInspectorGUI()
    {
        showSoundInstances = EditorGUILayout.Foldout(showSoundInstances, "Sound Instances");

        if (showSoundInstances)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            for(int i = 0; i < soundInstances.Length; i++)
            {
                SoundInstanceEditorUnitySound soundInstance = soundInstances[i];
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

    public void UpdateSoundInstances()
    {
        soundInstances = Object.FindObjectsOfType<SoundInstanceEditorUnitySound>();
        soundInstanceEditorFoldouts = new bool[soundInstances.Length];
    }
}

[CustomEditor(typeof(SoundInstanceManagerUnity))]
public class SoundInstanceManagerUnityEditor : UnityEditor.Editor
{
    SoundInstanceManagerUnity soundInstanceManager;
    
    private void OnEnable()
    {
        soundInstanceManager = (SoundInstanceManagerUnity)target;
        soundInstanceManager.UpdateSoundInstances();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        soundInstanceManager.DrawInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
