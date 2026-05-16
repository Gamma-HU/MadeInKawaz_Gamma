using UnityEngine;

public class IronSandGrain : MonoBehaviour
{
    [SerializeField] private bool disableColliderAfterCollected = true;

    private GameManager_IronSand gameManager;
    private Collider2D grainCollider;
    private Rigidbody2D grainRigidbody;
    private bool collected;

    public bool IsCollected => collected;

    public void Initialize(GameManager_IronSand owner)
    {
        gameManager = owner;
        grainCollider = GetComponent<Collider2D>();
        grainRigidbody = GetComponent<Rigidbody2D>();

        if (grainCollider == null)
        {
            grainCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        grainCollider.isTrigger = true;

        if (grainRigidbody == null)
        {
            grainRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        grainRigidbody.bodyType = RigidbodyType2D.Kinematic;
        grainRigidbody.gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IronSandMagnet magnet = other.GetComponentInParent<IronSandMagnet>();
        if (magnet == null)
        {
            return;
        }

        Collect(magnet);
    }

    private void Collect(IronSandMagnet magnet)
    {
        if (collected)
        {
            return;
        }

        collected = true;

        if (disableColliderAfterCollected && grainCollider != null)
        {
            grainCollider.enabled = false;
        }

        if (grainRigidbody != null)
        {
            grainRigidbody.simulated = false;
        }

        Transform attachRoot = magnet.AttachRoot != null ? magnet.AttachRoot : magnet.transform;
        transform.SetParent(attachRoot, true);

        gameManager?.NotifyCollected(this, magnet);
    }
}
