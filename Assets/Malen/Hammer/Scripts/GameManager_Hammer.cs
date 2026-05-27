using TMPro;
using UnityEngine;

public class GameManager_Hammer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private GameObject hammer;
    [SerializeField] private GameObject nail;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private ParticleSystem hitParticle;

    // [SerializeField] private float requiredMouseMoveAmount = 500f;
    [SerializeField] private float rotationDegreesPerScreenWidth = 72f;
    [SerializeField] private float[] hammerRotationZThresholds;
    [SerializeField] private float nailMoveDownAmount = 0.1f;

    private float currentMouseMoveAmount = 0f;
    private float totalHammerRotationAmount = 0f;
    private Vector2 prevMousePosition = Vector2.zero;
    private bool cleared = false;
    private int nextHammerThresholdIndex = 0;

    private void Start()
    {
        prevMousePosition = GetPointerPosition();
    }

    private void Update()
    {
        if (cleared)
        {
            return;
        }

        debugText.text = currentMouseMoveAmount.ToString();

        Vector2 currentMousePosition = GetPointerPosition();
        Vector2 delta = currentMousePosition - prevMousePosition;

        // accumulate distance for clearing condition
        Vector2 normalizedDelta = GetNormalizedDelta(delta);
        currentMouseMoveAmount += normalizedDelta.magnitude;

        // rotate hammer based on horizontal mouse movement (follow mouse)
        if (hammer != null)
        {
            float rotationDelta = Mathf.Abs(normalizedDelta.x) * rotationDegreesPerScreenWidth;
            hammer.transform.Rotate(0f, 0f, rotationDelta, Space.Self);
            totalHammerRotationAmount += rotationDelta;
        }

        prevMousePosition = currentMousePosition;

        CheckHammerRotation();

        // Clear when total hammer rotation reaches the last threshold value
        if (hammer != null && hammerRotationZThresholds != null && hammerRotationZThresholds.Length > 0)
        {
            float currentHammerRotationAmount = totalHammerRotationAmount;
            Debug.Log(currentHammerRotationAmount);
            float finalThreshold = hammerRotationZThresholds[hammerRotationZThresholds.Length - 1];
            if (currentHammerRotationAmount >= finalThreshold)
            {
                cleared = true;
                GameManager.Clear();
            }
        }
    }

    private void CheckHammerRotation()
    {
        if (hammer == null || nail == null || hammerRotationZThresholds == null)
        {
            return;
        }

        if (nextHammerThresholdIndex >= hammerRotationZThresholds.Length)
        {
            return;
        }

        float currentHammerRotationAmount = totalHammerRotationAmount;

        while (nextHammerThresholdIndex < hammerRotationZThresholds.Length &&
             currentHammerRotationAmount >= hammerRotationZThresholds[nextHammerThresholdIndex])
        {
            Vector3 hammerPosition = hammer.transform.position;
            hammerPosition.y -= nailMoveDownAmount;
            hammer.transform.position = hammerPosition;

            Vector3 nailPosition = nail.transform.position;
            nailPosition.y -= nailMoveDownAmount;
            nail.transform.position = nailPosition;

            seSource.PlayOneShot(hitClip);
            hitParticle.Play();

            nextHammerThresholdIndex++;
        }
    }

    private Vector2 GetPointerPosition()
    {
        if (Input.touchCount > 0)
        {
            return Input.touches[0].position;
        }

        return Input.mousePosition;
    }

    private Vector2 GetNormalizedDelta(Vector2 delta)
    {
        float referenceWidth = Mathf.Max(1f, Screen.width);
        return delta / referenceWidth;
    }
}
