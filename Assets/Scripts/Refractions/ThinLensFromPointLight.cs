using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Refractions
{
    public class ThinLensFromPointLight : MonoBehaviour
    {
        public GameObject lens;
        public Vector3 emitDir = Vector3.forward;
        public float locIor = 1f;

        private Vector3 _myPos;
        private float _distToLens;
        private Vector3 _lensContactPoint;
        private float _vertDistFromCenterOfLens;

        private ThinLensDef _thinLensDef;

        private GameObject image;


        // Start is called before the first frame update
        void Start()
        {
            image = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            image.SetActive(false);
            Destroy(image.GetComponent<SphereCollider>());
            image.transform.localScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            _myPos = transform.position;
            _vertDistFromCenterOfLens = _myPos.y - lens.transform.position.y;

            if (Physics.Raycast(_myPos, emitDir, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("First")))
            {
                _distToLens = hit.distance;
                _lensContactPoint = hit.point;

                print("Distance to lens: " + _distToLens + "; contact point: " + _lensContactPoint);

                // Debug.DrawRay(_myPos, emitDir * _distToLens, Color.cyan, Mathf.Infinity);

                lens = hit.collider.gameObject.transform.parent.gameObject;
                _thinLensDef = lens.GetComponent<ThinLensDef>();

                if (!image.activeSelf)
                {
                    image.SetActive(true);
                }

                Render();
            }
            else
            {
                if (image.activeSelf)
                {
                    image.SetActive(false);
                }
            }
        }

        private void Render()
        {
            // Find focal length
            float numerator = locIor * _thinLensDef.radius1 * _thinLensDef.radius2;
            float denominator = (locIor - _thinLensDef.ior) * (_thinLensDef.radius1 - _thinLensDef.radius2);
            float focalLen = numerator / denominator;
            print("Focal Len: " + focalLen);

            // Find image distance
            numerator = _distToLens * focalLen;
            denominator = _distToLens - focalLen;
            float imageDist = numerator / denominator;
            print("Image Distance: " + imageDist);
            // Debug.DrawRay(_lensContactPoint, Vector3.forward * imageDist, Color.green, Mathf.Infinity);

            // Find image height
            numerator = _vertDistFromCenterOfLens * imageDist;
            float imageHeight = -1f * numerator / _distToLens;
            print("Image Height: " + imageHeight);

            // Draw this
            Vector3 imageLoc = lens.transform.position;
            imageLoc += Vector3.forward * imageDist;
            imageLoc += Vector3.up * imageHeight;
            // Debug.DrawLine(_lensContactPoint, imageLoc, Color.magenta, Mathf.Infinity);

            image.transform.position = imageLoc;
        }
    }
}