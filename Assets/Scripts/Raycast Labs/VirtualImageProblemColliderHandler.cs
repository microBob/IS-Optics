using System;
using UnityEngine;

namespace Raycast_Labs
{
    public class VirtualImageProblemColliderHandler : MonoBehaviour
    {
        public GameObject sourceObject;

        public GameObject targetObject;

        private Rigidbody _collRb;
        private Collider _myCol;

        private bool _once;
        private bool _started;

        private void Start()
        {
            _started = true;
            print("Detecting collisions");

            _myCol = gameObject.GetComponent<Collider>();
            
            _collRb = gameObject.AddComponent<Rigidbody>();
            // _collRb.isKinematic = true;
            _collRb.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (_started)
            {
                Gizmos.DrawWireCube(transform.position,_myCol.bounds.size);
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if (!_once)
            {
                Vector3 sourcePos = sourceObject.transform.position;
                print("Collision occured");
                Vector3 contactPoint = sourcePos;
                print("Number of contacts: " + other.contacts.Length);
                foreach (ContactPoint otherContact in other.contacts)
                {
                    Debug.DrawLine(sourcePos, otherContact.point, Color.magenta, Mathf.Infinity);
                    print("This Collider GO: "+otherContact.thisCollider.gameObject.name);
                    print("Other Collider GO: "+otherContact.otherCollider.gameObject.name);
                    // if (otherContact.otherCollider.gameObject.Equals(targetObject))
                    // {
                    //     contactPoint = otherContact.point;
                    //     break;
                    // }
                }

                // VirtualImageProblem sourceScript = sourceObject.GetComponent<VirtualImageProblem>();
                // if (contactPoint != sourcePos)
                // {
                //     print("Hit target object at " + contactPoint);
                //     Debug.DrawLine(sourcePos, contactPoint, Color.magenta, Mathf.Infinity);
                //     sourceScript.SetGoodPoint(contactPoint);
                //     sourceScript.SetStatusComplete();
                //     print("Self destruct");
                //     // Destroy(gameObject);
                // }
                // else
                // {
                //     print("Failed to hit target");
                //     sourceScript.SetStatusComplete(false);
                // }

                _once = true;
            }
        }
    }
}