using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Instance Editor Audio Property Preset", menuName = "Sound Instance Editor Audio Property Preset")]
[System.Serializable]
public class SoundInstanceEditorAudioPropertyPreset : ScriptableObject
{
    public SoundInstanceEditorAudioProperty[] propertiesArray;

    public void UpdatePropertiesArray(SoundInstanceEditorAudioProperty[] propertyArray)
    {
        this.propertiesArray = propertyArray;
    }
}
