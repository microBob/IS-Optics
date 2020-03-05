using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastLabs : MonoBehaviour
{
    public PointLight lightSource;
    // Start is called before the first frame update
    void Start()
    {
        if (Physics.Raycast(lightSource.transform.position,
            (transform.position - lightSource.transform.position).normalized, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.DrawRay(lightSource.transform.position,
                (transform.position - lightSource.transform.position).normalized * hit.distance, Color.cyan,Mathf.Infinity);
            hit.collider.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
