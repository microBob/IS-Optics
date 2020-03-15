using UnityEngine;

namespace PlaneMirror
{
    public class PlaneMirrorUsingPointLightStraight : MonoBehaviour
    {
        public PointLight lightSource;

        public bool animate = true;
        public int animSpeed = 3;
        private int _animDir = 1;

        private GameObject _virtualImage;

        private Transform _lightTrans;
        private Transform _myTrans;

        private Vector3 _myNormal;

        // Start is called before the first frame update
        void Start()
        {
            _lightTrans = lightSource.transform;
            _myTrans = transform;

            _virtualImage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _virtualImage.SetActive(false);
            _virtualImage.transform.localScale = _lightTrans.localScale;

            _myNormal = _myTrans.TransformPoint(GetComponent<MeshFilter>().mesh.normals[0]);
            Debug.DrawRay(_myTrans.position, _myNormal, Color.red, Mathf.Infinity);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 lightPos = _lightTrans.position;
            if (Physics.Raycast(lightPos, -_myNormal, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 hitPos = hit.point;
                float hitDist = hit.distance;

                Debug.DrawRay(lightPos, -_myNormal * hitDist, Color.cyan);

                Vector3 vertLoc = hitPos + -_myNormal * hitDist;
                Debug.DrawLine(hitPos, vertLoc, Color.green);

                if (!_virtualImage.activeSelf)
                {
                    _virtualImage.SetActive(true);
                }

                _virtualImage.transform.position = vertLoc;
            }
            else
            {
                if (_virtualImage.activeSelf)
                {
                    _virtualImage.SetActive(false);
                }
            }

            if (animate)
            {
                _lightTrans.Translate(0, Time.deltaTime * animSpeed * _animDir, 0);
                if (_lightTrans.position.y > 5)
                {
                    _animDir = -1;
                } else if (_lightTrans.position.y < -5)
                {
                    _animDir = 1;
                }
            }
        }
    }
}