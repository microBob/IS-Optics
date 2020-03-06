using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Serialization;

public class PointLight : MonoBehaviour
{
    public Color color = Color.green;
    public float intensity = 2;

    public bool animate = false;
    private float _time = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            _time += Time.deltaTime;
            float speed = 0.5f;
            float size = 2;
            transform.position =
                new Vector3(Mathf.Cos(_time * speed), Mathf.Sin(_time * speed), Mathf.Cos(_time * speed)) * size;
        }
    }
}