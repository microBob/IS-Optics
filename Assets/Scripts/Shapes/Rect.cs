using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rect : MonoBehaviour
{
    public GameObject pointObjc;
    public PointLight lightSource;

    public int renderResolution = 10;

    public Vector3 loc = Vector3.zero;
    public Vector3 size = Vector3.one;
    public Vector3 rot = Vector3.zero;

    private Vector3 _pointSize;
    private List<GameObject> points = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        PopulatePoints();
        RenderPoints();
    }

    private void PopulatePoints()
    {
        // Instantiate stuff
        _pointSize = new Vector3(size.x / renderResolution, size.y / renderResolution, size.z / renderResolution);

        // Create Render Points
        float usedXDim = size.x / 2;
        float usedYDim = size.y / 2;
        float usedZDim = size.z / 2;

        for (int xDirection = 0; xDirection < renderResolution + 1; xDirection++)
        {
            float xCoordinate = -usedXDim + (xDirection * size.x / renderResolution);
            for (int yDirection = 0; yDirection < renderResolution + 1; yDirection++)
            {
                float yCoordinate = -usedYDim + (yDirection * size.y / renderResolution);
                Debug.Log("Working on Coordinate: " + xCoordinate + ", " + yCoordinate + ", " + usedZDim);

                Vector3 posVec = new Vector3(xCoordinate, yCoordinate, usedZDim);

                GameObject item = (GameObject) Instantiate(pointObjc, loc + posVec, Quaternion.Euler(rot));
                item.transform.localScale = _pointSize;

                points.Add(item);
            }
        }

        for (int zDirection = 1; zDirection < renderResolution; zDirection++)
        {
            for (int xDirection = 0; xDirection < renderResolution + 1; xDirection++)
            {
                float xCoordinate = -usedXDim + (xDirection * size.x / renderResolution);

                int tRenderResolution = 2;
                if (xDirection == 0 || xDirection == renderResolution)
                {
                    tRenderResolution = renderResolution + 2;
                }

                for (int yDirection = 0; yDirection < tRenderResolution; yDirection++)
                {
                    float yCoordinate = -usedYDim + (yDirection * size.y / (tRenderResolution - 1));
                    float zCoordinate = -usedZDim + (zDirection * size.z / renderResolution);

                    Vector3 posVec = new Vector3(xCoordinate, yCoordinate, zCoordinate);

                    GameObject item = (GameObject) Instantiate(pointObjc, loc + posVec, Quaternion.Euler(rot));
                    item.transform.localScale = _pointSize;

                    points.Add(item);
                }
            }
        }

        for (int xDirection = 0; xDirection < renderResolution + 1; xDirection++)
        {
            float xCoordinate = -usedXDim + (xDirection * size.x / renderResolution);
            for (int yDirection = 0; yDirection < renderResolution + 1; yDirection++)
            {
                float yCoordinate = -usedYDim + (yDirection * size.y / renderResolution);
                Debug.Log("Working on Coordinate: " + xCoordinate + ", " + yCoordinate + ", " + usedZDim);

                Vector3 posVec = new Vector3(xCoordinate, yCoordinate, -usedZDim);

                GameObject item = (GameObject) Instantiate(pointObjc, loc + posVec, Quaternion.Euler(rot));
                item.transform.localScale = _pointSize;

                points.Add(item);
            }
        }

        Debug.Log(points.Count);
    }

    private void RenderPoints(Boolean animate=false)
    {
        foreach (GameObject point in points)
        {
            float distance = Vector3.Distance(lightSource.position, point.transform.position);
            float distance2 = Mathf.Pow(distance, 2);

            Vector3 hsvColor;
            Color.RGBToHSV(lightSource.color, out hsvColor.x, out hsvColor.y, out hsvColor.z);
            hsvColor.z = lightSource.intensity / distance2;
            hsvColor.z = Mathf.Clamp(hsvColor.z, 0, 1);
            
            Debug.Log("Distance: "+distance+"; Color: "+hsvColor.ToString());
            
            point.GetComponent<Renderer>().material.color = Color.HSVToRGB(hsvColor.x, hsvColor.y, hsvColor.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}