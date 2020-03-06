using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R2SelfRender : MonoBehaviour
{
    private PointLight _lightSource = null;
    private Vector3? _lastLightLoc = null;

    private Vector3 _myPos;

    private bool _clearedForRender = false;
    private bool _doRender = false;

    private Collider _myCollider;
    private Renderer _myRenderer;

    // Start is called before the first frame update
    void Start()
    {
        // Init values
        _myPos = transform.position;

        _myRenderer = GetComponent<Renderer>();
        _myCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_lightSource != null && _lastLightLoc != null)
        {
            _clearedForRender = true;
        }
        else
        {
            _clearedForRender = false;
        }

        if (_doRender && _clearedForRender)
        {
            Vector3 lightPos = _lightSource.transform.position;

            float distance = Vector3.Distance(lightPos, _myPos);
            float distance2 = Mathf.Pow(distance, 2);
            Vector3 rayDir = (_myPos - lightPos).normalized;

            Vector3 hsvColor = Vector3.zero;

            if (Physics.Raycast(lightPos, rayDir, out RaycastHit hit))
            {
                // Debug.DrawRay(lightPos, rayDir * hit.distance, Color.cyan);

                if (hit.collider == _myCollider)
                {
                    Color.RGBToHSV(_lightSource.color, out hsvColor.x, out hsvColor.y, out hsvColor.z);
                    hsvColor.z = _lightSource.intensity / distance2;
                    hsvColor.z = Mathf.Clamp(hsvColor.z, 0f, 1f);
                }
            }

            _myRenderer.material.color =
                Color.HSVToRGB(hsvColor.x, hsvColor.y, hsvColor.z);

            _lastLightLoc = lightPos;
            _doRender = false;
        }
    }

    public void SetLightSource(PointLight lightObj)
    {
        _lightSource = lightObj;
        _lastLightLoc = _lightSource.transform.position;
    }

    public void SetDoRender(bool render = true)
    {
        _doRender = render;
    }
}