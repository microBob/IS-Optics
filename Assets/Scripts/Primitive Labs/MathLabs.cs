using UnityEngine;

namespace Primitive_Labs
{
    public class MathLabs : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            float sin = Mathf.Sin(31 * Mathf.Deg2Rad);
            print(sin);
            Vector3 vect = new Vector3(sin, 0, 0);
            print(vect.x);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}