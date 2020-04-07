using UnityEngine;

namespace Raycast_Labs
{
    public enum Status
    {
        SeekingSource,
        SeekingTarget,
        CompletedSuccess,
        CompletedFailed
    }

    public class VirtualImageProblem : MonoBehaviour
    {
        public GameObject sourceMirror;
        public GameObject targetObejct;

        // SECTION: Private var
        private Vector3 _myPos;

        // Test loc
        private Vector3 _knownWorkingPoint;

        // State
        private Status _status;
        private Vector3 _direction;
        private Vector3 _seekOrigin;

        // Start is called before the first frame update
        void Start()
        {
            _status = Status.SeekingSource;
            _myPos = transform.position;
            _knownWorkingPoint = new Vector3(-8, 5, 0);

            // To working point
            _direction = (_knownWorkingPoint - _myPos).normalized;
            _seekOrigin = _myPos;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_status)
            {
                case Status.SeekingSource:
                    if (Physics.Raycast(_seekOrigin, _direction, out RaycastHit toSourceMirrorHit,
                        Mathf.Infinity))
                    {
                        _seekOrigin = toSourceMirrorHit.point;
                        if (toSourceMirrorHit.collider.gameObject.Equals(sourceMirror))
                        {
                            _status = Status.SeekingTarget;
                        }
                    }

                    _seekOrigin += _direction * 0.001f;

                    break;
                case Status.SeekingTarget:
                    if (Physics.Raycast(_seekOrigin, _direction, out RaycastHit toTargetHit, Mathf.Infinity))
                    {
                        Color msgColor = Color.green;
                        if (toTargetHit.collider.gameObject.Equals(targetObejct))
                        {
                            _status = Status.CompletedSuccess;
                        }
                        else
                        {
                            _status = Status.CompletedFailed;
                            msgColor = Color.red;
                        }

                        Debug.DrawLine(_myPos, toTargetHit.point, msgColor, Mathf.Infinity);
                        print("Completed with status: " + _status);
                    }

                    break;
                default:
                    break;
            }
        }
    }
}