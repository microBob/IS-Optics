using UnityEngine;

namespace Diffraction
{
    public class YoungSource : MonoBehaviour
    {
        public GameObject otherSource;

        public GameObject targetScreen;

        public float wavelength = 500;
        public float initIntensity = 1;

        public float drawResolution = 0.5f;
        public Vector3 emmitDir = Vector3.right;

        // SECTION: private vars
        private Transform _myTransform;
        private Vector3 _myPos;
        private Vector3 _otherPos;
        private Vector3 _midPoint;

        private int _m;
        private float _d;
        private float _D;

        // Start is called before the first frame update
        void Start()
        {
            _myTransform = transform;
            _myPos = _myTransform.position;
            _otherPos = otherSource.transform.position;
            _myTransform.forward = emmitDir;
            _midPoint = (_otherPos + _myPos) / 2;

            // Convert Wavelength to meters
            wavelength *= Mathf.Pow(10f, -9f);

            // calculate m and d (downscale by 1000)
            _d = Vector3.Distance(_myPos, _otherPos) * Mathf.Pow(10, -3);
            _m = Mathf.FloorToInt(_d / wavelength);
            print("Highest order constructive interference: " + _m);

            // Draw ym locations
            _D = Vector3.Distance(_midPoint, targetScreen.transform.position);
            for (float m = -_m; m < _m; m += drawResolution)
            {
                float y = m * wavelength * _D / _d;
                Vector3 constructivePoint = targetScreen.transform.position + y * targetScreen.transform.up;

                // Calculating intensity
                float phaseDiff = _d * y / Vector3.Distance(_myPos, constructivePoint) / wavelength * (Mathf.PI * 2);
                float intensity = 4f * initIntensity * Mathf.Pow(Mathf.Cos(phaseDiff / 2f), 2);

                // Drawing point
                print("With Intensity: " + intensity);
                print("Drawing constructive point at " + constructivePoint);
                // Debug.DrawLine(_midPoint, constructivePoint, Color.cyan, Mathf.Infinity);
                Debug.DrawRay(constructivePoint, Vector3.left * intensity, Color.cyan, Mathf.Infinity);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        // SECTION: Private functions
        // SECTION: Public Get
        // SECTION: Public Set
    }
}