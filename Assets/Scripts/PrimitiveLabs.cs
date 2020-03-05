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
    private RaycastHit[] _hits;

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

        _lightPos = raySource.transform.position;
        _hits = new RaycastHit[points.Count];

        // for (int i = 0; i < hits.Length; i++)
        // {
        //         // Debug.Log("Hit Collider at: "+hits[i].collider.transform.position);
        // }


        // var results = new NativeArray<RaycastHit>(points.Count, Allocator.TempJob);
        // var commands = new NativeArray<RaycastCommand>(points.Count, Allocator.TempJob);
        //
        // for (int i = 0; i < points.Count; i++)
        // {
        //     Vector3 myPos = points[i].transform.position;
        //     Vector3 dir = (myPos - sourcePos).normalized;
        //
        //     commands[i] = new RaycastCommand(myPos, dir);
        // }
        //
        // JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1);
        //
        // handle.Complete();
        //
        // var hits = new List<RaycastHit>();
        // foreach (RaycastHit hit in results)
        // {
        //     hits.Add(hit);
        // }
        //
        // results.Dispose();
        // commands.Dispose();
        //
        // if (hits.Count > 0)
        // {
        //     foreach (RaycastHit raycastHit in hits)
        //     {
        //         Debug.DrawRay(sourcePos,
        //             (raycastHit.collider.transform.position - sourcePos).normalized * raycastHit.distance, Color.cyan,
        //             Mathf.Infinity);
        //     }
        // }

        // if (Physics.Raycast(sourcePos, (myPos - sourcePos).normalized, out RaycastHit hit, Mathf.Infinity))
        // {
        //     Debug.DrawRay(sourcePos, (myPos - sourcePos).normalized * hit.distance, Color.cyan, Mathf.Infinity);
        //
        //     if (hit.collider.gameObject.transform.position == myPos)
        //     {
        //         point.GetComponent<Renderer>().material.color = Color.green;
        //     }
        //     else
        //     {
        //         hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
        //         point.GetComponent<Renderer>().material.color = Color.cyan;
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        Vector3 direction = (points[_iterationIndex].transform.position - _lightPos).normalized;

        if (Physics.Raycast(_lightPos, direction, out _hits[_iterationIndex], 20))
        {
            Debug.DrawRay(_lightPos, direction * _hits[_iterationIndex].distance, Color.cyan,
                Mathf.Infinity);
            
        }

        _iterationIndex++;
        if (_iterationIndex == points.Count)
        {
            _iterationIndex = 0;
        }
    }
}