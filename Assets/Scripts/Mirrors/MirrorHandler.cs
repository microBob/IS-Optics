using System.Collections.Generic;
using UnityEngine;

namespace Mirrors
{
    public class MirrorHandler : MonoBehaviour
    {
        private List<GameObject> _mirrors = new List<GameObject>();

        // Start is called before the first frame update
        void Awake()
        {
            foreach (Transform childMirror in transform)
            {
                _mirrors.Add(childMirror.gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        public List<GameObject> GetMirrors()
        {
            return _mirrors;
        }

        public LayerMask GetMirrorMask(int index)
        {
            switch (index)
            {
                case 0:
                    return LayerMask.GetMask("First");
                case 1:
                    return LayerMask.GetMask("Second");
                case 2:
                    return LayerMask.GetMask("Third");
                case 3:
                    return LayerMask.GetMask("Fourth");
                case 4:
                    return LayerMask.GetMask("Fifth");
                default:
                    return LayerMask.GetMask("Default");
            }
        }
    }
}