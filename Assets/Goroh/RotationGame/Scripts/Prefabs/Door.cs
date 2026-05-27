using UnityEngine;

namespace Goroh.RotationGame
{
    public class Door : MonoBehaviour, IRotationgameDecoration
    {
        [SerializeField] Vector3 _initialPos;
        [SerializeField] Vector3 _openPos;
        [SerializeField] GameObject _DoorObject;
        [SerializeField] Score _score;
        public void Init(Score score)
        {
            this.transform.position = new Vector3 (0, 1.83f, 0);
            _score = score;
            _DoorObject.transform.localPosition = _initialPos;
        }
        private void Update()
        {
            if (_score == null)
            {
                return;
            }

            float t = Mathf.Clamp01(_score.CurrentScore / _score.Quota);
            _DoorObject.transform.localPosition = Vector3.Lerp(_initialPos, _openPos, t);
        }
    }
}
