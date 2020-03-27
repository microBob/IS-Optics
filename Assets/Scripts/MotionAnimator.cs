using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAnimator : MonoBehaviour
{
    private Transform _myTrans;

    private Vector3 _myPos;
    private Vector3 _initPos;

    private float _inc;

    // Start is called before the first frame update
    void Start()
    {
        _myTrans = transform;
        _myPos = _myTrans.position;
        _initPos = _myPos;
    }

    // Update is called once per frame
    void Update()
    {
        float change = 7 * Mathf.Sin(_inc);
        _myTrans.position = new Vector3(_myPos.x, _myPos.y, _initPos.z + change);
        _inc += Time.deltaTime;
    }
}