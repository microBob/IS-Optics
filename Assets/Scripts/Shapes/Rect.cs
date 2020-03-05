using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shapes
{
    public class Rect : MonoBehaviour
    {
        public PointLight lightSource;

        public int renderResolution = 10;

        public Vector3 size = Vector3.one;
        private Vector3 _loc;

        private Vector3 _pointSize;
        private List<GameObject> _points = new List<GameObject>();

        public bool render = true;
        private int _iteratorIndex = 0;
        private Vector3 _lastLightLoc;

        // Start is called before the first frame update
        void Start()
        {
            // Update public points
            Transform rectTransform = transform;
            _loc = rectTransform.position;

            // Initialize
            _lastLightLoc = lightSource.transform.position;

            PopulatePoints();
        }

        private void PopulatePoints()
        {
            // Instantiate stuff
            _pointSize = size / renderResolution;

            // Create Render Points
            Vector3 usedDim = size / 2;

            for (int xDirection = 0; xDirection < renderResolution + 1; xDirection++)
            {
                float xCoordinate = -usedDim.x + (xDirection * size.x / renderResolution);
                for (int yDirection = 0; yDirection < renderResolution + 1; yDirection++)
                {
                    float yCoordinate = -usedDim.y + (yDirection * size.y / renderResolution);
                    Debug.Log("Working on Coordinate: " + xCoordinate + ", " + yCoordinate + ", " + usedDim.z);

                    Vector3 posVec = new Vector3(xCoordinate, yCoordinate, -usedDim.z);

                    _points.Add(CreatePoint(posVec));
                }
            }

            for (int zDirection = 1; zDirection < renderResolution; zDirection++)
            {
                for (int xDirection = 0; xDirection < renderResolution + 1; xDirection++)
                {
                    float xCoordinate = -usedDim.x + (xDirection * size.x / renderResolution);

                    int tRenderResolution = 2;
                    if (xDirection == 0 || xDirection == renderResolution)
                    {
                        tRenderResolution = renderResolution + 2;
                    }

                    for (int yDirection = 0; yDirection < tRenderResolution; yDirection++)
                    {
                        float yCoordinate = -usedDim.y + (yDirection * size.y / (tRenderResolution - 1));
                        float zCoordinate = -usedDim.z + (zDirection * size.z / renderResolution);

                        Vector3 posVec = new Vector3(xCoordinate, yCoordinate, zCoordinate);

                        _points.Add(CreatePoint(posVec));
                    }
                }
            }

            for (int xDirection = 0; xDirection < renderResolution + 1; xDirection++)
            {
                float xCoordinate = -usedDim.x + (xDirection * size.x / renderResolution);
                for (int yDirection = 0; yDirection < renderResolution + 1; yDirection++)
                {
                    float yCoordinate = -usedDim.y + (yDirection * size.y / renderResolution);
                    Debug.Log("Working on Coordinate: " + xCoordinate + ", " + yCoordinate + ", " + usedDim.z);

                    Vector3 posVec = new Vector3(xCoordinate, yCoordinate, usedDim.z);

                    _points.Add(CreatePoint(posVec));
                }
            }

            Debug.Log(_points.Count);
        }

        // Update is called once per frame
        void Update()
        {
            if (render)
            {
                Vector3 lightPos = lightSource.transform.position;
                GameObject workingPoint = _points[_iteratorIndex];
                Vector3 pointPos = workingPoint.transform.position;

                float distance = Vector3.Distance(lightPos, pointPos);
                float distance2 = Mathf.Pow(distance, 2);
                Vector3 rayDir = (pointPos - lightPos).normalized;

                Vector3 hsvColor = Vector3.zero;

                if (Physics.Raycast(lightPos, rayDir, out RaycastHit hit))
                {
                    Debug.DrawRay(lightPos, rayDir * hit.distance, Color.cyan);

                    if (hit.collider == workingPoint.GetComponent<Collider>())
                    {
                        Color.RGBToHSV(lightSource.color, out hsvColor.x, out hsvColor.y, out hsvColor.z);
                        hsvColor.z = lightSource.intensity / distance2;
                        hsvColor.z = Mathf.Clamp(hsvColor.z, 0f, 1f);

                        workingPoint.GetComponent<Renderer>().material.color =
                            Color.HSVToRGB(hsvColor.x, hsvColor.y, hsvColor.z);
                    }
                }

                _iteratorIndex++;
                if (_iteratorIndex == _points.Count)
                {
                    render = false;
                }
            }
            else
            {
                if (_iteratorIndex == _points.Count && lightSource.transform.position != _lastLightLoc)
                {
                    _iteratorIndex = 0;
                    render = true;
                }
            }

            Debug.Log("Delta Time: " + Time.deltaTime);
        }


        private GameObject CreatePoint(Vector3 loc)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = _loc + loc;
            point.transform.localScale = _pointSize;
            point.GetComponent<Renderer>().material.color = Color.white;

            return point;
        }
    }
}