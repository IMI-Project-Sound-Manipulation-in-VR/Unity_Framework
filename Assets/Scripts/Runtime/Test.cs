using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // fields should match parameter name to be automatically used by sound instance manager
    [SerializeField] private float volume;
    public float Volume { get { return volume; } set { volume = value; } }
    [SerializeField] private float editorLevel;
    public float EditorLevel { get { return editorLevel; } set { editorLevel = value; } }
    private float minValue = 0f;
    private float maxValue = 1f;

    private void Start()
    {
        InvokeRepeating("UpdateValue", 0f, 1f);
    }

    private void UpdateValue()
    {
        volume = Random.Range(minValue, maxValue);
        editorLevel = Random.Range(minValue, maxValue);
    }
}
