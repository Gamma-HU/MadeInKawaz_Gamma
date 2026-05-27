using UnityEngine;

namespace Goroh.RotationGame
{
    public class Periodic : MonoBehaviour, IRotationgameDecoration
    {
        [SerializeField] Score _score;
        [SerializeField] Transform _objectToRotate;
        [SerializeField] SpriteRenderer _spriteRenderer;
        [SerializeField] Sprite _daytimeSprite;
        [SerializeField] Color _daytimeColor;
        [SerializeField] Sprite _eveningSprite;
        [SerializeField] Color _eveningColor;
        [SerializeField] Sprite _nightSprite;
        [SerializeField] Color _nightColor;
        public void Init(Score score)
        {
            this.transform.position = new Vector3 (0, 1f, 0);
            _score = score;
            _objectToRotate.localRotation = Quaternion.identity;
        }
        private void Update()
        {
            if (_score == null)
            {
                return;
            }
            float t = Mathf.Clamp01(_score.CurrentScore / _score.Quota);
            float angle = t * 180f; // 0 to 180 degrees
            _objectToRotate.localRotation = Quaternion.Euler(0, 0, angle);
            if (angle < 60f)
            {
                _spriteRenderer.sprite = _daytimeSprite;
                _spriteRenderer.color = _daytimeColor;
            }
            else if (angle < 120f)
            {
                _spriteRenderer.sprite = _eveningSprite;
                _spriteRenderer.color = _eveningColor;
            }
            else
            {
                _spriteRenderer.sprite = _nightSprite;
                _spriteRenderer.color = _nightColor;
            }
        }
    }
}
