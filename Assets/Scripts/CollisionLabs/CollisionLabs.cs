using System;
using UnityEngine;

namespace CollisionLabs
{
    public class CollisionLabs : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnCollisionEnter(Collision other)
        {
            print("Entered");
        }

        private void OnCollisionStay(Collision other)
        {
            print("Stay "+other.contactCount);
            
        }

        private void OnCollisionExit(Collision other)
        {
            print("Exit");
        }
    }
}
