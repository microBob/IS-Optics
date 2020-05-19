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

    private Transform _myTrans;
    private Vector3 _myPos;
    private CamStage _myCamStage;

    private float _timeCounter;
    // Start is called before the first frame update
    void Start()
    {
        _myTrans = transform;
        _myCamStage = CamStage.HorizontalOrbit;
    }

    // Update is called once per frame
    void Update()
    {
        _myPos = _myTrans.position;
        
        switch (_myCamStage)
        {
            case CamStage.Static:
                break;
            case CamStage.HorizontalOrbit:
                _myTrans.position = Vector3.zero;
                _myTrans.Rotate(Vector3.up, Time.deltaTime * flySpeed);
                _myTrans.Translate(0,0,-10);
                break;
            default:
                print("Unhandled Camera Stage: "+_myCamStage);
                break;
        }
    }
}
