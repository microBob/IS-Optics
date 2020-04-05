using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirrors;
using Refractions;
using UnityEngine;


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
    private GameObject _curSceneObject;
    private RaycastHit _verifyPointSeek;
    private Vector3 _curInteractionPoint;
    private int _objectPointsIndex;

    // Location services
    private Vector3 _myPos;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _myPos = transform.position;

        switch (_status)
        {
            case Status.SeekingPoints: // Look for points to render to
                // Get all GameObjects with tag "SceneObject"
                _objectsInScene = GameObject.FindGameObjectsWithTag("SceneObject").ToList();
                print("number of objects: " + _objectsInScene.Count);

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
                    _status = Status.VerifyingPoints;
                }

                break;
            case Status.VerifyingPoints: // check that these points are physically valid
                // Get a point from the list to work with
                Vector3 point = _objectPointsToRender[_objectPointsIndex];
                print(gameObject.name + " Verifying point: " + point);

                // Raycast to see if this exact point is accessible from the object
                bool dontRemoveFlag = Physics.Raycast(_myPos, (point - _myPos).normalized, out _verifyPointSeek,
                    Mathf.Infinity, LayerMask.GetMask("SceneObject"));

                if (dontRemoveFlag)
                {
                    // yes, this point on another object is accessible
                    switch (imageType)
                    {
                        case ImageType.OriginalObject:
                            Debug.DrawRay(_myPos, (point - _myPos).normalized * _verifyPointSeek.distance, Color.blue);

                            dontRemoveFlag =
                                _verifyPointSeek.collider.gameObject.Equals(_objectsInScene[_objectPointsIndex]);
                            if (dontRemoveFlag)
                            {
                                // Update points list to the exact point to render with
                                _objectPointsToRender[_objectPointsIndex] = _verifyPointSeek.point;
                                _objectPointsIndex++;
                            }

                            break;
                        case ImageType.MirrorVirtualImage:
                            
                            break;
                    }
                }

                // Remove this point and object from the list (it not interacting with this particle)
                if (!dontRemoveFlag)
                {
                    _objectPointsToRender.RemoveAt(_objectPointsIndex);
                    _objectsInScene.RemoveAt(_objectPointsIndex);
                    _objectPointsIndex--;
                }

                // Once all points are handled, move on to rendering
                if (_objectPointsIndex.Equals(_objectPointsToRender.Count))
                {
                    _objectPointsIndex = 0;
                    _status = Status.PreRendering;
                }

                break;
            case Status.PreRendering: // collect data and render with appropriate method
                // Complete this object's job is it is done
                if (_objectPointsIndex.Equals(_objectPointsToRender.Count))
                {
                    _status = Status.Complete;
                    break;
                }

                // Get the appropriate data for the current object indexed
                _curSceneObject = _objectsInScene[_objectPointsIndex];
                _curInteractionPoint = _objectPointsToRender[_objectPointsIndex];

                // Check the type of object, and render
                if (_curSceneObject.GetComponent<PlaneMirrorDef>() != null)
                {
                    _status = Status.PlaneMirrorRendering;
                }
                else if (_curSceneObject.GetComponent<SphericalMirrorDef>() != null)
                {
                    _status = Status.SphericalMirrorRendering;
                }
                else if (_curSceneObject.GetComponent<ThinLensDef>() != null)
                {
                    _status = Status.ThinLensRendering;
                }
                else // WTF?
                {
                    print("Encountered unknown object: " + _curSceneObject.name);
                    _curSceneObject.GetComponent<Renderer>().material.color = Color.red;
                }

                break;
            case Status.PlaneMirrorRendering:
                PlaneMirrorRender(_myPos, _objectPointsToRender[_objectPointsIndex],
                    _verifyPointSeek, out Vector3 outputImagePos);

                // GameObject image = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                // image.transform.position = outputImagePos;
                // image.transform.localScale = transform.localScale;
                //
                // _status = Status.Idle;

                GameObject image = Instantiate(Resources.Load<GameObject>("Objects/ParticleImagePoint"), outputImagePos,
                    Quaternion.identity);

                IncorporatedParticleImage incorporatedParticleImage = image.GetComponent<IncorporatedParticleImage>();
                incorporatedParticleImage.imageType = ImageType.MirrorVirtualImage;
                incorporatedParticleImage.sourceSceneObject = _objectsInScene[_objectPointsIndex];

                // Return to prerendering for the next point
                _objectPointsIndex++;
                _status = Status.PreRendering;

                break;
            case Status.SphericalMirrorRendering:
                break;
            case Status.ThinLensRendering:
                break;
            case Status.Complete:
                break;
            default:
                print("Paused with status: " + _status);
                break;
        }
    }


    private void PlaneMirrorRender(Vector3 sourceOrigin, Vector3 strikePoint, RaycastHit hitData, out Vector3 imagePos,
        bool withRays = true)
    {
        // Vector3 raySource = sourceOrigin;
        // Vector3 rayDir = strikePoint;
        //
        // RaycastHit hit;
        // do
        // {
        //     if (Physics.Raycast(raySource, rayDir, out hit, Mathf.Infinity, layerMask))
        //     {
        //         raySource = hit.point;
        //     }
        //     else
        //     {
        //         break;
        //     }
        // } while (hit.collider.gameObject != targetingObject);

        print("hitData Mirror " + hitData.collider.gameObject.name);

        Vector3 mirrorNorm = hitData.normal;

        Vector3 exitDir = Quaternion.AngleAxis(180, mirrorNorm) * -(strikePoint - sourceOrigin).normalized;

        if (withRays)
        {
            Debug.DrawLine(sourceOrigin, strikePoint, Color.cyan, Mathf.Infinity);
            Debug.DrawRay(hitData.point, mirrorNorm, Color.red, Mathf.Infinity);
            Debug.DrawRay(hitData.point, exitDir, Color.green, Mathf.Infinity);
            Debug.DrawRay(hitData.point, -exitDir * hitData.distance, Color.yellow, Mathf.Infinity);
        }

        imagePos = hitData.point - exitDir * hitData.distance;
    }
}