using UnityEngine;

namespace Goroh.RotationGame
{
    public class CursorRotationRecorder : MonoBehaviour
    {
        [SerializeField] Camera _targetCamera;
        [SerializeField] Transform _pivotTransform;
        [SerializeField] Vector3 _pivotPoint;
        [SerializeField] bool _useTransformAsPivot = true;
        [SerializeField] bool _recordOnStart = true;

        private Vector2 _previousDirection;
        private bool _isRecording;
        private bool _hasPreviousDirection;

        public float TotalRotationDegrees { get; private set; }
        public float LatestDeltaDegrees { get; private set; }

        private Camera TargetCamera => _targetCamera != null ? _targetCamera : Camera.main;
        private Vector3 PivotPoint => _useTransformAsPivot && _pivotTransform != null ? _pivotTransform.position : _pivotPoint;

        private void Start()
        {
            if (_recordOnStart)
            {
                StartRecording();
            }
        }

        private void Update()
        {
            if (!_isRecording)
            {
                return;
            }

            Camera cameraToUse = TargetCamera;
            if (cameraToUse == null)
            {
                return;
            }

            Vector2 currentDirection = GetCursorDirection(cameraToUse);
            if (currentDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            if (_hasPreviousDirection)
            {
                LatestDeltaDegrees = Vector2.SignedAngle(_previousDirection, currentDirection);
                TotalRotationDegrees += LatestDeltaDegrees;
            }
            else
            {
                LatestDeltaDegrees = 0f;
                _hasPreviousDirection = true;
            }

            _previousDirection = currentDirection;
            // Debug.Log($"Latest Delta Degrees: {LatestDeltaDegrees}, Total Rotation Degrees: {TotalRotationDegrees}");
        }

        public void StartRecording()
        {
            _isRecording = true;
            _hasPreviousDirection = false;
            LatestDeltaDegrees = 0f;
        }

        public void StopRecording()
        {
            _isRecording = false;
        }

        public void ResetRotation()
        {
            TotalRotationDegrees = 0f;
            LatestDeltaDegrees = 0f;
            _hasPreviousDirection = false;
        }

        public void SetPivotPoint(Vector3 pivotPoint)
        {
            _pivotPoint = pivotPoint;
            _useTransformAsPivot = false;
            _hasPreviousDirection = false;
        }

        private Vector2 GetCursorDirection(Camera cameraToUse)
        {
            Vector3 pivot = PivotPoint;
            float depth = cameraToUse.orthographic
                ? Mathf.Abs(cameraToUse.transform.position.z - pivot.z)
                : Vector3.Dot(pivot - cameraToUse.transform.position, cameraToUse.transform.forward);

            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Mathf.Max(depth, 0f);

            Vector3 cursorWorldPosition = cameraToUse.ScreenToWorldPoint(mousePosition);
            Vector2 direction = cursorWorldPosition - pivot;
            return direction.normalized;
        }
    }
}
