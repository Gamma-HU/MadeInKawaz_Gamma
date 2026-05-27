using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace Goroh.Fishing
{
    public class Fishing : MonoBehaviour
    {
        [SerializeField] float _gracePeriod = 0.8f;
        private float _catchTime;
        private bool _canCatch = false;
        private Coroutine _catchCoroutine;
        [SerializeField] float _GameTime = 8f;
        [SerializeField] GameObject _fisher;
        [SerializeField] Sprite _fisherNormal;
        [SerializeField] Sprite _drowningFisher;
        [SerializeField] GameObject _clearEffect;
        [SerializeField] AudioSource _audioSource;
        [SerializeField] AudioClip _catchSound;
        [SerializeField] List<GameObject> _notcatched;
        [SerializeField] List<GameObject> _catched;
        private void Start()
        {
            Setcatch(false);
            _catchTime = Random.Range(0.5f, _GameTime - _gracePeriod - 0.5f);
            _catchCoroutine = StartCoroutine(Catch(_catchTime));
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (_canCatch)
                {
                    Debug.Log("Catch!");
                    GameManager.Clear();
                    _clearEffect.SetActive(true);
                    _clearEffect.transform.DOMoveY(1.5f, 1f).SetEase(Ease.OutCubic);
                    _audioSource.PlayOneShot(_catchSound);
                    StopCoroutine(_catchCoroutine);
                    _fisher.GetComponent<SpriteRenderer>().sprite = _fisherNormal;
                }
                else
                {
                    Debug.Log("Miss...");
                    Miss();
                }
            }
        }
        private IEnumerator Catch(float time)
        {
            yield return new WaitForSeconds(time);
            Setcatch(true);
            _fisher.transform.DOShakePosition(_gracePeriod / 2f, 0.5f, 100, 90, false, false).OnComplete(() =>
            {
                foreach (GameObject g in _catched)
                {
                    g.SetActive(false);
                }
            });
            yield return new WaitForSeconds(_gracePeriod);
            Setcatch(false);
            Miss();
        }
        private void Miss()
        {
            Debug.Log("Miss...");
            StopCoroutine(_catchCoroutine);
            _fisher.GetComponent<SpriteRenderer>().sprite = _drowningFisher;
            _fisher.transform.position = new Vector3(0, 1.5f, 0);
            _fisher.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);
        }
        private void Setcatch(bool c)
        {
            _canCatch = c;
            foreach (GameObject g in _notcatched)
            {
                g.SetActive(!c);
            }
            foreach (GameObject g in _catched)
            {
                g.SetActive(c);
            }
        }
        private void OnDestroy()
        {
            if (_catchCoroutine != null)
            {
                StopCoroutine(_catchCoroutine);
            }
        }
    }
}
