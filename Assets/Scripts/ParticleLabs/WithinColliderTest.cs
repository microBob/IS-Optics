using UnityEngine;

namespace ParticleLabs
{
    public class WithinColliderTest : MonoBehaviour
    {
        public Collider interactingCollider;

        public bool Within;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            Within = IsPointWithinCollider(interactingCollider, transform.position);
        }

        // Method created by user Daniel1112
        private static bool IsPointWithinCollider(Collider collider, Vector3 point)
        {
            return (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
        }
    }
}