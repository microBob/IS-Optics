using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class PointLight : MonoBehaviour
{
    public Color color = Color.green;
    public Vector3 position = Vector3.zero;
    public float intensity = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
