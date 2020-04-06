using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirrors;
using Refractions;
using UnityEngine;

// ReSharper disable AssignmentInConditionalExpression


public enum Status
{
    Complete,
    Error,
    SeekingPoints,
    VerifyingPoints,
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

public class IncorporatedParticleImage : MonoBehaviour
{
    //SECTION: Public variables (set stuff in editor)
    public ImageType imageType = ImageType.OriginalObject;
    public GameObject sourceSceneObject; // What object created it (if any)

    //SECTION: Private variables
    // State of particle
    private Status _status = Status.SeekingPoints;

    // Storage for render point seeking data and objects in scene
    private List<GameObject> _objectsInScene;
    private readonly List<Vector3> _objectPointsToRender = new List<Vector3>();
    private readonly List<RaycastHit> _interactionRaycastHits = new List<RaycastHit>();
    private GameObject _curTargetObject;
    private Vector3 _curInteractionPoint;
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
                // Get all GameObjects with tag "SceneObject"
                _objectsInScene = GameObject.FindGameObjectsWithTag("SceneObject").ToList();
                print(_myName + ": number of objects: " + _objectsInScene.Count);

                // Iter through, temporarily change MeshCollider to convex to use ClosestPoint function to get approx closest point
                foreach (GameObject o in _objectsInScene)
                {
                    o.GetComponent<MeshCollider>().convex = true;
                    _objectPointsToRender.Add(o.GetComponent<Collider>().ClosestPoint(_myPos));
                    o.GetComponent<MeshCollider>().convex = false;
                }

                // Move forward if there were actually objects in the scene
                if (_objectPointsToRender.Count > 0)
                {
                    _objectPointsIndex = 0;
                    _status = Status.VerifyingPoints;
                    print(_myName + ": Moving to Verifying status");
                }

                break;
            case Status.VerifyingPoints: // check that these points are physically valid
                // Get the appropriate data for the current object indexed
                _curInteractionPoint = _objectPointsToRender[_objectPointsIndex];
                _curTargetObject = _objectsInScene[_objectPointsIndex];
                print(_myName + ": Verifying point: " + _curInteractionPoint);

                // Raycast to see if this exact point is accessible from the object
                bool dontRemoveFlag = Physics.Raycast(_myPos, (_curInteractionPoint - _myPos).normalized,
                    out RaycastHit verifyPointSeek,
                    Mathf.Infinity, _sceneObjectLayerMask);

                if (dontRemoveFlag)
                {
                    // yes, this point on another object is accessible
                    print(_myName + ": Found point " + _curInteractionPoint + " accessible");
                    Debug.DrawRay(_myPos, (_curInteractionPoint - _myPos).normalized * verifyPointSeek.distance,
                        Color.blue);
                    switch (imageType)
                    {
                        case ImageType.OriginalObject:
                            print(_myName + ": is an original object");
                            // check that the new point is the target object
                            if (dontRemoveFlag =
                                verifyPointSeek.collider.gameObject.Equals(_curTargetObject))
                            {
                                // Update points list to the exact point to render with
                                print(_myName + ": Verified render point, moving to point index " +
                                      (_objectPointsIndex + 1));
                                _objectPointsToRender[_objectPointsIndex] = verifyPointSeek.point;
                                _interactionRaycastHits.Add(verifyPointSeek);
                                _objectPointsIndex++;
                            }

                            break;
                        case ImageType.MirrorVirtualImage:
                            print(_myName + ": is a virtual image");
                            // Exit early if target object is source mirror
                            if (sourceSceneObject.Equals(_curTargetObject))
                            {
                                print(_myName + ": point " + _curInteractionPoint +
                                      " is source mirror, moving to next");
                                dontRemoveFlag = false;
                                break;
                            }

                            // check that the new point's hit (will go through) the source mirror
                            if (dontRemoveFlag = verifyPointSeek.collider.gameObject.Equals(sourceSceneObject))
                            {
                                print(_myName + ": point " + _curInteractionPoint + " goes through source mirror");
                                // Raycast a second time from source mirror surface (same target)
                                Vector3 newSeekDir = (_curInteractionPoint - verifyPointSeek.point).normalized;
                                Vector3 pushedSeekOrigin = verifyPointSeek.point + newSeekDir * 0.01f;
                                if (dontRemoveFlag = Physics.Raycast(pushedSeekOrigin,
                                    newSeekDir, out RaycastHit postSourceMirrorPointSeek,
                                    Mathf.Infinity, _sceneObjectLayerMask))
                                {
                                    Debug.DrawLine(pushedSeekOrigin, postSourceMirrorPointSeek.point, Color.red,
                                        Mathf.Infinity);
                                    print(_myName + ": second raycast from " + pushedSeekOrigin + " to " +
                                          postSourceMirrorPointSeek.point);
                                    // Progress forward if the second cast made it to the target (no obstacles)
                                    // 1) ray from source passes through source mirror
                                    // 2) leaving source mirror goes directly to point
                                    if (dontRemoveFlag =
                                        postSourceMirrorPointSeek.collider.gameObject.Equals(
                                            _curTargetObject))
                                    {
                                        print(_myName + ": point " + _curInteractionPoint +
                                              " found actual hit point on target as virutal image");
                                        _objectPointsToRender[_objectPointsIndex] = postSourceMirrorPointSeek.point;
                                        _interactionRaycastHits.Add(postSourceMirrorPointSeek);
                                        _objectPointsIndex++;
                                    }
                                    else
                                    {
                                        print(_myName + ": point " + _curInteractionPoint +
                                              " hit point after source mirror isn't target object. was expecting " +
                                              _curTargetObject.name + " got " +
                                              postSourceMirrorPointSeek.collider.gameObject.name);
                                    }
                                }
                                else
                                {
                                    print(_myName + ": point " + _curInteractionPoint +
                                          "unable to hit after passing source mirror");
                                }
                            }

                            break;
                    }
                }

                // Remove this point and object from the list (it's not interacting with this particle)
                if (!dontRemoveFlag)
                {
                    print(_myName + ": removing point " + _curInteractionPoint);
                    _objectPointsToRender.RemoveAt(_objectPointsIndex);
                    _objectsInScene.RemoveAt(_objectPointsIndex);
                }

                // Once all points are handled, move on to rendering
                if (_objectPointsIndex.Equals(_objectPointsToRender.Count))
                {
                    _objectPointsIndex = 0;
                    _status = Status.PreRendering;
                    print(_myName + ": all points have been verified, moving to pre-rendering");
                }

                break;
            case Status.PreRendering: // collect data and render with appropriate method
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
                RaycastHit curInteractionRaycastHit = _interactionRaycastHits[_objectPointsIndex];
                
                print(_myName + ": rendering a plane mirror from " + _myPos + " to " + _curInteractionPoint +
                      " on mirror " + curInteractionRaycastHit.collider.gameObject.name);

                Vector3 mirrorNorm = curInteractionRaycastHit.normal;

                Vector3 exitDir = Quaternion.AngleAxis(180, mirrorNorm) * -(_curInteractionPoint - _myPos).normalized;
                float imageDistance = Vector3.Distance(_myPos, _curInteractionPoint);

                Debug.DrawLine(_myPos, _curInteractionPoint, Color.cyan, Mathf.Infinity);
                Debug.DrawRay(_curInteractionPoint, mirrorNorm, Color.red, Mathf.Infinity);
                Debug.DrawRay(_curInteractionPoint, exitDir, Color.green, Mathf.Infinity);
                Debug.DrawRay(_curInteractionPoint, -exitDir * imageDistance, Color.yellow,
                    Mathf.Infinity);

                Vector3 outputImagePos = _curInteractionPoint - exitDir * imageDistance;


                // PlaneMirrorRender(_myPos, _curInteractionPoint,
                //     _verifyPointSeek, out Vector3 outputImagePos);

                // GameObject image = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // image.transform.position = outputImagePos;
                // image.transform.localScale = transform.localScale;
                //
                // _status = Status.Idle;

                print(_myName + ": Found image location at " + outputImagePos);

                GameObject image = Instantiate(Resources.Load<GameObject>("Objects/ParticleImagePoint"), outputImagePos,
                    Quaternion.identity);

                // Give this new virtual image a unique name to be identified by
                image.name = _myName + " (Virtual image) " + _generatedImageId;
                _generatedImageId++;

                IncorporatedParticleImage incorporatedParticleImage = image.GetComponent<IncorporatedParticleImage>();
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
                break;
            case Status.ThinLensRendering:
                break;
            case Status.Complete:
                _objectPointsToRender.Clear();
                _interactionRaycastHits.Clear();
                break;
            default:
                print("Paused with status: " + _status);
                break;
        }
    }
}