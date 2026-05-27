using UnityEngine;

namespace Goroh.RotationGame
{
    public class Sushi : MonoBehaviour, IRotationgameDecoration
    {
        [SerializeField] Vector3 _initialPos;
        [SerializeField] Vector3 _endPos;
        [SerializeField] GameObject _SushiObject;
        [SerializeField] Score _score;
        public void Init(Score score)
        {
            this.transform.position = new Vector3 (0, 1.09f, 0);
            _score = score;
            _SushiObject.transform.localPosition = _initialPos;
        }
        private void Update()
        {
            if (_score == null)
            {
                return;
            }

            float t = Mathf.Clamp01(_score.CurrentScore / _score.Quota);
            _SushiObject.transform.localPosition = Vector3.Lerp(_initialPos, _endPos, t);
        }
    }
}
