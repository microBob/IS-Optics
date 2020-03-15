using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneMirror
{
    public class PlaneMirrorFromPointLight : MonoBehaviour
    {
        public PlaneMirrorHandler mirrorHandler;

        private Transform _myTrans;

        private int _reflectionIndex = 0;
        private Vector3 _emissionDir;
        private GameObject _targetMirror;
        private Transform _targetMirrorTrans;

        private bool _reflected = false;

        // Start is called before the first frame update
        void Start()
        {
            if (_reflectionIndex == mirrorHandler.GetMirrors().Count)
            {
                print("Completed all mirrors");
                // _reflected = true;
                enabled = false;
                return;
            }

            _targetMirror = mirrorHandler.GetMirrors()[_reflectionIndex];
            _targetMirrorTrans = _targetMirror.transform;

            _myTrans = transform;

            // Initial emission
            if (_reflectionIndex == 0)
            {
                _emissionDir = _myTrans.forward;
                // _emissionDir = (mirrorHandler.GetMirrors()[3].transform.position - _myTrans.position).normalized;
            }

            print("Working on reflection " + _reflectionIndex);
        }

        private void Update()
        {
            if (!_reflected && Physics.Raycast(_myTrans.position, _emissionDir,
                out RaycastHit hit, Mathf.Infinity, mirrorHandler.GetMirrorMask(_reflectionIndex)))
            {
                print("Hit Mirror " + hit.collider.gameObject.name);

                Vector3 mirrorNorm = hit.normal;

                Debug.DrawRay(_myTrans.position, _emissionDir * hit.distance, Color.cyan, Mathf.Infinity);
                Debug.DrawRay(hit.point, mirrorNorm, Color.red, Mathf.Infinity);

                // float incidentAngle = Vector3.Angle(mirrorNorm, -_emissionDir);

                Vector3 exitDir = Quaternion.AngleAxis(180, mirrorNorm) * -_emissionDir;

                Debug.DrawRay(hit.point, exitDir, Color.green, Mathf.Infinity);
                Debug.DrawRay(hit.point, -exitDir * hit.distance, Color.yellow, Mathf.Infinity);

                Vector3 virtualImagePos = hit.point - exitDir * hit.distance;

                GameObject virtualImage = Instantiate(Resources.Load<GameObject>("Objects/PlaneMirrorPointLight"),
                    virtualImagePos, Quaternion.identity);
                PlaneMirrorFromPointLight virtualImageHandler =
                    virtualImage.GetComponent<PlaneMirrorFromPointLight>();
                virtualImageHandler.NewInstanceConstructor(_reflectionIndex + 1, exitDir, mirrorHandler);

                _reflected = true;
            }
        }

        public void NewInstanceConstructor(int refIndex, Vector3 emiDir, PlaneMirrorHandler mirHolder)
        {
            _reflectionIndex = refIndex;
            _emissionDir = emiDir;
            mirrorHandler = mirHolder;
        }
    }
}