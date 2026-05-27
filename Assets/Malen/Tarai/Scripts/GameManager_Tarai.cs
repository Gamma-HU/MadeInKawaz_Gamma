using UnityEngine;
using DG.Tweening;
using System.Collections;

public class GameManager_Tarai : MonoBehaviour
{
    [SerializeField] SpriteRenderer player;
    [SerializeField] Animator playerAnimator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hitClip;
    [SerializeField] AudioClip suceedClip;
    [SerializeField] AudioClip failedClip;
    [SerializeField] GameObject tarai;
    [SerializeField] SpriteRenderer borderSprite;
    [SerializeField] Vector3 finalTaraiPosition;
    [SerializeField] float taraiMoveDuration;
    [SerializeField] float startDelay;
    [SerializeField] Ease[] taraiEaseList;

    private Tween taraiTween;
    private float borderSpriteMaxY;
    private float borderSpriteMinY;
    private bool clickFlag = false;

    private void Start()
    {
        borderSpriteMaxY = borderSprite.bounds.max.y;
        borderSpriteMinY = borderSprite.bounds.min.y;
        taraiTween = tarai.transform.DOMove(finalTaraiPosition, taraiMoveDuration)
                        .SetDelay(startDelay)
                        .SetEase(taraiEaseList[Random.Range(0, taraiEaseList.Length)])
                        .OnComplete(() =>
                        {
                            audioSource.PlayOneShot(hitClip);
                            playerAnimator.SetTrigger("Hit");
                            clickFlag = true;
                        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !clickFlag)
        {
            taraiTween.Kill();
            float taraiY = tarai.transform.position.y;
            clickFlag = true;
            if (borderSpriteMinY <= taraiY && taraiY <= borderSpriteMaxY)
            {
                audioSource.PlayOneShot(suceedClip);
                playerAnimator.SetTrigger("Success");
                // player.sprite = playerSuccess;
                GameManager.Clear();
            }
            else
            {
                audioSource.PlayOneShot(failedClip);
                playerAnimator.SetTrigger("Failed");
                // player.sprite = playerFailed;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (borderSprite == null)
        {
            return;
        }

        Bounds bounds = borderSprite.bounds;
        float minY = bounds.min.y;
        float maxY = bounds.max.y;

        Vector3 minLeft = new Vector3(bounds.min.x, minY, 0f);
        Vector3 minRight = new Vector3(bounds.max.x, minY, 0f);
        Vector3 maxLeft = new Vector3(bounds.min.x, maxY, 0f);
        Vector3 maxRight = new Vector3(bounds.max.x, maxY, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(minLeft, minRight);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(maxLeft, maxRight);
    }
}
