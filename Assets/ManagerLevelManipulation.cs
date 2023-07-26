using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerLevelManipulation : MonoBehaviour
{
    public SoundInstanceManager soundInstanceManager;
    private float minValue = 0f;
    private float maxValue = 1f;
    private void Start()
    {
        InvokeRepeating("UpdateValue", 0f, 1f);
    }

    private void UpdateValue()
    {
        soundInstanceManager.SetManagerLevel(true, Random.Range(minValue, maxValue));
    }
}
