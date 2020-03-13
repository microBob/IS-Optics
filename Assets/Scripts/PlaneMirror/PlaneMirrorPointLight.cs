using System;
using UnityEngine;

namespace PlaneMirror
{
    public class PlaneMirrorPointLight : MonoBehaviour
    {
        public PointLight lightSource;
        public int rays = 2;

        private Transform _lightTrans;

        // Start is called before the first frame update
        void Start()
        {
            //Init Variables
            _lightTrans = lightSource.transform;

            _lightTrans.eulerAngles = new Vector3(-45, 0, 0);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 rayDir = _lightTrans.forward;
            if (Physics.Raycast(_lightTrans.position, rayDir, out RaycastHit hit))
            {
                Debug.DrawRay(_lightTrans.position, _lightTrans.forward * hit.distance, Color.cyan);
                Debug.DrawRay(hit.point, hit.normal, Color.red);

                float incidentAngle = 180 - Vector3.Angle(hit.normal, rayDir);
                Vector3 exitDir = hit.normal;
                Vector3 exitAngle = new Vector3(0, Mathf.Sin(incidentAngle * Mathf.Deg2Rad), 0);
                if (hit.point.y < _lightTrans.position.y)
                {
                    exitDir -= exitAngle;
                }
                else
                {
                    exitDir += exitAngle;
                }

                print(hit.normal + "; " + incidentAngle + "; " + exitDir);

                Debug.DrawRay(hit.point, exitDir, Color.green);
                Debug.DrawRay(hit.point, -exitDir * hit.distance, Color.yellow);
                Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), hit.point + (-exitDir * hit.distance),
                    Quaternion.identity);
                
            }
        }
    }
}