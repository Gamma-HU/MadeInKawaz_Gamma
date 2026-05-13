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
    [SerializeField] private float rotationDegreesPerPixel = 0.2f;
    [SerializeField] private float[] hammerRotationZThresholds;
    [SerializeField] private float nailMoveDownAmount = 0.1f;

    private float currentMouseMoveAmount = 0f;
    private float totalHammerRotationAmount = 0f;
    private Vector2 prevMousePosition = Vector2.zero;
    private bool cleared = false;
    private int nextHammerThresholdIndex = 0;

    private void Start()
    {
        prevMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        if (cleared)
        {
            return;
        }

        debugText.text = currentMouseMoveAmount.ToString();

        Vector2 currentMousePosition = Input.mousePosition;
        Vector2 delta = currentMousePosition - prevMousePosition;

        // accumulate distance for clearing condition
        currentMouseMoveAmount += delta.magnitude;

        // rotate hammer based on horizontal mouse movement (follow mouse)
        if (hammer != null)
        {
            float rotationDelta = Mathf.Abs(delta.x) * rotationDegreesPerPixel;
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
}
