using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Instance Editor Property Preset", menuName = "Sound Instance Editor Property Preset")]
[System.Serializable]
public class SoundInstanceEditorUnityPropertyPreset : ScriptableObject
{
    public SoundInstanceEditorUnityProperty[] propertiesList;

    public void AddPropertyArrayToList(SoundInstanceEditorUnityProperty[] propertyArray)
    {
        propertiesList = propertyArray;
        
    }
}
