using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyLevelManipulation : MonoBehaviour
{
    [SerializeField] private float traffic;
    public float Traffic { get { return traffic; } set { traffic = value; } }
    [SerializeField] private float walla;
    public float Walla { get { return walla; } set { walla = value; } }
    [SerializeField] private float volume;
    public float Volume { get { return volume; } set { volume = value; } }
    private float minValue = 0f;
    private float maxValue = 1f;

    private void Start()
    {
        InvokeRepeating("UpdateValue", 0f, 1f);
    }

    private void UpdateValue()
    {
        Traffic = Random.Range(minValue, maxValue);
        Walla = Random.Range(minValue, maxValue);
        Volume = Random.Range(minValue, maxValue);
    }
}
