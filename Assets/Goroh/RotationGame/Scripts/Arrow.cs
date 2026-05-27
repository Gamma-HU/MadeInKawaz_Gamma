using UnityEngine;
using DG.Tweening;

namespace Goroh.RotationGame
{
    public class Arrow : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.transform.DOMoveY(1.3f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }
}
