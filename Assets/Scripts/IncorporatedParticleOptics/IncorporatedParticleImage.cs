using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirrors;
using ParticleLabs;
using Raycast_Labs;
using Refractions;
using UnityEditor;
using UnityEngine;

// ReSharper disable AssignmentInConditionalExpression

namespace IncorporatedParticleOptics
{
    public enum Status
    {
        Complete,
        Error,
        SeekingPoints,
        WaitingForPointSeek,
        PreRendering,
        PlaneMirrorRendering,
        SphericalMirrorRendering,
        ThinLensRendering
    }

    public enum ImageType
    {
        OriginalObject,
        MirrorVirtualImage,
        MirrorRealImage,
        ThinLensVirtualImage,
        ThinLensRealImage
    }

    public struct ObjectSeekHits
    {
        public readonly string ObjName;
        public List<Vector3> HitNormals;

        public ObjectSeekHits(string objName, List<Vector3> norms)
        {
            ObjName = objName;
            HitNormals = norms;
        }
    }

    public class IncorporatedParticleImage : MonoBehaviour
    {
        //SECTION: Public variables (set stuff in editor)
        public ImageType imageType = ImageType.OriginalObject;
        public GameObject sourceSceneObject; // What object created it (if any)

        //SECTION: Private variables
        // State of particle
        private Status _status = Status.SeekingPoints;

        // Storage for render point seeking data and objects in scene
        private readonly List<GameObject> _objectsInScene = new List<GameObject>();
        private readonly List<List<Vector3>> _objectPointsToRender = new List<List<Vector3>>();
        private readonly List<ObjectSeekHits> _objectSeekHits = new List<ObjectSeekHits>();
        private GameObject _curTargetObject;
        private List<Vector3> _curInteractionPoints;
        private GameObject _seekParticleSystem;
        private LayerMask _sceneObjectLayerMask;
        private int _objectPointsIndex;

        // ID services
        private Vector3 _myPos;
        private String _myName;
        private int _generatedImageId;

        // Start is called before the first frame update
        void Start()
        {
            _myPos = transform.position;
            _myName = gameObject.name;
            _sceneObjectLayerMask = LayerMask.GetMask("SceneObject");
        }

        // Update is called once per frame
        void Update()
        {
            switch (_status)
            {
                case Status.SeekingPoints: // Look for points to render to
                    // Change how this is done depending on the image type
                    if (imageType == ImageType.OriginalObject || imageType == ImageType.MirrorRealImage)
                    {
                        // Generate corresponding particle system
                        _seekParticleSystem =
                            Instantiate(Resources.Load<GameObject>("Simulations/OriginalObjectSurfaceSeekPS"));

                        if (_seekParticleSystem != null)
                        {
                            // Set transforms to match this object
                            _seekParticleSystem.transform.position = _myPos;
                            _seekParticleSystem.transform.localScale = transform.localScale;

                            // set it's source to this object
                            SeekParticleSystemHandler seekHandler =
                                _seekParticleSystem.GetComponent<SeekParticleSystemHandler>();
                            seekHandler.sourceLight = gameObject;
                        }
                    }
                    else if (imageType == ImageType.MirrorVirtualImage)
                    {
                        // Get border verts
                        Mesh sourceMirrorMesh = sourceSceneObject.GetComponent<MeshFilter>().mesh;
                        Vector3[] verts = sourceMirrorMesh.vertices;

                        // print("Full list of Verts (" + verts.Length + "):\n");
                        // VertListToString(verts);

                        // SECTION: only unique
                        verts = verts.ToList().Distinct().ToArray();

                        // print("Only unique verts (" + verts.Length + "):\n");
                        // VertListToString(verts);

                        // SECTION: convert to world space
                        for (int i = 0; i < verts.Length; i++)
                        {
                            verts[i] = sourceSceneObject.transform.TransformPoint(verts[i]);
                        }

                        // print("Transformed unique verts (" + verts.Length + "):\n");
                        // VertListToString(verts);

                        // create seek mesh verts, set 0 -> myPos
                        Vector3[] newVerts = new Vector3[verts.Length + 1];

                        newVerts[0] = _myPos;

                        for (int i = 1; i < newVerts.Length; i++)
                        {
                            newVerts[i] = _myPos + (verts[i - 1] - _myPos).normalized * 15;
                        }

                        // print("New mesh verts:");
                        // VertListToString(newVerts);

                        // assign triangles from newVerts
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

                        // print("Tris list:");
                        // IntListToString(tris);

                        // create gameobject from verts and tris
                        GameObject seekObject = new GameObject(gameObject.name + "VirtualImageSeekCast");
                        Mesh seekMesh = new Mesh();

                        seekMesh.vertices = newVerts;
                        seekMesh.triangles = tris;
                        seekMesh.RecalculateNormals();
                        seekMesh.RecalculateBounds();

                        MeshFilter meshFilter = seekObject.AddComponent<MeshFilter>();
                        meshFilter.mesh = seekMesh;

                        MeshCollider meshCollider = seekObject.AddComponent<MeshCollider>();
                        meshCollider.convex = true;

                        // create particle system and check for points
                        _seekParticleSystem =
                            Instantiate(Resources.Load<GameObject>("Simulations/ViewableSurfaceSeekPS"));

                        if (_seekParticleSystem != null)
                        {
                            Vector3 sourceSceneObjectPos = sourceSceneObject.transform.position;
                            _seekParticleSystem.transform.position = sourceSceneObjectPos;

                            Vector3 sourceSceneObjectForward = sourceSceneObject.transform.forward;
                            Vector3 dirToSourceObject =
                                (sourceSceneObjectPos - _myPos).normalized;
                            float dot = Vector3.Dot(dirToSourceObject, sourceSceneObjectForward);
                            dot = dot > 0 ? 1 : -1;
                            _seekParticleSystem.transform.rotation =
                                Quaternion.LookRotation(sourceSceneObjectForward * dot);

                            Vector3 sourceMirrorTransformLocalScale = sourceSceneObject.transform.localScale;
                            _seekParticleSystem.transform.localScale = new Vector3(
                                sourceMirrorTransformLocalScale.x / 100,
                                sourceMirrorTransformLocalScale.y / 100,
                                sourceMirrorTransformLocalScale.z / 300);

                            SeekParticleSystemHandler particleSystemHandler =
                                _seekParticleSystem.GetComponent<SeekParticleSystemHandler>();

                            particleSystemHandler.sourceLight = gameObject;
                            particleSystemHandler.sourceImageObject = sourceSceneObject;
                            particleSystemHandler.validVolume = meshCollider;
                        }
                    }
                    else
                    {
                        print(_myName + ": has unknown imageType: " + imageType);
                    }

                    _status = Status.WaitingForPointSeek;
                    _objectPointsIndex = 0;

                    break;
                case Status.PreRendering: // collect data and render with appropriate method
                    // Destroy seek particle system (it's done with its work)
                    Destroy(_seekParticleSystem);
                    // Complete this object's job is it is done
                    if (_objectPointsIndex.Equals(_objectPointsToRender.Count))
                    {
                        print(_myName + ": completed all points to render! Status is now COMPLETE");
                        _status = Status.Complete;
                        break;
                    }

                    // Get the appropriate data for the current object indexed
                    _curTargetObject = _objectsInScene[_objectPointsIndex];
                    _curInteractionPoints = _objectPointsToRender[_objectPointsIndex];
                    print(_myName + ": Rendering point: " + _curInteractionPoints);

                    // Check the type of object, and render
                    if (_curTargetObject.GetComponent<PlaneMirrorDef>() != null)
                    {
                        print(_myName + ": point " + _curInteractionPoints + " is a plane mirror! Rendering...");
                        _status = Status.PlaneMirrorRendering;
                    }
                    else if (_curTargetObject.GetComponent<SphericalMirrorDef>() != null)
                    {
                        print(_myName + ": point " + _curInteractionPoints + " is a spherical mirror! Rendering...");
                        _status = Status.SphericalMirrorRendering;
                    }
                    else if (_curTargetObject.GetComponent<ThinLensDef>() != null)
                    {
                        print(_myName + ": point " + _curInteractionPoints + " is a thin lens! Rendering...");
                        _status = Status.ThinLensRendering;
                    }
                    else // WTF?
                    {
                        print(_myName + ": Encountered unknown object: " + _curTargetObject.name);
                        _curTargetObject.GetComponent<Renderer>().material.color = Color.red;
                    }

                    break;
                case Status.PlaneMirrorRendering:
                    ObjectSeekHits curSeekHit = _objectSeekHits[_objectPointsIndex];
                    Vector3 curInteractionVector3 = _curInteractionPoints[0];

                    print(_myName + ": rendering a plane mirror from " + _myPos + " to " + _curInteractionPoints +
                          " on mirror " + curSeekHit.ObjName);

                    Vector3 mirrorNorm = curSeekHit.HitNormals[0];

                    Vector3 exitDir = Quaternion.AngleAxis(180, mirrorNorm) *
                                      -(curInteractionVector3 - _myPos).normalized;
                    float imageDistance = Vector3.Distance(_myPos, curInteractionVector3);

                    Debug.DrawLine(_myPos, curInteractionVector3, Color.cyan, Mathf.Infinity);
                    Debug.DrawRay(curInteractionVector3, mirrorNorm, Color.red, Mathf.Infinity);
                    Debug.DrawRay(curInteractionVector3, exitDir, Color.green, Mathf.Infinity);
                    Debug.DrawRay(curInteractionVector3, -exitDir * imageDistance, Color.yellow,
                        Mathf.Infinity);

                    Vector3 outputImagePos = curInteractionVector3 - exitDir * imageDistance;

                    print(_myName + ": Found image location at " + outputImagePos);

                    GameObject image = Instantiate(Resources.Load<GameObject>("Objects/ParticleImagePoint"),
                        outputImagePos,
                        Quaternion.identity);

                    // Give this new virtual image a unique name to be identified by
                    image.name = _myName + " (Virtual image) " + _generatedImageId;
                    _generatedImageId++;

                    IncorporatedParticleImage incorporatedParticleImage =
                        image.GetComponent<IncorporatedParticleImage>();
                    incorporatedParticleImage.imageType = ImageType.MirrorVirtualImage;
                    incorporatedParticleImage.sourceSceneObject = _curTargetObject;

                    print(_myName + ": created virtual image of " + _curTargetObject.name + " with name " + image.name);

                    // Return to prerendering for the next point
                    _objectPointsIndex++;
                    _status = Status.PreRendering;
                    print(_myName + ": moving to work on point index " + _objectPointsIndex +
                          ". going back to prerendering");

                    break;
                case Status.SphericalMirrorRendering:
                    ObjectSeekHits curSeekHits = _objectSeekHits[_objectPointsIndex];

                    print(_myName + ": rendering a spherical mirror from " + _myPos + " to " +
                          _curInteractionPoints[0] +
                          " on mirror " + curSeekHits.ObjName);

                    Vector3[] lineOuts = new Vector3[2];
                    for (int i = 0; i < 2; i++)
                    {
                        mirrorNorm = curSeekHits.HitNormals[i];
                        lineOuts[i] = Quaternion.AngleAxis(180, mirrorNorm) *
                                      -(_curInteractionPoints[i] - _myPos).normalized;

                        // Draw rays to visualize
                        Debug.DrawLine(_myPos, _curInteractionPoints[i], Color.cyan, Mathf.Infinity);
                        Debug.DrawRay(_curInteractionPoints[i], mirrorNorm, Color.red, Mathf.Infinity);
                        Debug.DrawRay(_curInteractionPoints[i], lineOuts[i], Color.green, Mathf.Infinity);
                    }

                    // Check for intersection
                    if (Math3d.ClosestPointsOnTwoLines(out Vector3 cp1, out Vector3 cp2, _curInteractionPoints[0],
                        lineOuts[0], _curInteractionPoints[1], lineOuts[1]))
                    {
                        if (Vector3.Distance(cp1, cp2) < 0.1f)
                        {
                            Vector3 avgVec3 = (cp1 + cp2) / 2;
                            print(_myName + ": Found image location at " + avgVec3);


                            image = Instantiate(Resources.Load<GameObject>("Objects/ParticleImagePoint"),
                                avgVec3,
                                Quaternion.identity);

                            // Give this new virtual image a unique name to be identified by
                            image.name = _myName + " (Virtual image) " + _generatedImageId;
                            _generatedImageId++;

                            incorporatedParticleImage =
                                image.GetComponent<IncorporatedParticleImage>();

                            // Calculate if this is a virtual image (opposite side of mirror) or real (same side)
                            Vector3 targetPos = _curTargetObject.transform.position;
                            int sourceDot = Vector3.Dot(_myPos, targetPos) > 0 ? 1 : -1;
                            int imageDot = Vector3.Dot(avgVec3, targetPos) > 0 ? 1 : -1;

                            if (sourceDot != imageDot)
                            {
                                incorporatedParticleImage.imageType = ImageType.MirrorVirtualImage;
                            }
                            else
                            {
                                incorporatedParticleImage.imageType = ImageType.MirrorRealImage;
                            }

                            incorporatedParticleImage.sourceSceneObject = _curTargetObject;

                            print(_myName + ": created virtual image of " + _curTargetObject.name + " with name " +
                                  image.name);
                        }
                        else
                        {
                            print("Lines to not interset well. Distance: " + Vector3.Distance(cp1, cp2));
                        }
                    }
                    else
                    {
                        print("failed to find intersection point");
                    }

                    break;
                case Status.ThinLensRendering:
                    break;
                case Status.Complete:
                    _objectPointsToRender.Clear();
                    _objectSeekHits.Clear();
                    break;
                default:
                    print("Paused with status: " + _status);
                    break;
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

        // SECTION: public functions
        public List<GameObject> GetSceneObjects()
        {
            return _objectsInScene;
        }

        public void AddObjectsInSceneAndRenderPoints(GameObject sceneObject, List<Vector3> points)
        {
            _objectsInScene.Add(sceneObject);
            _objectPointsToRender.Add(points);
        }

        public void AddHitObjectNameAndNormal(string objName, List<Vector3> norms)
        {
            _objectSeekHits.Add(new ObjectSeekHits(objName, norms));
        }

        public void SetStatus(Status status)
        {
            _status = status;
        }
    }
}