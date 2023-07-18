using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Instance Editor Property Preset", menuName = "Sound Instance Editor Property Preset")]
[System.Serializable]
public class SoundInstanceEditorUnityPropertyPreset : ScriptableObject
{
    public SoundInstanceEditorProperty[] propertiesList;

    public void AddPropertyArrayToList(SoundInstanceEditorProperty[] propertyArray)
    {
        propertiesList = propertyArray;
        
    }
}
