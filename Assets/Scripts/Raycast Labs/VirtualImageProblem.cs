using System;
using System.Collections.Generic;
using System.Linq;
using IncorporatedParticleOptics;
using ParticleLabs;
using UnityEngine;

namespace Raycast_Labs
{
    public enum Status
    {
        SeekingSource,
        SeekingTarget,
        CompletedSuccess,
        CompletedFailed
    }

    public class VirtualImageProblem : MonoBehaviour
    {
        public GameObject sourceMirror;
        public GameObject targetObejct;

        // SECTION: Private var
        private Vector3 _myPos;

        // Test loc
        private Vector3 _knownWorkingPoint;

        // State
        private Status _status;
        private Vector3 _direction;
        private Vector3 _seekOrigin;

        private Vector3 _goodPoint;

        // Start is called before the first frame update
        void Start()
        {
            _status = Status.SeekingSource;
            _myPos = transform.position;
            _knownWorkingPoint = new Vector3(-8, 5, 0);

            // To working point
            _direction = (_knownWorkingPoint - _myPos).normalized;
            _seekOrigin = _myPos;
        }

        // Update is called once per frame
        void Update()
        {
            if (_status == Status.SeekingSource)
            {
                // SECTION: get border verts
                Mesh sourceMirrorMesh = sourceMirror.GetComponent<MeshFilter>().mesh;
                Vector3[] verts = sourceMirrorMesh.vertices;

                print("Full list of Verts (" + verts.Length + "):\n");
                VertListToString(verts);

                // SECTION: only unique
                verts = verts.ToList().Distinct().ToArray();

                print("Only unique verts (" + verts.Length + "):\n");
                VertListToString(verts);

                // SECTION: convert to world space
                for (int i = 0; i < verts.Length; i++)
                {
                    verts[i] = sourceMirror.transform.TransformPoint(verts[i]);
                }

                print("Transformed unique verts (" + verts.Length + "):\n");
                VertListToString(verts);

                // SECTION: Debug with rays
                // foreach (Vector3 vert in verts)
                // {
                //     Debug.DrawRay(_myPos, (vert - _myPos).normalized * 15, Color.cyan, Mathf.Infinity);
                // }

                // SECTION: create seek mesh verts, set 0 -> myPos
                Vector3[] newVerts = new Vector3[verts.Length + 1];

                newVerts[0] = _myPos;

                for (int i = 1; i < newVerts.Length; i++)
                {
                    newVerts[i] = _myPos + (verts[i - 1] - _myPos).normalized * 15;
                }

                print("New mesh verts:");
                VertListToString(newVerts);

                // SECTION: Debug new verts with lines
                // foreach (Vector3 vert in newVerts)
                // {
                //     Debug.DrawLine(_myPos, vert, Color.magenta, Mathf.Infinity);
                // }

                // SECTION: assign triangles from newVerts
                int[] tris = new int[(newVerts.Length - 1) * 3];
                for (int i = 0; i < newVerts.Length - 1; i++)
                {
                    tris[i * 3] = i + 1;

                    if (i + 2 == newVerts.Length)
                    {
                        tris[i * 3 + 1] = 1;
                    }
                    else
                    {
                        tris[i * 3 + 1] = i + 2;
                    }

                    tris[i * 3 + 2] = 0;
                }

                print("Tris list:");
                IntListToString(tris);

                // SECTION: create gameobject from verts and tris
                GameObject seekObject = new GameObject(gameObject.name + "VirtualImageSeekCast");
                Mesh seekMesh = new Mesh();

                seekMesh.vertices = newVerts;
                seekMesh.triangles = tris;
                seekMesh.RecalculateNormals();
                seekMesh.RecalculateBounds();

                // seekObject.AddComponent<MeshRenderer>();

                MeshFilter meshFilter = seekObject.AddComponent<MeshFilter>();
                meshFilter.mesh = seekMesh;

                MeshCollider meshCollider = seekObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                // meshCollider.sharedMesh = seekMesh;

                _status = Status.SeekingTarget;

                // SECTION: create particle system and check for points
                GameObject psObject = Instantiate(Resources.Load<GameObject>("Simulations/ViewableSurfaceSeekPS"));

                if (psObject != null)
                {
                    Vector3 sourceSceneObjectPos = sourceMirror.transform.position;
                    psObject.transform.position = sourceSceneObjectPos;

                    Vector3 sourceSceneObjectForward = sourceMirror.transform.forward;
                    Vector3 dirToSourceObject =
                        (sourceSceneObjectPos - _myPos).normalized;
                    float dot = Vector3.Dot(dirToSourceObject, sourceSceneObjectForward);
                    dot = dot > 0 ? 1 : -1;
                    psObject.transform.rotation =
                        Quaternion.LookRotation(sourceSceneObjectForward * dot);
                    
                    Vector3 sourceMirrorTransformLocalScale = sourceMirror.transform.localScale;
                    psObject.transform.localScale = new Vector3(sourceMirrorTransformLocalScale.x / 100,
                        sourceMirrorTransformLocalScale.y / 100, sourceMirrorTransformLocalScale.z / 300);

                    VirtualImageParticleSystemHandler particleSystemHandler =
                        psObject.GetComponent<VirtualImageParticleSystemHandler>();

                    // particleSystemHandler.targetObject = targetObejct;
                    particleSystemHandler.sourceLight = gameObject;
                    particleSystemHandler.validVolume = meshCollider;
                }


                // VirtualImageProblemColliderHandler colliderHandler =
                //     seekObject.AddComponent<VirtualImageProblemColliderHandler>();
                // colliderHandler.sourceObject = gameObject;
                // colliderHandler.targetObject = targetObejct;
            }
        }

        //SECTION: private functions
        private void VertListToString(Vector3[] list)
        {
            foreach (Vector3 vert in list)
            {
                print("(" + vert.x + ", " + vert.y + ", " + vert.z + ")\n");
            }

            print("\n");
        }

        private void IntListToString(int[] list)
        {
            foreach (int i in list)
            {
                print(i + "\n");
            }

            print("\n");
        }

        //SECTION: public
        public void SetStatusComplete(bool success = true)
        {
            if (success)
            {
                _status = Status.CompletedSuccess;
            }
            else
            {
                _status = Status.CompletedFailed;
            }
        }

        public void SetGoodPoint(Vector3 point)
        {
            _goodPoint = point;
            Debug.DrawLine(_myPos, _goodPoint, Color.magenta, Mathf.Infinity);
            print("Good Point: " + _goodPoint);
        }
    }
}