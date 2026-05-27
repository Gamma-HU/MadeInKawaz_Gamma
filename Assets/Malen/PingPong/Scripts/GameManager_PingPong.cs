using UnityEngine;
using DG.Tweening;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager_PingPong : MonoBehaviour
{
    [SerializeField] SpriteRenderer player;
    [SerializeField] SpriteRenderer enemy;
    [SerializeField] GameObject ball;

    [SerializeField] Animator playerAnimator;
    [SerializeField] Animator enemyAnimator;

    [SerializeField] float bounceDuration = 0.25f;
    [SerializeField] float leftDistance = 1f;
    [SerializeField] float firstBounceHeight = 1.2f;
    [SerializeField] float secondBounceHeight = 0.8f;
    [SerializeField] float tableDropHeight = 0.5f;
    [SerializeField] float floorDropHeight = 1.5f;
    [SerializeField] Color gizmoColor = new Color(0.2f, 1f, 0.7f, 0.9f);

    // [SerializeField] AudioSource audioSource;
    // [SerializeField] AudioClip hitClip;
    // [SerializeField] AudioClip suceedClip;
    // [SerializeField] AudioClip failedClip;


    private void Start()
    {

    }

    private void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (ball == null) return;

#if UNITY_EDITOR
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != ball && selectedObject != gameObject) return;
#endif

        Transform ballTransform = ball.transform;
        Vector3 startPos = ballTransform.position;
        Vector3 min = startPos;
        Vector3 max = startPos;
        for (int i = 0; i <= 32; i++)
        {
            float t = (float)i / 32f;
            Vector3 point = EvaluateTrajectoryPoint(startPos, t);
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }
        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;
        size.z = Mathf.Max(size.z, 0.05f);

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(center, size);

        DrawTrajectoryGizmo(startPos);
    }

    private void DrawTrajectoryGizmo(Vector3 startPos)
    {
        const int sampleCount = 32;

        Gizmos.color = gizmoColor;

        Vector3 previousPoint = EvaluateTrajectoryPoint(startPos, 0f);
        for (int i = 1; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 currentPoint = EvaluateTrajectoryPoint(startPos, t);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    private Vector3 EvaluateTrajectoryPoint(Vector3 startPos, float normalizedTime)
    {
        float t = Mathf.Clamp01(normalizedTime);
        float x = Mathf.Lerp(startPos.x, startPos.x - leftDistance, t);

        float segmentDuration = 1f / 5f;
        float elapsed = t;
        int segmentIndex = Mathf.Clamp(Mathf.FloorToInt(elapsed / segmentDuration), 0, 4);
        float segmentProgress = Mathf.InverseLerp(segmentIndex * segmentDuration, (segmentIndex + 1) * segmentDuration, elapsed);

        float tableY = startPos.y - tableDropHeight;
        float floorY = startPos.y - floorDropHeight;
        float firstPeakY = tableY + firstBounceHeight;
        float secondPeakY = tableY + secondBounceHeight;

        float y;
        if (segmentIndex == 0)
        {
            y = Mathf.Lerp(startPos.y, tableY, EaseInFlash(segmentProgress));
        }
        else if (segmentIndex == 1)
        {
            y = Mathf.Lerp(tableY, firstPeakY, EaseOutFlash(segmentProgress));
        }
        else if (segmentIndex == 2)
        {
            y = Mathf.Lerp(firstPeakY, tableY, EaseInFlash(segmentProgress));
        }
        else if (segmentIndex == 3)
        {
            y = Mathf.Lerp(tableY, secondPeakY, EaseOutFlash(segmentProgress));
        }
        else
        {
            y = Mathf.Lerp(secondPeakY, floorY, EaseInFlash(segmentProgress));
        }

        return new Vector3(x, y, startPos.z);
    }

    private float EaseInFlash(float t)
    {
        return DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(t), Ease.InFlash);
    }

    private float EaseOutFlash(float t)
    {
        return DOVirtual.EasedValue(0f, 1f, Mathf.Clamp01(t), Ease.OutFlash);
    }
}
