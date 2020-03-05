using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class PointLight : MonoBehaviour
{
    public Color color = Color.green;
    public float intensity = 3;

    private bool _animate = true;
    private float _time = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_animate)
        {
            _time += Time.deltaTime;
            int speed = 2;
            transform.position = new Vector3(Mathf.Cos(_time * speed),Mathf.Sin(_time * speed),Mathf.Cos(_time * speed));
        }
    }
}
