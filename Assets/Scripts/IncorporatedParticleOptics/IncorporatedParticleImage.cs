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
        public Vector3 HitNormal;

        public ObjectSeekHits(string objName, Vector3 norm)
        {
            ObjName = objName;
            HitNormal = norm;
        }
    }

    public class IncorporatedParticleImage : MonoBehaviour
    {
        //SECTION: Public variables (set stuff in editor)
        public ImageType imageType = ImageType.OriginalObject;
        public GameObject sourceSceneObject; // What object created it (if any)
        public float localIor = 1f;

        //SECTION: Private variables
        // State of particle
        private Status _status = Status.SeekingPoints;
        private bool _postComplete;

        // Storage for render point seeking data and objects in scene
        private readonly List<GameObject> _objectsInScene = new List<GameObject>();
        private readonly List<Vector3> _objectPointsToRender = new List<Vector3>();
        private readonly List<ObjectSeekHits> _objectSeekHits = new List<ObjectSeekHits>();
        private GameObject _curTargetObject;
        private Vector3 _curInteractionPoint;

        private GameObject _seekParticleSystem;

        // private LayerMask _sceneObjectLayerMask;
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
            // _sceneObjectLayerMask = LayerMask.GetMask("SceneObject");
        }

        // Update is called once per frame
        void Update()
        {
            switch (_status)
            {
                case Status.SeekingPoints: // Look for points to render to
                    _postComplete = false;
                    // Change how this is done depending on the image type
                    if (imageType == ImageType.OriginalObject || imageType == ImageType.MirrorRealImage)
                    {
                        // Generate corresponding particle system
                        _seekParticleSystem =
                            Instantiate(Resources.Load<GameObject>("Simulations/SphericalSurfaceSeekPS"));

                        if (_seekParticleSystem != null)
                        {
                            // Set transforms to match this object
                            _seekParticleSystem.transform.position = _myPos;
                            _seekParticleSystem.transform.localScale = transform.localScale;

                            // set it's source to this object
                            SeekParticleSystemHandler seekHandler =
                                _seekParticleSystem.AddComponent<SeekParticleSystemHandler>();
                            seekHandler.myParticleSystem =
                                _seekParticleSystem.GetComponent<ParticleSystem>();
                            seekHandler.sourceLight = gameObject;
                            if (imageType == ImageType.MirrorRealImage)
                            {
                                seekHandler.sourceImageObject = sourceSceneObject;
                            }
                        }
                    }
                    else if (imageType == ImageType.MirrorVirtualImage)
                    {
                        // SECTION: get border verts
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

                        // print("New mesh verts:");
                        // VertListToString(newVerts);

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

                        // print("Tris list:");
                        // IntListToString(tris);

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

                        // SECTION: create particle system and check for points
                        _seekParticleSystem =
                            Instantiate(Resources.Load<GameObject>("Simulations/SemiSphericalSurfaceSeekPS"));

                        if (_seekParticleSystem != null)
                        {
                            Vector3 sourceSceneObjectPos = sourceSceneObject.transform.position;

                            Vector3 sourceSceneObjectForward = sourceSceneObject.transform.forward;
                            Vector3 dirToSourceObject =
                                (sourceSceneObjectPos - _myPos).normalized;
                            float dot = Vector3.Dot(dirToSourceObject, sourceSceneObjectForward);
                            dot = dot > 0 ? 1 : -1;
                            _seekParticleSystem.transform.rotation =
                                Quaternion.LookRotation(sourceSceneObjectForward * dot);

                            _seekParticleSystem.transform.position =
                                sourceSceneObjectPos + sourceSceneObjectForward * (dot * 0.05f);

                            Vector3 sourceMirrorTransformLocalScale = sourceSceneObject.transform.localScale;
                            _seekParticleSystem.transform.localScale = new Vector3(
                                sourceMirrorTransformLocalScale.x / 100,
                                sourceMirrorTransformLocalScale.y / 100,
                                sourceMirrorTransformLocalScale.z / 300);

                            SeekParticleSystemHandler particleSystemHandler =
                                _seekParticleSystem.AddComponent<SeekParticleSystemHandler>();
                            
                            particleSystemHandler.myParticleSystem =
                                _seekParticleSystem.GetComponent<ParticleSystem>();
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
                    _curInteractionPoint = _objectPointsToRender[_objectPointsIndex];
                    print(_myName + ": Rendering point: " + _curInteractionPoint);

                    // Check the type of object, and render
                    if (_curTargetObject.GetComponent<PlaneMirrorDef>() != null)
                    {
                        print(_myName + ": point " + _curInteractionPoint + " is a plane mirror! Rendering...");
                        _status = Status.PlaneMirrorRendering;
                    }
                    else if (_curTargetObject.GetComponent<SphericalMirrorDef>() != null)
                    {
                        print(_myName + ": point " + _curInteractionPoint + " is a spherical mirror! Rendering...");
                        _status = Status.SphericalMirrorRendering;
                    }
                    else if (_curTargetObject.GetComponent<ThinLensDef>() != null)
                    {
                        print(_myName + ": point " + _curInteractionPoint + " is a thin lens! Rendering...");
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

                    print(_myName + ": rendering a plane mirror from " + _myPos + " to " + _curInteractionPoint +
                          " on mirror " + curSeekHit.ObjName);

                    Vector3 mirrorNorm = curSeekHit.HitNormal;

                    Vector3 exitDir = Quaternion.AngleAxis(180, mirrorNorm) *
                                      -(_curInteractionPoint - _myPos).normalized;
                    float imageDistance = Vector3.Distance(_myPos, _curInteractionPoint);

                    Debug.DrawLine(_myPos, _curInteractionPoint, Color.cyan, Mathf.Infinity);
                    Debug.DrawRay(_curInteractionPoint, mirrorNorm, Color.red, Mathf.Infinity);
                    Debug.DrawRay(_curInteractionPoint, exitDir, Color.green, Mathf.Infinity);
                    Debug.DrawRay(_curInteractionPoint, -exitDir * imageDistance, Color.yellow,
                        Mathf.Infinity);

                    Vector3 outputImagePos = _curInteractionPoint - exitDir * imageDistance;

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
                    curSeekHit = _objectSeekHits[_objectPointsIndex];
                    print(_myName + ": rendering a spherical mirror from " + _myPos + " to " + curSeekHit.ObjName);
                    // Get target spherical mirror def
                    SphericalMirrorDef sphericalMirrorDef = _curTargetObject.GetComponent<SphericalMirrorDef>();

                    // Get direction to mirror origin
                    Vector3 rayToOrigin = (sphericalMirrorDef.GetPos() - _myPos).normalized;

                    // Get direction to mirror center of curvature
                    Vector3 rayToCenterOfCurvature = (sphericalMirrorDef.GetCenter() - _myPos).normalized;

                    //// Calculate reflection vector
                    // Get which side object is on
                    Vector3 targetForward = _curTargetObject.transform.forward;
                    float objectDot = Vector3.Dot(targetForward, rayToOrigin);
                    int clampDot = objectDot > 0 ? 1 : -1;

                    // Calculate exit dir
                    Vector3 hitNormal = targetForward * clampDot;

                    exitDir = Quaternion.AngleAxis(180, hitNormal) * -rayToOrigin;

                    // Calculate intersection point between
                    if (Math3d.ClosestPointsOnTwoLines(out Vector3 cp1, out Vector3 cp2, sphericalMirrorDef.GetPos(),
                        exitDir.normalized, _myPos, rayToCenterOfCurvature))
                    {
                        float distanceBetween = Vector3.Distance(cp1, cp2);
                        if (distanceBetween < 0.1f)
                        {
                            Vector3 avgPoint = (cp1 + cp2) / 2;
                            print(_myName + ": Found spherical image loc at " + avgPoint);

                            // Generate new image from point
                            image = Instantiate(Resources.Load<GameObject>("Objects/ParticleImagePoint"),
                                avgPoint,
                                Quaternion.identity);

                            // Give this new virtual image a unique name to be identified by
                            image.name = _myName + " (Virtual image) " + _generatedImageId;
                            _generatedImageId++;

                            incorporatedParticleImage =
                                image.GetComponent<IncorporatedParticleImage>();

                            // Detect if it is a virtual or real image
                            float imagePointDot = Vector3.Dot(targetForward,
                                (sphericalMirrorDef.GetCenter() - avgPoint).normalized);
                            incorporatedParticleImage.imageType = imagePointDot > 0
                                ? ImageType.MirrorRealImage
                                : ImageType.MirrorVirtualImage;

                            incorporatedParticleImage.sourceSceneObject = _curTargetObject;

                            print(_myName + ": created virtual image of " + _curTargetObject.name + " with name " +
                                  image.name);

                            // Return to prerendering for the next point
                            _objectPointsIndex++;
                            _status = Status.PreRendering;
                            print(_myName + ": moving to work on point index " + _objectPointsIndex +
                                  ". going back to prerendering");
                        }
                        else
                        {
                            print(_myName + ": Points are not close! Distance: " + distanceBetween);
                        }
                    }
                    else
                    {
                        print(_myName + "Handle image straight on");
                    }

                    break;
                case Status.ThinLensRendering:
                    curSeekHit = _objectSeekHits[_objectPointsIndex];
                    print(_myName + ": rendering a thin lens from " + _myPos + " to " + curSeekHit.ObjName);

                    // Get target spherical mirror def
                    ThinLensDef thinLensDef = _curTargetObject.GetComponent<ThinLensDef>();

                    // Find focal length
                    float numerator = localIor * thinLensDef.radius1 * thinLensDef.radius2;
                    float denominator = (localIor - thinLensDef.ior) * (thinLensDef.radius1 - thinLensDef.radius2);
                    float focalLen = numerator / denominator;
                    print("Focal Len: " + focalLen);

                    // Find image distance
                    var targetObjectPos = _curTargetObject.transform.position;
                    float lateralDistToLens = Vector2.Distance(new Vector2(_myPos.x, _myPos.z),
                        new Vector2(targetObjectPos.x, targetObjectPos.z));

                    numerator = focalLen * lateralDistToLens;
                    denominator = focalLen - lateralDistToLens;
                    float imageDist = -1f * numerator / denominator;
                    print("Image Distance: " + imageDist);
                    // Debug.DrawRay(_lensContactPoint, Vector3.forward * imageDist, Color.green, Mathf.Infinity);

                    // Find image height
                    float vertDistFromCenterOfLens = _myPos.y - targetObjectPos.y;
                    numerator = vertDistFromCenterOfLens * imageDist;
                    float imageHeight = -1f * numerator / lateralDistToLens;
                    print("Image Height: " + imageHeight);

                    // Draw this
                    Vector3 imageLoc = targetObjectPos;
                    imageLoc += Vector3.forward * imageDist;
                    imageLoc += Vector3.up * imageHeight;


                    // Create Image
                    // Generate new image from point
                    image = Instantiate(Resources.Load<GameObject>("Objects/ParticleImagePoint"),
                        imageLoc,
                        Quaternion.identity);

                    // Give this new virtual image a unique name to be identified by
                    image.name = _myName + " (Virtual image) " + _generatedImageId;
                    _generatedImageId++;

                    incorporatedParticleImage = image.GetComponent<IncorporatedParticleImage>();

                    // Check if virtual or real
                    int myDot = Vector3.Dot(_myPos, targetObjectPos) > 0 ? 1 : -1;
                    int imageDot = Vector3.Dot(imageLoc, targetObjectPos) > 0 ? 1 : -1;

                    incorporatedParticleImage.imageType =
                        myDot != imageDot ? ImageType.ThinLensVirtualImage : ImageType.ThinLensRealImage;

                    // incorporatedParticleImage.sourceSceneObject = _curT argetObject;

                    print(_myName + ": created virtual image of " + _curTargetObject.name + " with name " +
                          image.name);

                    // Return to prerendering for the next point
                    _objectPointsIndex++;
                    _status = Status.PreRendering;
                    print(_myName + ": moving to work on point index " + _objectPointsIndex +
                          ". going back to prerendering");

                    break;
                case Status.Complete:
                    if (!_postComplete)
                    {
                        print(_myName + ": Status is COMPLETE. Cleaning up.");

                        _objectsInScene.Clear();
                        _objectPointsToRender.Clear();
                        _objectSeekHits.Clear();
                        Destroy(_seekParticleSystem);
                        _postComplete = true;
                    }

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

        public void AddObjectsInSceneAndRenderPoints(GameObject sceneObject, Vector3 point)
        {
            _objectsInScene.Add(sceneObject);
            _objectPointsToRender.Add(point);
        }

        public void AddHitObjectNameAndNormal(string objName, Vector3 norm)
        {
            _objectSeekHits.Add(new ObjectSeekHits(objName, norm));
        }

        public void SetStatus(Status status)
        {
            _status = status;
        }
    }
}