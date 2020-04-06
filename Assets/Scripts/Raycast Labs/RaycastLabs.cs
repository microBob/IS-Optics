using System;
using UnityEngine;

namespace Raycast_Labs
{
    public class RaycastLabs : MonoBehaviour
    {
        public GameObject targetObject;
        public GameObject secondObject;

        private Vector3 _myPos;

        private Vector3 _direction;
        private Vector3 _lastDir;
        private int _layerMask;
        private RaycastHit _hit;
        private int _runs;
        private bool _once;

        // Start is called before the first frame update
        void Start()
        {
            _myPos = transform.position;


            _direction = (secondObject.transform.position - _myPos).normalized;
            _layerMask = LayerMask.GetMask("SceneObject");

            print(_direction);
            _once = false;
            if (Physics.Raycast(_myPos,
                _direction, out _hit, Mathf.Infinity, _layerMask))
            {
                Debug.DrawLine(_myPos, _hit.point, Color.green, Mathf.Infinity);

                if (Physics.Raycast(_hit.point,_direction,out RaycastHit secondHit,Mathf.Infinity,_layerMask))
                {
                    Debug.DrawLine(_hit.point, secondHit.point, Color.red, Mathf.Infinity);

                    _once = secondHit.collider.gameObject.Equals(targetObject);
                }
            }

            if (_once)
            {
                print("Failed");
            }
            else
            {
                print("made it through");
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}