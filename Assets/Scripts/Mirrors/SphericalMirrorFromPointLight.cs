using System;
using UnityEngine;

namespace Mirrors
{
    public class SphericalMirrorFromPointLight : MonoBehaviour
    {
        public MirrorHandler mirrorHandler;

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
            // PlaneMirrorBehaviour();
            FindFocalData();
        }

        private void FindFocalData()
        {
            // SECTION: initial cast to find focal point direction
            if (!_reflected && Physics.Raycast(_myTrans.position, _emissionDir, out RaycastHit focalPointSeekHit,
                Mathf.Infinity,
                mirrorHandler.GetMirrorMask(_reflectionIndex)))
            {
                print("Hit the mirror at " + focalPointSeekHit.point);
                Vector3 focalPointSeekNorm = focalPointSeekHit.normal;

                Debug.DrawRay(_myTrans.position, _emissionDir * focalPointSeekHit.distance, Color.cyan, Mathf.Infinity);
                Debug.DrawRay(focalPointSeekHit.point, focalPointSeekNorm, Color.red, Mathf.Infinity);

                Vector3 focalPointSeekExitDir = Quaternion.AngleAxis(180, focalPointSeekNorm) * -_emissionDir;

                Debug.DrawRay(focalPointSeekHit.point, focalPointSeekExitDir, Color.green, Mathf.Infinity);

                // SECTION: second cast to find focal point
                if (Physics.Raycast(focalPointSeekHit.point, -focalPointSeekExitDir, out RaycastHit focalPointHit,
                    Mathf.Infinity,
                    LayerMask.GetMask(
                        "Second")))
                {
                    print("Hit Axis at " + focalPointHit.point);
                    Debug.DrawRay(focalPointSeekHit.point, -focalPointSeekExitDir * focalPointHit.distance,
                        Color.yellow, Mathf.Infinity);

                    GameObject focalPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    focalPoint.GetComponent<Renderer>().material.color = Color.yellow;
                    focalPoint.transform.position = focalPointHit.point;
                    focalPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                    // SECTION: third cast to find image point direction
                    Vector3 imageSeekEmissionDir = (focalPointHit.point - _myTrans.position).normalized;
                    if (Physics.Raycast(_myTrans.position, imageSeekEmissionDir,
                        out RaycastHit imageSeekHit, Mathf.Infinity, mirrorHandler.GetMirrorMask(_reflectionIndex)))
                    {
                        print("Hit the mirror at " + imageSeekHit.point);
                        Vector3 imageSeekNorm = imageSeekHit.normal;

                        Debug.DrawRay(_myTrans.position, imageSeekEmissionDir * imageSeekHit.distance, Color.cyan,
                            Mathf.Infinity);
                        Debug.DrawRay(imageSeekHit.point, imageSeekNorm, Color.red, Mathf.Infinity);

                        Vector3 imageSeekExitDir = Quaternion.AngleAxis(180, imageSeekNorm) * -imageSeekEmissionDir;

                        Debug.DrawRay(imageSeekHit.point, imageSeekExitDir, Color.green, Mathf.Infinity);

                        // SECTION: check intersection find image point
                        if (LineLineIntersection(out Vector3 imagePoint, imageSeekHit.point, -imageSeekExitDir,
                            focalPointSeekHit.point, -focalPointSeekExitDir))
                        {
                            Debug.DrawLine(imageSeekHit.point, imagePoint, Color.yellow, Mathf.Infinity);
                            print("Image located at " + imagePoint);
                            GameObject image = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            image.transform.position = imagePoint;
                            image.transform.localScale = _myTrans.localScale;
                        }
                    }
                }

                _reflected = true;
            }
        }

        private void PlaneMirrorBehaviour()
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

        public void NewInstanceConstructor(int refIndex, Vector3 emiDir, MirrorHandler mirHolder)
        {
            _reflectionIndex = refIndex;
            _emissionDir = emiDir;
            mirrorHandler = mirHolder;
        }

        //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
        //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
        //same plane, use ClosestPointsOnTwoLines() instead.
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1,
            Vector3 linePoint2, Vector3 lineVec2)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1And2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3And2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1And2);

            //is coplanar, and not parrallel
            if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1And2.sqrMagnitude > 0.0001f)
            {
                float s = Vector3.Dot(crossVec3And2, crossVec1And2) / crossVec1And2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }
    }
}