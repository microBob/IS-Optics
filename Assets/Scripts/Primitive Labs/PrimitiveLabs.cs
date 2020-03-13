using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PrimitiveLabs : MonoBehaviour
{
    public GameObject raySource;

    private List<GameObject> points = new List<GameObject>();

    private Vector3 _lightPos;
    private int _iterationIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Add cubes to scene and list
        for (int i = 0; i < 5; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(i, 0, 0);

            if (i % 2 == 0)
            {
                cube.GetComponent<Renderer>().material.color = Color.black;
            }

            points.Add(cube);
            Debug.Log("Added cube at: " + cube.transform.position);
        }

        Debug.Log("======================");
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        _lightPos = raySource.transform.position;
        GameObject workingPoint = points[_iterationIndex];
        Vector3 direction = (workingPoint.transform.position - _lightPos).normalized;

        if (Physics.Raycast(_lightPos,direction,out RaycastHit hit, 20))
        {
            Debug.DrawRay(_lightPos, direction * hit.distance, Color.cyan);
            if (hit.collider == workingPoint.GetComponent<Collider>())
            {
                workingPoint.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                workingPoint.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        _iterationIndex++;
        if (_iterationIndex == points.Count)
        {
            _iterationIndex = 0;
        }
    }
}