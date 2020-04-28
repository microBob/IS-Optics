using System;
using UnityEngine;

namespace Refractions
{
    public class WaveGenerator : MonoBehaviour
    {
        public float amplitude = 1;

        public float waveLength = 500;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            print("Detected collision");
            
        }

    }
}