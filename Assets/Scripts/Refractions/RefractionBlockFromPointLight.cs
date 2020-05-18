using System.Collections.Generic;
using IncorporatedParticleOptics;
using UnityEngine;


public enum RefractionCalculationStatus
{
    SeekingSurface,
    TestSeek,
    WaitingForSeekData,
    Drawing,
    Complete
}

public struct AllHitData
{
    public GameObject HitObj;
    public string HitObjName;
    public Vector3 HitLoc;
    public Vector3 HitNormal;

    public AllHitData(GameObject obj, string objName, Vector3 hitLoc, Vector3 hitNorm)
    {
        HitObj = obj;
        HitObjName = objName == "" ? obj.name : objName;
        HitLoc = hitLoc;
        HitNormal = hitNorm;
    }
}

namespace Refractions
{
    public class RefractionBlockFromPointLight : MonoBehaviour
    {
        public float localIor = 1f;

        public float finalExitDrawLength = 2f;


        private GameObject _seekParticleSystem;

        private List<AllHitData> _renderPoints = new List<AllHitData>();

        // ID services
        private Vector3 _myPos;

        private RefractionCalculationStatus _myRefractionCalculationStatus = RefractionCalculationStatus.SeekingSurface;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            _myPos = transform.position;
            switch (_myRefractionCalculationStatus)
            {
                case RefractionCalculationStatus.SeekingSurface:
                    print("Seeking Surface");
                    _seekParticleSystem =
                        Instantiate(Resources.Load<GameObject>("Simulations/SphericalSurfaceSeekPS"));

                    if (_seekParticleSystem != null)
                    {
                        // Set transforms to match this object
                        _seekParticleSystem.transform.position = _myPos;
                        _seekParticleSystem.transform.localScale = transform.localScale;

                        // set it's source to this object
                        RefractionSeekParticleSystemHandler seekHandler =
                            _seekParticleSystem.AddComponent<RefractionSeekParticleSystemHandler>();
                        seekHandler.sourceLight = gameObject;
                        seekHandler.myParticleSystem = _seekParticleSystem.GetComponent<ParticleSystem>();
                        seekHandler.numOfSamples = 2;

                        _myRefractionCalculationStatus = RefractionCalculationStatus.WaitingForSeekData;
                    }

                    break;
                case RefractionCalculationStatus.TestSeek:
                    print("Creating test seek");
                    if (Physics.Raycast(_myPos, transform.forward, out RaycastHit hit, Mathf.Infinity))
                    {
                        AddHitData(hit.collider.gameObject, hit.point, hit.normal);
                        _myRefractionCalculationStatus = RefractionCalculationStatus.Drawing;
                    }

                    break;
                case RefractionCalculationStatus.Drawing:
                    Destroy(_seekParticleSystem);
                    foreach (AllHitData renderPoint in _renderPoints)
                    {
                        Vector3 curLoc = renderPoint.HitLoc;
                        Vector3 curNorm = renderPoint.HitNormal;

                        print("Drawing hit point at " + curLoc + " with normal " + curNorm + " on GameObject " +
                              renderPoint.HitObjName);
                        float rayDur = Mathf.Infinity;

                        Debug.DrawLine(_myPos, curLoc, Color.cyan, rayDur);
                        Debug.DrawLine(curLoc, curNorm + curLoc, Color.red, rayDur);

                        // Apply Snell's Law on incident
                        float incidentAngle =
                            Vector3.Angle((_myPos - curLoc).normalized, curNorm.normalized);
                        float targetIor = renderPoint.HitObj.GetComponent<RefractionBlockDef>().ior;
                        float exitAngle = Mathf.Asin(localIor * Mathf.Sin(Mathf.Deg2Rad * incidentAngle) / targetIor) *
                                          Mathf.Rad2Deg;
                        // print("Incident angle: " + incidentAngle + "; Exit Angle: " + exitAngle);

                        GameObject rotGo = new GameObject();
                        Transform exitDir = rotGo.transform;
                        exitDir.position = curLoc;

                        exitDir.forward = -curNorm;
                        Debug.DrawRay(curLoc, exitDir.forward, Color.yellow, rayDur);

                        exitDir.forward = (curLoc - _myPos).normalized;

                        Debug.DrawRay(curLoc, exitDir.forward, Color.blue, rayDur);

                        Vector3 cross = Vector3.Cross(curNorm, (curLoc - _myPos).normalized).normalized;
                        // print("Cross: " + cross);
                        Debug.DrawRay(curLoc, cross, Color.magenta, rayDur);

                        float angleDiff = incidentAngle - exitAngle;
                        // print("Angle Diff: " + angleDiff);
                        print(exitDir.localRotation.eulerAngles);
                        exitDir.Rotate(cross, angleDiff, Space.World);
                        print(exitDir.localRotation.eulerAngles);

                        Debug.DrawRay(curLoc, exitDir.forward, Color.green, rayDur);

                        Destroy(rotGo);
                    }

                    // _renderPoints.Clear();
                    _myRefractionCalculationStatus = RefractionCalculationStatus.Complete;

                    break;
                case RefractionCalculationStatus.Complete:
                    break;
                default:
                    print("Status: " + _myRefractionCalculationStatus);
                    break;
            }
        }

        // SECTION: public get
        public List<AllHitData> GetRenderPoints()
        {
            return _renderPoints;
        }

        // SECTION: public set
        public void SetStatus(RefractionCalculationStatus status)
        {
            _myRefractionCalculationStatus = status;
        }

        public void AddHitData(GameObject obj, Vector3 hitLoc, Vector3 hitNorm, string objName = "")
        {
            _renderPoints.Add(new AllHitData(obj, objName, hitLoc, hitNorm));
        }
    }
}