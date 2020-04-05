using UnityEngine;

namespace Refractions
{
    public class ThinLensDef : MonoBehaviour
    {
        public float curveCoefficient = 0.09f;

        public float ior = 1.5f; //IOR of glass

        public float radius1;
        public float radius2;

        // Start is called before the first frame update
        void Start()
        {
            float secondDer = curveCoefficient * 2f;
            float scale = transform.localScale.z / 300f;

            radius1 = 1f / (secondDer / scale);
            radius2 = -radius1;
            print("Radius: " + radius1);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}