using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamStage
{
    Static,
    HorizontalOrbit
}

public class PresentationCamera : MonoBehaviour
{
    public float flySpeed = 2f;

    public float orbitRadius = 10f;

    public Camera mainCamera;

    private Transform _myTrans;
    private Vector3 _myPos;
    private CamStage _myCamStage;

    private float _timeCounter;

    // Start is called before the first frame update
    void Start()
    {
        _myTrans = transform;
        _myCamStage = CamStage.Static;
    }

    // Update is called once per frame
    void Update()
    {
        float horizInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");

        _myCamStage = Input.GetButton("Jump") ? CamStage.HorizontalOrbit : CamStage.Static;

        if (Input.GetKey(KeyCode.R))
        {
            _myTrans.rotation = Quaternion.Euler(Vector3.zero);
        }

        switch (_myCamStage)
        {
            case CamStage.Static:
                if (!mainCamera.orthographic)
                {
                    mainCamera.orthographic = true;
                }
                break;
            case CamStage.HorizontalOrbit:
                if (mainCamera.orthographic)
                {
                    mainCamera.orthographic = false;
                }
                
                _myTrans.Rotate(Vector3.up, horizInput * Time.deltaTime * flySpeed, Space.World);
                _myTrans.Rotate(Vector3.right, vertInput * Time.deltaTime * flySpeed);
                break;
            default:
                print("Unhandled Camera Stage: " + _myCamStage);
                break;
        }
    }
}