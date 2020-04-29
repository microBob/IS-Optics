using System.Collections.Generic;
using UnityEngine;

namespace Diffraction
{
    public class WaveGenerator : MonoBehaviour
    {
        public float amplitude = 1;

        public float waveLength = 500;

        public float maxRadius = 5;

        public float animationSpeedScale = 1;

        public Material positiveMaterial;
        public Material negativeMaterial;

        // SECTION: Private
        private Vector3 _myPos;

        private readonly List<GameObject> _waveHeads = new List<GameObject>();

        private bool _isPositiveWave = false;

        private float _waveSpacing;

        // Start is called before the first frame update
        void Start()
        {
            _myPos = transform.position;
            // Scale wavelength (nm) to cm
            waveLength *= Mathf.Pow(10, -3);

            GameObject wave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wave.transform.position = _myPos;
            wave.transform.localScale = Vector3.zero;
            wave.GetComponent<Renderer>().material = positiveMaterial;
            _waveHeads.Add(wave);
        }

        // Update is called once per frame
        void Update()
        {
            float radiusIncrease = Time.deltaTime * animationSpeedScale;

            // _waveHeads[0].transform.localScale = Vector3.one * radiusIncrease;
            //
            // radiusIncrease = radiusIncrease > maxRadius ? 0 : radiusIncrease;

            // SECTION: increase radius or remove
            for (int i = _waveHeads.Count - 1; i >= 0; i--)
            {
                _waveHeads[i].transform.localScale += Vector3.one * radiusIncrease * 2;
                // localScale is diameter
                Vector3 curRadius = _waveHeads[i].transform.localScale / 2f;
                if (curRadius.x + radiusIncrease > maxRadius)
                {
                    Destroy(_waveHeads[i]);
                    _waveHeads.RemoveAt(i);
                    continue;
                }

                // Change color
                Color tempColor = _waveHeads[i].GetComponent<Renderer>().material.color;
                print("Cur: " + curRadius.x + "Max: " + maxRadius);
                tempColor.a = Remap(SphereSurfaceArea(curRadius.x), 0, SphereSurfaceArea(maxRadius), 1, 0);

                _waveHeads[i].GetComponent<Renderer>().material.color = tempColor;
            }


            // SECTION: create new waves
            _waveSpacing += radiusIncrease;
            if (_waveSpacing >= waveLength)
            {
                GameObject wave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wave.transform.position = _myPos;
                wave.transform.localScale = Vector3.zero;
                wave.GetComponent<Renderer>().material = _isPositiveWave ? positiveMaterial : negativeMaterial;
            
                _waveHeads.Insert(0, wave);
            
                _isPositiveWave = !_isPositiveWave;
                _waveSpacing = 0;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            print("Detected collision");
        }

        private float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        private float SphereSurfaceArea(float withRadius)
        {
            return 4 * Mathf.PI * Mathf.Pow(withRadius, 2);
        }
    }
}