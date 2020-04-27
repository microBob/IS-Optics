using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refractions
{
    public class RefractionSeekParticleSystemHandler : MonoBehaviour
    {
        public GameObject sourceLight;

        private RefractionBlockFromPointLight _sourceHandler;

        public ParticleSystem myParticleSystem;

        public int numOfSamples = 1;
        private int _samplesTaken = 0;

        private bool _once;
        private int _roundsOfWaiting;

        private void OnParticleCollision(GameObject other)
        {
            print("Refraction particle system as collision");

            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            myParticleSystem.GetCollisionEvents(other, collisionEvents);
            print("There are " + collisionEvents.Count + " hits with target");

            foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
            {
                // Gather Data
                Vector3 curPoint = particleCollisionEvent.intersection;
                Vector3 pointNormal = particleCollisionEvent.normal;
                
                // SECTION: validity tests
                if (curPoint.Equals(Vector3.zero))
                {
                    print("Removed point for being (0,0,0)");
                    continue;
                }

                print("Found valid point");
                _sourceHandler.AddHitData(other, curPoint, pointNormal);
                _samplesTaken++;
                if (_samplesTaken == numOfSamples)
                {
                    break;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            print("Spawned Refraction Particle System");
            _sourceHandler = sourceLight.GetComponent<RefractionBlockFromPointLight>();
            
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
                if (_sourceHandler.GetRenderPoints().Count > 0)
                {
                    _sourceHandler.SetStatus(RefractionCalculationStatus.Drawing);
                }
                else
                {
                    _roundsOfWaiting++;
                    if (_roundsOfWaiting == 10)
                    {
                        _sourceHandler.SetStatus(RefractionCalculationStatus.Complete);

                    }
                }

            }
        }
    }
}
