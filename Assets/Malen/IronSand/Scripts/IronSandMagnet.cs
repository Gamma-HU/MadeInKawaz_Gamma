using UnityEngine;

public class IronSandMagnet : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private BoxCollider2D movementArea;
    [SerializeField] private Transform attachRoot;
    [SerializeField] private bool dragFromAnywhere = true;
    [SerializeField] private bool keepInsideMovementArea = false;

    private Collider2D magnetCollider;
    private Rigidbody2D magnetRigidbody;
    private Vector3 dragOffset;
    private bool dragging;

    public Transform AttachRoot => attachRoot != null ? attachRoot : transform;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        magnetCollider = GetComponent<Collider2D>();
        if (magnetCollider == null)
        {
            magnetCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        magnetCollider.isTrigger = true;

        magnetRigidbody = GetComponent<Rigidbody2D>();
        if (magnetRigidbody == null)
        {
            magnetRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        magnetRigidbody.bodyType = RigidbodyType2D.Kinematic;
        magnetRigidbody.gravityScale = 0f;
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 pointerWorldPosition = GetPointerWorldPosition();

        if (Input.GetMouseButtonDown(0) && CanStartDrag(pointerWorldPosition))
        {
            dragging = true;
            dragOffset = transform.position - pointerWorldPosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (!dragging || !Input.GetMouseButton(0))
        {
            return;
        }

        Vector3 nextPosition = pointerWorldPosition + dragOffset;
        nextPosition.z = transform.position.z;

        if (keepInsideMovementArea && movementArea != null)
        {
            Bounds bounds = movementArea.bounds;
            nextPosition.x = Mathf.Clamp(nextPosition.x, bounds.min.x, bounds.max.x);
            nextPosition.y = Mathf.Clamp(nextPosition.y, bounds.min.y, bounds.max.y);
        }

        magnetRigidbody.MovePosition(nextPosition);
    }

    private bool CanStartDrag(Vector3 pointerWorldPosition)
    {
        return dragFromAnywhere || magnetCollider.OverlapPoint(pointerWorldPosition);
    }

    private Vector3 GetPointerWorldPosition()
    {
        Vector3 pointerPosition = Input.mousePosition;
        pointerPosition.z = Mathf.Abs(targetCamera.transform.position.z - transform.position.z);
        return targetCamera.ScreenToWorldPoint(pointerPosition);
    }
}
