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

        private void OnParticleCollision(GameObject other)
        {
            print("Particle system has collision!");

            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
            myParticleSystem.GetCollisionEvents(other, collisionEvents);
            print("There are " + collisionEvents.Count + " hits with target");

            List<Vector3> pointsToSend = new List<Vector3>();
            List<Vector3> normsToSend = new List<Vector3>();

            foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
            {
                // Gather Data
                Vector3 curPoint = particleCollisionEvent.intersection;
                Vector3 pointNormal = particleCollisionEvent.normal;

                // SECTION: Validity test

                // returns zero if it can't find a point
                if (curPoint.Equals(Vector3.zero))
                {
                    continue;
                }

                if (validVolume != null)
                {
                    if (other.Equals(sourceImageObject))
                    {
                        break;
                    }

                    if (!IsPointWithinCollider(validVolume, curPoint))
                    {
                        continue;
                    }
                }


                print("Found valid point: " + curPoint);

                pointsToSend.Add(curPoint);
                normsToSend.Add(pointNormal);

                // PlaneMirrors only need one points and norm
                if (other.GetComponent<PlaneMirrorDef>() != null)
                {
                    break;
                }

                // Spherical mirrors need two before exiting
                if (other.GetComponent<SphericalMirrorDef>() != null && pointsToSend.Count.Equals(2))
                {
                    break;
                }
            }

            if (pointsToSend.Count > 0)
            {
                _sourceHandler.AddObjectsInSceneAndRenderPoints(other, pointsToSend);
                _sourceHandler.AddHitObjectNameAndNormal(other.name, normsToSend);
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

            if (!myParticleSystem.IsAlive() && _sourceHandler.GetSceneObjects().Count > 0)
            {
                //TODO: detect when all collisions are done
                _sourceHandler.SetStatus(Status.PreRendering);
            }
        }

        // Method created by user Daniel1112
        private bool IsPointWithinCollider(Collider col, Vector3 point)
        {
            return (col.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
        }
    }
}