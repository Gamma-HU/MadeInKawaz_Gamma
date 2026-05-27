using UnityEngine;

namespace Goroh.RotationGame
{
    public class Score : MonoBehaviour
    {
        [SerializeField] CursorRotationRecorder _rotationRecorder;
        [SerializeField] float _scoreMultiplier = 1f;
        [SerializeField] float _quota = 100f;
        public float Quota => _quota;
        private float _currentScore;

        public float CurrentScore => _currentScore;

        private void Update()
        {
            if (_rotationRecorder == null)
            {
                return;
            }

            _currentScore += _rotationRecorder.LatestDeltaDegrees * _scoreMultiplier;

            if (_currentScore >= _quota)
            {
                Debug.Log("Quota reached! Score: " + _currentScore);
                GameManager.Clear();
            }
        }
    }
}