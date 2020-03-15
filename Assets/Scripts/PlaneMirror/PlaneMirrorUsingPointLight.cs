using System;
using UnityEngine;

namespace PlaneMirror
{
    public class PlaneMirrorUsingPointLight : MonoBehaviour
    {
        public PointLight lightSource;
        public int rays = 2;

        private Transform _lightTrans;
        private GameObject _virtualImage;

        private int _animDir = 1;

        // Start is called before the first frame update
        void Start()
        {
            //Init Variables
            _lightTrans = lightSource.transform;
            _virtualImage = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            _lightTrans.eulerAngles = new Vector3(-45, 0, 0);
            _virtualImage.SetActive(false);
            _virtualImage.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 rayDir = _lightTrans.forward;
            if (Physics.Raycast(_lightTrans.position, rayDir, out RaycastHit hit))
            {
                // Used hit data
                Vector3 mirrorNorm = hit.normal;
                Vector3 rayHitPoint = hit.point;
                float rayDistance = hit.distance;

                // print("Hit distance: " + rayDistance);

                // Show incident ray
                Debug.DrawRay(_lightTrans.position, _lightTrans.forward * rayDistance, Color.cyan);

                // Show mirror normal
                Debug.DrawRay(rayHitPoint, mirrorNorm, Color.red);

                // Calculate exit angle
                float incidentAngle = 180 - Vector3.Angle(mirrorNorm, rayDir);
                Vector3 exitDir = mirrorNorm;
                Vector3 exitAngle = new Vector3(0, Mathf.Sin(incidentAngle * Mathf.Deg2Rad), 0);
                if (rayHitPoint.y < _lightTrans.position.y)
                {
                    exitDir -= exitAngle;
                }
                else
                {
                    exitDir += exitAngle;
                }

                // print(exitDir + ", " + exitDir.normalized);

                Debug.DrawRay(rayHitPoint, exitDir.normalized, Color.green);
                Debug.DrawRay(rayHitPoint, -exitDir.normalized * hit.distance, Color.yellow, 3);

                if (!_virtualImage.activeSelf)
                {
                    _virtualImage.SetActive(true);
                }

                _virtualImage.transform.position = rayHitPoint + -exitDir.normalized * hit.distance;
            }

            float xAngle = _lightTrans.eulerAngles.x;
            print(xAngle);
            if (xAngle > 45)
            {
                _animDir = -1;
                xAngle = 45;
            }
            else if (xAngle < -45)
            {
                _animDir = 1;
                xAngle = -45;
            }

            _lightTrans.eulerAngles = new Vector3(xAngle + Time.deltaTime * 3 * _animDir, 0, 0);
        }
    }
}