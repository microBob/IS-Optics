using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;

public enum CamStage
{
    Static,
    HorizontalOrbit
}

public class PresentationCamera : MonoBehaviour
{
    public float flySpeed = 2f;

    public Camera mainCamera;

    private Transform _myTrans;
    private Vector3 _myPos;
    private CamStage _myCamStage;

    private float _defaultDistance;

    // Start is called before the first frame update
    void Start()
    {
        _myTrans = transform;
        _myCamStage = CamStage.Static;
        _defaultDistance = _myTrans.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        float horizInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");
        float yVertInput = Input.GetAxis("YVertical");

        _myCamStage = Input.GetButton("Jump") ? CamStage.HorizontalOrbit : CamStage.Static;

        if (Input.GetKey(KeyCode.R))
        {
            _myTrans.rotation = Quaternion.Euler(Vector3.zero);
            _myTrans.position = new Vector3(0, 0, _defaultDistance);
        }


        switch (_myCamStage)
        {
            case CamStage.Static:
                if (!mainCamera.orthographic)
                {
                    mainCamera.orthographic = true;
                }

                mainCamera.orthographicSize -= yVertInput * Time.deltaTime;

                break;
            case CamStage.HorizontalOrbit:
                if (mainCamera.orthographic)
                {
                    mainCamera.orthographic = false;
                }

                _myTrans.Translate(0, 0, yVertInput * Time.deltaTime);
                _myTrans.Rotate(Vector3.up, -horizInput * Time.deltaTime * flySpeed, Space.World);
                _myTrans.Rotate(Vector3.right, vertInput * Time.deltaTime * flySpeed);
                break;
            default:
                print("Unhandled Camera Stage: " + _myCamStage);
                break;
        }
    }
}