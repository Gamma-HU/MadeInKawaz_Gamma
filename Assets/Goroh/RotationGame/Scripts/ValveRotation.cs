using UnityEngine;

namespace Goroh.RotationGame
{
    public class ValveRotation : MonoBehaviour
    {
        [SerializeField] float _rotationMultiplier = 1f;
        [SerializeField] CursorRotationRecorder _rotationRecorder;
        private void Update()
        {
            if (_rotationRecorder == null)
            {
                return;
            }

            float rotationAmount = _rotationRecorder.LatestDeltaDegrees * _rotationMultiplier;
            transform.Rotate(Vector3.forward, rotationAmount, Space.Self);
        }
    }
}