using UnityEngine;

namespace Goroh.RotationGame
{
    public class Whale : MonoBehaviour, IRotationgameDecoration
    {
        [SerializeField] Vector3 _initialPos;
        [SerializeField] Vector3 _jumpPos;
        [SerializeField] GameObject _WhaleObject;
        [SerializeField] SpriteRenderer _spriteRenderer;
        [SerializeField] Sprite _normalSprite;
        [SerializeField] Sprite _jumpSprite;
        [SerializeField] Score _score;
        public void Init(Score score)
        {
            this.transform.position = new Vector3 (0, 2.3f, 0);
            _score = score;
            _WhaleObject.transform.localPosition = _initialPos;
            _spriteRenderer.sprite = _normalSprite;
        }
        private void Update()
        {
            if (_score == null)
            {
                return;
            }

            float t = Mathf.Clamp01(_score.CurrentScore / _score.Quota);
            _WhaleObject.transform.localPosition = Vector3.Lerp(_initialPos, _jumpPos, t);
            _spriteRenderer.sprite = t < 1 ? _normalSprite : _jumpSprite;
        }
    }
}
