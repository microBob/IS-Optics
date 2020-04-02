using System.Collections.Generic;
using UnityEngine;

namespace Raycast_Labs
{
    public class SeekClosestPointOnCollider : MonoBehaviour
    {
        private int _status = 0; // 0=seeking, 1 = writing

        private List<Vector3> _lightPoints = new List<Vector3>();
        private int _lightPointIndex = 0;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            Transform myTrans = transform;
            Vector3 myTransPosition = myTrans.position;
            
            if (_status == 0)
            {
                GameObject[] objectsInScene = GameObject.FindGameObjectsWithTag("SceneObject");
                print("number of objects: " + objectsInScene.Length);

                foreach (GameObject o in objectsInScene)
                {
                    o.GetComponent<MeshCollider>().convex = true;
                    _lightPoints.Add(o.GetComponent<Collider>().ClosestPoint(myTransPosition));
                    o.GetComponent<MeshCollider>().convex = false;
                }

                if (_lightPoints.Count > 0)
                {
                    _status = 1;
                }
            }
            else if (_status == 1)
            {
                Vector3 point = _lightPoints[_lightPointIndex];
                print("Working on point: " + point);

                if (Physics.Raycast(myTransPosition, (point - myTransPosition).normalized,
                    out RaycastHit closestPointSeek, Mathf.Infinity, LayerMask.GetMask("Mirrors")))
                {
                    Debug.DrawRay(myTransPosition, (point - myTransPosition).normalized * closestPointSeek.distance,
                        Color.green);
                }

                _lightPointIndex++;
                if (_lightPointIndex == _lightPoints.Count)
                {
                    _lightPoints.Clear();
                    _lightPointIndex = 0;
                    _status = 0;
                }
            }
        }
    }
}