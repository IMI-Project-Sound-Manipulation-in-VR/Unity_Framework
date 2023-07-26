using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Instance Editor Audio Property Template", menuName = "Sound Instance Editor Audio Property Template")]
[System.Serializable]
public class SoundInstanceEditorAudioPropertyTemplate : ScriptableObject
{
    [SerializeField]
    public SoundInstanceEditorAudioProperty propertyData;

    public void SetupPropertyData(SoundInstanceEditorAudioProperty propertyData)
    {
        this.propertyData = propertyData;
    }
}
