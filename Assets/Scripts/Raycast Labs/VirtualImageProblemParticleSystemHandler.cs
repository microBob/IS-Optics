using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raycast_Labs
{
    public class VirtualImageProblemParticleSystemHandler : MonoBehaviour
    {
        public GameObject targetObject;

        public GameObject sourceLight;
        private VirtualImageProblem _sourceHandler;

        public Collider validVolume;

        public ParticleSystem myParticleSystem;

        private bool _once;

        private void OnParticleCollision(GameObject other)
        {
            print("Particle system has collision!");
            if (other == targetObject)
            {
                print("Particle system collision is with target object!");
                List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
                myParticleSystem.GetCollisionEvents(other, collisionEvents);
                print("There are " + collisionEvents.Count + " hits with target");

                foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
                {
                    if (IsPointWithinCollider(validVolume, particleCollisionEvent.intersection))
                    {
                        Vector3 goodPoint = particleCollisionEvent.intersection;

                        print("Found valid point: " + goodPoint);

                        _sourceHandler.SetGoodPoint(goodPoint);
                        _sourceHandler.SetStatusComplete();
                        Destroy(myParticleSystem);
                        break;
                    }
                }

                _sourceHandler.SetStatusComplete(false);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            print("Spawned Particle system");
            _sourceHandler = sourceLight.GetComponent<VirtualImageProblem>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!_once)
            {
                print("Playing particle system");
                myParticleSystem.Play();

                _once = true;
            }
        }

        // Method created by user Daniel1112
        private bool IsPointWithinCollider(Collider collider, Vector3 point)
        {
            return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
        }
    }
}