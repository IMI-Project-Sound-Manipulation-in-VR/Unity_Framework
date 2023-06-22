using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // fields should match parameter name to be automatically used by sound instance manager
    [SerializeField] private float cover;
    [SerializeField] private float rain;
    [SerializeField] private float wind;
    private float minValue = 0f;
    private float maxValue = 1f;

    private void Start()
    {
        // parameterLink = ScriptableObject.CreateInstance<SoundInstanceParameterLink>();
        InvokeRepeating("UpdateValue", 0f, 1f);
    }

    private void UpdateValue()
    {
        cover = Random.Range(minValue, maxValue);
        rain = Random.Range(minValue, maxValue);
        wind = Random.Range(minValue, maxValue);
        // parameterLink.SetParameterLink(eventReference, eventReference.Name, "Cover", value);
    }
}
