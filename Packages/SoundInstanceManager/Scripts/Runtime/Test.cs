using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // fields should match parameter name to be automatically used by sound instance manager
    [SerializeField] private float traffic;
    public float Traffic { get { return traffic; } set { traffic = value; } }
    [SerializeField] private float walla;
    public float Walla { get { return walla; } set { walla = value; } }
    private float minValue = 0f;
    private float maxValue = 1f;

    private void Start()
    {
        InvokeRepeating("UpdateValue", 0f, 1f);
    }

    private void UpdateValue()
    {
        traffic = Random.Range(minValue, maxValue);
        Walla = Random.Range(minValue, maxValue);
    }
}
