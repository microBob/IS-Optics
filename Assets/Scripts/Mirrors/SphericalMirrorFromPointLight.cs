using System;
using UnityEngine;

namespace Mirrors
{
    public class SphericalMirrorFromPointLight : MonoBehaviour
    {
        public MirrorHandler mirrorHandler;
        public bool runOnce = false;

        private Transform _myTrans;

        private int _reflectionIndex = 0;
        private Vector3 _emissionDir;
        private GameObject _targetMirror;

        private bool _render = true;
        private GameObject _focalPointGameObject;
        private GameObject _imageGameObject;

        // Start is called before the first frame update
        void Start()
        {
            _imageGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _focalPointGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _focalPointGameObject.GetComponent<Renderer>().material.color = Color.yellow;

            _imageGameObject.SetActive(false);
            _focalPointGameObject.SetActive(false);


            if (_reflectionIndex == mirrorHandler.GetMirrors().Count)
            {
                print("Completed all mirrors");
                // _reflected = true;
                enabled = false;
                return;
            }

            _targetMirror = mirrorHandler.GetMirrors()[_reflectionIndex];

            _myTrans = transform;

            // Initial emission
            if (_reflectionIndex == 0)
            {
                _emissionDir = _myTrans.forward;
            }

            print("Working on reflection " + _reflectionIndex);
        }

        private void Update()
        {
            if (_render)
            {
                Render();
            }

            if (runOnce)
            {
                _render = false;
            }
        }

        private void Render()
        {
            // SECTION: identify mirror type (concave vs convex)
            int convexDir = 1;
            if (Physics.Raycast(_myTrans.position, _emissionDir, out RaycastHit typeSeek, Mathf.Infinity,
                LayerMask.GetMask("ShapeSeekCollider")))
            {
                if (typeSeek.collider.gameObject.name.Equals("SphericalMirrorColliderConvex"))
                {
                    convexDir = -1;
                }
            }

            // SECTION: initial cast to find focal point direction
            if (Physics.Raycast(_myTrans.position, _emissionDir, out RaycastHit focalPointSeekHit,
                Mathf.Infinity,
                LayerMask.GetMask("Mirrors")))
            {
                print("Hit the mirror at " + focalPointSeekHit.point);
                Vector3 focalPointSeekNorm = focalPointSeekHit.normal;

                Debug.DrawRay(_myTrans.position, _emissionDir * focalPointSeekHit.distance, Color.cyan);
                Debug.DrawRay(focalPointSeekHit.point, focalPointSeekNorm, Color.red);

                Vector3 focalPointSeekExitDir =
                    Quaternion.AngleAxis(180, focalPointSeekNorm) * -_emissionDir;

                Debug.DrawRay(focalPointSeekHit.point, focalPointSeekExitDir, Color.green);

                print("Convex Dir: " + convexDir);
                // SECTION: second cast to find focal point
                if (Math3d.LinePlaneIntersection(out Vector3 focalPointLoc, focalPointSeekHit.point,
                    convexDir * focalPointSeekExitDir,
                    focalPointSeekHit.collider.gameObject.transform.up,
                    focalPointSeekHit.collider.gameObject.transform.position))
                {
                    print("Hit Axis at " + focalPointLoc);
                    Debug.DrawRay(focalPointSeekHit.point,
                        focalPointSeekExitDir * (convexDir * Vector3.Distance(focalPointSeekHit.point, focalPointLoc)),
                        Color.yellow);

                    if (!_focalPointGameObject.activeSelf)
                    {
                        _focalPointGameObject.SetActive(true);
                    }
                    _focalPointGameObject.transform.position = focalPointLoc;
                    _focalPointGameObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                    // SECTION: third cast to find image point direction
                    Vector3 imageSeekEmissionDir = (focalPointLoc - _myTrans.position).normalized;
                    if (Physics.Raycast(_myTrans.position, imageSeekEmissionDir,
                        out RaycastHit imageSeekHit, Mathf.Infinity, LayerMask.GetMask("Mirrors")))
                    {
                        print("Hit the mirror at " + imageSeekHit.point);
                        Vector3 imageSeekNorm = imageSeekHit.normal;

                        Debug.DrawRay(_myTrans.position, imageSeekEmissionDir * imageSeekHit.distance, Color.cyan);
                        Debug.DrawRay(imageSeekHit.point, imageSeekNorm, Color.red);

                        Vector3 imageSeekExitDir = Quaternion.AngleAxis(180, imageSeekNorm) * -imageSeekEmissionDir;

                        Debug.DrawRay(imageSeekHit.point, imageSeekExitDir, Color.green);

                        // SECTION: check intersection find image point
                        if (Math3d.ClosestPointsOnTwoLines(out Vector3 imagePoint, out Vector3 focalSeekPoint,
                            imageSeekHit.point, convexDir * imageSeekExitDir, focalPointSeekHit.point,
                            convexDir * focalPointSeekExitDir))
                        {
                            print("Image seek point: " + imagePoint + " Focal point seek: " + focalSeekPoint);
                            Debug.DrawLine(imageSeekHit.point, imagePoint, Color.yellow);
                            print("Image located at " + imagePoint);

                            if (!_imageGameObject.activeSelf)
                            {
                                _imageGameObject.SetActive(true);
                            }
                            _imageGameObject.transform.position = imagePoint;
                            _imageGameObject.transform.localScale = _myTrans.localScale;
                        }
                    }
                }
            }
        }

        public void NewInstanceConstructor(int refIndex, Vector3 emiDir, MirrorHandler mirHolder)
        {
            _reflectionIndex = refIndex;
            _emissionDir = emiDir;
            mirrorHandler = mirHolder;
        }
    }
}