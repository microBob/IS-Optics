using UnityEngine;

namespace Mirrors
{
    public class SphericalMirrorDef : MonoBehaviour
    {
        public float curveCoefficient;

        private Vector3 _myPos;
        
        // Start is called before the first frame update
        void Start()
        {
            _myPos = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        public float GetRadius()
        {
            return curveCoefficient * 2;
        }
        
        public Vector3 GetPos()
        {
            return _myPos;
        }
        
        public Vector3 GetCenter()
        {
            return _myPos + transform.forward * GetRadius();
        }
    }
}
