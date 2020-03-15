using UnityEngine;

namespace Raycast_Labs
{
    public class LayerMaskLabs : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity,
                LayerMask.GetMask("Second","Fifth")))
            {
                Debug.DrawRay(transform.position, transform.forward*hit.distance, Color.cyan, Mathf.Infinity);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}