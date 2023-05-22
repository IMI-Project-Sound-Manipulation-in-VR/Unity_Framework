using Extensions.UnityExtensions.Attributes;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField, MinMaxRange(-10, 10)]
    private Vector2 _test;
    
    [SerializeField, MinMaxRangeInt(-50, 20)]
    private Vector2Int _test2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
