using UnityEngine;

namespace Goroh.RotationGame
{
    public class Sunrise : MonoBehaviour, IRotationgameDecoration
    {
        [SerializeField] Vector3 _initialPos;
        [SerializeField] Vector3 _risePos;
        [SerializeField] GameObject _Sun;
        [SerializeField] GameObject _SunshineObject;
        [SerializeField] Score _score;
        public void Init(Score score)
        {
            this.transform.position = new Vector3(0, -3.32f, 0);
            _score = score;
            _Sun.transform.localPosition = _initialPos;
        }
        private void Update()
        {
            if (_score == null)
            {
                return;
            }

            float t = Mathf.Clamp01(_score.CurrentScore / _score.Quota);
            _Sun.transform.localPosition = Vector3.Lerp(_initialPos, _risePos, t);
            _SunshineObject.SetActive(Mathf.Approximately(t, 1f));
        }
    }
}
