using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rect : MonoBehaviour
{
    public GameObject pointObjc;
    public PointLight lightSource;

    public int renderResolution = 10;

    public Vector3 size = Vector3.one;
    private Vector3 loc;
    private Vector3 rot;

    private Vector3 _pointSize;
    private List<GameObject> points = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // Update public points
        Transform rectTransform = transform;
        loc = rectTransform.position;
        rot = rectTransform.rotation.eulerAngles;
        
        PopulatePoints();
        RenderPoint(0);
        // RenderPoints();
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

    private void RenderPoint(int pointIndex=0)
    {
        GameObject point = points[pointIndex];
        
        Vector3 lightPos = lightSource.transform.position;
        Vector3 myPos = point.transform.position;
        
        if (Physics.Raycast(lightPos,
            (myPos - lightPos).normalized, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.DrawRay(lightPos,
                (myPos - lightPos).normalized * hit.distance, Color.cyan,Mathf.Infinity);
            hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }
    private void RenderPoints(Boolean animate = false)
    {
        foreach (GameObject point in points)
        {
            Vector3 lightPos = lightSource.transform.position;
            Vector3 myPos = point.transform.position;
            float distance = Vector3.Distance(lightPos, myPos);
            float distance2 = Mathf.Pow(distance, 2);

            Vector3 hsvColor = Vector3.zero;

            if (Physics.Raycast(lightPos, (myPos - lightPos).normalized, out RaycastHit hit,
                Mathf.Infinity))
            {
                Debug.DrawRay(lightPos, (myPos - lightPos).normalized * hit.distance, Color.cyan, Mathf.Infinity);
                Debug.Log("Distance: "+hit.distance);
                
                hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
                
                Color.RGBToHSV(lightSource.color, out hsvColor.x, out hsvColor.y, out hsvColor.z);
                hsvColor.z = lightSource.intensity / distance2;
                hsvColor.z = Mathf.Clamp(hsvColor.z, 0f, 1f);
            }


            point.GetComponent<Renderer>().material.color = Color.HSVToRGB(hsvColor.x, hsvColor.y, hsvColor.z);

            // bool cont = true;
            // while (cont)
            // {
            //     if (Input.GetKeyDown(KeyCode.Return))
            //     {
            //         cont = false;
            //     }
            // }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}