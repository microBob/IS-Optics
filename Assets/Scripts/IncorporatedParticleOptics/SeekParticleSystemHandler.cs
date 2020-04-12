using System;
using System.Collections.Generic;
using Mirrors;
using Raycast_Labs;
using UnityEngine;

namespace IncorporatedParticleOptics
{
    public class SeekParticleSystemHandler : MonoBehaviour
    {
        public GameObject sourceImageObject;

        public GameObject sourceLight;

        private IncorporatedParticleImage _sourceHandler;
        // private VirtualImageProblem _sourceHandlerTest;

        public Collider validVolume;

        public ParticleSystem myParticleSystem;

        private bool _once;

        private int _roundsOfWaiting;

        private void OnParticleCollision(GameObject other)
        {
            print("Particle system has collision!");

            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            myParticleSystem.GetCollisionEvents(other, collisionEvents);
            print("There are " + collisionEvents.Count + " hits with target");

            foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
            {
                // Gather Data
                Vector3 curPoint = particleCollisionEvent.intersection;
                Vector3 pointNormal = particleCollisionEvent.normal;

                // SECTION: Validity test

                // returns zero if it can't find a point
                if (curPoint.Equals(Vector3.zero))
                {
                    print("Removed point for being 0,0,0");
                    continue;
                }

                // break out if this object is the source mirror
                if (sourceImageObject != null && other.Equals(sourceImageObject))
                {
                    print("This is the source mirror (stopping)");
                    break;
                }

                // skip if this point doesn't fit inside the viewable cone
                if (validVolume != null && !IsPointWithinCollider(validVolume, curPoint))
                {
                    print("Point is not in valid volume");
                    continue;
                }

                // skip if normal is not aligned with plane mirror
                if (other.GetComponent<PlaneMirrorDef>() != null)
                {
                    Vector3 normDir = (pointNormal - curPoint).normalized;
                    float dot = Vector3.Dot(other.transform.forward, normDir);

                    // remove point if greater than tolerance of 0.5
                    if (Mathf.Abs(dot) < 0.5)
                    {
                        print("Normal with mirror is not aligned with face");
                        continue;
                    }
                }


                print("Found valid point: " + curPoint);

                _sourceHandler.AddObjectsInSceneAndRenderPoints(other, curPoint);
                _sourceHandler.AddHitObjectNameAndNormal(other.name, pointNormal);
                break;
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
                if (_sourceHandler.GetSceneObjects().Count > 0)
                {
                    //TODO: detect when all collisions are done
                    _sourceHandler.SetStatus(Status.PreRendering);
                }
                else
                {
                    _roundsOfWaiting++;
                    if (_roundsOfWaiting == 10)
                    {
                        _sourceHandler.SetStatus(Status.Complete);
                    }
                }
            }
        }

        // Method created by user Daniel1112
        private bool IsPointWithinCollider(Collider col, Vector3 point)
        {
            return (col.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
        }
    }
}