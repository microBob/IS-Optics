using System.Collections.Generic;
using UnityEngine;

namespace Diffraction
{
    public class WaveFront2D : MonoBehaviour
    {
        public LineRenderer lineRenderer;

        public float lineWidth = 0.1f;
        public bool isPositiveWave;

        public Material positiveMaterial;

        public Material negativeMaterial;

        // Start is called before the first frame update
        void Start()
        {
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < 46; i++)
            {
                float deg = Mathf.Deg2Rad * (i * 4 - 90) ;
                vertices.Add(new Vector3(Mathf.Cos(deg), Mathf.Sin(deg)));
            }

            lineRenderer.positionCount = 46;
            lineRenderer.SetPositions(vertices.ToArray());

            lineRenderer.material = isPositiveWave ? positiveMaterial : negativeMaterial;

            lineRenderer.useWorldSpace = false;

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }
    }
}