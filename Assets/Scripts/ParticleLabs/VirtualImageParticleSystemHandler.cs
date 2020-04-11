using System.Collections.Generic;
using IncorporatedParticleOptics;
using UnityEngine;
using Status = IncorporatedParticleOptics.Status;

namespace ParticleLabs
{
    public class VirtualImageParticleSystemHandler : MonoBehaviour
    {
        
        public GameObject sourceLight;

        private IncorporatedParticleImage _sourceHandler;
        // private VirtualImageProblem _sourceHandlerTest;

        public Collider validVolume;

        public ParticleSystem myParticleSystem;

        private bool _once;

        private void OnParticleCollision(GameObject other)
        {
            print("Particle system has collision!");

            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            myParticleSystem.GetCollisionEvents(other, collisionEvents);
            print("There are " + collisionEvents.Count + " hits with target");

            foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
            {
                if (IsPointWithinCollider(validVolume, particleCollisionEvent.intersection))
                {
                    Vector3 goodPoint = particleCollisionEvent.intersection;

                    print("Found valid point: " + goodPoint);

                    _sourceHandler.AddObjectsInSceneAndRenderPoints(other, goodPoint);

                    // _sourceHandlerTest.SetGoodPoint(goodPoint);
                    // _sourceHandlerTest.SetStatusComplete();
                    // Destroy(myParticleSystem);
                    break;
                }
            }

            // _sourceHandler.SetStatusComplete(false);
            // _sourceHandlerTest.SetStatusComplete(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            print("Spawned Particle system");
            _sourceHandler = sourceLight.GetComponent<IncorporatedParticleImage>();
            // _sourceHandlerTest = sourceLight.GetComponent<VirtualImageProblem>();
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

            if (!myParticleSystem.IsAlive())
            {
                //TODO: detect when all collisions are done
                _sourceHandler.SetStatus(Status.PreRendering);
                
            }
        }

        // Method created by user Daniel1112
        private bool IsPointWithinCollider(Collider collider, Vector3 point)
        {
            return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
        }
    }
}