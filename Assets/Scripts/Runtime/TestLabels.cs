using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MyEnum
{
    Night,
    Morning,
    Moon,
    Evening
}

public class TestLabels : MonoBehaviour
{
    [SerializeField] private MyEnum hour;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
