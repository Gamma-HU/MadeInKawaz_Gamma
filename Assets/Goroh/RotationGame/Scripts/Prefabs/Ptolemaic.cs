using UnityEngine;

namespace Goroh.RotationGame
{
    public class Ptolemaic : MonoBehaviour, IRotationgameDecoration
    {
        [SerializeField] Score _score;
        [SerializeField] Transform _earth;
        [SerializeField] Transform _sun;
        [SerializeField] Vector3 _sunInitialPos;
        public void Init(Score score)
        {
            _score = score;
            this.transform.position = new Vector3 (1.21f, 2.48f, 0);
            _sun.localPosition = _sunInitialPos;
        }
        private void Update()
        {
            if (_score == null)
            {
                return;
            }

            float t = Mathf.Clamp01(_score.CurrentScore / _score.Quota);
            float angle = t * 360f + 90f; // Start from the top
            _sun.localPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * 0.8f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * 0.8f,
                0
            ) + _earth.localPosition;
        }
    }
}
